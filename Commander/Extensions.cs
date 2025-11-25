using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    public static class Extensions
    {
        public static string ToShortGuid(this String guid)
        {
            if (guid.Length < 10)
                return guid;
            return guid.Replace("-", "").Substring(0, 10);
        }

        public static string ToShortGuid(this Guid guid)
        {
            return guid.ToString().Replace("-", "").Substring(0, 10);
        }

        public static string[] GetArgs(this string src)
        {
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

        public static string ExtractAfterParam(this string src, int prmIndex)
        {
            src = src.Trim();

            bool inQuotes = false;
            bool inDoubleQuotes = false;

            string currentValue = string.Empty;
            int strIndex = -1;
            int prmCount = 0;
            foreach (var c in src)
            {
                strIndex++;
                if (c == '\"')
                {
                    if (!inQuotes)
                    {
                        if (inDoubleQuotes)
                        {
                            //end of params => 
                            inDoubleQuotes = false;

                            if (prmCount == prmIndex)
                                break;
                            else
                                prmCount++;
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
                            if (prmCount == prmIndex)
                                break;
                            else
                                prmCount++;
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
                            if (prmCount == prmIndex)
                                break;
                            else
                                prmCount++;
                            currentValue = string.Empty;
                        }
                    }
                    else
                        currentValue += c;
                    continue;
                }

                currentValue += c;
            }

            strIndex++;
            return src.Substring(strIndex, src.Length - strIndex);
        }
            
    }
}

