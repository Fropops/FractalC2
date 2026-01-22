using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinAPI.Data.AdvApi;
using WinAPI.Data.Kernel32;

namespace WinAPI.DInvoke
{
    public static class Advapi
    {
        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool OpenProcessToken(
                IntPtr hProcess,
                DesiredAccess dwDesiredAccess,
                out IntPtr hToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool DuplicateTokenEx(
                IntPtr hExistingToken,
                TokenAccess dwTokenAccess,
                SECURITY_ATTRIBUTES lpTokenAttributes,
                SecurityImpersonationLevel ImpersonationLevel,
                TokenType TokenType,
                out IntPtr hNewToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool ImpersonateLoggedOnUser(
                IntPtr hToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool CreateProcessWithTokenW(
                IntPtr hToken,
                LogonFlags dwLogonFlags,
                [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
                [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
                PROCESS_CREATION_FLAGS dwCreationFlags,
                IntPtr lpEnvironment,
                [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
                ref STARTUPINFOEX lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool CreateProcessWithLogonW(
               [MarshalAs(UnmanagedType.LPWStr)] string lpUsername,
               [MarshalAs(UnmanagedType.LPWStr)] string lpDomain,
               [MarshalAs(UnmanagedType.LPWStr)] string lpPassword,
               LogonFlags dwLogonFlags,
               [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
               [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
               PROCESS_CREATION_FLAGS dwCreationFlags,
               IntPtr lpEnvironment,
               [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
               ref STARTUPINFOEX lpStartupInfo,
               out PROCESS_INFORMATION lpProcessInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool LookupPrivilegeValue(
                string lpSystemName,
                string lpName,
                ref LUID lpLuid);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool AdjustTokenPrivileges(
                IntPtr TokenHandle,
                bool DisableAllPrivileges,
                ref TOKEN_PRIVILEGES NewState,
                int BufferLength,
                IntPtr PreviousState,
                IntPtr ReturnLength);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool GetTokenInformation(
                IntPtr TokenHandle,
                TOKEN_INFORMATION_CLASS TokenInformationClass,
                IntPtr TokenInformation,
                int TokenInformationLength,
                out int ReturnLength);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool LookupAccountSid(
                string lpSystemName,
                IntPtr Sid,
                StringBuilder lpName,
                ref int cchName,
                StringBuilder lpReferencedDomainName,
                ref int cchReferencedDomainName,
                out int peUse);

            #region ServiceManager
            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate IntPtr OpenSCManagerW(
       [MarshalAs(UnmanagedType.LPWStr)] string machineName,
       [MarshalAs(UnmanagedType.LPWStr)] string databaseName,
       SCM_ACCESS_RIGHTS desiredAccess);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate IntPtr CreateServiceW(
                IntPtr hSCManager,
                [MarshalAs(UnmanagedType.LPWStr)] string serviceName,
                [MarshalAs(UnmanagedType.LPWStr)] string displayName,
                SERVICE_ACCESS_RIGHTS desiredAccess,
                SERVICE_TYPE serviceType,
                START_TYPE startType,
                ERROR_CONTROL errorControl,
                [MarshalAs(UnmanagedType.LPWStr)] string binaryPathName,
                [MarshalAs(UnmanagedType.LPWStr)] string loadOrderGroup,
                IntPtr tagId,
                [MarshalAs(UnmanagedType.LPWStr)] string dependencies,
                [MarshalAs(UnmanagedType.LPWStr)] string serviceStartName,
                [MarshalAs(UnmanagedType.LPWStr)] string password);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate bool StartServiceW(
                IntPtr hService,
                uint numServiceArgs,
                [MarshalAs(UnmanagedType.LPWStr)] string serviceArgVectors);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate bool DeleteService(IntPtr hService);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            public delegate bool CloseServiceHandle(IntPtr hSCObject);
            #endregion
        }

        public static bool OpenProcessToken(IntPtr hProcess, DesiredAccess dwDesiredAccess, out IntPtr hToken)
        {
            object[] parameters = { hProcess, (uint)dwDesiredAccess, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"OpenProcessToken", typeof(Delegates.OpenProcessToken), ref parameters);
            hToken = (IntPtr)parameters[2];
            return retVal;
        }

        public static bool ImpersonateLoggedOnUser(IntPtr hToken)
        {
            object[] parameters = { hToken };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"ImpersonateLoggedOnUser", typeof(Delegates.ImpersonateLoggedOnUser), ref parameters);
            return retVal;
        }

        public static bool DuplicateTokenEx(
              IntPtr hExistingToken,
              TokenAccess dwTokenAccess,
              ref SECURITY_ATTRIBUTES lpTokenAttributes,
              SecurityImpersonationLevel ImpersonationLevel,
              TokenType TokenType,
              out IntPtr hNewToken)
        {
            object[] parameters = { hExistingToken, (uint)dwTokenAccess, lpTokenAttributes, (int)ImpersonationLevel, (int)TokenType, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"DuplicateTokenEx", typeof(Delegates.DuplicateTokenEx), ref parameters);
            hNewToken = (IntPtr)parameters[5];
            return retVal;
        }

        public static bool CreateProcessWithLogonW(string lpUsername,
            string lpDomain,
            string lpPassword,
            LogonFlags dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            PROCESS_CREATION_FLAGS dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFOEX lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation)
        {
            object[] parameters = { lpUsername, lpDomain, lpPassword, (uint)dwLogonFlags, lpApplicationName, lpCommandLine, (uint)dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"CreateProcessWithLogonW", typeof(Delegates.CreateProcessWithLogonW), ref parameters);
            lpProcessInformation = (PROCESS_INFORMATION)parameters[10];
            return retVal;
        }

        public static bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid)
        {
            object[] parameters = { lpSystemName, lpName, lpLuid };

            var retVal = (bool)Generic.DynamicApiInvoke(
                @"advapi32.dll",
                @"LookupPrivilegeValueW",
                typeof(Delegates.LookupPrivilegeValue),
                ref parameters);

            lpLuid = (LUID)parameters[2];
            return retVal;
        }

        public static bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength)
        {
            object[] parameters = { TokenHandle, DisableAllPrivileges, NewState, BufferLength, PreviousState, ReturnLength };

            var retVal = (bool)Generic.DynamicApiInvoke(
                @"advapi32.dll",
                @"AdjustTokenPrivileges",
                typeof(Delegates.AdjustTokenPrivileges),
                ref parameters);

            NewState = (TOKEN_PRIVILEGES)parameters[2];
            return retVal;
        }

        public static bool CreateProcessWithTokenW(IntPtr hToken,
           LogonFlags dwLogonFlags,
           string lpApplicationName,
           string lpCommandLine,
           PROCESS_CREATION_FLAGS dwCreationFlags,
           IntPtr lpEnvironment,
           string lpCurrentDirectory,
           ref STARTUPINFOEX lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation)
        {
            object[] parameters = { hToken, (uint)dwLogonFlags, lpApplicationName, lpCommandLine, (uint)dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, null };

            var retVal = (bool)Generic.DynamicApiInvoke(@"advapi32.dll", @"CreateProcessWithTokenW", typeof(Delegates.CreateProcessWithTokenW), ref parameters);
            lpProcessInformation = (PROCESS_INFORMATION)parameters[8];
            return retVal;
        }

        public static bool GetTokenInformation(
            IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            int TokenInformationLength,
            out int ReturnLength)
        {
            int retLen = 0;
            object[] parameters = { TokenHandle, TokenInformationClass, TokenInformation, TokenInformationLength, retLen };

            var retVal = (bool)Generic.DynamicApiInvoke(
                @"advapi32.dll",
                @"GetTokenInformation",
                typeof(Delegates.GetTokenInformation),
                ref parameters);

            ReturnLength = (int)parameters[4];
            return retVal;
        }

        public static bool LookupAccountSid(
            string lpSystemName,
            IntPtr Sid,
            StringBuilder lpName,
            ref int cchName,
            StringBuilder lpReferencedDomainName,
            ref int cchReferencedDomainName,
            out int peUse)
        {
            int pe = 0;
            object[] parameters = { lpSystemName, Sid, lpName, cchName, lpReferencedDomainName, cchReferencedDomainName, pe };

            var retVal = (bool)Generic.DynamicApiInvoke(
                @"advapi32.dll",
                @"LookupAccountSidW",
                typeof(Delegates.LookupAccountSid),
                ref parameters);

            cchName = (int)parameters[3];
            cchReferencedDomainName = (int)parameters[5];
            peUse = (int)parameters[6];
            return retVal;
        }

        #region ServiceManager
        public static IntPtr OpenSCManager(string machineName, SCM_ACCESS_RIGHTS desiredAccess)
        {
            object[] parameters = { machineName, null, desiredAccess };

            return (IntPtr)Generic.DynamicApiInvoke(
                "advapi32.dll",
                "OpenSCManagerW",
                typeof(Delegates.OpenSCManagerW),
                ref parameters);
        }

        public static IntPtr CreateService(IntPtr hSCManager, string serviceName, string displayName,
            SERVICE_ACCESS_RIGHTS desiredAccess, SERVICE_TYPE serviceType, START_TYPE startType, string binaryPathName)
        {
            object[] parameters =
            {
            hSCManager, serviceName, displayName, desiredAccess, serviceType, startType,
            ERROR_CONTROL.SERVICE_ERROR_IGNORE, binaryPathName, null, IntPtr.Zero, null,
            "NT AUTHORITY\\SYSTEM", null
        };

            return (IntPtr)Generic.DynamicApiInvoke(
                "advapi32.dll",
                "CreateServiceW",
                typeof(Delegates.CreateServiceW),
                ref parameters);
        }

        public static bool StartService(IntPtr hService)
        {
            object[] parameters = { hService, (uint)0, null };

            return (bool)Generic.DynamicApiInvoke(
                "advapi32.dll",
                "StartServiceW",
                typeof(Delegates.StartServiceW),
                ref parameters);
        }

        public static bool DeleteService(IntPtr hService)
        {
            object[] parameters = { hService };

            return (bool)Generic.DynamicApiInvoke(
                "advapi32.dll",
                "DeleteService",
                typeof(Delegates.DeleteService),
                ref parameters);
        }

        public static bool CloseServiceHandle(IntPtr hSCObject)
        {
            object[] parameters = { hSCObject };

            return (bool)Generic.DynamicApiInvoke(
                "advapi32.dll",
                "CloseServiceHandle",
                typeof(Delegates.CloseServiceHandle),
                ref parameters);
        }
        #endregion

    }


}
