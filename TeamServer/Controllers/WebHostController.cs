using Common.APIModels.WebHost;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TeamServer.Helper;
using TeamServer.Services;
namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WebHostController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IFileService _fileService;
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IWebHostService _webHostService;
        private readonly IAuditService _auditService;
        public WebHostController(IFileService fileService, IListenerService listenerService, IAgentService agentService, IWebHostService webHostService, IAuditService auditService)
        {
            _fileService = fileService;
            _listenerService = listenerService;
            _agentService = agentService;
            _webHostService = webHostService;
            _auditService = auditService;
        }

        [HttpPost]
        public IActionResult WebHost([FromBody] FileWebHost wb)
        {
            try
            {
                //var outPath = this._fileService.GetWebHostPath(wb.FileName);
                //System.IO.File.WriteAllBytes(outPath, wb.Data);
                //Logger.Log($"WebHost push {wb.Path}");

                this._webHostService.Add(wb.Path, wb);
                this._auditService.Record(this.UserContext, $"Web hosting {wb.Path} - {wb.Description}");
                return Ok();
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet]
        public IActionResult WebHosts()
        {
            try
            {
                List<FileWebHost> fileWebHosts = new List<FileWebHost>();   
                foreach(var ex in this._webHostService.GetAll())
                {
                    fileWebHosts.Add(new FileWebHost()
                    {
                        Description = ex.Description,
                        Path = ex.Path,
                        IsPowershell = ex.IsPowershell,
                    });
                }

                return Ok(fileWebHosts);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet("Logs")]
        public IActionResult WebHostLogs()
        {
            try
            {
                return Ok(this._webHostService.GetLogs());
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpPost("Remove")]
        public IActionResult Remove([FromBody] FileWebHost wb)
        {
            try
            {
                this._webHostService.Remove(wb.Path);
                this._auditService.Record(this.UserContext, $"Web hosted {wb.Path} removed.");
                return Ok();
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet("Clear")]
        public IActionResult Clear()
        {
            try
            {
                this._auditService.Record(this.UserContext, $"Web host cleard.");
                this._webHostService.Clear();
                return Ok();
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }
    }
}
