using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Get : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "get <client> <server> [<prop>] {<pwd>}";
            string description = "Prints the password stored under <prop> from the vault (located in <server>) in plain-text, if a <prop> is provided.\n"
                + "If <prop> does not exist, then nothing is printed.\n"
                + "If <prop> is not provided, then the command lists all stored properties (but not their passwords) from the vault in plain-text.\n"
                + "You will be prompted for your master password.\n"
                + "If the master password is incorrect, or the secret key in <client> is incorrect for the <server> and decryption hence fails, then the command is aborted and an error message is printed.\n";

            string options = "<client>     Path to client file.\n"
                + "<server>     Path to server file.\n"
                + "<prop>       Key used to identify the property you wish to access.\n"
                + "<pwd>        Your master password.\n";

            string examples = "Retrieving list of all properties:\n"
                + "$ get client.json server.json\n\n"
                + "Retrieving the password stored under the property 'password.google.com':\n"
                + "$ get client.json server.json password.example.com\n\n"
                + "Retrieving the password stored under the property 'username.google.com':\n"
                + "$ get client.json server.json username.example.com";




            return TextHelper.Format(synopsis, description, options, examples);
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var prop = args.PerhapsGet(2);
            var psw = args.GetInteractive("password", secret: true);

            var client_file = Manager.ReadClientFile(client);
            var server_file = Manager.ReadServerFile(server);
            var vault_key = Crypto.GenerateVaultKey(client_file.secret, psw);

            var vault = EncryptedDict.From(server_file, vault_key);

            if(prop == null)
            {
                string out_str = "";
                foreach(var entry in vault.GetKeys())
                {
                    out_str += $"{entry}, ";
                }
                Console.WriteLine(out_str);
            } else
            {
                try
                {
                    var psw_as_string = vault.Get(prop);
                    Console.WriteLine();
                    Console.WriteLine(psw_as_string);
                } catch(KeyNotFoundException err) {
                    TextHelper.WriteWarning($"Prop {prop} fanns inte i vaultet");
                }
            }


                //TextHelper.WriteInformation($"Ny client skapad!\n\ndin secret_key för denna är \"{decrypted_secret}\"\n\nSe till att lagra denna secret på ett säkert ställe. Skriv gärna ner det på en papperslapp och spara det på ett ställe där du inte slarvar bort det. Vi rekomenderar till exempel att du lämnar denna lapp där du har ditt pass.");
            return 0;
        }
    }
}
