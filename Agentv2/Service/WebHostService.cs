using Agent.Communication;
using Agent.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.IO;

namespace Agent.Service
{
    public class FileWebHost
    {
        public string Path { get; set; }
        public string Description { get; set; }

        public bool IsPowershell { get; set; }
        public byte[] Data { get; set; }
    }

    public class WebHostLog
    {
        public DateTime Date { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }

        public string UserAgent { get; set; }
        public int StatusCode { get; set; }
    }
    public interface IWebHostService
    {
        void Add(string path, FileWebHost file);
        void Remove(string path);
        byte[] GetFile(string path);

        FileWebHost Get(string path);

        List<FileWebHost> GetAll();

        void Clear();

        List<WebHostLog> GetLogs();
        void ClearLogs();

        void Addlog(WebHostLog log);

    }

    public class WebHostService : IWebHostService
    {
        private Dictionary<string, FileWebHost> files = new Dictionary<string, FileWebHost>();
        private List<WebHostLog> logs = new List<WebHostLog>();

        public void Add(string path, FileWebHost file)
        {
            if (!this.files.ContainsKey(path))
                files.Add(path, file);
            else
                files[path] = file;
        }


        public void Remove(string path)
        {
            if (this.files.ContainsKey(path))
                this.files.Remove(path);
        }

        public byte[] GetFile(string path)
        {
            if (this.files.ContainsKey(path))
                return this.files[path].Data;
            return null;
        }

        public FileWebHost Get(string path)
        {
            if (this.files.ContainsKey(path))
                return this.files[path];
            return null;
        }

        public List<FileWebHost> GetAll()
        {
            return this.files.Values.ToList();
        }

        public void Clear()
        {
            this.files.Clear();
        }

        public List<WebHostLog> GetLogs()
        {
            return logs;
        }
        public void ClearLogs()
        {
            this.logs.Clear();
        }

        public void Addlog(WebHostLog log)
        {
            this.logs.Add(log);
        }

    }
}
