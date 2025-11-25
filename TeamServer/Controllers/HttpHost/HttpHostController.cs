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
        private IFileService _fileService;
        public HttpHostController(IListenerService listenerService, IFileService fileService)
        {
            this._listenerService = listenerService;
            this._fileService = fileService;
        }

        public IActionResult Index()
        {
            //var path = 
            return this.Content("Index", "text/html");
        }

        public IActionResult Dload(string id)
        {
            
            string fileName = Path.GetFileName(id);
            var host = this.Request.Host.Host;

            var listener = _listenerService.GetListeners().FirstOrDefault(l => l.Ip.ToLower() == host.ToLower());

            if (listener == null)
                return this.NotFound();

            var path = _fileService.GetListenerPath(listener.Name, fileName);

            if(!System.IO.File.Exists(path))
                return this.NotFound();

            return this.File(System.IO.File.ReadAllBytes(path), "application/octet-stream");

            //return this.Content("Hi  " + path, "text/html");
        }
    }
}
