using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.APIModels
{
    public class CreateTaskRequest
    {
        public string Id { get; set; }

        public string Command { get; set; }

        public string TaskBin { get; set; }
    }
}
