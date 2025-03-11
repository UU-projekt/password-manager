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
    /// <summary>
    /// Klass för att hantera krypteranded / de-krypterandet av vårat vault
    /// </summary>
    public class EncryptedDict {
        private byte[] secret_key;
        private byte[] initialization_vector;
        private Dictionary<string,string> dict = new();

        public EncryptedDict(byte[] secret_key, byte[] initialization_vector)
        {
            this.secret_key = secret_key;
            this.initialization_vector = initialization_vector;
        }
        public EncryptedDict(byte[] secret_key, byte[] initialization_vector, byte[]? encrypted_vault)
        {
            this.secret_key = secret_key;
            this.initialization_vector = initialization_vector;

            if(encrypted_vault != null)
            {
                this.FromEncrypted(encrypted_vault);
            }
        }

        public void FromEncrypted(byte[] encrypted_data)
        {
            FromEncrypted(encrypted_data, null);
        }
        /// <summary>
        /// Läser in ett krypterat vault och lägger in det dekrypterat i denna klass
        /// </summary>
        /// <param name="encrypted_data">det krypterade valtet</param>
        /// <exception cref="Exception"></exception>
        public void FromEncrypted(byte[] encrypted_data, byte[]? new_secret_key)
        {
            byte[] decrypted_bytes = Crypto.Decrypt(encrypted_data, new_secret_key ?? this.secret_key, this.initialization_vector);
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

        /// <summary>
        /// gör om vaulted som hanteras av denna instans till en krypterad byte[]
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
            if(this.dict.ContainsKey(key))
            {
                this.dict.Remove(key);
            }

            this.dict.Add(key, value);
        }

        public void Remove(string key)
        {
            this.dict.Remove(key);
        }

        public string Get(string key)
        {
            return this.dict[key];
        }

        public string[] GetKeys()
        {
            return this.dict.Keys.ToArray();
        }

        public void SetSecretKey(byte[] secret_key)
        {
            this.secret_key = secret_key;
        }

        public static EncryptedDict From(ServerFile server, byte[] secret_key)
        {
            var instance = new EncryptedDict(secret_key, server.initialization_vector);
            instance.FromEncrypted(server.vault);
            return instance;
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

    /// <summary>
    /// Hanterar allt som har med filsystemet att göra
    /// </summary>
    public static class Manager
    {
        /// <summary>
        /// Kollar om en path enbart är en fil eller om den också innehåller en path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsJustFilename(string path)
        {
            return string.IsNullOrEmpty(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Gör om användarens path så att allt funkar som det ska. Fixar filnamnet om användaren glömmer ange .json eller använder en annan filändelse
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string NormalizedPath(string path)
        {
            if (Program.IsRunningInTestEnvironment()) return path;

            // check för att se att filen har en filändelse och om filändelsen är .json
            // om användaren har gjort fel så fixar vi det åt denna och visar en liten varning
            if(!Path.HasExtension(path) || !Path.HasExtension(".json"))
            {
                TextHelper.WriteWarning($"filen du angav slutade inte i \".json\". Vi har automatiskt ändrat din input till \"{path}.json\"");
                path = Path.ChangeExtension(path, ".json");
            }

            // Om det användaren skickar med inte bara är ett filnamn så returnerar vi helt enkelt det använderen skickade
            if (!IsJustFilename(path)) return path;

            // Om användaren bara anger ett filnamn så lagrar vi filen i användaren mapp för lokal applikations-data
            string user_appdata_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Vi lagrar filerna i vår egna mapp i användarens app-data med det kreativa namnet PG26_CliPswMgr som står för Projektgrupp 26 CLI Password Manager
            string pswmgr_path = Path.Combine(user_appdata_path, "PG26_CliPswMgr");

            // Vi kikar om våran mapp (<applikations data>/PG26_CliPswMgr) existerar 
            if(!Directory.Exists(pswmgr_path))
            {
                // om mappen inte existerar så skapar vi den
                var dir_info = Directory.CreateDirectory(pswmgr_path);

                // Förutom att skapa mappen så lägger vi in en s.k "README" fil i mappen som förklarar varför mappen skapades och av vem
                // tror inte det är betygsmässigt bra om våra handledare hittar en random mapp som ser lite sus ut utan vidare information
                var readme_file_path = Path.Join(pswmgr_path, "README.txt");
                string readme_file_text = $"Tjenare!\n\nOroa dig inte, trots mappens lite udda namn är detta INTE relaterat till ett virus eller något annat sattyg :)\nDenna mapp skapades {dir_info.CreationTime.ToShortDateString()} av CLI baserade lösenordshanteraren skapad av projektgrupp 26.";
                File.WriteAllText(readme_file_path, readme_file_text);
            }

            // Nu kan vi äntligen returnera den nya pathen till filen
            return Path.Join(pswmgr_path, path);
        }

        /// <summary>
        /// Generisk funktion (dvs funktion som fungerar med olika typer av data) som används för att skriva en fil
        /// </summary>
        /// <typeparam name="T">typen på datan du vill spara</typeparam>
        /// <param name="path">var du vill spara datan</param>
        /// <param name="data">den data du vill spara</param>
        private static void WriteFileGeneric<T>(string path, T data)
        {
            string file_path = NormalizedPath(path);
            var json = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(file_path, json);
        }

        /// <summary>
        /// Generisk funktion (dvs funktion som fungerar med olika typer av data) som används för att läsa en fil
        /// </summary>
        /// <typeparam name="T">datatypen på datan du förväntar dig från filen</typeparam>
        /// <param name="path">var du förväntar dig att hitta datan</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
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

        public static void WriteServerFile(string path, byte[] iv, EncryptedDict vault)
        {
            ServerFile server_file;
            server_file.initialization_vector = iv;
            server_file.vault = vault.IntoEncrypted();

            WriteFileGeneric(path, server_file);
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

    /// <summary>
    /// Helper-klass för allt som har med det kryptografiska att göra
    /// </summary>
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

        /// <summary>
        /// Bygger an vault key med secret och lösenord
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] GenerateVaultKey(byte[] secret, string password)
        {
            var as_bytes = Encoding.UTF8.GetBytes(password);
            var vault_key = Rfc2898DeriveBytes.Pbkdf2(as_bytes, secret, ITERATION_ROUNDS, HashAlgorithmName.SHA512, 32);
            return vault_key;
        }

        /// <summary>
        /// AES Krypterar data 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] buffer, byte[] vault_key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = vault_key;
            aes.IV = iv;
            aes.Mode = CIPHER_MODE;
            using var cryptoTransform = aes.CreateEncryptor();
            return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// AES de-krypterar data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] buffer, byte[] vault_key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = vault_key;
            aes.IV = iv;
            aes.Mode = CIPHER_MODE;
            using var cryptoTransform = aes.CreateDecryptor();
            return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
        }
    }

}
