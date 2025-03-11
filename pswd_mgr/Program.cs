using pswd_mgr;
using pswd_mgr.Commands;
using System.Security.Cryptography;


public class Program
{
    public static bool IsRunningInTestEnvironment()
    {
        return Environment.GetEnvironmentVariable("IS_TEST_ENVIRONMENT") == "true";
    }
    public static void Main(string[] args)
    {
        if(!IsRunningInTestEnvironment())
        {
            Console.Title = "Password Manager";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        var commands = new Dictionary<string, I_Command>();
        commands.Add("init", new Init());
        commands.Add("create", new Create());
        commands.Add("get", new Get());
        commands.Add("set", new Set());
        commands.Add("delete", new Delete());
        commands.Add("secret", new Secret());
        commands.Add("change", new Change());


        // om inten input gavs (listan är tom) eller första strängen i listen är tom så testar vi igen
        if (args.Length == 0 || String.IsNullOrEmpty(args[0])) return;
            // Namnet på kommandot vi ska köra är den första strängen i listan
            var command_name = args[0];


            // Ladda in kommandot från dicten commands (om kommandot finns)
            I_Command? command;
            commands.TryGetValue(command_name, out command);

            // Finns inte kommandot visar vi ett felmeddelande
            if (command == null)
            {
                Console.WriteLine($"Kommandot \"{command_name}\" finns inte");
            return;
            }

            try
            {
                var arguments = args.Skip(1).ToArray();
                var args_instance = new CommandArguments(arguments);

                if (args_instance.PerhapsGet(0) == "--help")
                {
                    TextHelper.PrettyPrint(command.GetCommandUsage());
                    return;
                }

                command.Run(args_instance);
            }
            catch (ArgumentException err)
            {
                Console.WriteLine(err);
                Console.WriteLine($"Inte säker på hur du använder kommandot? Kör {command_name} --help");
            } catch(CryptographicException err)
            {
                Console.WriteLine("nej nekj");
            }

        
    }
}