using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IConfigService
    {
        string SpawnToX64 { get; set; }

        byte[] ServerKey { get; set; }
        WinAPI.Wrapper.APIAccessType APIAccessType { get; set; }

        WinAPI.Wrapper.InjectionMethod APIInjectionMethod { get; set; }

        bool EncryptFrames { get; set; }

        int JobResultDelay { get; set; }
    }
    public class ConfigService : IConfigService
    {
        public byte[] ServerKey { get; set; }
        public string SpawnToX64 { get; set; } = @"c:\windows\system32\dllhost.exe";

        public bool EncryptFrames { get; set; } = true;

        public int JobResultDelay { get; set; } = 5000;

        public WinAPI.Wrapper.APIAccessType APIAccessType { get; set; } = WinAPI.Wrapper.APIAccessType.DInvoke;

        public WinAPI.Wrapper.InjectionMethod APIInjectionMethod { get; set; } = WinAPI.Wrapper.InjectionMethod.CreateRemoteThread;
    }
}
