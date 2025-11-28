using System.Net.Sockets;
using Common.Config;
using Microsoft.Extensions.Configuration;

namespace TeamServer.Helper
{
    public static class ConfigurationExtension
    {
        public static FoldersConfig FoldersConfigs(this IConfiguration config)
        {
            var conf = new FoldersConfig();
            conf.FromSection(config.GetSection("Folders"));
            return conf;
        }

        public static SpawnConfig SpawnConfigs(this IConfiguration config)
        {
            var conf = new SpawnConfig();
            conf.FromSection(config.GetSection("Spawn"));
            return conf;
        }
    }
}
