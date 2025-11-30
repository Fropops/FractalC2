using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Services;

namespace TeamServer.Controllers.HttpHost
{
    [Controller]
    public class HttpHostController : ControllerBase
    {
        private IListenerService _listenerService;
        public HttpHostController(IListenerService listenerService)
        {
            this._listenerService = listenerService;
        }

        public IActionResult Index()
        {
            //var path = 
            return this.Content("Index", "text/html");
        }
    }
}
