using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security;

namespace Agent
{
    public static class Extensions
    {
        public static SecureString ToSecureString(this string value)
        {
            var secure = new SecureString();

            foreach (var c in value)
                secure.AppendChar(c);

            return secure;
        }

        public static string[] GetArgs(this string src)
        {
            if(src == null)
                return new string[0];

            var res = new List<string>();
            src = src.Trim();

            bool inQuotes = false;
            bool inDoubleQuotes = false;

            string currentValue = string.Empty;
            foreach (var c in src)
            {
                if (c == '\"')
                {
                    if (!inQuotes)
                    {
                        if (inDoubleQuotes)
                        {
                            //end of params => 
                            inDoubleQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inDoubleQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c== '\'')
                {
                    if (!inDoubleQuotes)
                    {
                        if (inQuotes)
                        {
                            //end of params => 
                            inQuotes = false;
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        else
                            inQuotes = true;
                    }
                    else
                    {
                        currentValue += c;
                    }
                    continue;
                }

                if (c == ' ')
                {
                    if (!inQuotes && !inDoubleQuotes)
                    {
                        if (!string.IsNullOrEmpty(currentValue))
                        {
                            res.Add(currentValue);
                            currentValue = string.Empty;
                        }
                    }
                    else
                        currentValue += c;
                    continue;
                }

                currentValue += c;
            }


            if (!string.IsNullOrEmpty(currentValue))
            {
                res.Add(currentValue);
                currentValue = string.Empty;
            }

            return res.ToArray();
        }

        public static void Clear(this MemoryStream stream)
        {
            var buffer = stream.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            stream.Position = 0;
            stream.SetLength(0);
        }
    }

}
