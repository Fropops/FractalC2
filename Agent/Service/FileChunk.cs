using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    [DataContract]
    public class FileChunk
    {
        [DataMember(Name = "fileId")]
        public string FileId { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "index")]
        public int Index { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }
      
    }
}
