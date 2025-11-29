using Common.APIModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Models.File;
using TeamServer.Services;
namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ToolsController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly IToolsService _toolsService;
        private readonly IAuditService _auditService;
        public ToolsController(IToolsService toolsService, IAuditService auditService)
        {
           this._toolsService = toolsService;
            this._auditService = auditService;
        }

        [HttpPost]
        public IActionResult AddTool([FromBody] Tool tool)
        {
            try
            {
                if (this._toolsService.AddTool(tool))
                {
                    this._auditService.Record(this.UserContext, $"Adding Tool {tool.Name} - {tool.Type.ToString()}");

                    return Ok();
                }
                return Problem("Unable to Add Tool");
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet]
        public IActionResult Tools([FromQuery]ToolType? type = null, [FromQuery]string name = null)
        {
            try
            {
                return Ok(_toolsService.GetTools(type, name));
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }

        [HttpGet("{name}")]
        public IActionResult Tool(string name)
        {
            try
            {
                return Ok(_toolsService.GetTool(name));
            }
            catch (Exception ex)
            {
                return this.Problem(ex.ToString());
            }
        }


    }


}
