using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{
    public static class CommandCategory
    {
        public static string Agent { get; set; } = "Agent";
        public static string Execution { get; set; } = "Execution";
        public static string LateralMovement { get; set; } = "Lateral Movement";
        public static string Media { get; set; } = "Media";
        public static string Network { get; set; } = "Network";
        public static string System { get; set; } = "System";
        public static string Token { get; set; } = "Token";
        public static string Commander { get; set; } = "Commander";

        public static string Navigation { get; set; } = "Navigation";

        public static string Services { get; set; } = "Services";

        public static List<string> All { get; }  = new List<string> { Agent, Execution, LateralMovement, Media, Network, System, Token, Services, Navigation };
    }
}
