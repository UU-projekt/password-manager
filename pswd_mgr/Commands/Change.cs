using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Change : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "change <client> <server> {<pwd>} {<new_pwd>}";
            string description = "Changes the master password for the vault located in <server>.\n"
                + "You will be prompted for both the current master password, <pwd>, and the new master password, <new_pwd>.\n"
                + "If the master password is incorrect and decryption fails, then the command is aborted and an error message is printed.\n";

            string options = "<client>     Path to client file.\n"
                + "<server>     Path to server file.\n"
                + "<pwd>        Your master password.\n"
                + "<password>   The new master password.\n";

            string examples = "Requesting to change the password for a vault:\n"
                + "$ change client.json server.json";


            return TextHelper.Format(synopsis, description, options, examples);
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var current_vault_psw = args.GetInteractive("current master password", true);
            var new_vault_psw = args.GetInteractive("new master password", true);

            var client_file = Manager.ReadClientFile(client);
            var server_file = Manager.ReadServerFile(server);
            var old_vault_key = Crypto.GenerateVaultKey(client_file.secret, current_vault_psw);
            var new_vault_key = Crypto.GenerateVaultKey(client_file.secret, new_vault_psw);

            var vault = EncryptedDict.From(server_file, old_vault_key);
            vault.SetSecretKey(new_vault_key);

            Manager.WriteServerFile(server, server_file.initialization_vector, vault);

            TextHelper.WriteInformation($"Changed password");
            return 0;
        }
    }
}
