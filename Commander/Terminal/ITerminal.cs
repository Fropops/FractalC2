using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Commander.Terminal
{
    public interface ITerminal
    {
        event EventHandler<string> InputValidated;

        bool CanHandleInput { get; set; }

        string Prompt { get; set; }

        //public ConsoleColor DefaultColor { get; set; }
        Task Start();

        void stop();

        void Interrupt();

        void Restore();

        void NewLine(bool brk = true);


        void WriteLine(TerminalMessageType typ, params string[] strs);

       // void WritePrompt();

        public void WriteSuccess(params string[] parm);
        public void WriteError(params string[] parm);
        void WriteInfo(params string[] parm);

        void WriteLine();

        void Write(string text);
        void Write(string text, int count);

        void WriteLine(params string[] parm);

        void Write(IRenderable renderabel);

        void WriteMarkup(string markup);

        void WriteLineMarkup(string markup);

        void SaveCursorPosition();

        void ResetCursorPosition();

        void SetCursorPosition(int left, int top);

        public void DrawBackGround(ConsoleColor color, int height);

        void SetForeGroundColor(ConsoleColor color);

        void SetBackGroundColor(ConsoleColor color);

        void ShowProgress(string label, int progress, bool newLine = false);
    }
}
