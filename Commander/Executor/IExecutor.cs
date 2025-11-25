using Commander.Commands;
using Commander.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Executor
{
    public interface IExecutor
    {
        ExecutorMode Mode { get; set; }
        Agent CurrentAgent { get; set; }
        void InputHandled(ExecutorCommand cmd, bool cmdResult);

        IEnumerable<ExecutorCommand> GetCommandsInMode(ExecutorMode mode);

        ExecutorCommand GetCommandInMode(ExecutorMode mode, string commandName);

        void Start();
        void Stop();
    }
}
