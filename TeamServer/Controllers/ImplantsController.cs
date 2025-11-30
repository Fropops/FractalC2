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
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ImplantsController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly ISocksService _socksService;
        private readonly IChangeTrackingService _changeService;
        private readonly IAuditService _auditService;
        private readonly ITaskResultService _agentTaskResultService;
        private readonly IFrameService _frameService;
        private readonly ITaskService _taskService;
        private readonly IImplantService _implantService;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IConfiguration _config;
        private readonly ICryptoService _cryptoService;

        public ImplantsController(IAgentService agentService, ISocksService socksService, IChangeTrackingService changeService, IAuditService auditService, ITaskResultService agentTaskResultService, IFrameService frameService, ITaskService taskService, IImplantService implantService, IChangeTrackingService changeTrackingService, IConfiguration config, ICryptoService cryptoService)
        {
            this._socksService = socksService;
            this._changeService = changeService;
            this._auditService = auditService;
            this._agentTaskResultService = agentTaskResultService;
            this._frameService = frameService;
            this._taskService= taskService;
            this._implantService = implantService;
            this._changeTrackingService = changeTrackingService;
            this._config = config;
            this._cryptoService = cryptoService;
        }

        [HttpGet]
        public IActionResult GetImplants()
        {
            var agents = _implantService.GetImplants();
            return Ok(agents);
        }

        [HttpGet("{implantId}")]
        public IActionResult GetImplant(string implantId, [FromQuery]bool  withData = true)
        {
            var implant = _implantService.GetImplant(implantId);
            if (implant == null)
                return NotFound();

            return Ok(new TeamServerImplant()
            {
                Id = implant.Id,
                Data = withData ? implant.Data : null,
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
            config.ImplantName = Payload.GenerateName();
            config.ServerKey = _cryptoService.ServerKey;
            string logs = string.Empty;
            try
            {
                (logs, var implant) = GenerateImplant(config);
                if (implant == null)
                {
                    return this.Problem(logs);
                }
                _implantService.AddImplant(implant);
                this._changeTrackingService.TrackChange(ChangingElement.Implant, implant.Id);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }

            return Ok(logs);
        }

        private (string, Implant) GenerateImplant(ImplantConfig config)
        {
             string logs = string.Empty;

            var generator = new PayloadGenerator(this._config.FoldersConfigs(), this._config.SpawnConfigs());
            generator.MessageSent += (sender, message) =>
            {
                logs += message.ToString() + Environment.NewLine;
            };
            var data = generator.GenerateImplant(config);
            return (logs, new Implant(ShortGuid.NewGuid())
            {
                Config = config,
                Data = Convert.ToBase64String(data),
            });
            
        }

       
    }
}
