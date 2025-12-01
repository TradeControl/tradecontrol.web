/// <summary>
/// AES string encryptor using CBC + PKCS7. Ciphertext is Base64-encoded for safe storage.
/// No legacy char->byte fallback is included — callers must supply correct key/IV and use Base64 ciphertext.
/// </summary>
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace TradeControl.Web.Mail
{
    /// <summary>
    /// Performs AES encryption/decryption of UTF-8 strings. Ciphertext is returned/accepted as Base64.
    /// </summary>
    public class Encrypt
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        /// <summary>
        /// Create an Encryptor with an explicit key and IV.
        /// Key must be 16 (AES-128) or 32 (AES-256) bytes. IV must be 16 bytes.
        /// </summary>
        /// <param name="_key">AES key (16 or 32 bytes)</param>
        /// <param name="_iv">AES IV (16 bytes)</param>
        public Encrypt(byte[] _key, byte[] _iv)
        {
            if (_key is null) throw new ArgumentNullException(nameof(_key));
            if (_iv is null) throw new ArgumentNullException(nameof(_iv));
            if (!(_key.Length == 16 || _key.Length == 32)) throw new ArgumentException("Key must be 16 or 32 bytes.", nameof(_key));
            if (_iv.Length != 16) throw new ArgumentException("IV must be 16 bytes.", nameof(_iv));

            key = (byte[])_key.Clone();
            iv = (byte[])_iv.Clone();
        }

        /// <summary>
        /// Encrypts the provided plaintext using AES-CBC/PKCS7 and returns Base64 ciphertext.
        /// Returns empty string on error (keeps calling code simple).
        /// </summary>
        /// <param name="plaintext">UTF-8 plaintext to encrypt</param>
        /// <returns>Base64 ciphertext or empty string on failure</returns>
        public string EncryptString(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);

                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using MemoryStream ms = new();
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(plainBytes, 0, plainBytes.Length);
                    cs.FlushFinalBlock();
                }

                byte[] cipher = ms.ToArray();
                return Convert.ToBase64String(cipher);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts Base64 ciphertext produced by EncryptString and returns UTF-8 plaintext.
        /// Returns empty string on error.
        /// </summary>
        /// <param name="encrypted">Base64 ciphertext</param>
        /// <returns>Decrypted plaintext or empty string on failure</returns>
        public string DecryptString(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            try
            {
                byte[] cipher = Convert.FromBase64String(encrypted);

                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using MemoryStream ms = new(cipher);
                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

                using MemoryStream plainMs = new();
                cs.CopyTo(plainMs);
                byte[] plainBytes = plainMs.ToArray();
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}