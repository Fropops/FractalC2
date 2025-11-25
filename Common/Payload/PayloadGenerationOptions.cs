using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Common.Payload
{
    public class PayloadGenerationOptions
    {
        public ConnexionUrl Endpoint { get; set; }
        public PayloadType Type { get; set; } = PayloadType.Executable;
        public PayloadArchitecture Architecture { get; set; } = PayloadArchitecture.x64;
        public string ServerKey { get; set; }
        public bool IsDebug { get; set; }
        public string DebugPath { get; set; }
        public bool IsVerbose { get; set; }

        public string ImplantName { get; set; }
        public bool IsInjected { get; set; }

        public int InjectionDelay { get; set; } = 60;
        public string InjectionProcess { get; set; }

        public override string ToString()
        {
            var s = $"{Type.ToString()} {Architecture.ToString()} {Endpoint.ToString()}";
            if (IsDebug) s += " Debug";
            if (IsInjected) s += " Injection";
            return s;
        }
    }
}
