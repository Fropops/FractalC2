using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace WinAPI
{
    /// <summary>
    /// SafeHandle wrapper for native token handles.
    /// Automatically closes the underlying handle via CloseHandle when disposed or finalized.
    /// </summary>
    [SecurityCritical]
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        public SafeTokenHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        public static SafeTokenHandle InvalidHandle => new SafeTokenHandle(IntPtr.Zero, false);

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            try
            {
                return APIWrapper.CloseHandle(handle);
            }
            catch
            {
                return false;
            }
        }
    }
}
