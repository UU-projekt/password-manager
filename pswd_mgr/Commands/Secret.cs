using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pswd_mgr.Commands
{
    class Secret : I_Command
    {
        public string GetCommandUsage()
        {
            string synopsis = "secret <client>";
            string description = "Prints secret key stored in <client> to standard out in plain-text.\n";

            string options = "<client>     Path to client file.\n";

            string examples = "Print secret key stored in \"client.json\":\n"
                + "$ secret client.json";


            return TextHelper.Format(synopsis, description, options, examples);
        }

        public int Run(CommandArguments args)
        {
            var client = args.Get(0, "client");
            var client_file = Manager.ReadClientFile(client);

            TextHelper.WriteInformation($"secret: {Convert.ToBase64String(client_file.secret)}");
            return 0;
        }
    }
}
