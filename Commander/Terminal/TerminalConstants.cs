using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Terminal
{
    static class TerminalConstants
    {
        public static ConsoleColor DefaultBackGroundColor = Console.BackgroundColor;
        public static ConsoleColor DefaultForeGroundColor = ConsoleColor.White;

        public const ConsoleColor PromptColor = ConsoleColor.Yellow;
        public const ConsoleColor SuccessColor = ConsoleColor.Green;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;
        public const ConsoleColor InfoColor = ConsoleColor.Cyan;

        
    }
}
