using pswd_mgr;
using pswd_mgr.Commands;

Console.Title = "Password Manager";
Console.OutputEncoding = System.Text.Encoding.UTF8;

bool running = true;

TextHelper.LinedMessage("Lorem ipsum odor amet, consectetuer adipiscing elit. Nec torquent facilisi ex egestas risus nullam; ridiculus senectus sollicitudin. Justo donec vehicula elit nec posuere nascetur integer purus. Molestie a phasellus commodo congue tristique quam; nam litora rutrum. Luctus in enim tempus nisl ullamcorper malesuada tincidunt. Pulvinar sapien diam elementum auctor torquent augue. Cubilia massa eleifend condimentum aenean leo neque congue dapibus. Donec quam rutrum vivamus vitae molestie sagittis erat montes.\r\n\r\nConubia lobortis proin nisl aliquet ad sociosqu praesent platea. Habitant commodo eleifend ridiculus ut laoreet vivamus quis. Sagittis diam at mus class dui risus. Sed ex leo sapien sociosqu amet nec consequat. Egestas aptent montes odio fringilla suspendisse. Suspendisse finibus odio posuere phasellus odio. Nunc mi mi nulla platea metus; habitasse posuere.\r\n\r\nMattis etiam id sapien turpis cubilia. Varius mollis convallis vehicula vehicula, adipiscing ultricies tortor. Varius pharetra fringilla fringilla semper senectus habitant ullamcorper ornare orci. Ullamcorper sollicitudin tellus; aliquam netus eros mauris curabitur eleifend. Est cursus sit lacus vitae aliquam himenaeos class et. Metus mus egestas nisi, leo leo non molestie id. Class etiam ad habitant tincidunt congue. Avulputate malesuada pulvinar ipsum sagittis.\r\n\r\nVelit tristique faucibus consectetur lacinia cubilia neque nunc. Fermentum nec lectus at posuere laoreet in. Amet habitasse malesuada ullamcorper bibendum primis inceptos. Habitant phasellus nam himenaeos tortor per amet pretium. Pulvinar risus est ligula habitasse quam nec commodo convallis enim. Tortor maximus sit est luctus sem eros justo odio. Maximus posuere sem bibendum eros nunc cursus auctor lobortis. Velit sollicitudin in in porta magna tempus.\r\n\r\nPraesent libero eget quis pretium magna a rhoncus sapien. Sollicitudin netus placerat luctus libero per torquent odio. Eu nibh feugiat nam nullam habitant mattis diam pulvinar. Imperdiet ornare dolor habitasse quisque imperdiet fusce nunc. Taciti nascetur facilisis luctus mauris, quam mauris. Velit venenatis conubia pulvinar nostra, augue senectus. Tortor tortor velit orci posuere amet; libero sociosqu aliquet.");

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