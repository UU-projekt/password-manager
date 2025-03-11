using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Delete : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "delete <client> <server> <prop> {<pwd>}";
            string description = "Drops the property identified by <prop>.\n"
                + "You will be prompted for your master password.\n"
                + "If the master password is incorrect and decryption fails, then the command is aborted and an error message is printed.\n";

            string options = "<client>     Path to client file.\n"
                + "<server>     Path to server file.\n"
                + "<prop>       Key used to identify the property you wish to access.\n"
                + "<pwd>        Your master password.\n";

            string examples = "Requesting to delete the property identified by \"username.example.com\":\n"
                + "$ delete client.json server.json username.example.com";

            return TextHelper.Format(synopsis, description, options, examples);
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var prop = args.Get(2, "prop");

            var pwd = args.GetInteractive("master password", true);

            var server_file = Manager.ReadServerFile(server);
            var client_file = Manager.ReadClientFile(client);
            var vault_key = Crypto.GenerateVaultKey(client_file.secret, pwd);
            var vault = EncryptedDict.From(server_file, vault_key);

            vault.Remove(prop);

            Manager.WriteServerFile(server, server_file.initialization_vector, vault);

            TextHelper.WriteInformation($"tog bort {prop}");
            return 0;
        }
    }
}
