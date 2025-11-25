using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public enum PayloadType
    {
        Executable,
        Library,
        Service,
        PowerShell,
        Binary
    }

    public enum PayloadArchitecture
    {
        x64,
        x86
    }
}
