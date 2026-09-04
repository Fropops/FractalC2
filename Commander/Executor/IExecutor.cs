using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands;

using Common.CommandLine.Execution;

namespace Commander.Executor
{
    public interface IExecutor : IDisposable
    {
        public Common.Models.Agent CurrentAgent { get; set; }

        event Action Stopped;

        void InputHandled(bool cmdResult);

        List<CommandDefinition> GetAllCommands();

        void Start();
        void Stop();
    }
}
