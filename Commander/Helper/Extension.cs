using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Commander.Helper
{
    public static class Extension
    {
        public static string Command(this CommandVerbs verb)
        {
            return verb.ToString().ToLower();
        }
    }
}
