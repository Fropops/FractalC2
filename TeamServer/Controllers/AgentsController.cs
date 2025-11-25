using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Services;
using BinarySerializer;
using Shared;
using Common.APIModels;
using Common.Models;
using TeamServer.Service;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AgentsController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IAgentService _agentService;
        private readonly IFileService _fileService;
        private readonly ISocksService _socksService;
        private readonly IChangeTrackingService _changeService;
        private readonly IAuditService _auditService;
        private readonly ITaskResultService _agentTaskResultService;
        private readonly IFrameService _frameService;
        private readonly ITaskService _taskService;

        public AgentsController(IAgentService agentService, IFileService fileService, ISocksService socksService, IChangeTrackingService changeService, IAuditService auditService, ITaskResultService agentTaskResultService, IFrameService frameService, ITaskService taskService)
        {
            this._agentService = agentService;
            this._fileService = fileService;
            this._socksService = socksService;
            this._changeService = changeService;
            this._auditService = auditService;
            this._agentTaskResultService = agentTaskResultService;
            this._frameService = frameService;
            this._taskService= taskService;
        }

        [HttpGet]
        public IActionResult GetAgents()
        {
            var agents = _agentService.GetAgents();
            return Ok(agents);
        }

        [HttpGet("{id}")]
        public IActionResult GetAgent(string id)
        {
            var agent = _agentService.GetAgent(id);
            if (agent == null)
                return NotFound();

            return Ok(new TeamServerAgent()
            {
                Id = agent.Id,
                FirstSeen = agent.FirstSeen,
                LastSeen = agent.LastSeen,
                RelayId = agent.RelayId,
                Links = agent.Links.Values.Select(c => c.ChildId).ToList(),
            });
        }

        [HttpGet("{id}/metadata")]
        public IActionResult GetAgentMetadata(string id)
        {
            var agent = _agentService.GetAgent(id);
            if (agent == null)
                return NotFound();

            return Ok(agent.Metadata);
        }


        //[HttpGet("{agentId}/tasks")]
        //public ActionResult GetTaskresults(string agentId, DateTime? from)
        //{
        //    var agent = this._agentService.GetAgent(agentId);
        //    if (agent is null)
        //        return NotFound("Agent not found");


        //    var results = agent.GetTaskResults();

        //    //if (from.HasValue)
        //    //    results = results.Where(r => r.r)

        //    return Ok(results);
        //}

        [HttpGet("{agentId}/tasks/{taskId}")]
        public ActionResult GetTaskresult(string agentId, string taskId)
        {
            var result = this._agentTaskResultService.GetAgentTaskResult(taskId);
            if (result is null)
                return NotFound("Task not found");

            return Ok(result);
        }

        [HttpPost("{agentId}")]
        public ActionResult TaskAgent(string agentId, [FromBody] CreateTaskRequest ctr)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound();

            byte[] ser = Convert.FromBase64String(ctr.TaskBin);
            var task = ser.BinaryDeserializeAsync<AgentTask>().Result;

            this._frameService.CacheFrame(agentId, NetFrameType.Task, task);
            var teamTask = new TeamServerAgentTask(ctr.Id, task.CommandId, agentId, ctr.Command, DateTime.Now);
            this._taskService.Add(teamTask);
            this._changeService.TrackChange(ChangingElement.Task, task.Id);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/tasks/{task.Id}";

            this._auditService.Record(this.UserContext, agentId, $"Command tasked to agent : {task.CommandId.ToString()}");

            return Created(path, task);
        }

        //[HttpGet("{agentId}/File")]
        //public ActionResult RequestAgentDowload(string agentId, string fileId)
        //{
        //    var agent = this._agentService.GetAgent(agentId);
        //    if (agent is null)
        //        return NotFound();

        //    agent.QueueDownload(this._fileService.GetFileChunksForAgent(fileId));

        //    return Ok();
        //}

        [HttpGet("{agentId}/stop")]
        public ActionResult StopAgent(string agentId)
        {
            var agent = this._agentService.GetAgent(agentId);
            if (agent is null)
                return NotFound("Agent not found");

            this._agentService.RemoveAgent(agent);

            var tasks = this._taskService.GetForAgent(agent.Id);
            foreach(var task in _taskService.RemoveAgent(agent.Id))
            {
                var res = _agentTaskResultService.GetAgentTaskResult(task.Id);
                if (res != null)
                    _agentTaskResultService.Remove(res);
            }

            this._changeService.TrackChange(ChangingElement.Agent, agentId);

            return Ok();
        }

        [HttpGet("{agentId}/startproxy")]
        public async Task<ActionResult> StartProxy(string agentId, int port)
        {
            if (this._socksService.Contains(agentId))
                return this.Problem($"Socks Proxy is already running for this agent !");

            if (!await this._socksService.StartProxy(agentId, port))
                return this.Problem($"Cannot start proxy on port {port}!");

            return Ok();
        }

        [HttpGet("{agentId}/stopproxy")]
        public async Task<ActionResult> StopProxy(string agentId)
        {
            if (!await this._socksService.StopProxy(agentId))
                return this.Problem($"Cannot stop proxy!");

            return Ok();
        }

        [HttpGet("proxy")]
        public async Task<ActionResult> ShowProxy()
        {
            List<ProxyInfo> list = new List<ProxyInfo>();
            foreach (var pair in this._socksService.GetProxies())
            {
                list.Add(new ProxyInfo()
                {
                    AgentId = pair.Key,
                    Port = pair.Value.BindPort
                });
            }

            return Ok(list);
        }

    }
}
