using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Agent
{
    public static class ShortGuid
    {
        public static string NewGuid()
        {
            var newGuid = string.Empty;
            do
                newGuid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            while (Regex.IsMatch(newGuid, @"^\d+$")); //only digits

            return newGuid;
        }
    }
}
