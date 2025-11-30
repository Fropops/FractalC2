using System.Collections.Generic;

namespace WebCommander.Models
{
    public static class CommandCategory
    {
        public static string Core { get; set; } = "Core";
        public static string Execution { get; set; } = "Execution";
        public static string Services { get; set; } = "Agent Services";
        public static string Navigation { get; set; } = "Navigation";
        public static string Commander { get; set; } = "Commander";
        public static string Network { get; set; } = "Network";
        public static string Media { get; set; } = "Media";
        public static string LateralMovement { get; set; } = "Lateral Movement";
        public static string Listeners { get; set; } = "Listeners";
        public static string System { get; set; } = "System";
        public static string Others { get; set; } = "Others";

        public static List<string> All { get; } = new List<string> { Commander, Network, Core, Services, LateralMovement, Media, Listeners, Navigation, Others };
    }
}
