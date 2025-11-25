using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public partial class PayloadGenerator
    {
        public List<string> ComputeNimBuildParameters(string scriptPath, string outFile, bool isX86 = false,bool isDebug = false, bool isDll = false)
        {
            if (!Path.GetExtension(scriptPath).Equals(".nim", StringComparison.OrdinalIgnoreCase))
                scriptPath += ".nim";

            var parms = new List<string>();

            parms.Add("c");

            if (isDll)
                parms.Add("--app:lib");
            else if (isDebug)
                parms.Add("--app:console");
            else
                parms.Add("--app:gui");

            if (isX86)
                parms.Add($"--cpu:i386");
            else
                parms.Add( $"--cpu:amd64");

            if(isDll)
                parms.Add($"--nomain");

            if (!isDebug)
            {
                parms.Add("-d:release");
                parms.Add("-d:strip");
                parms.Add("--passL:-s");
            }

            parms.Add("-f");
            parms.Add("-d:mingw");
            parms.Add($"-o:{outFile}");


            parms.Add($"{scriptPath}");

            return parms;
        }

        public ExecuteResult NimBuild(List<string> parms)
        {
            return this.ExecuteCommand(this.Config.NimPath, parms, this.Config.WorkingFolder);
        }

        public string[] SplitIntoChunks(string input, int chunkSize)
        {
            int stringLength = input.Length;

            // Calculate the number of chunks we will need.
            int chunkCount = stringLength / chunkSize;
            if (stringLength % chunkSize > 0)
            {
                chunkCount++;
            }

            // Initialize the array to hold the chunks.
            string[] chunks = new string[chunkCount];

            // Split the input string into the array.
            for (int i = 0; i < chunkCount; i++)
            {
                int startIndex = i * chunkSize;
                int length = Math.Min(chunkSize, stringLength - startIndex);
                chunks[i] = input.Substring(startIndex, length);
            }

            return chunks;
        }
    }
}
