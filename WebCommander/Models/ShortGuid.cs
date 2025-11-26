using System;
using System.Text.RegularExpressions;

namespace WebCommander.Models
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
