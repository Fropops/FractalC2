using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Helper;
using TeamServer.Models;
using TeamServer.Models.File;
using TeamServer.Service;
using TeamServer.Services;
namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DownloadFileController : ControllerBase
    {
        private readonly IDownloadFileService _fileService;

        public DownloadFileController(IDownloadFileService fileService)
        {
            _fileService = fileService;

        }

        [HttpGet]
        public IActionResult Index()
        {
            var files = _fileService.GetAll().Select(c => new TeamServerDownloadFile()
            {
                Id = c.Id,
                FileName = c.FileName,
                Path = c.Path,
                Source = c.Source
            });
            return Ok(files);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var f = _fileService.Get(id);
            if (f == null)
                return NotFound();

            var file = new TeamServerDownloadFile()
            {
                Id = f.Id,
                FileName = f.FileName,
                Path = f.Path,
                Source = f.Source,
                Data = Convert.ToBase64String(f.Data)
            };

            return Ok(file);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var f = _fileService.Get(id);
            if (f == null)
                return NotFound();

            _fileService.Remove(id);
            return Ok();
        }
    }
}
