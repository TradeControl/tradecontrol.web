using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TradeControl.Web.Mail;

internal static class FileUploadValidationTests
{
    public static async Task<int> RunAsync()
    {
        var assertions = 0;

        async Task AssertSvgAsync(string source, bool expected, string message)
        {
            assertions++;
            var bytes = Encoding.UTF8.GetBytes(source);
            await using var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", "icon.svg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/svg+xml"
            };
            var modelState = new ModelStateDictionary();

            var result = await FileHelper.ProcessFormFile(file, modelState, [".svg"], 1_048_576);
            if ((modelState.IsValid && result.Length > 0) != expected)
                throw new InvalidOperationException(message);
        }

        await AssertSvgAsync(
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 64 64\"><circle cx=\"32\" cy=\"32\" r=\"30\" fill=\"#198754\"/><text x=\"32\" y=\"40\" text-anchor=\"middle\">B1</text></svg>",
            expected: true,
            "A safe company SVG icon was rejected.");
        await AssertSvgAsync(
            "<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>",
            expected: false,
            "An SVG script payload was accepted.");
        await AssertSvgAsync(
            "<svg xmlns=\"http://www.w3.org/2000/svg\"><image href=\"https://example.test/tracker.png\"/></svg>",
            expected: false,
            "An SVG external resource was accepted.");

        return assertions;
    }
}
