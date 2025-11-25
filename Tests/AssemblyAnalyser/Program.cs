using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyAnalyser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var assembly = Assembly.LoadFile(@"E:\Share\Projects\FractalC2\Payloads\x64\Agent.exe");
                var a = assembly.GetReferencedAssemblies();
                var m = assembly.GetModule("Agent");
                var t = assembly.GetType("Agent.Entry");
                //foreach (var t in assembly.GetTypes())
                //{
                //    Console.WriteLine(t.Name);
                //}
                foreach (var module in assembly.GetModules())
                {
                    Console.WriteLine(m.Name);
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                }
            }
        }
    }
}
