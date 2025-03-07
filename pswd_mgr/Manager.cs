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
    public class EncryptedDict {
        private byte[] secret_key;
        private byte[] initialization_vector;
        private Dictionary<string,string> dict = new();

        public EncryptedDict(byte[] secret_key, byte[] initialization_vector)
        {
            this.secret_key = secret_key;
            this.initialization_vector = initialization_vector;
        }

        public void FromEncrypted(byte[] encrypted_data)
        {
            byte[] decrypted_bytes = Crypto.Decrypt(encrypted_data, this.secret_key, this.initialization_vector);
            var serialized_dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted_bytes);

            if(serialized_dictionary == null)
            {
                throw new Exception("could not deserialize `encrypted_data` into a dictionary");
            }
            
            foreach(var entry in serialized_dictionary)
            {
                this.dict.Add(entry.Key, entry.Value);
            }
        }

        public byte[] IntoEncrypted()
        {
            var serialized = JsonSerializer.Serialize(this.dict);

            if(serialized == null)
            {
                throw new Exception("could not serialize dict");
            }

            return Crypto.Encrypt(Encoding.UTF8.GetBytes(serialized), this.secret_key, this.initialization_vector);
        }

        public void Add(string key, string value)
        {
            this.dict.Add(key, value);
        }
    }
    public struct ServerFile {
        [JsonInclude]
        public byte[] vault;
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
        private static bool IsJustFilename(string path)
        {
            return string.IsNullOrEmpty(Path.GetDirectoryName(path));
        }
        private static string NormalizedPath(string path)
        {
            if(!Path.HasExtension(path))
            {
                TextHelper.WriteWarning($"filen du angav slutade inte i \".json\". Vi har automatiskt ändrat din input till \"{path}.json\"");
                path += ".json";
            }

            if (!IsJustFilename(path)) return path;
            string user_appdata_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string pswmgr_path = Path.Combine(user_appdata_path, "PG26_CliPswMgr");

            if(!Directory.Exists(pswmgr_path))
            {
                var dir_info = Directory.CreateDirectory(pswmgr_path);

                var readme_file_path = Path.Join(pswmgr_path, "README.txt");
                string readme_file_text = $"Tjenare!\n\nOroa dig inte, trots mappens lite udda namn är detta INTE relaterat till ett virus eller något annat sattyg :)\nDenna mapp skapades {dir_info.CreationTime.ToShortDateString()} av CLI baserade lösenordshanteraren skapad av projektgrupp 26.";
                File.WriteAllText(readme_file_path, readme_file_text);
            }

            return Path.Join(pswmgr_path, path);
        }

        private static void WriteFileGeneric<T>(string path, T data)
        {
            string file_path = NormalizedPath(path);
            var json = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(file_path, json);
        }
        private static T ReadFileGeneric<T>(string path)
        {
            string file_path = NormalizedPath(path);

            var json = System.IO.File.ReadAllText(file_path);
            var deserialized = JsonSerializer.Deserialize<T>(json);

            if(deserialized == null)
            {
                throw new InvalidDataException($"tried deserializing \"{file_path}\" (existed) but netted a null value when {typeof(T).FullName} was expected");
            }

            return deserialized;
        }

        public static void WriteServerFile(string path, ServerFile file)
        {
            WriteFileGeneric(path, file);
        }
        public static void WriteClientFile(string path, ClientFile file)
        {
            WriteFileGeneric(path, file);
        }

        public static ServerFile ReadServerFile(string path)
        {
            return ReadFileGeneric<ServerFile>(path);
        }

        public static ClientFile ReadClientFile(string path)
        {
            return ReadFileGeneric<ClientFile>(path);
        }
    }

    public static class Crypto
    {
        public static int ITERATION_ROUNDS = 100_000;
        public static CipherMode CIPHER_MODE = CipherMode.CBC;


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
            var vault_key = Rfc2898DeriveBytes.Pbkdf2(as_bytes, secret, ITERATION_ROUNDS, HashAlgorithmName.SHA512, 32);
            return vault_key;
        }

        public static byte[] Encrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CIPHER_MODE;
            using var cryptoTransform = aes.CreateEncryptor();
            return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
        }

        public static byte[] Decrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CIPHER_MODE;
            using var cryptoTransform = aes.CreateDecryptor();
            return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
        }
    }

}
