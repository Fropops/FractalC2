using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.Kernel32;
using static WinAPI.DInvoke.Data.Native;
using static WinAPI.DInvoke.Kernel32;

namespace WinAPI.PInvoke
{
    public static class Native
    {
        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        public static extern UInt32 NtCreateSection(
            ref IntPtr SectionHandle,
            UInt32 DesiredAccess,
            IntPtr ObjectAttributes,
            ref UInt32 MaximumSize,
            UInt32 SectionPageProtection,
            UInt32 AllocationAttributes,
            IntPtr FileHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern UInt32 NtMapViewOfSection(
            IntPtr SectionHandle,
            IntPtr ProcessHandle,
            ref IntPtr BaseAddress,
            UIntPtr ZeroBits,
            UIntPtr CommitSize,
            out ulong SectionOffset,
            out uint ViewSize,
            uint InheritDisposition,
            uint AllocationType,
            uint Win32Protect);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern UInt32 NtQueueApcThread(
            IntPtr ThreadHandle,
            IntPtr ApcRoutine,
            IntPtr ApcArgument1,
            IntPtr ApcArgument2,
            IntPtr ApcArgument3);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern UInt32 NtResumeThread(
               IntPtr ThreadHandle,
               ref UInt32 PreviousSuspendCount);


        [DllImport("ntdll.dll")]
        public static extern UInt32 NtOpenProcess(
               out IntPtr ProcessHandle,
               ProcessAccessFlags DesiredAccess,
               ref OBJECT_ATTRIBUTES ObjectAttributes,
               ref CLIENT_ID ClientId
           );
    }
}
