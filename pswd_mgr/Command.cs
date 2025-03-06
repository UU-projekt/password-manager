using System;

namespace pswd_mgr
{
    interface I_Command
    {
        string GetCommandUsage();
        int Run(CommandArguments args);
    }

    class CommandArguments
    {
        private string[] arg_arr;
        public CommandArguments(string[]? arg_list)
        {
            if (arg_list == null) throw new ArgumentException("Inga argument skickades");
            this.arg_arr = arg_list;
        }

        public string? PerhapsGet(int index) {
            if (index >= arg_arr.Length) return null;
            return arg_arr[index];
        }

        public string Get(int index, string? argument_name)
        {
            var argument_value = this.PerhapsGet(index);
            if (String.IsNullOrEmpty(argument_value)) throw new ArgumentException($"Försökte nå argument på position {index} trots att enbart {arg_arr.Length} argument finns", argument_name);
            return argument_value;
        }

        public string Get(int index)
        {
            return this.Get(index, null);
        }

        public string GetInteractive(string name)
        {
            return GetInteractive(name, false);
        }
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

        private string CreateObscuredString(string value, int show_chars)
        {
            string? obscured_chars = String.IsNullOrEmpty(value) ? null : new string('*', value.Length - show_chars);
            string? visible_chars = String.IsNullOrEmpty(value) ? null : value.Substring(value.Length - show_chars);
            return $"{obscured_chars}{visible_chars}";
        }

        private string InternalGetInteractiveSecret(string name)
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

                    Console.SetCursorPosition(0, cursor_pos + 1);
                    return buffer;
                } else if(key.Key == ConsoleKey.Backspace)
                {
                    buffer = buffer.Substring(0, int.Max(buffer.Length - 1, 0));
                } else
                {
                    buffer += key.KeyChar;
                }
            }

            return buffer;
        }
    }
}
