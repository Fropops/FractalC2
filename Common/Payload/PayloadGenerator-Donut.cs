using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public partial class PayloadGenerator
    {
        public string GenerateB64(string sourceFile)
        {
            byte[] bytes = File.ReadAllBytes(sourceFile);
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        //public ExecuteResult GenerateDll(string binPath, string outPath, string processName)
        //{
        //    var cmd = this.Config.ReanimatorPath;
        //    var inputFile = binPath;
        //    var outFile = outPath;

        //    List<string> args = new List<string>();
        //    args.Add("-f");
        //    args.Add(inputFile);
        //    args.Add("-t");
        //    args.Add("raw");
        //    args.Add($"-o");
        //    args.Add(outFile);
        //    args.Add($"-e");
        //    args.Add($"-u");
        //    args.Add($"-b");
        //    args.Add($"-p");
        //    args.Add(processName);

        //    //Console.WriteLine(String.Join(' ', args));
        //    var ret = ExecuteCommand(cmd, args, Path.GetDirectoryName(this.Config.ReanimatorPath));
        //    return ret;
        //}

        //public ExecuteResult GenerateInj(string binPath, string outPath, string processName)
        //{
        //    var cmd = this.Config.ReanimatorPath;
        //    var inputFile = binPath;
        //    var outFile = outPath;

        //    List<string> args = new List<string>();
        //    args.Add("-f");
        //    args.Add(inputFile);
        //    args.Add("-t");
        //    args.Add("raw");
        //    args.Add($"-o");
        //    args.Add(outFile);
        //    args.Add($"-e");
        //    args.Add($"-u");
        //    args.Add($"-p");
        //    args.Add(processName);

        //    //Console.WriteLine(String.Join(' ', args));
        //    var ret = ExecuteCommand(cmd, args, Path.GetDirectoryName(this.Config.ReanimatorPath));
        //    return ret;
        //}

        public ExecuteResult GenerateBin(string inputPath, string outFile, bool x86, string dotNetParams = null)
        {
            var cmd = this.Config.DonutFolder + "/donut";

           
            List<string> args = new List<string>();
            args.Add("-f");
            args.Add("1");
            args.Add("-a");
            if (x86)
                args.Add("1");
            else
                args.Add("2");
            args.Add($"-o");
            args.Add(outFile);
            if (!string.IsNullOrEmpty(dotNetParams))
            {
                args.Add($"-p");
                args.Add(dotNetParams);
            }
            args.Add("-i");
            args.Add(inputPath);

            //Console.WriteLine(String.Join(' ', args));

            //string args = $"'{inputFile}' -f 1 -a 2 -o '{outFile}' -p '{listener.Ip}:{listener.BindPort}'";
            var ret = ExecuteCommand(cmd, args, this.Config.WorkingFolder);
            return ret;
        }
    }
}
