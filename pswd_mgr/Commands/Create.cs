using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Create : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "create <client> <server> {<pwd>} {<secret>}";
            string description = "Create new client at <client> that serves as a log in for <server>.\n"
                + "You will be prompted for both master password and secret key.\n"
                + "If master password and secret key, together with the initialization vector stored in <server>, cannot be used to decrypt the vault in <server>, then the command is aborted and an error message is printed.\n"
                + "If a file exists at <client>, it will be overridden without prompting.\n";

            string options = "<client>     Path to client file.\n"
                + "<server>     Path to server file.\n"
                + "<pwd>        Your master password.\n"
                + "<secret>     Your secret key.\n";

            string examples = "Creating a client at \"client.json\" serving as a login for \"server.json\":\n"
                + "$ create client.json server.json";



            return TextHelper.Format(synopsis, description, options, examples);
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var pwd = args.GetInteractive("password", true);
            var secret = args.GetInteractive("secret", false);

            var decrypted_secret = Convert.FromBase64String(secret);

            var server_file = Manager.ReadServerFile(server);
            var vault_key = Crypto.GenerateVaultKey(decrypted_secret, pwd);
            EncryptedDict.From(server_file, vault_key);

            ClientFile new_client_file;
            new_client_file.secret = decrypted_secret;

            Manager.WriteClientFile(client, new_client_file);

            TextHelper.WriteInformation($"Ny client skapad!\n\ndin secret_key för denna är \"{decrypted_secret}\"\n\nSe till att lagra denna secret på ett säkert ställe. Skriv gärna ner det på en papperslapp och spara det på ett ställe där du inte slarvar bort det. Vi rekomenderar till exempel att du lämnar denna lapp där du har ditt pass.");
            return 0;
        }
    }
}
