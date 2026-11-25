using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TradeControl.Tax.UK.Adapters.Submission.TransactionEngine;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

/// <summary>
/// Process-local state owned by the development fake authority, not the product
/// workflow. It simulates HMRC retaining a transaction after the application has
/// durably moved to SubmissionUncertain.
/// </summary>
internal sealed class CorporationTaxFakeAuthorityState
{
    private readonly ConcurrentDictionary<string, string> _correlations =
        new(StringComparer.Ordinal);

    internal string Accept(string transactionId) => _correlations.GetOrAdd(transactionId,
        value => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..32]);

    internal IReadOnlyList<KeyValuePair<string, string>> Transactions() =>
        _correlations.OrderByDescending(value => value.Key).Take(100).ToArray();
}

/// <summary>
/// Development-only in-process Transaction Engine double. The named HttpClient
/// receives this handler only in DevelopmentFake mode, so a realistic production-
/// shaped wire exchange is exercised without opening a network connection.
/// </summary>
internal sealed class CorporationTaxFakeAuthorityHandler(
    CorporationTaxFakeAuthorityState state) : HttpMessageHandler
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private const string Endpoint =
        "https://transaction-engine.tax.service.gov.uk/submission";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Post
            || request.RequestUri?.AbsoluteUri != Endpoint
            || request.Content is null)
            throw new InvalidOperationException(
                "The development fake authority accepts only the pinned production-shaped request.");

        var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
        var document = Load(bytes);
        XNamespace gov = TransactionEngineWire.GovTalkNamespace;
        var function = Required(document, gov + "Function");
        var qualifier = Required(document, gov + "Qualifier");
        var correlation = document.Descendants(gov + "CorrelationID")
            .Select(value => value.Value.Trim()).SingleOrDefault();

        byte[] response = (qualifier, function) switch
        {
            ("request", "submit") => Acknowledge(document, gov),
            ("request", "list") => DataResponse(),
            ("poll", "submit") => Terminal(Required(correlation, "CorrelationID")),
            ("request", "delete") => Delete(Required(correlation, "CorrelationID")),
            _ => throw new InvalidOperationException(
                "The development fake authority received an unsupported protocol operation.")
        };
        var result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(response)
        };
        result.Content.Headers.ContentType = new("application/xml") { CharSet = "utf-8" };
        result.Headers.Add("X-TradeControl-Fake-Authority", "true");
        return result;
    }

    private byte[] Acknowledge(XDocument request, XNamespace gov)
    {
        var transactionId = Required(request, gov + "TransactionID");
        var correlation = state.Accept(transactionId);
        return Message("acknowledgement", "submit", correlation,
            $"<ResponseEndPoint PollInterval=\"1\">{Endpoint}</ResponseEndPoint>");
    }

    private byte[] DataResponse()
    {
        var records = string.Concat(state.Transactions().Select(value =>
            $"<StatusRecord><TimeStamp>2026-01-01T00:00:00Z</TimeStamp>" +
            $"<CorrelationID>{value.Value}</CorrelationID>" +
            $"<TransactionID>{value.Key}</TransactionID>" +
            "<Status>SUBMISSION_ACKNOWLEDGE</Status></StatusRecord>"));
        return Message("response", "list", null, body:
            "<StatusReport xmlns=\"urn:gateway/statusrequest\">" + records +
            "</StatusReport><FakeAuthorityResponse xmlns=\"urn:tradecontrol:ct:fake-authority\">true</FakeAuthorityResponse>");
    }

    private static byte[] Terminal(string correlation) => Message("response", "submit",
        correlation,
        $"<ResponseEndPoint PollInterval=\"1\">{Endpoint}</ResponseEndPoint>",
        "<SuccessResponse xmlns=\"http://www.inlandrevenue.gov.uk/SuccessResponse\">" +
        "<IRmarkReceipt><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />" +
        "</IRmarkReceipt></SuccessResponse>" +
        "<FakeAuthorityResponse xmlns=\"urn:tradecontrol:ct:fake-authority\">true</FakeAuthorityResponse>");

    private static byte[] Delete(string correlation) => Message("response", "delete",
        correlation, body:
        "<FakeAuthorityResponse xmlns=\"urn:tradecontrol:ct:fake-authority\">true</FakeAuthorityResponse>");

    private static byte[] Message(string qualifier, string function, string? correlation,
        string details = "", string body = "") => Utf8.GetBytes($"""
        <?xml version="1.0" encoding="utf-8"?><GovTalkMessage xmlns="http://www.govtalk.gov.uk/CM/envelope"><EnvelopeVersion>2.0</EnvelopeVersion><Header><MessageDetails><Class>HMRC-CT-CT600</Class><Qualifier>{qualifier}</Qualifier><Function>{function}</Function>{(correlation is null ? "" : $"<CorrelationID>{correlation}</CorrelationID>")}{details}</MessageDetails><SenderDetails /></Header><GovTalkDetails><Keys /></GovTalkDetails><Body>{body}</Body></GovTalkMessage>
        """);

    private static XDocument Load(ReadOnlySpan<byte> bytes)
    {
        using var stream = new MemoryStream(bytes.ToArray(), writable: false);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 8 * 1024 * 1024
        });
        return XDocument.Load(reader, LoadOptions.None);
    }

    private static string Required(XDocument document, XName name) =>
        Required(document.Descendants(name).Select(value => value.Value.Trim()).SingleOrDefault(),
            name.LocalName);

    private static string Required(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidDataException($"The fake-authority request has no {name}.")
            : value;
}
