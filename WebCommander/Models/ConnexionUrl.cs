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

    public bool IsLoopBack =>
    !String.IsNullOrEmpty(Address) && (
    Address == "127.0.0.1" ||
    Address == "::1" ||
    Address.Equals("localhost", StringComparison.OrdinalIgnoreCase));

    public static ConnexionUrl FromString(string connStr)
    {
        var conn = new ConnexionUrl { IsValid = false };
        
        try
        {
            if (!connStr.Contains("://")) return conn;

            var sep = new[] { "://" };
            var tab = connStr.Split(sep, StringSplitOptions.None);
            if (tab.Length != 2) return conn;

            var protocol = tab[0].ToLower();
            var part = tab[1];

            // Trouver le dernier ":"
            var lastColonIndex = part.LastIndexOf(':');
            if (lastColonIndex == -1) return conn;

            var address = part.Substring(0, lastColonIndex).Trim();
            var complement = part.Substring(lastColonIndex + 1).Trim();

            if (string.IsNullOrEmpty(complement)) return conn;

            // Déterminer le mode (Listener ou Client)
            var isListener = string.IsNullOrEmpty(address) || 
                           address == "*" || 
                           address == "0.0.0.0" ||
                           address == "::";

            var mode = isListener ? ConnexionMode.Listener : ConnexionMode.Client;

            // HTTP/HTTPS
            if (protocol == "http" || protocol == "https")
            {
                if (isListener) return conn; // HTTP ne peut être qu'en mode Client

                if (!int.TryParse(complement, out int port))
                {
                    // Port par défaut si vide
                    if (string.IsNullOrEmpty(complement))
                        port = protocol == "https" ? 443 : 80;
                    else
                        return conn;
                }

                if (!IsValidPort(port)) return conn;

                conn.Protocol = ConnexionType.Http;
                conn.Mode = ConnexionMode.Client;
                conn.IsSecure = protocol == "https";
                conn.Address = address;
                conn.Port = port;
                conn.IsValid = true;
                return conn;
            }

            // TCP
            if (protocol == "tcp")
            {
                if (!int.TryParse(complement, out int port) || !IsValidPort(port))
                    return conn;

                conn.Protocol = ConnexionType.Tcp;
                conn.Mode = mode;
                conn.Address = isListener ? string.Empty : address;
                conn.Port = port;
                conn.IsValid = true;
                return conn;
            }

            // Named Pipe
            if (protocol == "pipe")
            {
                if (!IsValidPipeName(complement))
                    return conn;

                conn.Protocol = ConnexionType.NamedPipe;
                conn.Mode = mode;
                conn.Address = isListener ? string.Empty : address;
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
                        return $"tcp://*:{Port}";
                    return $"tcp://{Address}:{Port}";
                }

            case ConnexionType.NamedPipe:
                {
                    if (Mode == ConnexionMode.Listener)
                        return $"pipe://*:{PipeName}";
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
