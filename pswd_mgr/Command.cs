using System;

namespace pswd_mgr
{
    interface I_Command
    {
        string GetCommandUsage();
        int Run(CommandArguments args);
    }

    /// <summary>
    /// <b>extremt</b> over-engineered kod bara för att det ska se lite snyggare ut 
    /// </summary>
    class TextHelper
    {
        static ConsoleColor mandatory_option = ConsoleColor.Red;
        static ConsoleColor interactive_option = ConsoleColor.DarkBlue;
        static ConsoleColor text_warning = ConsoleColor.Yellow;

        /// <summary>
        /// Funktion som tar en sträng vid slutet av "linen" och delar upp den i delar som inte överstiger längden max_len.
        /// Skulle en sträng vara för lång (dvs längre än max_len) kommer denna sträng delas upp i olika entrys i listan
        /// </summary>
        /// <example>
        /// <code>
        /// var split = ContentAwareSplit("hej\ntja\ntjaa\nhej", 3);
        /// </code>
        /// </example>
        /// <param name="str">den sträng du vill dela upp</param>
        /// <param name="max_len">den maximala längden en sträng för vara</param>
        /// <returns>strängen delad vid slutet av varje rad, där varje entry är säkerställt att vara under max_len i längd</returns>
        private static string[] ContentAwareSplit(string str, int max_len)
        {
            string[] lines = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> out_lines = new();

            foreach(var line in lines)
            {
                if(line.Length > max_len)
                {
                    int times_over = line.Length / max_len;

                    for(int i = 0; i <= times_over; i++)
                    {
                        int chunk_start = max_len * i;
                        int chars_left_until_end = line.Length - chunk_start;
                        int take_amount = int.Min(max_len, chars_left_until_end);
                        //Console.WriteLine($"Len: {line.Length}\nchunk_start: {chunk_start}\ntake_amount: {take_amount}\n");

                        out_lines.Add(line.Substring(chunk_start, take_amount).TrimStart());
                    }
                } else
                {
                    out_lines.Add(line);
                }
            }

            return out_lines.ToArray();
        }

        private static IEnumerable<string> DefaultIdentation(string padding)
        {
            while(true)
            {
                yield return padding;
            }
        }
        private static string IndentString(string str)
        {
            string default_indent = "       ";
            return IndentString(str, DefaultIdentation(default_indent), default_indent.Length);
        }
        /// <summary>
        /// Tar en sträng och "indentar" den. Dvs flyttar den åt höger och lämnar space åt vänster. 
        /// Denna funktion har support för situationer där strängen är för lång och "loopar" runt till en ny rad
        /// </summary>
        /// <param name="str">strängen vi vill indenta</param>
        /// <param name="indentation">en <see cref="IEnumerable{string}">enumerator</see> som ger oss det vi använder för att indenta med</param>
        /// <param name="indentation_length">längden på vår ident (hur mycket åt höger vi vill flytta texten)</param>
        /// <returns></returns>
        private static string IndentString(string str, IEnumerable<string> indentation, int indentation_length)
        {
            if (indentation == null) indentation = DefaultIdentation("      ");
            int max_width = Console.WindowWidth - indentation_length;
            string[] lines = ContentAwareSplit(str, max_width);
            string indented = "";

            var enumerator = indentation.GetEnumerator();
            enumerator.MoveNext();

            foreach(var line in lines)
            {
                indented += $"{enumerator.Current}{line}\n";
                enumerator.MoveNext();
            }
            return indented;
        }

        /// <summary>
        /// Lite finare Console.WriteLine()
        /// </summary>
        /// <param name="str">texten vi vill snygga till</param>
        public static void PrettyPrint(string str)
        {
            Console.Clear();
            ConsoleColor default_fg = Console.ForegroundColor;
            ConsoleColor default_bg = Console.BackgroundColor;

            bool reset_foreground_next = false;
            bool reset_background_next = false;

            foreach (char c in str)
            {
                if(reset_foreground_next)
                {
                    Console.ForegroundColor = default_fg;
                    reset_foreground_next = false;
                }

                if (reset_background_next)
                {
                    Console.BackgroundColor = default_bg;
                    reset_background_next = false;
                }

                switch (c)
                {
                    case '{':
                        Console.BackgroundColor = interactive_option;
                        break;
                    case '}':
                        reset_background_next = true;
                        break;
                    case '<':
                        Console.ForegroundColor = mandatory_option;
                        break;
                    case '>':
                        reset_foreground_next = true;
                        break;
                }
                Console.Write(c);
            }
            Console.WriteLine();
        }

        public static string Format(string synopsis, string description, string options, string examples)
        {
            return $"SYNOPSIS\n{IndentString(synopsis)}\nDESCRIPTION\n{IndentString(description)}\nOPTIONS\n{IndentString(options)}\nEXAMPLES\n{IndentString(examples)}";
        }

        private static IEnumerable<string> StatusMessageIndent(string first_time)
        {
            yield return first_time;
            string other_times = new string(' ', first_time.Length);
            while (true)
            {
                yield return other_times;
            }
        }

        /// <summary>
        /// Enumerator som get oss padding/indent med nummrerade rader
        /// </summary>
        /// <param name="max_width">maximala bredden vi kan tillåta minus ett. Tex så skulle 5 låta oss ha siffran 9999 som mest</param>
        /// <returns></returns>
        private static IEnumerable<string> LinedMessageIndent(int max_width)
        {
            for(int i = 0; i < 200_000; i++)
            {
                yield return i.ToString().PadRight(max_width - 1) + " ";
            }
        }

