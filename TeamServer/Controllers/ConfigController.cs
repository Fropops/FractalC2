using Common.Models;
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
    public class ConfigController : ControllerBase
    {
        private UserContext UserContext => this.HttpContext.Items["User"] as UserContext;

        private readonly ICryptoService _cryptoService;

        public ConfigController(ICryptoService cryptoService)
        {
            this._cryptoService = cryptoService;
        }

        [HttpGet]
        public IActionResult Config()
        {
            var config = new ServerConfig()
            {
                Key = Convert.ToBase64String(_cryptoService.Key)
            };
            return Ok(config);
        }


    }
}
