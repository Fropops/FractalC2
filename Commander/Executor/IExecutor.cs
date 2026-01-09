using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Commands;
using Commander.Models;
using Common.CommandLine.Execution;

namespace Commander.Executor
{
    public interface IExecutor
    {
        Agent CurrentAgent { get; set; }
        void InputHandled(bool cmdResult);

        List<CommandDefinition> GetAllCommands();

        void Start();
        void Stop();
    }
}
