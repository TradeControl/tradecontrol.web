using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace TradeControl.Web.Mail
{
    public class FileHelper
    {
        private static readonly byte[] _allowedChars = { };

        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }
        };

        public static async Task<byte[]> ProcessFormFile(IFormFile formFile,ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit)
        {
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(formFile.FileName);

            if (formFile.Length == 0)
            {
                modelState.AddModelError(formFile.Name, $"{trustedFileNameForDisplay} is empty.");
                return new byte[0];
            }

            if (formFile.Length > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / 1048576;
                modelState.AddModelError(formFile.Name, $"{trustedFileNameForDisplay} exceeds " + $"{megabyteSizeLimit:N1} MB.");
                return new byte[0];
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);

                    if (memoryStream.Length == 0)
                        modelState.AddModelError(formFile.Name,$"{trustedFileNameForDisplay} is empty.");

                    if (!IsValidFileExtensionAndSignature(formFile.FileName, memoryStream, permittedExtensions))
                        modelState.AddModelError(formFile.Name, $"{trustedFileNameForDisplay} file type isn't permitted or the file's signature  doesn't match the file's extension.");
                    else
                        return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                modelState.AddModelError(formFile.Name, $"{trustedFileNameForDisplay} upload failed. Error: {e.HResult}");
            }

            return new byte[0];
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                if (ext.Equals(".txt") || ext.Equals(".html"))
                {
                    if (_allowedChars.Length == 0)
                    {
                        for (var i = 0; i < data.Length; i++)
                        {
                            if (reader.ReadByte() > sbyte.MaxValue)
                                return false;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < data.Length; i++)
                        {
                            var b = reader.ReadByte();
                            if (b > sbyte.MaxValue || !_allowedChars.Contains(b))
                                return false;
                        }
                    }

                    return true;
                }

                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
