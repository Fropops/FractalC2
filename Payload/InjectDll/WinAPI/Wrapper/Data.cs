using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAPI.Wrapper
{
    public class ProcessCredentials
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } 
    }
    public class ProcessCreationParameters
    {
        public string Application { get; set; }
        public string Command { get; set; }
        public IntPtr Token { get; set; }
        public bool RedirectOutput { get; set; }
        public bool CreateSuspended { get; set; }
        public bool CreateNoWindow { get; set; }
        public string CurrentDirectory { get; set; }

        public ProcessCredentials Credentials { get; set; }
    }

    public class ProcessCreationResult
    {
        public int ProcessId { get; set; }
        public IntPtr ProcessHandle { get; set; }
        public IntPtr ThreadHandle { get; set; }
        public IntPtr OutPipeHandle { get; set; }
    }

    public enum APIAccessType
    {
        PInvoke,
        DInvoke
    }

    public enum InjectionMethod
    {
        CreateRemoteThread,
        ProcessHollowingWithAPC
    }
}
