using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace businessrules_fieldgateway_simulator_console.Helpers
{
    internal static class ConsoleHelper
    {
        internal static string UserInputPrompt(string prompt)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(prompt);
            var input = Console.ReadLine();
            Console.ResetColor();
            return input;
        }

        internal static bool UserYInputPrompt(string prompt)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(prompt);
            var key = Console.ReadLine();
            Console.ResetColor();
            return key == "Y";
        }

        internal static void WriteTitle(string title)
        {
            Console.WriteLine(title);
            Console.WriteLine("".PadRight(title.Length, '='));
        }


        internal static string UserChoicePrompt(string prompt, string[] choices)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            Console.WriteLine(prompt);
            Console.WriteLine("Choose From: [{0}]", string.Join(", ", choices));
            Console.ResetColor();

            var entry = Console.ReadLine();
            if (choices.Contains(entry))
            {
                return entry;
            }
            else
            {
                return UserChoicePrompt(prompt, choices);
            }

        }

        internal static void WriteSuccess(string successMessage)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(successMessage);
            Console.ResetColor();
        }
    }
}
