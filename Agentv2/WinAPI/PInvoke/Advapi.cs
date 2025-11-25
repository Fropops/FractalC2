using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.Data.Kernel32;

namespace WinAPI.PInvoke
{
    public static class Advapi
    {
        [DllImport("advapi32.dll")]
        public static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            DesiredAccess DesiredAccess,
            out IntPtr TokenHandle);

        [DllImport("advapi32.dll")]
        public extern static bool DuplicateTokenEx(
            IntPtr hExistingToken,
            TokenAccess dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            SecurityImpersonationLevel ImpersonationLevel,
            TokenType TokenType,
            out IntPtr phNewToken);

        [DllImport("advapi32.dll")]
        public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll")]
        public static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithTokenW(
           IntPtr hToken,
           uint dwLogonFlags,
           string lpApplicationName,
           string lpCommandLine,
           uint dwCreationFlags,
           IntPtr lpEnvironment,
           string lpCurrentDirectory,
           [In] ref STARTUPINFOEX lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll")]
        public static extern bool LogonUserA(
          string lpszUsername,
          string lpszDomain,
          string lpszPassword,
          LogonProvider dwLogonType,
          LogonUserProvider dwLogonProvider,
          ref IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
            string lpUsername,
            string lpDomain,
            string lpPassword,
            uint dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

    };
}
