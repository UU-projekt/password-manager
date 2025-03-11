using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Init : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "init <client> <server> {<pwd>}";
            string description = "Create new client at <client>, server at <server>, and encrypt your vault stored in <server> using <pwd>.\n"
            + "Your generated secret key will be stored in <client> while the generated initialization vector will be stored in the <server>.\n"
            + "To access the vault stored in <server> you must use the <client> or use the command ’create’ to create another client.\n"
            + "When using <client> you can get, set, and delete data in / from your vault without having to manually provide your secret key on every interaction.\n"
            + "You must however provide your master password."
            + "If files exist at <client> or <server> they will be overridden without prompting.\n\n"
            + "Your secret key will be printed in plain - text to standard out. Remember to write down BOTH your master password AND secret key and tore them safely.\n\n"
            + "You might e.g., want to print them and store them next to your passport.\n"
            + "If you lose your master password and / or your secret key then your data will be unrecoverable.\n";


            string options = "<client>     Path to client file.\n"
            + "<server>     Path to server file.\n"
            + "<pwd>        Your master password.\n";

            string examples = "Initializing a new client and server at \"client.json\" and \"server.json\"\n"
            + "  $ init client.json server.json";


            return TextHelper.Format(synopsis, description, options, examples);
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var pwd = args.GetInteractive("password", true);

            byte[] secret = Crypto.GenerateSecretKey();
            byte[] IV = Crypto.CreateInitializationVector();

            var vault_key = Crypto.GenerateVaultKey(secret, pwd);
            var vault = new EncryptedDict(vault_key, IV);
            ClientFile client_file = new ClientFile { secret = secret };


            Manager.WriteServerFile(server, IV, vault);
            Manager.WriteClientFile(client, client_file);

            TextHelper.WriteInformation($"Nytt Vault skapat!\n\ndin secret_key är \"{Convert.ToBase64String(secret)}\"\n\nSe till att lagra denna secret på ett säkert ställe. Skriv gärna ner det på en papperslapp och spara det på ett ställe där du inte slarvar bort det. Vi rekomenderar till exempel att du lämnar denna lapp där du har ditt pass.");
            return 0;
        }
    }
}
