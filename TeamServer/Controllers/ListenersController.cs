using Common.APIModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ListenersController : ControllerBase
    {
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly ITaskResultService _resultService;
        private readonly IFileService _fileService;
        private readonly IBinMakerService _binMakerService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IWebHostService _webHostService;
        private readonly ICryptoService _cryptoService;
        private readonly IAuditService _auditService;
        private readonly IFrameService _frameService;
        private readonly IServerService _serverService;
        private readonly IReversePortForwardService _reversePortForwardService;

        public ListenersController(ILoggerFactory loggerFactory, IListenerService listenerService, IAgentService agentService, IFileService fileService, IBinMakerService binMakerService, IChangeTrackingService trackService,
            IWebHostService webHostService,
            ICryptoService cryptoService,
            IAuditService auditService,
            ITaskResultService resultService, IFrameService frameService,
            IServerService serverService,
            IReversePortForwardService reversePortForwardService)
        {
            this._listenerService = listenerService;
            _agentService=agentService;
            _fileService = fileService;
            _binMakerService = binMakerService;
            _loggerFactory = loggerFactory;
            _changeTrackingService = trackService;
            _webHostService = webHostService;
            _cryptoService = cryptoService;
            _auditService = auditService;
            _resultService = resultService;
            _frameService = frameService;
            _serverService = serverService;
            _reversePortForwardService=reversePortForwardService;
        }

        [HttpGet]
        public IActionResult GetListeners()
        {
            var listeners = _listenerService.GetListeners();
            return Ok(listeners);
        }

        [HttpGet("{id}")]
        public IActionResult GetListener(string id)
        {
            var listener = _listenerService.GetListener(id);
            if (listener == null)
                return NotFound();

            return Ok(listener);
        }

        [HttpPost]
        public IActionResult StartListener([FromBody] StartHttpListenerRequest request)
        {
            var listener = new HttpListener(request.Name, request.BindPort, request.Ip, request.Secured);
            var logger = _loggerFactory.CreateLogger($"Listener {request.Name} Start");
            _listenerService.AddListener(listener); // should be added before starting cause it is initialiezd there
            listener.Start();

            

            this._changeTrackingService.TrackChange(ChangingElement.Listener, listener.Id);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/{listener.Name}";

            return Created(path, listener);
        }

        [HttpDelete]
        public IActionResult StopListener(string id, bool? clean)
        {
            var listener = this._listenerService.GetListeners().FirstOrDefault(l => l.Id == id);
            if (listener == null)
                return NotFound();

            listener.Stop();
            _listenerService.RemoveListener(listener);

            this._changeTrackingService.TrackChange(ChangingElement.Listener, listener.Id);

            if (clean == true)
            {
                System.IO.Directory.Delete(_fileService.GetListenerPath(listener.Name),true);
            }

            return Ok();
        }
    }
}
