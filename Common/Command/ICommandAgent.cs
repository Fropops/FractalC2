using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Common.Command
{
    public interface ICommandAgent
    {
        AgentMetadata Metadata { get; }

        List<AgentTask> GetTasks();

        void Echo(string message);
        void Delay(int delayInSecond);
        void Shell(string cmd);
        void Powershell(string cmd);
        void Upload(byte[] fileBytes, string path);
        void Link(ConnexionUrl url);
        void PsExec(string target, string path);
        void RegistryAdd(string path, string key, string value);
        void RegistryRemove(string path, string key);
        void DeleteFile(string path);

        void AddParameter<T>(ParameterId id, T item);
        void AddParameter(ParameterId id, byte[] item);
    }
}
