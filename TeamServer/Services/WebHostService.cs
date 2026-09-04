using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.APIModels.WebHost;
using TeamServer.Database;
using TeamServer.Service;

namespace TeamServer.Services;

[InjectableService]
public interface IWebHostService : IStorable
{
    Task AddAsync(string path, FileWebHost file);
    Task RemoveAsync(string path);
    byte[] GetFile(string path);

    FileWebHost Get(string path);

    List<FileWebHost> GetAll();

    Task ClearAsync();

    List<WebHostLog> GetLogs();
    Task ClearLogsAsync();

    Task AddlogAsync(WebHostLog log);

}

[InjectableServiceImplementation(typeof(IWebHostService))]
public class WebHostService : IWebHostService
{
    private readonly IDatabaseService _dbService;

    private Dictionary<string, FileWebHost> files = new Dictionary<string, FileWebHost>();
    private List<WebHostLog> logs = new List<WebHostLog>();

    public WebHostService(IDatabaseService dbService)
    {
        this._dbService = dbService;
    }

    public async Task AddAsync(string path, FileWebHost file)
    {
        if (!this.files.ContainsKey(path))
        {
            files.Add(path, file);
            await this._dbService.Insert((WebHostFileDao)file);
        }
        else
        {
            files[path] = file;
            await this._dbService.Update((WebHostFileDao)file);
        }
    }


    public async Task RemoveAsync(string path)
    {
        if (this.files.ContainsKey(path))
        {
            var file = this.files[path];
            this.files.Remove(path);
            await this._dbService.Remove((WebHostFileDao)file);
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

    public async Task ClearAsync()
    {
        this.files.Clear();
        await this._dbService.Clear<WebHostFileDao>();
    }

    public List<WebHostLog> GetLogs()
    {
        return logs;
    }
    public async Task ClearLogsAsync()
    {
        this.logs.Clear();
        await this._dbService.Clear<WebHostLogDao>();
    }

    public async Task AddlogAsync(WebHostLog log)
    {
        this.logs.Add(log);
        await this._dbService.Insert((WebHostLogDao)log);
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