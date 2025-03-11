using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Set : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "set <client> <server> <prop> [-g] {<pwd>} {<password>}";
            string description = "Sets the password stored under <prop> in the vault in <server>.\n"
                + "If a password already exists, then it is overridden without prompting the user for confirmation.\n"
                + "If <prop> is not provided, then the operation is aborted and an error message is printed.\n"
                + "You will be prompted for both master password and then the password that you want to store.\n"
                + "When supplying the flag -g or --generate, a random password will be generated and you will not be prompted.\n"
                + "If the master password is incorrect and decryption fails, then the command is aborted and an error message is printed.\n";

            string options = "<client>     Path to client file.\n"
                + "<server>     Path to server file.\n"
                + "<prop>       Key used to identify the property you wish to access.\n"
                + "<pwd>        Your master password.\n"
                + "<password>   The password you want to store.\n"
                + "-g, --generate The password is set to a random alphanumeric string with 20 characters that matches the regex /[a-zA-Z0-9]{20}/. The password is also printed to standard out.\n";

            string examples = "Requesting to store some password under the property \"username.example.com\":\n"
                + "$ set client.json server.json username.example.com\n\n"
                + "Requesting to store some generated password under the property \"password.example.com\":\n"
                + "$ set client.json server.json password.example.com --generate";




            return TextHelper.Format(synopsis, description, options, examples);
        }

        public static string GeneratePassword()
        {
            const int length = 20;
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder password = new StringBuilder(length);

            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var prop = args.Get(2, "prop");
            var generate_flag = args.PerhapsGet(3);

            var pwd = args.GetInteractive("master password", true);

            var server_file = Manager.ReadServerFile(server);
            var client_file = Manager.ReadClientFile(client);
            var vault_key = Crypto.GenerateVaultKey(client_file.secret, pwd);
            var vault = EncryptedDict.From(server_file, vault_key);

            string set_psw = "";

            if(generate_flag != null && (generate_flag == "-g" || generate_flag == "--generate"))
            {
                set_psw = GeneratePassword();
                Console.WriteLine(set_psw);
            } else
            {
                set_psw = args.GetInteractive("password", true);
            }

            vault.Add(prop, set_psw);

            Manager.WriteServerFile(server, server_file.initialization_vector, vault);

            TextHelper.WriteInformation($"lagrad {(generate_flag != null ? $"ditt genererade lösenord: {set_psw}" : "")}");
            return 0;
        }
    }
}
