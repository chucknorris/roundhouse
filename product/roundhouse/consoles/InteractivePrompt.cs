using System;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.logging;

namespace roundhouse.consoles
{
    public class InteractivePrompt
    {
        public static bool in_interactive_mode(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (Environment.UserInteractive && !configuration_property_holder.Silent) return true;
            return false;
        }

        public static void write_header(ConfigurationPropertyHolder configuration_property_holder)
        {
            Log.bound_to(typeof(InteractivePrompt)).log_an_info_event_containing("RoundhousE Interactive Setup (only prompts if not in silent mode)");
            Log.bound_to(typeof(InteractivePrompt)).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
        }

        public static string get_user(string default_user_name, ConfigurationPropertyHolder configuration_property_holder)
        {
            var user_name = string.Empty;
            if (in_interactive_mode(configuration_property_holder))
            {
                Console.WriteLine("Please enter a user name and press enter (leave empty for '{0}')", default_user_name);
                user_name = Console.ReadLine();
            }
            user_name = !string.IsNullOrEmpty(user_name) ? user_name : default_user_name;
            Log.bound_to(typeof(InteractivePrompt)).log_an_info_event_containing("Using '{0}' for the user name.", user_name);

            return user_name;
        }

        public static string get_password(string default_password, ConfigurationPropertyHolder configuration_property_holder)
        {
            string password = string.Empty;

            if (in_interactive_mode(configuration_property_holder))
            {
                Console.WriteLine("Please enter a password and press enter (leave empty for '{0}')", default_password);

                //http://www.c-sharpcorner.com/Forums/Thread/32102/
                ConsoleKeyInfo info = Console.ReadKey(true);
                while (info.Key != ConsoleKey.Enter)
                {
                    if (info.Key != ConsoleKey.Backspace)
                    {
                        Console.Write("*");
                        password += info.KeyChar;
                        info = Console.ReadKey(true);
                    }
                    else if (info.Key == ConsoleKey.Backspace)
                    {
                        if (!string.IsNullOrEmpty(password))
                        {
                            password = password.Substring(0, password.Length - 1);
                            // get the location of the cursor
                            int pos = Console.CursorLeft;
                            // move the cursor to the left by one character
                            Console.SetCursorPosition(pos - 1, Console.CursorTop);
                            // replace it with space
                            Console.Write(" ");
                            // move the cursor to the left by one character again
                            Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        }
                        info = Console.ReadKey(true);
                    }
                }
                for (int i = 0; i < password.Length; i++) Console.Write("*");
            }
            Console.WriteLine();
            password = !string.IsNullOrEmpty(password) ? password : default_password;
            Log.bound_to(typeof(InteractivePrompt)).log_an_info_event_containing("Using '*******' for the password. When YOU type hunter2, it shows to us as '*******'. ;-)");

            return password;
        }

        public static void write_footer()
        {
            Log.bound_to(typeof(InteractivePrompt)).log_an_info_event_containing("Completed. Thank you!");
            Log.bound_to(typeof(InteractivePrompt)).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
        }
    }
}