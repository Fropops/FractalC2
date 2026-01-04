using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;

namespace Common.APIModels
{
    public class APIImplantCreationResult
    {
        public APIImplant Implant { get; set; }
        public string Logs { get; set; }
    }
}
