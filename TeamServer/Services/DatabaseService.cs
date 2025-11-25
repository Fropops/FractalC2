using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TeamServer.Database;

namespace TeamServer.Service;
public interface IDatabaseService
{
    public Task<List<T>> Load<T>() where T : TeamServerDao, new();

    public Task Insert<T>(T item) where T : TeamServerDao, new();

    public Task Update<T>(T item) where T : TeamServerDao, new();

    public Task<int> Remove<T>(T item) where T : TeamServerDao, new();

    Task<int> Clear<T>() where T : TeamServerDao, new();

    public Task<T> Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> expr) where T : TeamServerDao, new();
}

public class DatabaseService : IDatabaseService
{
    private readonly SQLiteConnection _connection;
    private readonly SQLiteAsyncConnection _asyncConnection;

    public DatabaseService()
    {
        var directory = Directory.GetCurrentDirectory();

        var path = Path.Combine(directory, "data.db");

        using (var conn = new SQLiteConnection(path))
        {
            conn.CreateTable<HttpListenerDao>();
            conn.CreateTable<AgentDao>();
            conn.CreateTable<TaskDao>();
            conn.CreateTable<ResultDao>();
            conn.CreateTable<WebHostFileDao>();
            conn.CreateTable<WebHostLogDao>();
            conn.CreateTable<FileDao>();
        }

        // open connections
        _connection = new SQLiteConnection(path);
        _asyncConnection = new SQLiteAsyncConnection(path);
    }

    //public SQLiteConnection GetConnection()
    //    => _connection;

    //public SQLiteAsyncConnection GetAsyncConnection()
    //    => _asyncConnection;

    async Task<List<T>> IDatabaseService.Load<T>()
    {
        var list = await this._asyncConnection.Table<T>().ToListAsync();
        return list;
    }

    async Task IDatabaseService.Insert<T>(T item)
    {
        await this._asyncConnection.InsertAsync(item);
    }

    async Task IDatabaseService.Update<T>(T item)
    {
        await this._asyncConnection.UpdateAsync(item);
    }

    async Task<T> IDatabaseService.Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> expr)
    {
        return await this._asyncConnection.Table<T>().FirstOrDefaultAsync(expr);
    }

    async Task<int> IDatabaseService.Remove<T>(T item)
    {
        return await this._asyncConnection.DeleteAsync(item);
    }

    async Task<int> IDatabaseService.Clear<T>()
    {
        return await this._asyncConnection.DeleteAllAsync<T>();
    }
}