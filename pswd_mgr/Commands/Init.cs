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
            return "SYNOPSIS\r\ninit < client > < server > { < pwd >}\r\nDESCRIPTION\r\nCreate new client at < client > , server at < server > , and encrypt your\r\nvault stored in < server > using <pwd >.\r\nYour generated secret key will be stored in < client > while the\r\ngenerated initialization vector will be stored in the < server >.\r\nTo access the vault stored in < server > you must use the < client > or\r\nuse the command ’ create ’ to create another client .\r\nWhen using < client > you can get , set , and delete data in / from your\r\nvault without having to manually provide your secret key on every\r\ninteraction . You must however provide your master password .\r\nIf files exist at < client > or < server > they will be overridden\r\nwithout prompting .\r\nYour secret key will be printed in plain - text to standard out .\r\nRemember to write down BOTH your master password AND secret key and\r\nstore them safely . You might e . g . , want to print them and store them\r\nnext to your passport . If you lose your master password and / or your\r\nsecret key then your data will be unrecoverable .\r\nOPTIONS\r\n< client > Path to client file .\r\n< server > Path to server file .\r\n<pwd > Your master password .\r\nEXAMPLES\r\nInitializing a new client and server at \" client . json \" and\r\n\" server . json \":\r\n$ init client . json server . json";
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var server = args.Get(1, "server");
            var pwd = args.GetInteractive("password", true);

            byte[] secret = Crypto.GenerateSecretKey();
            byte[] IV = Crypto.CreateInitializationVector();

            var vault_key = Crypto.GenerateVaultKey(secret, pwd);

            ServerFile server_file = new ServerFile { initialization_vector = IV, vault = new() };
            ClientFile client_file = new ClientFile { secret = secret };


            Manager.WriteServerFile(server, server_file);
            Manager.WriteClientFile(client, client_file);

            return 0;
        }
    }
}
