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
using TeamServer.Models.Implant;
using Common.Payload;
using Common.Config;
using Common;
using System.Text;
using Newtonsoft.Json;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ImplantsController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IFileService _fileService;
        private readonly ISocksService _socksService;
        private readonly IChangeTrackingService _changeService;
        private readonly IAuditService _auditService;
        private readonly ITaskResultService _agentTaskResultService;
        private readonly IFrameService _frameService;
        private readonly ITaskService _taskService;
        private readonly IImplantService _implantService;
        private readonly IChangeTrackingService _changeTrackingService;

        public ImplantsController(IAgentService agentService, IFileService fileService, ISocksService socksService, IChangeTrackingService changeService, IAuditService auditService, ITaskResultService agentTaskResultService, IFrameService frameService, ITaskService taskService, IImplantService implantService, IChangeTrackingService changeTrackingService)
        {
            this._fileService = fileService;
            this._socksService = socksService;
            this._changeService = changeService;
            this._auditService = auditService;
            this._agentTaskResultService = agentTaskResultService;
            this._frameService = frameService;
            this._taskService= taskService;
            this._implantService = implantService;
            this._changeTrackingService = changeTrackingService;
        }

        [HttpGet]
        public IActionResult GetImplants()
        {
            var agents = _implantService.GetImplants();
            return Ok(agents);
        }

        [HttpGet("{implantId}")]
        public IActionResult GetImplant(string implantId)
        {
            var implant = _implantService.GetImplant(implantId);
            if (implant == null)
                return NotFound();

            return Ok(new TeamServerImplant()
            {
                Id = implant.Id,
                //Data = agent.FirstSeen,
               Config = implant.Config,
            });
        }

        [HttpDelete("{implantId}")]
        public ActionResult DeleteImplant(string implantId)
        {
            var implant = this._implantService.GetImplant(implantId);
            if (implant is null)
                return NotFound("Implant not found");

            this._implantService.RemoveImplant(implant);

            this._changeService.TrackChange(ChangingElement.Implant, implantId);

            return Ok();
        }

        [HttpPost]
        public IActionResult CreateImplant()
        {
            var body = this.Request.Body;
            string val = Encoding.UTF8.GetString(body.ReadStream().Result);
            var config = JsonConvert.DeserializeObject<ImplantConfig>(val);
            var implant = GenerateImplant(config);

            if(implant == null)
            {
                return this.Problem("Error Logs");
            }
            _implantService.AddImplant(implant);
            this._changeTrackingService.TrackChange(ChangingElement.Implant, implant.Id);

            return Ok("logs");
        }

        private Implant GenerateImplant(ImplantConfig config)
        {
            SpawnConfig spawnConfig = new SpawnConfig();
            PayloadConfig payloadConfig = new PayloadConfig();
            var generator = new PayloadGenerator(payloadConfig, spawnConfig);
            //var data = generator.GenerateImplant(config);
            return new Implant(ShortGuid.NewGuid())
            {
                Config = config
            };
            
        }

    }
}