        /// <summary>
        /// Skriver ut ett meddelande med nummrerade rader
        /// </summary>
        /// <param name="message">det vi vill skriva ut</param>
        public static void LinedMessage(string message)
        {
            int max_lines_len = 4;

            Console.WriteLine(IndentString(message, LinedMessageIndent(max_lines_len), max_lines_len));
        }

        /// <summary>
        /// Skriver ut ett varningsmedellande i gul text med en liten varningssymbol till vänster
        /// </summary>
        /// <param name="message"></param>
        public static void WriteWarning(string message)
        {
            var decoration = "⚠️ ";
            Console.ForegroundColor = text_warning;
            Console.WriteLine(IndentString(message, StatusMessageIndent(decoration), decoration.Length));
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Helper-klass för att göra det lite enklare att läsa av argumenten som skickas till våra kommandon
    /// </summary>
    class CommandArguments
    {
        private string[] arg_arr;
        public CommandArguments(string[]? arg_list)
        {
            if (arg_list == null) throw new ArgumentException("Inga argument skickades");
            this.arg_arr = arg_list;
        }

        /// <summary>
        /// Låter oss hämta ett argument på en viss plats. Till skillnad från vanliga <see cref="Get(int)">Get</see> så kommer denna metod inte generera ett fel om inget argument fanns
        /// </summary>
        /// <param name="index">platsen för argumentet</param>
        /// <returns></returns>
        public string? PerhapsGet(int index) {
            if (index >= arg_arr.Length) return null;
            return arg_arr[index];
        }

        /// <summary>
        /// Hämtar argumentet på platsen "index". Om inget argument med det <paramref name="index" /> vi vill ha finns så genereras ett fel
        /// </summary>
        /// <param name="index">platsen för argumentet</param>
        /// <param name="argument_name">vad vi kallar argumentet. Används för att generera felmeddelande om inget argument hittades</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string Get(int index, string? argument_name)
        {
            var argument_value = this.PerhapsGet(index);
            if (String.IsNullOrEmpty(argument_value)) throw new ArgumentException($"Försökte nå argument på position {index} trots att enbart {arg_arr.Length} argument finns", argument_name);
            return argument_value;
        }

        /// <summary>
        /// Hämtar argumentet på platsen "index". Om inget argument med det <paramref name="index" /> vi vill ha finns så genereras ett fel
        /// </summary>
        /// <param name="index">platsen för argumentet</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string Get(int index)
        {
            return this.Get(index, null);
        }

        /// <summary>
        /// Ber använderen interaktivt om ett input direkt i konsolen
        /// </summary>
        /// <param name="name">det du vill att använderen besvarar</param>
        /// <returns></returns>
        public string GetInteractive(string name)
        {
            return GetInteractive(name, false);
        }

        /// <summary>
        /// Ber använderen interaktivt om ett input direkt i konsolen. Om <paramref name="secret"/> är true så kommer enbart den sista bokstaven i användarens input visas av säkerhetsskäl (tex vid input av lösenord)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string GetInteractive(string name, bool secret)
        {
            string? answer;
            if(secret)
            {
                answer = InternalGetInteractiveSecret(name);
            } else
            {
                Console.Write($"{name}: ");
                answer = Console.ReadLine();
            }

            return String.IsNullOrWhiteSpace(answer) ? GetInteractive(name, secret) : answer;
        }

        /// <summary>
        /// Skapar en sträng där bara ett visst antal (<paramref name="show_chars"/>) bokstäver visas i slutet
        /// </summary>
        /// <param name="value"></param>
        /// <param name="show_chars"></param>
        /// <returns></returns>
        private static string CreateObscuredString(string value, int show_chars)
        {
            string? obscured_chars = String.IsNullOrEmpty(value) ? null : new string('*', value.Length - show_chars);
            string? visible_chars = String.IsNullOrEmpty(value) ? null : value.Substring(value.Length - show_chars);
            return $"{obscured_chars}{visible_chars}";
        }

        /// <summary>
        /// Frågar användaren interaktivt om input och visar bara den sista bokstaven i användarens input av säkerhetsskäl
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string InternalGetInteractiveSecret(string name)
        {
            string buffer = "";
            Console.Write($"{name}: ");
            var cursor_pos = Console.CursorTop;
            bool take_inputs = true;
            while(take_inputs)
            {
                string obscured = CreateObscuredString(buffer, 1);

                Console.SetCursorPosition(0, cursor_pos);
                Console.Write($"{name}: {obscured} ");
                Console.SetCursorPosition(Console.CursorLeft - 1, cursor_pos);

                var key = Console.ReadKey();

                if(key.Key == ConsoleKey.Enter)
                {
                    string fully_obscured = CreateObscuredString(buffer, 0);

                    Console.SetCursorPosition(0, cursor_pos);
                    Console.Write($"{name}: {fully_obscured} ");

                    Console.WriteLine();
                    return buffer;
                } else if(key.Key == ConsoleKey.Backspace)
                {
                    buffer = buffer.Substring(0, int.Max(buffer.Length - 1, 0));
                } else if(!char.IsControl(key.KeyChar))
                {
                    buffer += key.KeyChar;
                }
            }

            return buffer;
        }
    }
}
