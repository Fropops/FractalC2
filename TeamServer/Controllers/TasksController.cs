using Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TeamServer.Helper;
using TeamServer.Service;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly ITaskService _taskService;

        public TasksController(IAgentService agentService, ITaskService taskService)
        {
            this._agentService = agentService;
            this._taskService = taskService;
        }

        [HttpGet("{id}")]
        public ActionResult Result(string id)
        {
            TeamServerAgentTask task = this._taskService.Get(id);

            if (task != null)
                return Ok(task);
            else
                return NotFound();
        }

    }
}
