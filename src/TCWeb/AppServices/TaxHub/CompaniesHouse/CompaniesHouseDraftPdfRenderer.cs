using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public interface ICompaniesHouseDraftPdfRenderer
{
    byte[] Render(byte[] retainedIxbrl, string sourceSha256);
}

public sealed class CompaniesHouseDraftPdfRenderer : ICompaniesHouseDraftPdfRenderer
{
    private static readonly XNamespace Xhtml = "http://www.w3.org/1999/xhtml";
    private static readonly object FontGate = new();
    private static bool _fontConfigured;

    public byte[] Render(byte[] retainedIxbrl, string sourceSha256)
    {
        ArgumentNullException.ThrowIfNull(retainedIxbrl);
        if (retainedIxbrl.Length == 0)
            throw new ArgumentException("The retained accounts document is empty.", nameof(retainedIxbrl));
        if (string.IsNullOrWhiteSpace(sourceSha256) || sourceSha256.Length != 64)
            throw new ArgumentException("A valid source-document SHA-256 is required.", nameof(sourceSha256));

        EnsureFontResolver();
        var source = XDocument.Load(new MemoryStream(retainedIxbrl), LoadOptions.PreserveWhitespace);
        var accounts = source.Descendants(Xhtml + "div")
            .FirstOrDefault(element => HasClass(element, "accounts-document"))
            ?? throw new InvalidOperationException("The retained document does not contain the statutory accounts presentation.");

        using var document = new PdfDocument();
        document.Info.Title = "Companies House accounts draft";
        document.Info.Subject = $"DRAFT - NOT FILED. Source iXBRL SHA-256 {sourceSha256.ToUpperInvariant()}";
        document.Info.Author = "Trade Control";
        document.Info.Creator = "Trade Control Tax Hub";

        var writer = new DraftWriter(document, sourceSha256.ToUpperInvariant());
        writer.Render(accounts);
        writer.Complete();

        using var output = new MemoryStream();
        document.Save(output, false);
        return output.ToArray();
    }

    private static bool HasClass(XElement element, string value) =>
        (element.Attribute("class")?.Value ?? string.Empty)
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Contains(value, StringComparer.Ordinal);

    private static void EnsureFontResolver()
    {
        lock (FontGate)
        {
            if (_fontConfigured) return;
            GlobalFontSettings.FontResolver ??= new PortableSansFontResolver();
            _fontConfigured = true;
        }
    }

    private sealed class DraftWriter
    {
        private const double Margin = 42;
        private const double FooterHeight = 32;
        private readonly PdfDocument _document;
        private readonly string _sourceSha256;
        private readonly XFont _body = new("TradeControl Sans", 9.5, XFontStyleEx.Regular);
        private readonly XFont _bodyBold = new("TradeControl Sans", 9.5, XFontStyleEx.Bold);
        private readonly XFont _h1 = new("TradeControl Sans", 18, XFontStyleEx.Bold);
        private readonly XFont _h2 = new("TradeControl Sans", 14, XFontStyleEx.Bold);
        private readonly XFont _small = new("TradeControl Sans", 6.5, XFontStyleEx.Regular);
        private XGraphics? _graphics;
        private PdfPage? _page;
        private double _y;

        public DraftWriter(PdfDocument document, string sourceSha256)
        {
            _document = document;
            _sourceSha256 = sourceSha256;
            NewPage();
        }

        public void Render(XElement accounts)
        {
            foreach (var child in accounts.Elements()) RenderElement(child);
        }

        public void Complete() => _graphics?.Dispose();

        private void RenderElement(XElement element)
        {
            // Keep the short filing-classification block together. Splitting a few
            // list items onto an otherwise empty page makes the review copy harder
            // to read even though the underlying iXBRL remains unchanged.
            if (element.Name == Xhtml + "div" && HasClass(element, "filing-information"))
                Ensure(180);

            if (element.Name == Xhtml + "h1") { Heading(Text(element), _h1, 10, 10, centred: true); return; }
            if (element.Name == Xhtml + "h2") { Heading(Text(element), _h2, 8, 7, centred: true); return; }
            if (element.Name == Xhtml + "p") { Paragraph(Text(element)); return; }
            if (element.Name == Xhtml + "table") { Table(element); return; }
            if (element.Name == Xhtml + "ul")
            {
                foreach (var item in element.Elements(Xhtml + "li")) Paragraph($"- {Text(item)}", 14);
                _y += 3;
                return;
            }
            foreach (var child in element.Elements()) RenderElement(child);
        }

        private void Heading(string text, XFont font, double before, double after, bool centred)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _y += before;
            var lines = Wrap(text, font, ContentWidth);
            Ensure(lines.Count * (font.Size + 3) + after);
            foreach (var line in lines)
            {
                _graphics!.DrawString(line, font, XBrushes.Black,
                    new XRect(Margin, _y, ContentWidth, font.Size + 4),
                    centred ? XStringFormats.TopCenter : XStringFormats.TopLeft);
                _y += font.Size + 3;
            }
            _y += after;
        }

