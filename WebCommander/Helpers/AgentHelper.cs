using WebCommander.Models;
using Common.Models;

namespace WebCommander.Helpers
{
    public static class AgentHelper
    {
        public static bool? IsAgentAlive(Agent agent, IEnumerable<Agent> allAgents)
        {
            if (agent.Metadata == null)
                return null;

            if (agent.Metadata.SleepInterval == 0)
            {
                if (agent.LastSeen.AddSeconds(10) >= DateTime.UtcNow)
                    return true;
                return false;
            }

            int delta = 0;
            if (!string.IsNullOrEmpty(agent.RelayId))
            {
                var relay = allAgents.FirstOrDefault(a => a.Id == agent.RelayId);
                if (relay == null || relay.Metadata == null)
                    return null;
                delta = Math.Min(3, relay.Metadata.SleepInterval) * 3;
            }
            else
                delta = Math.Min(3, agent.Metadata.SleepInterval) * 3;

            if (agent.LastSeen.AddSeconds(delta) >= DateTime.UtcNow)
                return true;

            return false;
        }

        public static string FormatElapsedTime(double seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            string formattedTime = "";

            if (timeSpan.Days > 0)
            {
                formattedTime += $"{timeSpan.Days}d ";
            }

            if (timeSpan.Hours > 0 || timeSpan.Days > 0)
            {
                formattedTime += $"{timeSpan.Hours}h ";
            }

            if (timeSpan.Minutes > 0 || timeSpan.Hours > 0 || timeSpan.Days > 0)
            {
                formattedTime += $"{timeSpan.Minutes}m ";
            }

            formattedTime += $"{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00}s";

            return formattedTime.Trim();
        }

        public static string IpAsString(byte[]? ipAddressBytes)
        {
            if (ipAddressBytes == null)
                return string.Empty;
            return string.Join(".", ipAddressBytes);
        }
    }
}
