using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public enum ImplantType
    {
        PowerShell = 0,
        Executable,
        Library,
        ReflectiveLibrary,
        Service,
        Shellcode,
        Elf,
    }

    public enum ImplantArchitecture
    {
        x64 = 0,
        x86
    }
}
