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
    public class ProxyController : ControllerBase
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

        public ProxyController(IAgentService agentService, ISocksService socksService, IChangeTrackingService changeService, IAuditService auditService, ITaskResultService agentTaskResultService, IFrameService frameService, ITaskService taskService, ITaskInterceptionService taskInterceptionService)
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

        
        [HttpGet("start")]
        public async Task<ActionResult> StartProxy(string agentId, int port)
        {
            if (this._socksService.Contains(agentId))
                return this.Problem($"Socks Proxy is already running for this agent !");

            if (!await this._socksService.StartProxy(agentId, port))
                return this.Problem($"Cannot start proxy on port {port}!");

            return Ok();
        }

        [HttpGet("stop")]
        public async Task<ActionResult> StopProxy(int port)
        {
            if (!await this._socksService.StopProxy(port))
                return this.Problem($"Cannot stop proxy!");

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> ShowProxy()
        {
            List<ProxyInfo> list = new List<ProxyInfo>();
            foreach (var pair in this._socksService.GetProxies())
            {
                list.Add(new ProxyInfo()
                {
                    AgentId = pair.Value.AgentId,
                    Port = pair.Value.BindPort
                });
            }

            return Ok(list);
        }

    }
}
