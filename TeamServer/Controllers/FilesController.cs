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
using TeamServer.Services;
namespace TeamServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IListenerService _listenerService;
        private readonly IAgentService _agentService;
        private readonly IWebHostService _webHostService;
        public FilesController(IFileService fileService, IListenerService listenerService, IAgentService agentService, IWebHostService webHostService)
        {
            _fileService = fileService;
            _listenerService = listenerService;
            _agentService = agentService;
            _webHostService = webHostService;

        }



        //[HttpGet("SetupDownload/{id}")]
        //public IActionResult GetFileDescriptor(string id)
        //{
        //    var desc = _fileService.GetFile(id);
        //    if (desc == null)
        //        return NotFound();


        //    foreach (var agent in _agentService.GetAgents())
        //    {
        //        foreach (var res in agent.GetTaskResults())
        //        {
        //            foreach (var file in res.Files)
        //            {
        //                if (file.FileId == id)
        //                {
        //                    file.IsDownloaded = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }


        //    return Ok(new FileDescriptorResponse()
        //    {
        //        Id = desc.Id,
        //        Name = desc.Name,
        //        Length = desc.Length,
        //        ChunkCount = desc.ChunkCount,
        //        ChunkSize = desc.ChunkSize,
        //    });
        //}

        //[HttpGet("Download/{id}/{chunkIndex}")]
        //public IActionResult GetFileChunk(string id, int chunkIndex)
        //{
        //    var desc = _fileService.GetFile(id);
        //    if (chunkIndex < 0 || chunkIndex >= desc.ChunkCount)
        //        return NotFound();

        //    var chunck = desc.Chunks[chunkIndex];
        //    chunck.IsDownloaded = true;

        //    _fileService.CleanDownloaded();

        //    //Logger.Log($"File {desc.Name} => Downloading chunck #{chunck.Index} with length of {chunck.Data.Length}");

        //    //if (desc.IsDownloaded)
        //    //    Logger.Log($"File {desc.Name} uploaded with {desc.ChunkCount} chunks");

        //    return Ok(new FileChunckResponse()
        //    {
        //        Data = chunck.Data,
        //        FileId = chunck.FileId,
        //        Index = chunck.Index,
        //    });
        //}

        //[HttpPost("SetupUpload")]
        //public IActionResult PostFileDescriptor([FromBody] FileDescriptor desc)
        //{
        //    if (string.IsNullOrEmpty(desc.Id))
        //        return NotFound();

        //    _fileService.AddFile(desc);
        //    return Ok();
        //}

        //[HttpPost("Upload")]
        //public IActionResult PostFileChunk([FromBody] FileChunk chunk)
        //{
        //    try
        //    {
        //        var desc = _fileService.GetFile(chunk.FileId);
        //        if (desc == null)
        //            return NotFound();

        //        //Logger.Log($"File {desc.Name} => uploading chunck #{chunk.Index} with length of {chunk.Data.Length}");

        //        desc.Chunks.Add(chunk);
        //        //if(desc.IsUploaded)
        //        //    Logger.Log($"File {desc.Name} uploaded with {desc.ChunkCount} chunks");

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.Problem(ex.ToString());
        //    }
        //}
    }
}
