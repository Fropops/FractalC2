using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Common.Payload;
using Shared;

namespace Common.Command
{
    public interface ICommandCommander
    {

        void WriteError(string message);
        void WriteSuccess(string message);

        void WriteLine(string message);
        void WriteInfo(string message);
        Implant GeneratePayload(ImplantConfig options);

        void CallEndPointCommand(string commandName, CommandId commandId);
    }
}
