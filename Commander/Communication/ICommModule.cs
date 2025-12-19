using Commander.Models;
using Common.APIModels;
using Common.APIModels.WebHost;
using Common.Models;
using Common.Payload;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Communication
{
    public interface ICommModule
    {
        event EventHandler<ConnectionStatus> ConnectionStatusChanged;
        event EventHandler<List<TeamServerAgentTask>> RunningTaskChanged;
        event EventHandler<AgentTaskResult> TaskResultUpdated;
        event EventHandler<Agent> AgentMetaDataUpdated;
        event EventHandler<Agent> AgentAdded;
        Task Start();
        void Stop();
        void UpdateConfig();

        ConnectionStatus ConnectionStatus { get; set; }

        CommanderConfig Config { get; }

        List<Agent> GetAgents();

        Agent GetAgent(int index);
        Agent GetAgent(string id);

        Task<HttpResponseMessage> StopAgent(string id);
        IEnumerable<TeamServerAgentTask> GetTasks(string id);

        TeamServerAgentTask GetTask(string taskId);
        AgentTaskResult GetTaskResult(string taskId);
        Task<HttpResponseMessage> CreateListener(string name, int port, string address, bool secured);
        Task<HttpResponseMessage> StopListener(string id);
        IEnumerable<TeamServerListener> GetListeners();
        Task TaskAgent(string label, string agentId, CommandId commandId, ParameterDictionary parms = null);

        Task WebHost(string path, byte[] fileContent, bool isPowerShell, string description);
        Task<List<WebHostLog>> GetWebHostLogs();
        Task<List<FileWebHost>> GetWebHosts();
        Task RemoveWebHost(string path);
        Task ClearWebHosts();


        Task<bool> StartProxy(string agentId, int port);
        Task<bool> StopProxy(string agentId);
        Task<List<ProxyInfo>> ShowProxy();

        Task CloseSession();

        // Implants
        List<Implant> GetImplants();
        Implant GetImplant(string id);
        Task<ImplantCreationResult> GenerateImplant(ImplantConfig config);
        Task<Implant> GetImplantBinary(string id);
        Task DeleteImplant(string id);

        // Loot
        Task<List<Loot>> GetLoot(string agentId);
        Task<Loot> GetLootFile(string agentId, string fileName);

        Task<bool> CreateLootAsync(string agentId, Loot loot);
       
        Task DeleteLoot(string agentId, string fileName);

        event EventHandler<Implant> ImplantAdded;
    }
}
