using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ShortGuid
    {
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
        }
    }
}
