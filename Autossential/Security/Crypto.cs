using Autossential.Enums;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Autossential.Security
{
    /// <summary>
    /// Provide methods to symmetric encryption and decryption.
    /// </summary>
    public class Crypto : IDisposable
    {
        private readonly SymmetricAlgorithm _algorithm;
        private readonly Encoding _encoding;
        private readonly int _iterations;
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        private const int SALT_SIZE = 8;
        private const int MINIMUM_ITERATIONS_RECOMMENDED = 1000;

        /// <summary>
        /// Initializes a new instance of <see cref="Crypto{T}"/> class.
        /// </summary>
        /// <param name="algorithm">The algorithm to be used.</param>
        /// <param name="encoding">The encoding to be used in the input and password.</param>
        /// <param name="iterations">The number of iterations for the operation. The minimum recommended number is 1000.</param>
        public Crypto(SymmetricAlgorithms algorithm, Encoding encoding, int iterations = MINIMUM_ITERATIONS_RECOMMENDED)
        {
            _algorithm = GetSymmetricAlgorithm(algorithm);
            _encoding = encoding;
            _iterations = iterations;
        }

        private SymmetricAlgorithm GetSymmetricAlgorithm(SymmetricAlgorithms algorithm)
        {
            switch (algorithm)
            {
                case SymmetricAlgorithms.AES:
                    return new AesCryptoServiceProvider(); // TODO: Use AesCng after upgrading to .NET Framework 4.6.2
                case SymmetricAlgorithms.DES:
                    return new DESCryptoServiceProvider();

                case SymmetricAlgorithms.RC2:
                    return new RC2CryptoServiceProvider();

                case SymmetricAlgorithms.Rijndael:
                    return new RijndaelManaged();

                case SymmetricAlgorithms.TripleDES:
                    return new TripleDESCryptoServiceProvider();

                default:
                    throw new InvalidOperationException("Unsupported algorithm");
            }
        }

        /// <summary>
        /// Encrypts the specified input string.
        /// </summary>
        /// <param name="input">The text to be encrypted.</param>
        /// <param name="password">The password to be used on encryption.</param>
        /// <returns>A string that represents the encrypted input string.</returns>
        public string Encrypt(string input, string password)
        {
            return Convert.ToBase64String(Encrypt(_encoding.GetBytes(input), _encoding.GetBytes(password)));
        }

        /// <summary>
        /// Encrypts the specified input bytes.
        /// </summary>
        /// <param name="inputBytes">The bytes to be encrypted.</param>
        /// <param name="passwordBytes">The password bytes to be used on encryption.</param>
        /// <returns>A byte array that represents the encrypted input bytes.</returns>
        public byte[] Encrypt(byte[] inputBytes, byte[] passwordBytes)
        {
            var saltBytes = new byte[SALT_SIZE];
            _rng.GetBytes(saltBytes);

            using (var r = new Rfc2898DeriveBytes(passwordBytes, saltBytes, _iterations))
            {
                var key = r.GetBytes(_algorithm.KeySize >> 3);
                var IV = r.GetBytes(_algorithm.BlockSize >> 3);

                byte[] encryptedBytes;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, _algorithm.CreateEncryptor(key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    encryptedBytes = ms.ToArray();
                }

                var result = new byte[saltBytes.Length + IV.Length + encryptedBytes.Length];
                Array.Copy(saltBytes, 0, result, 0, saltBytes.Length);
                Array.Copy(IV, 0, result, saltBytes.Length, IV.Length);
                Array.Copy(encryptedBytes, 0, result, saltBytes.Length + IV.Length, encryptedBytes.Length);

                return result;
            }
        }

        /// <summary>
        /// Decrypts the specified input string.
        /// </summary>
        /// <param name="input">The text to be decrypted.</param>
        /// <param name="password">The password to be used on decryption.</param>
        /// <returns>A string that represents the decrypted input string.</returns>
        public string Decrypt(string input, string password)
        {
            return _encoding.GetString(Decrypt(Convert.FromBase64String(input), _encoding.GetBytes(password)));
        }

        /// <summary>
        /// Decrypts the specified byte array.
        /// </summary>
        /// <param name="inputBytes">The bytes to be decrypted.</param>
        /// <param name="passwordBytes">The password bytes to be used on decryption.</param>
        /// <returns>A byte array that represents the decrypted input bytes.</returns>
        public byte[] Decrypt(byte[] inputBytes, byte[] passwordBytes)
        {
            byte[] decryptedBytes;
            var saltBytes = new byte[SALT_SIZE];
            var IV = new byte[_algorithm.BlockSize >> 3];

            var encryptedData = new byte[inputBytes.Length - IV.Length - saltBytes.Length];
            Array.Copy(inputBytes, 0, saltBytes, 0, saltBytes.Length);
            Array.Copy(inputBytes, saltBytes.Length, IV, 0, IV.Length);
            Array.Copy(inputBytes, saltBytes.Length + IV.Length, encryptedData, 0, encryptedData.Length);
            using (var r = new Rfc2898DeriveBytes(passwordBytes, saltBytes, _iterations))
            {
                var key = r.GetBytes(_algorithm.KeySize >> 3);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, _algorithm.CreateDecryptor(key, IV), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, 0, encryptedData.Length);
                        cs.FlushFinalBlock();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }
            return decryptedBytes;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _rng.Dispose();
            _algorithm.Dispose();
        }
    }
}