        private void Paragraph(string text, double indent = 0)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var lines = Wrap(text, _body, ContentWidth - indent);
            Ensure(lines.Count * 13 + 5);
            foreach (var line in lines)
            {
                _graphics!.DrawString(line, _body, XBrushes.Black,
                    new XRect(Margin + indent, _y, ContentWidth - indent, 13), XStringFormats.TopLeft);
                _y += 13;
            }
            _y += 5;
        }

        private void Table(XElement table)
        {
            _y += 4;
            var rows = table.Descendants(Xhtml + "tr").ToArray();
            foreach (var row in rows)
            {
                var cells = row.Elements().Where(cell => cell.Name == Xhtml + "th" || cell.Name == Xhtml + "td").ToArray();
                if (cells.Length == 0) continue;
                var widths = ColumnWidths(cells.Length);
                var header = cells.Any(cell => cell.Name == Xhtml + "th");
                var total = HasClass(row, "total");
                var font = header || total ? _bodyBold : _body;
                var wrapped = cells.Select((cell, index) => Wrap(Text(cell), font, widths[index] - 10)).ToArray();
                var height = Math.Max(18, wrapped.Max(lines => lines.Count) * 12 + 6);
                Ensure(height);
                var x = Margin;
                for (var index = 0; index < cells.Length; index++)
                {
                    var format = index == 0 ? XStringFormats.TopLeft : XStringFormats.TopRight;
                    var lineY = _y + 4;
                    foreach (var line in wrapped[index])
                    {
                        _graphics!.DrawString(line, font, XBrushes.Black,
                            new XRect(x + 5, lineY, widths[index] - 10, 12), format);
                        lineY += 12;
                    }
                    x += widths[index];
                }
                _graphics!.DrawLine(total ? XPens.Black : XPens.LightGray, Margin, _y + height, Margin + ContentWidth, _y + height);
                _y += height;
            }
            _y += 7;
        }

        private double[] ColumnWidths(int count)
        {
            if (count <= 1) return [ContentWidth];
            var first = ContentWidth * .56;
            var remaining = (ContentWidth - first) / (count - 1);
            return Enumerable.Range(0, count).Select(index => index == 0 ? first : remaining).ToArray();
        }

        private IReadOnlyList<string> Wrap(string text, XFont font, double width)
        {
            var words = Normalise(text).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return [string.Empty];
            var lines = new List<string>();
            var line = words[0];
            foreach (var word in words.Skip(1))
            {
                var candidate = $"{line} {word}";
                if (_graphics!.MeasureString(candidate, font).Width <= width) line = candidate;
                else { lines.Add(line); line = word; }
            }
            lines.Add(line);
            return lines;
        }

        private void Ensure(double height)
        {
            if (_page is not null && _y + height <= _page.Height.Point - Margin - FooterHeight) return;
            _graphics?.Dispose();
            NewPage();
        }

        private void NewPage()
        {
            _page = _document.AddPage();
            _page.Size = PdfSharp.PageSize.A4;
            _graphics = XGraphics.FromPdfPage(_page);
            _y = Margin;
            DrawWatermark();
            DrawFooter(_document.PageCount);
        }

        private void DrawWatermark()
        {
            var state = _graphics!.Save();
            _graphics.TranslateTransform(_page!.Width.Point / 2, _page.Height.Point / 2);
            _graphics.RotateTransform(-35);
            var watermark = new XFont("TradeControl Sans", 42, XFontStyleEx.Bold);
            _graphics.DrawString("DRAFT - NOT FILED", watermark,
                new XSolidBrush(XColor.FromArgb(34, 120, 120, 120)),
                new XRect(-260, -35, 520, 70), XStringFormats.Center);
            _graphics.Restore(state);
        }

        private void DrawFooter(int pageNumber)
        {
            var y = _page!.Height.Point - Margin + 4;
            _graphics!.DrawLine(XPens.LightGray, Margin, y - 7, Margin + ContentWidth, y - 7);
            _graphics.DrawString($"DRAFT - NOT FILED | Source iXBRL SHA-256 {_sourceSha256}",
                _small, XBrushes.DimGray, new XRect(Margin, y, ContentWidth - 45, 18), XStringFormats.TopLeft);
            _graphics.DrawString($"Page {pageNumber}", _small, XBrushes.DimGray,
                new XRect(Margin + ContentWidth - 45, y, 45, 18), XStringFormats.TopRight);
        }

        private double ContentWidth => _page!.Width.Point - (2 * Margin);

        private static string Text(XElement element) => Normalise(element.Value);

        private static string Normalise(string value) => string.Join(' ', value
            .Replace('\u2013', '-')
            .Replace('\u2014', '-')
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private sealed class PortableSansFontResolver : IFontResolver
    {
        private const string RegularFace = "tradecontrol-regular";
        private const string BoldFace = "tradecontrol-bold";
        private readonly Lazy<byte[]> _regular = new(() => ReadFont(false));
        private readonly Lazy<byte[]> _bold = new(() => ReadFont(true));

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
            new(isBold ? BoldFace : RegularFace, mustSimulateBold: false, mustSimulateItalic: isItalic);

        public byte[]? GetFont(string faceName) => faceName switch
        {
            RegularFace => _regular.Value,
            BoldFace => _bold.Value,
            _ => null
        };

        private static byte[] ReadFont(bool bold)
        {
            var windowsFonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            var candidates = bold
                ? new[]
                {
                    Path.Combine(windowsFonts, "arialbd.ttf"),
                    "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
                    "/usr/share/fonts/truetype/liberation2/LiberationSans-Bold.ttf"
                }
                : new[]
                {
                    Path.Combine(windowsFonts, "arial.ttf"),
                    "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                    "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf"
                };
            var path = candidates.FirstOrDefault(File.Exists)
                ?? throw new InvalidOperationException("No supported PDF font is installed on this host.");
            return File.ReadAllBytes(path);
        }
    }
}
