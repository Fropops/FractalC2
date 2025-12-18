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
        public static string Execution { get; set; } = "Agent - Execution";
        public static string LateralMovement { get; set; } = "Agent - Lateral Movement";
        public static string Media { get; set; } = "Agent - Media";
        public static string Network { get; set; } = "Agent - Network";
        public static string System { get; set; } = "Agent - System";
        public static string Token { get; set; } = "Agent - Token";
        public static string Commander { get; set; } = "Commander";

        public static List<string> All { get; }  = new List<string> { Agent, Execution, LateralMovement, Media, Network, System, Token };
    }
}
