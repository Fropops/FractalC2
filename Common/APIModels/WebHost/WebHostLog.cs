using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.APIModels.WebHost
{
    public class WebHostLog
    {
        public DateTime Date { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }

        public string UserAgent { get; set; }
        public int StatusCode { get; set; }
    }
}
