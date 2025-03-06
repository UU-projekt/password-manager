using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace pswd_mgr
{
    public struct ServerFile {
        [JsonInclude]
        public Dictionary<string, string> vault;
        [JsonInclude]
        public byte[] initialization_vector;
    }

    public struct ClientFile
    {
        [JsonInclude]
        public byte[] secret;
    };

    public static class Manager
    {
        public static void WriteServerFile(string path, ServerFile file)
        {
            var json = JsonSerializer.Serialize(file);
            System.IO.File.WriteAllText(path, json);
        }
        public static void WriteClientFile(string path, ClientFile file)
        {
            var json = JsonSerializer.Serialize(file);
            System.IO.File.WriteAllText(path, json);
        }

        public static ServerFile ReadServerFile(string path)
        {
            var json = System.IO.File.ReadAllText(path);
            return JsonSerializer.Deserialize<ServerFile>(json);
        }

        public static ClientFile ReadClientFile(string path)
        {
            var json = System.IO.File.ReadAllText(path);
            return JsonSerializer.Deserialize<ClientFile>(json);
        }
    }

    public static class Crypto
    {
        public static byte[] CreateInitializationVector()
        {
            var aes_instance = Aes.Create();
            aes_instance.GenerateIV();
            return aes_instance.IV;
        }

        public static byte[] GenerateSecretKey()
        {
            return RandomNumberGenerator.GetBytes(20);
        }

        public static byte[] GenerateVaultKey(byte[] secret, string password)
        {
            var as_bytes = Encoding.UTF8.GetBytes(password);
            var vault_key = Rfc2898DeriveBytes.Pbkdf2(as_bytes, secret, 3, HashAlgorithmName.SHA512, 32);
            return vault_key;
        }

        public static byte[] Encrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            var mode = CipherMode.CBC;
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = key;
            if (!(iv is null))
                aes.IV = iv;
            aes.Mode = mode;
            using var cryptoTransform = aes.CreateEncryptor();
            return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
        }

        public static byte[] Decrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            var mode = CipherMode.CBC;
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = key;
            if (!(iv is null))
                aes.IV = iv;
            aes.Mode = mode;
            using var cryptoTransform = aes.CreateDecryptor();
            return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
        }
    }

}
