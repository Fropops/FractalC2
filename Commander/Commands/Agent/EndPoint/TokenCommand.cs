using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commander.Executor;
using Shared;
using System.Security.Cryptography;
using BinarySerializer;
using System.CommandLine;

namespace Commander.Commands.Agent.EndPoint;

public class StealTokenCommandOptions
{
    public int pid { get; set; }
}
public class StealTokenCommand : EndPointCommand<StealTokenCommandOptions>
{
    public override string Description => "Steal the token from a process";
    public override string Name => "steal-token";

    public override CommandId CommandId => CommandId.StealToken;

    public override RootCommand Command => new RootCommand(this.Description)
            {
                new Argument<int>(){Name = "pid", Description = "Id of the process" },
            };

    protected override void SpecifyParameters(CommandContext<StealTokenCommandOptions> context)
    {
        context.AddParameter(ParameterId.Id, context.Options.pid);
    }
}

public class MakeTokenCommandOptions
{
    public string username { get; set; }
    public string password { get; set; }
}
public class MakeTokenCommand : EndPointCommand<MakeTokenCommandOptions>
{
    public override string Description => "Make token for a specified user";
    public override string Name => "make-token";

    public override CommandId CommandId => CommandId.MakeToken;

    public override RootCommand Command => new RootCommand(this.Description)
            {

                new Option<string>("username", "Full username (DOMAIN\\User)"),
                new Argument<string>("password", "Password of the account"),
            };

    protected override async Task<bool> CheckParams(CommandContext<MakeTokenCommandOptions> context)
    {
        if(!context.Options.username.Contains('\\'))
        {
            context.Terminal.WriteError($"Username is not in a correct format.");
            return false;
        }
        return await base.CheckParams(context);
    }

    protected override void SpecifyParameters(CommandContext<MakeTokenCommandOptions> context)
    {
        var split = context.Options.username.Split('\\');
        var domain = split[0];
        var username = split[1];
        context.AddParameter(ParameterId.User, username);
        context.AddParameter(ParameterId.Domain, domain);
        context.AddParameter(ParameterId.Password, context.Options.password);
    }
}


public class Rev2SelfCommand : SimpleEndPointCommand
{
    public override string Description => "Remove Token Impersonation";
    public override string Name => "revert-self";
    public override CommandId CommandId => CommandId.RevertSelf;
}
