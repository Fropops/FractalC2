using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Commands
{



    public abstract class EnhancedCommand<T> : ExecutorCommand
    {
        private RootCommand _command;

        private bool _isCommandCorrectlyExecuting = false;

        private string currentLabel = string.Empty;
        private string currentParams = string.Empty;
        public abstract RootCommand Command { get; }

        public override void Execute(string parms)
        {
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();
            var label = this.Name;
            if (!string.IsNullOrEmpty(parms))
                label += " " + parms;

            CommandContext<T> context = new CommandContext<T>()
            {
                Executor = executor,
                Terminal = terminal,
                CommModule = comm,
                CommandLabel = label,
                CommandParameters = parms,
                Config = comm.Config,
            };

            InnerExecute(context);
        }

        protected async override void InnerExecute(CommandContext context)
        {
            if (this._command == null)
            {
                this._command = this.Command;
                this._command.Name = this.Name;
                this._command.Handler = CommandHandler.Create<T>(HandleCommandWrapper);
            }

            this.currentLabel = context.CommandLabel;
            this.currentParams = context.CommandParameters;
            _isCommandCorrectlyExecuting = false;

            var res = _command.Invoke(this.currentParams);
            if (res > 0)
            {
                context.Executor.InputHandled(this, false);
                return;
            }


            await Task.Delay(500);
            //Debug.WriteLine($"Awaited 1s => isCommandCorrectlyExecuting = {this._isCommandCorrectlyExecuting}");
            if (!_isCommandCorrectlyExecuting) //prevent blocking when Handler is not called
                context.Executor.InputHandled(this, false);

        }

        private async void HandleCommandWrapper(T options)
        {
            this._isCommandCorrectlyExecuting = true;
            var executor = ServiceProvider.GetService<IExecutor>();
            var terminal = ServiceProvider.GetService<ITerminal>();
            var comm = ServiceProvider.GetService<ICommModule>();

            CommandContext<T> context = new CommandContext<T>()
            {
                Executor = executor,
                Terminal = terminal,
                CommModule = comm,
                CommandLabel = this.currentLabel,
                CommandParameters = this.currentParams,
                Options = options,
                Config = comm.Config,

            };


            bool result = false;

            try
            {
                result = await this.HandleCommand(context);
            }
            catch (Exception ex)
            {
                terminal.WriteError(ex.ToString());
            }
            finally
            {
                executor.InputHandled(this, result);
            }
        }

        protected abstract Task<bool> HandleCommand(CommandContext<T> context);
    
    }

    public class EmptyCommandOptions
    {
    }

    public abstract class EnhancedCommand : EnhancedCommand<EmptyCommandOptions>
    {
    }

    



}
