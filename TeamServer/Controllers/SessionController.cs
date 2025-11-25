using Common.APIModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TeamServer.Helper;
using TeamServer.Service;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IAuditService _auditService;
        private readonly ITaskResultService _resultService;
        private readonly ITaskService _taskService;

        public SessionController(IChangeTrackingService trackService, IListenerService listenerService, IAgentService agentService, IAuditService auditService, ITaskResultService resultService, ITaskService taskService)
        {
            _changeTrackingService = trackService;
            _listenerService = listenerService;
            _agentService = agentService;
            _auditService = auditService;
            _resultService = resultService;
            _taskService = taskService;
        }

        [HttpGet("changes")]
        public IActionResult Changes(bool history = false)
        {
            var session = this.UserContext.Session;
            if (!this._changeTrackingService.ContainsSession(session))
            {
                this._auditService.Record(this.UserContext, $"Connexion to TeamServer");
            }

            if (!history)
                return Ok(this._changeTrackingService.ConsumeChanges(this.UserContext.Session).OrderBy(c => c.Element));
            else
            {
                List<Change> changes = new List<Change>();
                foreach (var listener in this._listenerService.GetListeners())
                    changes.Add(new Change(ChangingElement.Listener, listener.Id));
                foreach (var agent in this._agentService.GetAgents())
                {
                    changes.Add(new Change(ChangingElement.Agent, agent.Id));
                    changes.Add(new Change(ChangingElement.Metadata, agent.Id));
                    foreach (var task in this._taskService.GetForAgent(agent.Id))
                    {
                        changes.Add(new Change(ChangingElement.Task, task.Id));
                        var res = _resultService.GetAgentTaskResult(task.Id);
                        if (res != null)
                            changes.Add(new Change(ChangingElement.Result, res.Id));
                    }
                }

                return Ok(changes);
            }
        }

        [HttpGet("exit")]
        public IActionResult Exit()
        {
            var session = this.UserContext.Session;
            if (this._changeTrackingService.ContainsSession(session))
            {
                this._auditService.Record(this.UserContext, $"Exit TeamServer");
                return this.Problem();
            }
            this._changeTrackingService.CleanSession(session);
            return Ok();
        }

    }
}
