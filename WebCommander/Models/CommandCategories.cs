using System.Collections.Generic;

namespace WebCommander.Models
{
    public static class CommandCategory
    {
        public static string Execution { get; set; } = "Execution";
        public static string Services { get; set; } = "Agent Services";
        public static string Navigation { get; set; } = "Navigation";
        public static string Network { get; set; } = "Network";
        public static string Media { get; set; } = "Media";
        public static string LateralMovement { get; set; } = "Lateral Movement";
        public static string System { get; set; } = "System";
        public static string Token { get; set; } = "Token";
        public static string UI { get; set; } = "UI";   
        public static string Agent { get; set; } = "Agent";   

        public static List<string> All { get; } = new List<string> { Network, Services, LateralMovement, Media, Navigation, Agent, UI };
    }
}
