using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiscUtil.Conversion;

namespace Agent.Helpers
{
    public static class PipeExtensions
    {
        [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
        static extern bool PeekNamedPipe(IntPtr hNamedPipe, IntPtr lpBuffer,
           IntPtr nBufferSize, IntPtr lpBytesRead, ref uint lpTotalBytesAvail,
           IntPtr lpBytesLeftThisMessage);

        public static bool DataAvailable(this PipeStream pipe)
        {
            var hPipe = pipe.SafePipeHandle.DangerousGetHandle();
            uint nb = 0;
            bool result = PeekNamedPipe(hPipe, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref nb, IntPtr.Zero);
            if (result == false)
                throw new System.ComponentModel.Win32Exception("Named Pipe is not available.");

            return nb > 0;
        }
    }
}
