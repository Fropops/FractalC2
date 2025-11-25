using Agent.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public interface IFileService
    {
        bool IsDownloadComplete(string fileId);
        int GetDownloadPercent(string fileId);

        File ConsumeDownloadedFile(string fileId);
        bool IsUploadComplete(string fileId);
        int GetUploadPercent(string fileId);
        void AddFileChunck(FileChunk chunk);
        void AddFileToUpload(string id, string fileName, byte[] data);
        FileChunk GetChunkToSend();
    }
    public class FileService : IFileService
    {
        object _lockObjUpl = new object();
        object _lockObjDld = new object();

        public static int ChunkSize = 500000;
        private List<File> FilesToUpload { get; set; } = new List<File>();

        private List<File> FilesToDownload { get; set; } = new List<File>();

        public bool IsDownloadComplete(string fileId)
        {
            lock (_lockObjDld)
            {
                var file = this.FilesToDownload.FirstOrDefault(f => f.Id == fileId);
                return file != null && file.IsComplete;
            }
        }

        public int GetDownloadPercent(string fileId)
        {
            lock (_lockObjDld)
            {
                var file = this.FilesToDownload.FirstOrDefault(f => f.Id == fileId);
                if (file == null)
                    return 0;

                if (file.IsComplete)
                    return 100;

                return file.Chunks.Count * 100 / file.ChunckCount;
            }
        }

        public File ConsumeDownloadedFile(string fileId)
        {
            lock (_lockObjDld)
            {
                var file = this.FilesToDownload.FirstOrDefault(f => f.Id == fileId);
                this.FilesToDownload.Remove(file);
                return file;
            }
        }


        public bool IsUploadComplete(string fileId)
        {
            lock (_lockObjUpl)
            {
                var file = this.FilesToUpload.FirstOrDefault(f => f.Id == fileId);
                return file == null;
            }
        }

        public int GetUploadPercent(string fileId)
        {
            lock (_lockObjUpl)
            {
                var file = this.FilesToUpload.FirstOrDefault(f => f.Id == fileId);
                if (file == null)
                    return 100;

                if (file.IsComplete)
                    return 0;

                return (file.ChunckCount - file.Chunks.Count) * 100 / file.ChunckCount;
            }
        }



        public void AddFileChunck(FileChunk chunk)
        {
            lock (_lockObjDld)
            {
                if (chunk == null)
                    return;

                File file = null;
                if (!this.FilesToDownload.Any(f => f.Id == chunk.FileId))
                {
                    file = new File()
                    {
                        Id = chunk.FileId,
                        Name = chunk.FileName,
                    };
                    this.FilesToDownload.Add(file);
                }
                else
                {
                    file = this.FilesToDownload.First(f => f.Id == chunk.FileId);
                }

                file.Chunks.Add(chunk);
            }
        }

        public void AddFileToUpload(string id, string fileName, byte[] data)
        {
            lock (_lockObjUpl)
            {
                var file = new File();
                file.Name = fileName;
                file.Id = id;

                int index = 0;
                using (var ms = new MemoryStream(data))
                {

                    var buffer = new byte[ChunkSize];
                    int numBytesToRead = (int)ms.Length;

                    while (numBytesToRead > 0)
                    {

                        int n = ms.Read(buffer, 0, ChunkSize);
                        //var data =
                        var chunk = new FileChunk()
                        {
                            FileId = id,
                            FileName = fileName,
                            Data = System.Convert.ToBase64String(buffer.Take(n).ToArray()),
                            Index = index,
                        };
                        file.Chunks.Add(chunk);
                        numBytesToRead -= n;

                        index++;
                    }

                    foreach (var chunk in file.Chunks)
                        chunk.Count = file.Chunks.Count;
                }

                this.FilesToUpload.Add(file);
            }
        }

        public FileChunk GetChunkToSend()
        {
            lock (_lockObjUpl)
            {
                var file = FilesToUpload.FirstOrDefault();
                if (file == null)
                    return null;

                var chunk = file.Chunks.FirstOrDefault();
                file.Chunks.Remove(chunk);
                if (file.Chunks.Count == 0)
                    this.FilesToUpload.Remove(file);

                return chunk;
            }
        }
    }

    public class File
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<FileChunk> Chunks { get; set; } = new List<FileChunk>();

        public int ChunckCount
        {
            get
            {
                if (!Chunks.Any())
                    return 0;

                var chunk = Chunks.First();
                return chunk.Count;
            }
        }

        public bool IsComplete
        {
            get
            {
                if (!Chunks.Any())
                    return false;

                var chunk = Chunks.First();
                if (Chunks.Count == chunk.Count)
                    return true;

                return false;
            }
        }

        public byte[] GetFileContent()
        {
            if (!this.IsComplete)
                return null;

            byte[] fileBytes = null;
            using (var ms = new MemoryStream())
            {
                foreach (var chunk in this.Chunks.OrderBy(c => c.Index))
                {
                    var bytes = Convert.FromBase64String(chunk.Data);
                    ms.Write(bytes, 0, bytes.Length);
                }

                fileBytes =  ms.ToArray();
            }

            return fileBytes;
        }
    }
}
