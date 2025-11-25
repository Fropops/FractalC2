using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using TeamServer.Database;

namespace TeamServer.Service;

public interface IDownloadFileService: IStorable
{
    void Add(DownloadFile file);

    DownloadFile Get(string id);
    List<DownloadFile> GetAll();

    void Remove(string id);
}

public class DownloadFileService : IDownloadFileService
{
    private Dictionary<string, DownloadFile> _files = new Dictionary<string, DownloadFile>();

    private readonly IDatabaseService _dbService;
    public DownloadFileService(IDatabaseService dbService)
    {
        _dbService = dbService;
    }

    public void Add(DownloadFile file)
    {
        if(!_files.ContainsKey(file.Id))
        {
            _files.Add(file.Id, file);
            this._dbService.Insert((FileDao)file).Wait();
        }
        else
        {
            _files[file.Id] = file;
            this._dbService.Update((FileDao)file).Wait();
        }
    }

    public DownloadFile Get(string id)
    {
        if (!_files.ContainsKey(id))
            return null;
        return _files[id];
    }

    public List<DownloadFile> GetAll()
    {
        return _files.Values.ToList();
    }

    public void Remove(string id)
    {
        var file = _files[id];
        _files.Remove(id);
        this._dbService.Remove((FileDao)file).Wait();
    }

    public async Task LoadFromDB()
    {
        foreach(var file in await this._dbService.Load<FileDao>())
        {
            this._files.Add(file.Id, file);
        }
    }
}