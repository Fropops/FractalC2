using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.APIModels
{
    public class Loot
    {
        public string FileName { get; set; }
        public string AgentId { get; set; }
        public string ThumbnailData { get; set; }
        public string IsImage { get; set; }
        public string Data { get; set; }
    }
}
