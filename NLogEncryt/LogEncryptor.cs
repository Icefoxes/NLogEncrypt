using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NLogEncryt
{
    // AES Log Encryptor
    public class LogEncryptor
    {
        // 64 bit key
        private static readonly byte[] Key = {
            0x4a, 0xfa, 0xbc, 0x2f, 0x85, 0xb9, 0x3d, 0xad,
            0x30, 0x5a, 0x4a, 0x96, 0x8e, 0x23, 0xd1, 0x2a,
            0xf0, 0xbc, 0x8b, 0x04, 0x69, 0xa4, 0xaa, 0x50,
            0xf5, 0x8d, 0x95, 0x10, 0x7a, 0x73, 0xee, 0x61 };

        // 32 bit IV
        private static readonly byte[] IV = {
            0xd1, 0xa1, 0xd3, 0x1f, 0xda, 0x89, 0xbf, 0x19,
            0x25, 0x76, 0x3a, 0x7a, 0x69, 0x6b, 0xaf, 0xcf };

        /// <summary>
        /// Padding Mode
        /// </summary>
        private static readonly PaddingMode Padding = PaddingMode.PKCS7;

        /// <summary>
        /// Encrypt Data
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string EncryptData(string original)
        {
            return EncryptStringToBytes(original, Key, IV);
        }

        /// <summary>
        /// Decrypt Data
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        public static string DecryptData(string encrypted)
        {
            return DecryptStringFromBytes(encrypted, Key, IV);
        }

        /// <summary>
        /// Encrypt String To Bytes With AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        private static string EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            string encrypted = string.Empty;
            // Create an Aes object with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = Padding;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor();
                byte[] planbytes = Encoding.UTF8.GetBytes(plainText);
                // Create the streams used for encryption.
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        cs.Write(planbytes, 0, planbytes.Length);
                    encrypted = Convert.ToBase64String(ms.ToArray());
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Decrypt String From Bytes with AES
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        private static string DecryptStringFromBytes(string cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

            // Declare the string used to hold the decrypted text.
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            string plaintext = string.Empty;

            // Create an Aes object with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = Padding;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor();

                // Create the streams used for decryption.
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    plaintext = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return plaintext;
        }
    }
}