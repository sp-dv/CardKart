using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CardKartShared.Network
{
    public static class RSAEncryption
    {
        //private static RSAParameters PublicKey { get; set; } = JsonConvert.DeserializeObject<RSAParameters>("{\"D\":null,\"DP\":null,\"DQ\":null,\"Exponent\":\"AQAB\",\"InverseQ\":null,\"Modulus\":\"qIsOn6mUS08MDm2MNngj9UN1ZqM5bqKbic4nRBSrt4FkzE5vxv7gFlRW0t6phBvrlTBcGpYWxput6PMHJQ2zHzgPnOt9kgHKUy/Oh44p7IqeYoGKmSBDeUfw1vr6+kCRmBXSUVxug9RcRgnT1daVClCaKsLs/zTNosVlgx17RgU=\",\"P\":null,\"Q\":null}");
        //private static RSAParameters PrivateKey { get; set; }

        /// <summary>
        /// Generates a RSA key pair for client/server handshake encryption.
        /// To use the keys run 'JsonConvert.DeserializeObject<RSAParameters>(keyString)'
        /// </summary>
        /// <returns>(PrivateKeyString, PublicKeyString)</returns>
        public static (string, string) GenerateKeys()
        {
            RSACryptoServiceProvider rsaSP1 = new RSACryptoServiceProvider();

            var privKey = rsaSP1.ExportParameters(true);
            var privKeyString = JsonConvert.SerializeObject(privKey);
            var pubKey = rsaSP1.ExportParameters(false);
            var pubKeyString = JsonConvert.SerializeObject(pubKey);

            return (privKeyString, pubKeyString);
        }

        private const int MaxRSABlockSize = 86;
        private const int RSAOutputBlockSize = 128;

        public static byte[] RSAEncrypt(string plainText, RSAParameters publicKey)
        {
            var dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
            try
            {
                List<byte> encryptedData = new List<byte>(); ;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(publicKey);

                    int i = 0;
                    while (i < dataToEncrypt.Length)
                    {
                        var block = dataToEncrypt.Skip(i).Take(MaxRSABlockSize).ToArray();
                        i += MaxRSABlockSize;
                        encryptedData.AddRange(RSA.Encrypt(block, true));
                    }
                }
                return encryptedData.ToArray();
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static string RSADecrypt(byte[] dataToDecrypt, RSAParameters privateKey)
        {
            try
            {
                List<byte> decryptedData = new List<byte>();
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(privateKey);

                    int i = 0;
                    while (i < dataToDecrypt.Length)
                    {
                        var block = dataToDecrypt.Skip(i).Take(RSAOutputBlockSize).ToArray();
                        i += RSAOutputBlockSize;
                        decryptedData.AddRange(RSA.Decrypt(block, true));
                    }
                }

                return Encoding.UTF8.GetString(decryptedData.ToArray());
            }
            catch (CryptographicException e)
            {
                Logging.Log(LogLevel.Error, $"Error during RSA decryption.");
                return null;
            }
        }
    }

    public class AesParams
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public AesParams()
        {
        }

        public AesParams(Aes aes)
        {
            Key = aes.Key;
            IV = aes.IV;
        }

        public Aes ToAes()
        {
            var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            return aes;
        }
    }

    public class EncryptionSuite
    {
        public byte[] Encrypt(byte[] data)
        {
            return Encryptor.TransformFinalBlock(data, 0, data.Length);
        }
        public byte[] Decrypt(byte[] data)
        {
            return Decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        private ICryptoTransform Encryptor;
        private ICryptoTransform Decryptor;
        public AesParams AesParams { get; }

        public EncryptionSuite() : this(Aes.Create())
        {
        }

        public EncryptionSuite(Aes aes)
        {
            Encryptor = aes.CreateEncryptor();
            Decryptor = aes.CreateDecryptor();

            AesParams = new AesParams(aes);
        }

        public EncryptionSuite(AesParams aesParams)
        {
            AesParams = aesParams;
            
            var aes = aesParams.ToAes();
            Encryptor = aes.CreateEncryptor();
            Decryptor = aes.CreateDecryptor();
        }
    }
}
