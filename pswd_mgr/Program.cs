using pswd_mgr;
using pswd_mgr.Commands;

Console.Title = "Password Manager";
Console.OutputEncoding = System.Text.Encoding.UTF8;

bool running = true;

var commands = new Dictionary<string, I_Command>();
commands.Add("init", new Init());

while(running)
{
    var input = Console.ReadLine();
    if (input == null) continue;

    // Dela upp inputted till en lista för att enklare tolka den
    var cmd_args = input.Split(" ");

    // om inten input gavs (listan är tom) eller första strängen i listen är tom så testar vi igen
    if (cmd_args.Length == 0 || String.IsNullOrEmpty(cmd_args[0])) continue;
    // Namnet på kommandot vi ska köra är den första strängen i listan
    var command_name = cmd_args[0];

    // Om kommandots namn börjar med 'q' antar vi att användaren vill avsluta programmet 
    if (command_name[0] == 'q') break;
    
    // Ladda in kommandot från dicten commands (om kommandot finns)
    I_Command? command;
    commands.TryGetValue(command_name, out command);

    // Finns inte kommandot visar vi ett felmeddelande
    if(command == null)
    {
        Console.WriteLine($"Kommandot \"{command_name}\" finns inte");
        continue;
    }

    try
    {
        var arguments = cmd_args.Skip(1).ToArray();
        var args_instance = new CommandArguments(arguments);

        if(args_instance.PerhapsGet(0) == "--help")
        {
            TextHelper.PrettyPrint(command.GetCommandUsage());
            continue;
        }

        command.Run(args_instance);
    } catch(ArgumentException err)
    {
        Console.WriteLine(err);
        Console.WriteLine($"Inte säker på hur du använder kommandot? Kör {command_name} --help");
    }
}