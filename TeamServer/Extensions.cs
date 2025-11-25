using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamServer
{
    public static class Extensions
    {
        public static string GetListenerUri(this HttpRequest req)
        {
            var port = req.Host.Port;
            if (!port.HasValue)
            {
                if (req.Scheme == "https")
                    port = 443;
                else
                    port = 80;
            }
            return $"{req.Scheme}://{req.Host.Host}:{port}".ToLower();
        }

        public static int GetPort(this HttpRequest req)
        {
            var port = req.Host.Port;
            if (!port.HasValue)
            {
                if (req.Scheme == "https")
                    return 443;
                else
                    return 80;
            }
            return port.Value;
        }
    }
}
