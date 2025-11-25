using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.APIModels.WebHost;
using TeamServer.Database;
using TeamServer.Service;

namespace TeamServer.Services;

public interface IWebHostService : IStorable
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
    private readonly IDatabaseService _dbService;

    private Dictionary<string, FileWebHost> files = new Dictionary<string, FileWebHost>();
    private List<WebHostLog> logs = new List<WebHostLog>();

    public WebHostService(IDatabaseService dbService)
    {
        this._dbService = dbService;
    }

    public void Add(string path, FileWebHost file)
    {
        if (!this.files.ContainsKey(path))
        {
            files.Add(path, file);
            this._dbService.Insert((WebHostFileDao)file).Wait();
        }
        else
        {
            files[path] = file;
            this._dbService.Update((WebHostFileDao)file).Wait();
        }
    }


    public void Remove(string path)
    {
        if (this.files.ContainsKey(path))
        {
            var file = this.files[path];
            this.files.Remove(path);
            this._dbService.Remove((WebHostFileDao)file).Wait();
        }

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
        this._dbService.Clear<WebHostFileDao>().Wait();
    }

    public List<WebHostLog> GetLogs()
    {
        return logs;
    }
    public void ClearLogs()
    {
        this.logs.Clear();
        this._dbService.Clear<WebHostLogDao>().Wait();
    }

    public void Addlog(WebHostLog log)
    {
        this.logs.Add(log);
        this._dbService.Insert((WebHostLogDao)log).Wait();
    }

    public async Task LoadFromDB()
    {
        this.files.Clear();
        this.logs.Clear();
        foreach(var dao in await _dbService.Load<WebHostFileDao>())
            this.files.Add(dao.Path, dao);

        foreach (var dao in await _dbService.Load<WebHostLogDao>())
            this.logs.Add(dao);
    }
}