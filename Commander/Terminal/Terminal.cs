using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Commander.Terminal
{



    public partial class Terminal : ITerminal
    {
        public const string DefaultPrompt = "$> ";

        public event EventHandler<string> InputValidated;

        CancellationTokenSource _token = new CancellationTokenSource();

        public bool CanHandleInput { get; set; } = true;

        public Terminal()
        {
            Console.TreatControlCAsInput = true;
        }
        public async Task Start()
        {
            this.Write(new FigletText("Fractal C2")
            .LeftJustified()
            .Color(Color.Green));

            this.WriteLineMarkup($"[grey]Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}[/]");
            this.WriteLine();

            this.WriteLine();

            //this.WriteLine(Console.WindowWidth + "-" + Console.WindowHeight);
            this.NewLine(false);
            while (!_token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    try
                    {
                        this.HandleKey(key);
                    }
                    catch (Exception e)
                    {
                        this.WriteLine();
                        this.WriteError("Terminal Error :");
                        this.WriteError("----------------");
                        this.WriteError(e.ToString());
                        this.CanHandleInput = true;
                    }
                }

                await Task.Delay(2);
            }
        }

        private CommandHistory History = new CommandHistory();

        public string Prompt { get; set; } = "$> ";

        private CommandDetail CurrentCommand { get; set; }

        protected void HandleKey(ConsoleKeyInfo key)
        {
           

            if (!this.CanHandleInput)
                return;
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow: this.CurrentCommand.HandleInput(CommandDetail.HandledKey.LeftArrow); break;
                case ConsoleKey.RightArrow: this.CurrentCommand.HandleInput(CommandDetail.HandledKey.RightArrow); break;
                case ConsoleKey.Home: this.CurrentCommand.HandleInput(CommandDetail.HandledKey.Home); break;
                case ConsoleKey.End: this.CurrentCommand.HandleInput(CommandDetail.HandledKey.End); break;
                case ConsoleKey.Backspace: this.CurrentCommand.HandleInput(CommandDetail.HandledKey.BackSpace); break;
                case ConsoleKey.Delete: this.CurrentCommand.HandleInput(CommandDetail.HandledKey.Delete); break;
                default:
                    {
                        if (key.Key == ConsoleKey.C && (key.Modifiers & ConsoleModifiers.Control) != 0)
                        {
                            this.History.Pop();
                            this.NewLine();
                            break;
                        }

                        this.CurrentCommand.HandleInput(key.KeyChar);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    {
                        var cmd = this.History.Previous();
                        if (cmd != null)
                            this.CreateNewCommandAndPrint(true, cmd.Value);
                    }
                    break;
                case ConsoleKey.DownArrow:
                    {
                        var cmd = this.History.Next();
                        if (cmd != null)
                            if (this.History.IsMostRecent(cmd))
                            {
                                this.CurrentCommand.Interrupt();
                                cmd.CursorStartY = this.CurrentCommand.CursorStartY;
                                this.CurrentCommand = cmd;
                                this.CurrentCommand.Print();
                            }
                            else
                                this.CreateNewCommandAndPrint(true, cmd.Value);
                    }
                    break;
                case ConsoleKey.Enter:
                    {
                        //Save to history
                        var cmd = this.CurrentCommand;

                        string line = this.CurrentCommand.Value.Trim();

                        if (!string.IsNullOrEmpty(line))
                        {
                            this.History.Pop();
                            this.History.Register(cmd);
                            Console.WriteLine();
                            this.InputValidated?.Invoke(this, line);
                        }
                        else
                        {
                            this.History.Pop();
                            this.NewLine();
                        }
                    }
                    break;
            }
        }


        //protected void WriteAndResetCursor(string str)
        //{
        //    Console.Write(str);
        //    Console.CursorLeft = this.CursorLeft;
        //}

        private void CreateNewCommandAndPrint(bool replace = false, string cmd = null)
        {
            int top = Console.CursorTop;
            if (replace)
            {
                this.CurrentCommand.Interrupt();
                top = this.CurrentCommand.CursorStartY;
            }

            this.CurrentCommand = new CommandDetail(top, this.Prompt, cmd);
            this.CurrentCommand.Print();
        }

        public void NewLine(bool brk = true)
        {
            if (brk)
                Console.WriteLine();
            this.CreateNewCommandAndPrint();
            this.History.Register(this.CurrentCommand);
        }

        public void stop()
        {
            _token.Cancel();
        }


        public void Interrupt()
        {
            this.CurrentCommand.Interrupt();
        }

        public void Restore()
        {
            this.CurrentCommand.Reset(Console.CursorTop);
            this.CurrentCommand.Print();
        }




    }
}
