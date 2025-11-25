using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum CommandId : byte
    {

        None = 0x0,
        CheckIn = 0x01,
        Whoami = 0x02,
        Exit = 0x03,

        Cat = 0x04,
        Cd = 0x05,
        Pwd = 0x06,
        Ls = 0x07,

        Powershell = 0x08,
        PowershellImport = 0x09,
        Shell = 0x010,

        Job = 0x11,
        ForkAndRun = 0x12,
        Assembly = 0x13,
        Inject = 0x14,
        Link = 0x15,

        ListProcess = 0x16,

        Upload = 0x17,
        Download = 0x18,

        Proxy = 0x19,
        Sleep = 0x20,
        RportFwd = 0x21,

        MakeToken = 0x22,
        StealToken = 0x23,
        RevertSelf = 0x24,

        Idle = 0x25,

        Script = 0x26,
        Delay = 0x27,
        Echo = 0x28,

        Winrm = 0x29,
        PsExec = 0x30,

        KeyLog = 0x31,
        Run = 0x32,
        Start = 0x33,
        StartAs = 0x34,

        RmDir = 0x35,
        MkDir = 0x36,
        Del = 0x37,
        Reg = 0x38,

        Capture = 0x39

    }
}   
