using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace Commander
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceProvider.Instance)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                var c = new CommanderConfig(config);

                var terminal = new Terminal.Terminal();
                ServiceProvider.RegisterSingleton<ITerminal>(terminal);
                var apiCommModule = new ApiCommModule(terminal, c);
                ServiceProvider.RegisterSingleton<ICommModule>(apiCommModule);
                var exec = new Executor.Executor(terminal, apiCommModule);
                ServiceProvider.RegisterSingleton<IExecutor>(exec);

                var exitEvent = new ManualResetEventSlim(false);
                exec.Stopped += () => exitEvent.Set();

                exec.Start();
                exitEvent.Wait();
            }
        }
    }
}
