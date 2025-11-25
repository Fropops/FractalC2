using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public partial class PayloadGenerator
    {
        public ExecuteResult ReplaceRessource(string scriptPath, string sourceExePath, string resourcePath, string destExePath)
        {
            var cmd = this.Config.PythonFolder + "/bin/python";

            List<string> args = new List<string>();
            args.Add(scriptPath);
            args.Add(sourceExePath);
            args.Add(resourcePath);
            args.Add(destExePath);

            var ret = ExecuteCommand(cmd, args, this.Config.WorkingFolder);
            return ret;
        }
    }
}
