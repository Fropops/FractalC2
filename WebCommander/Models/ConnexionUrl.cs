namespace WebCommander.Models
{
    public class ConnexionUrl
    {
        public ConnexionType Protocol { get; set; }
        public ConnexionMode Mode { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string PipeName { get; set; }
        public bool IsSecure { get; set; }
        public bool IsValid { get; set; }

        public string ProtocolString
        {
            get
            {
                var str = Protocol.ToString().ToLower();
                if (IsSecure && Protocol == ConnexionType.Http)
                    str += "s";
                return str;
            }
        }

        public static ConnexionUrl FromString(string connStr)
        {
            var conn = new ConnexionUrl { IsValid = false };

            try
            {
                string protocol = string.Empty;
                // Mode Listener (server)
                if (connStr.Contains(">"))
                {
                    var parts = connStr.Split('>');
                    if (parts.Length != 2) return conn;

                    protocol = parts[0].ToLower();
                    var param = parts[1].Trim();

                    if (protocol == "tcp")
                    {
                        if (!int.TryParse(param, out int port) || !IsValidPort(port))
                            return conn;

                        conn.Protocol = ConnexionType.Tcp;
                        conn.Mode = ConnexionMode.Listener;
                        conn.Port = port;
                        conn.IsValid = true;
                        return conn;
                    }

                    if (protocol == "pipe")
                    {
                        if (!IsValidPipeName(param))
                            return conn;

                        conn.Protocol = ConnexionType.NamedPipe;
                        conn.Mode = ConnexionMode.Listener;
                        conn.PipeName = param;
                        conn.IsValid = true;
                        return conn;
                    }

                    return conn;
                }

                // Mode Client
                if (!connStr.Contains("://")) return conn;

                var sep = new[] { "://" };
                var tab = connStr.Split(sep, StringSplitOptions.None);
                if (tab.Length != 2) return conn;

                protocol = tab[0].ToLower();
                var part = tab[1];
                var parmTab = part.Split(':');

                if (parmTab.Length < 1 || parmTab.Length > 2) return conn;

                var address = parmTab[0].Trim();
                if (string.IsNullOrEmpty(address)) return conn;

                var complement = parmTab.Length > 1 ? parmTab[1].Trim() : string.Empty;

                // HTTP/HTTPS
                if (protocol == "http" || protocol == "https")
                {
                    conn.Protocol = ConnexionType.Http;
                    conn.Mode = ConnexionMode.Client;
                    conn.IsSecure = protocol == "https";
                    conn.Address = address;

                    if (string.IsNullOrEmpty(complement))
                    {
                        conn.Port = conn.IsSecure ? 443 : 80;
                    }
                    else
                    {
                        if (!int.TryParse(complement, out int port) || !IsValidPort(port))
                            return conn;
                        conn.Port = port;
                    }

                    conn.IsValid = true;
                    return conn;
                }

                // TCP Client
                if (protocol == "tcp")
                {
                    if (string.IsNullOrEmpty(complement)) return conn;
                    if (!int.TryParse(complement, out int port) || !IsValidPort(port))
                        return conn;

                    conn.Protocol = ConnexionType.Tcp;
                    conn.Mode = ConnexionMode.Client;
                    conn.Address = address;
                    conn.Port = port;
                    conn.IsValid = true;
                    return conn;
                }

                // Named Pipe Client
                if (protocol == "pipe")
                {
                    if (!IsValidPipeName(complement))
                        return conn;

                    conn.Protocol = ConnexionType.NamedPipe;
                    conn.Mode = ConnexionMode.Client;
                    conn.Address = address;
                    conn.PipeName = complement;
                    conn.IsValid = true;
                    return conn;
                }
            }
            catch
            {
                conn.IsValid = false;
            }

            return conn;
        }

        public override string ToString()
        {
            switch (Protocol)
            {
                case ConnexionType.Http:
                    {
                        var prot = IsSecure ? "https" : "http";
                        return $"{prot}://{Address}:{Port}";
                    }

                case ConnexionType.Tcp:
                    {
                        if (Mode == ConnexionMode.Listener)
                            return $"tcp>{Port}";
                        return $"tcp://{Address}:{Port}";
                    }

                case ConnexionType.NamedPipe:
                    {
                        if (Mode == ConnexionMode.Listener)
                            return $"pipe>{PipeName}";
                        return $"pipe://{Address}:{PipeName}";
                    }
            }

            return string.Empty;
        }

        private static bool IsValidPort(int port)
        {
            return port >= 0 && port <= 65535;
        }

        private static bool IsValidPipeName(string pipeName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(pipeName, @"^[a-zA-Z0-9]+$");
        }
    }

    public enum ConnexionType
    {
        Http,
        Tcp,
        NamedPipe
    }

    public enum ConnexionMode
    {
        Client,
        Listener
    }
}
