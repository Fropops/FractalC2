using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Terminal
{
    public class CommandDetail
    {
        public enum HandledKey
        {
            LeftArrow,
            RightArrow,
            BackSpace,
            Delete,
            Home,
            End
        }

        public CommandDetail(int y, string prompt, string value = null)
        {
            CursorStartY = y;
            Prompt = prompt; // + "(" + y + ")>";
            Value = value ?? String.Empty;
        }
        public int CursorStartY { get; set; }
        public string Prompt { get; set; }
        public string Value { get; set; }

        public void PrintPromp()
        {
            Console.ForegroundColor = TerminalConstants.PromptColor;
            Console.Write(this.Prompt);
            Console.ForegroundColor = TerminalConstants.DefaultForeGroundColor;
        }

        private void ResetPosition()
        {
            Console.CursorLeft = 0;
            Console.CursorTop = this.CursorStartY;
        }

        public void Print()
        {
            this.ResetPosition();
            this.PrintPromp();
            this.PrintToConsole(this.Value);
        }

        private void PrintToConsole(string val)
        {
            foreach(var c in val)
            {
                if(Console.CursorLeft == Console.WindowWidth -1)
                {
                    Console.WriteLine();
                    if(Console.WindowHeight - 1 == Console.CursorTop)
                    {
                        this.CursorStartY -= 1;
                    }
                }
                Console.Write(c);
            }
        }

        private void Clean()
        {
            ResetPosition();
            PrintToConsole(string.Empty.PadLeft(this.Prompt.Length));
            PrintToConsole(string.Empty.PadLeft(this.Value.Length));
        }
        public void Interrupt()
        {
            this.Clean();
            ResetPosition();
        }
        public void Reset(int newY)
        {
            this.CursorStartY = newY;
        }

        private int FullLength
        {
            get
            {
                return this.Prompt.Length + this.Value.Length;
            }
        }

        private int LastLineLength
        {
            get
            {
                return FullLength % Console.WindowWidth;
            }
        }

        private int LocalCursorX
        {
            get
            {
                return Console.CursorLeft;
            }
        }

        private int LocalCursorY
        {
            get
            {
                return Console.CursorTop - this.CursorStartY;
            }
        }

        private int CursorValueIndex
        {
            get
            {
                var index = LocalCursorY * Console.WindowWidth + LocalCursorX - this.Prompt.Length;
                return index;
            }
        }

        private void ClearAfter(int index)
        {
            PrintToConsole(String.Empty.PadLeft(Value.Length - index));
        }

        private void PrintAfter(int index)
        {
            PrintToConsole(Value.Substring(CursorValueIndex, Value.Length - index));
        }

        private void PlaceCursorAtIndex(int index)
        {
            Console.CursorTop = this.CursorStartY + (index + this.Prompt.Length) / Console.WindowWidth;
            Console.CursorLeft = (index + this.Prompt.Length) % Console.WindowWidth;
        }

        public void HandleInput(char c)
        {
            try
            {
                this.PutCharAt(c, CursorValueIndex);
            }
            catch { }
        }

        private void PutCharAt(char? c, int index)
        {
            PlaceCursorAtIndex(index);

            this.ClearAfter(index);

            PlaceCursorAtIndex(index);

            if (c.HasValue)
                this.Value = this.Value.Insert(index, c.ToString());
            else
                this.Value = this.Value.Substring(0, index) + this.Value.Substring(index + 1);

            this.PrintAfter(index);
            if (c.HasValue)
                index++;
            PlaceCursorAtIndex(index);
        }

        public void HandleInput(HandledKey key)
        {
            switch (key)
            {
                case HandledKey.Home:
                    {
                        Console.CursorTop = this.CursorStartY;
                        Console.CursorLeft = this.Prompt.Length;
                    }
                    break;
                case HandledKey.End:
                    {
                        Console.CursorTop = this.CursorStartY + this.FullLength / Console.WindowWidth;
                        Console.CursorLeft = this.LastLineLength;
                    }
                    break;
                case HandledKey.LeftArrow:
                    {
                        if (CursorValueIndex <= 0)
                            break;

                        if (Console.CursorLeft == 0)
                        {
                            Console.CursorLeft = Console.WindowWidth -1;
                            Console.CursorTop -= 1;
                        }
                        else
                            Console.CursorLeft -= 1;
                    }
                    break;
                case HandledKey.BackSpace:
                    {
                        if (CursorValueIndex <= 0)
                            break;

                        this.PutCharAt(null, CursorValueIndex - 1);
                    }
                    break;
                case HandledKey.RightArrow:
                    {
                        if (CursorValueIndex >= this.Value.Length)
                            break;

                        if (Console.CursorLeft == Console.WindowWidth -1)
                        {
                            Console.CursorLeft = 0;
                            Console.CursorTop += 1;
                        }
                        else
                            Console.CursorLeft += 1;
                    }
                    break;
                case HandledKey.Delete:
                    {
                        if (CursorValueIndex >= this.Value.Length)
                            break;

                        this.PutCharAt(null, CursorValueIndex);
                    }
                    break;

            }
        }


    }
}
