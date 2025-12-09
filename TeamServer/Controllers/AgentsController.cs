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
        private readonly ISocksService _socksService;
        private readonly IChangeTrackingService _changeService;
        private readonly IAuditService _auditService;
        private readonly ITaskResultService _agentTaskResultService;
        private readonly IFrameService _frameService;
        private readonly ITaskService _taskService;
        private readonly ITaskInterceptionService _taskInterceptionService;

        public AgentsController(IAgentService agentService, ISocksService socksService, IChangeTrackingService changeService, IAuditService auditService, ITaskResultService agentTaskResultService, IFrameService frameService, ITaskService taskService, ITaskInterceptionService taskInterceptionService)
        {
            this._agentService = agentService;
            this._socksService = socksService;
            this._changeService = changeService;
            this._auditService = auditService;
            this._agentTaskResultService = agentTaskResultService;
            this._frameService = frameService;
            this._taskService= taskService;
            this._taskInterceptionService = taskInterceptionService;
        }

        [HttpGet]
        public IActionResult GetAgents()
        {
            var agents = _agentService.GetAgents();
            return Ok(agents);
        }

        [HttpGet("{agentId}")]
        public IActionResult GetAgent(string agentId)
        {
            var agent = _agentService.GetAgent(agentId);
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

        [HttpGet("{agentId}/metadata")]
        public IActionResult GetAgentMetadata(string agentId)
        {
            var agent = _agentService.GetAgent(agentId);
            if (agent == null)
                return NotFound();

            return Ok(agent.Metadata);
        }


      

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


            var interceptionResult = this._taskInterceptionService.Intercept(task, agent);
            if (!interceptionResult.Success)
                return Problem(interceptionResult.Error);
          

            this._frameService.CacheFrame(agentId, NetFrameType.Task, task);



            var teamTask = new TeamServerAgentTask(ctr.Id, task.CommandId, agentId, ctr.Command, DateTime.Now);
            this._taskService.Add(teamTask);
            this._changeService.TrackChange(ChangingElement.Task, task.Id);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/tasks/{task.Id}";

            this._auditService.Record(this.UserContext, agentId, $"Command tasked to agent : {task.CommandId.ToString()}");

            return Created(path, task);
        }

      

        [HttpDelete("{agentId}")]
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
    }
}
