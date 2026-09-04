using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Spectre.Console;

namespace Commander.Terminal
{



    public partial class Terminal : ITerminal
    {
        public const string DefaultPrompt = "$> ";

        public event EventHandler<string> InputValidated;

        CancellationTokenSource _token = new CancellationTokenSource();
        private readonly Channel<ConsoleKeyInfo> _inputChannel = Channel.CreateUnbounded<ConsoleKeyInfo>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
        private readonly Channel<bool> _resizeChannel = Channel.CreateUnbounded<bool>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

        private int _lastWindowWidth;
        private int _lastWindowHeight;
        private readonly object _layoutLock = new object();

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

            this.InitializeResizeTracking();

            if (OperatingSystem.IsWindows())
            {
                // Windows : thread dédié + Channel => CPU nulle en idle
                var inputThread = new Thread(this.ReadKeysLoop)
                {
                    IsBackground = true,
                    Name = "TerminalInputReader"
                };
                inputThread.Start();
            }
            else
            {
                // Linux/Unix : Console.ReadKey sur un thread dédié pose souvent problème (termios/raw mode).
                // On reste en mode poll avec un délai allongé pour limiter la conso CPU.
                _ = Task.Run(this.PollKeysLoopAsync, _token.Token);
            }

            // Surveillance périodique du redimensionnement de la console
            _ = Task.Run(this.MonitorResizeAsync, _token.Token);

            await this.ProcessInputLoopAsync();
        }

        private void InitializeResizeTracking()
        {
            _lastWindowWidth = Console.WindowWidth;
            _lastWindowHeight = Console.WindowHeight;
        }

        private void ReadKeysLoop()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    // Drainer d'abord les touches déjà en buffer (par ex. copier-coller)
                    while (Console.KeyAvailable && !_token.IsCancellationRequested)
                    {
                        var key = Console.ReadKey(true);
                        _inputChannel.Writer.TryWrite(key);
                    }

                    // Puis attendre la prochaine touche de façon bloquante (CPU nulle en idle)
                    var nextKey = Console.ReadKey(true);
                    _inputChannel.Writer.TryWrite(nextKey);
                }
                catch (InvalidOperationException)
                {
                    // Canal fermé : arrêt du terminal
                    break;
                }
                catch (Exception)
                {
                    // Console inaccessible (redirection, etc.) : éviter la boucle infinie bruyante
                    break;
                }
            }
        }

        private async Task PollKeysLoopAsync()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    // Vider le buffer d'entrée en une seule passe pour accélérer le copier-coller
                    while (Console.KeyAvailable && !_token.IsCancellationRequested)
                    {
                        var key = Console.ReadKey(true);
                        _inputChannel.Writer.TryWrite(key);
                    }
                    await Task.Delay(50, _token.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Console inaccessible : arrêter la boucle
                    break;
                }
            }
        }

        private async Task MonitorResizeAsync()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(100, _token.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                int currentWidth = Console.WindowWidth;
                int currentHeight = Console.WindowHeight;

                lock (_layoutLock)
                {
                    if (currentWidth != _lastWindowWidth || currentHeight != _lastWindowHeight)
                    {
                        _lastWindowWidth = currentWidth;
                        _lastWindowHeight = currentHeight;
                        _resizeChannel.Writer.TryWrite(true);
                    }
                }
            }
        }

        private async Task ProcessInputLoopAsync()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    // Attendre une touche OU un redimensionnement, sans consommer de CPU
                    var inputWait = _inputChannel.Reader.WaitToReadAsync(_token.Token).AsTask();
                    var resizeWait = _resizeChannel.Reader.WaitToReadAsync(_token.Token).AsTask();
                    await Task.WhenAny(inputWait, resizeWait);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // Traiter d'abord les redimensionnements en attente
                if (_resizeChannel.Reader.TryRead(out _))
                {
                    lock (_layoutLock)
                    {
                        while (_resizeChannel.Reader.TryRead(out _)) { }
                        try
                        {
                            this.CurrentCommand?.RefreshLayout();
                        }
                        catch (Exception e)
                        {
                            this.WriteLine();
                            this.WriteError("Terminal Resize Error :");
                            this.WriteError("----------------------");
                            this.WriteError(e.ToString());
                        }
                    }
                }

                // Puis traiter les touches en attente
                if (_inputChannel.Reader.TryRead(out var key))
                {
                    this.HandleKeyWithErrorHandling(key);
                }
            }
        }

        private void HandleKeyWithErrorHandling(ConsoleKeyInfo key)
        {
            try
            {
                lock (_layoutLock)
                {
                    this.HandleKey(key);
                }
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
