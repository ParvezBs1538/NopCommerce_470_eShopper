using System;
using System.Security.Cryptography;
using System.Text;

namespace NopStation.Plugin.Payments.Nagad
{
    public static class NagadCryptoExtension
    {
        public static string RsaEncrypt(string serializedSensitiveData, string publicKey)
        {
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                var dataBytes = Encoding.UTF8.GetBytes(serializedSensitiveData);
                var encryptedData = rsa.Encrypt(dataBytes, false);

                string base64EncryptedData = Convert.ToBase64String(encryptedData);
                return base64EncryptedData;
            }
        }
        public static string RsaDecrypt(string encryptedData, string privateKey)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var privateKeyBytes = Convert.FromBase64String(privateKey);

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                var decryptedData = Encoding.UTF8.GetString(rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1));
                return decryptedData;
            }
        }
        public static string GenerateDigitalSignature(string serializedSensitiveData, string privateKey)
        {
            var privateKeyBytes = Convert.FromBase64String(privateKey);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

                var dataBytes = Encoding.UTF8.GetBytes(serializedSensitiveData);

                using (SHA256Managed sha256 = new SHA256Managed())
                {
                    var hashBytes = sha256.ComputeHash(dataBytes);

                    var signature = rsa.SignHash(hashBytes, CryptoConfig.MapNameToOID("SHA256"));
                    string base64Signature = Convert.ToBase64String(signature);
                    return base64Signature;
                }
            }
        }
    }
}
