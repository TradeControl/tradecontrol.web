using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace TradeControl.Web.Mail
{
    /// <summary>
    /// Encrypts or decrypts strings according to passed keys/vectors using the RijndaelManaged algorithm
    /// </summary>
    public class Encrypt
    {
        readonly byte[] key;
        readonly byte[] iv;

        public Encrypt(byte[] _key, byte[] _iv)
        {
            key = _key;
            iv = _iv;
        }

        public Encrypt(byte[] _fullKey)
        {
            key = new byte[16];
            iv = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                key[i] = _fullKey[i];
                iv[i + 16] = _fullKey[i + 16];
            }

        }

        private byte[] ToByte(char[] _chars)
        {
            byte[] bytes = new byte[_chars.Length];
            for (int i = 0; i < _chars.Length; i++)
                bytes[i] = (byte)_chars[i];
            return bytes;
        }

        private string ByteToString(byte[] _bytes)
        {
            string result = string.Empty;
            for (int i = 0; i < _bytes.Length; i++)
                result = result + (char)_bytes[i];

            return result;
        }

        #region Encryption
        private byte[] Key
        {
            get
            {
                return key;
            }
        }

        private byte[] IV
        {
            get
            {
                return iv;
            }
        }

        public string DecryptString(string _encrypted)
        {
            string decrypt;

            try
            {
                ASCIIEncoding textConverter = new();
                byte[] encrypted = ToByte(_encrypted.ToCharArray());

                using Aes aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream msDecrypt = new(encrypted);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);

                byte[] fromEncrypt = new byte[encrypted.Length];
                int bytesRead = 0, read;
                while (bytesRead < fromEncrypt.Length &&
                       (read = csDecrypt.Read(fromEncrypt, bytesRead, fromEncrypt.Length - bytesRead)) > 0)
                {
                    bytesRead += read;
                }

                decrypt = textConverter.GetString(fromEncrypt, 0, bytesRead).Trim(new char[] { ' ', '\0' });
            }
            catch
            {
                decrypt = string.Empty;
            }

            return decrypt;
        }

        public string EncryptString(string _decrypted)
        {
            try
            {
                ASCIIEncoding textConverter = new();
                using Aes aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream msEncrypt = new();
                using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);

                byte[] toEncrypt = textConverter.GetBytes(_decrypted);

                csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
                csEncrypt.FlushFinalBlock();

                byte[] encrypted = msEncrypt.ToArray();

                return ByteToString(encrypted);
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
