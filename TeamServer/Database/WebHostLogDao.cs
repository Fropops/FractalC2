using System;
using System.Threading.Tasks;
using BinarySerializer;
using Common;
using Common.APIModels.WebHost;
using Common.Models;
using Shared;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("web_host_log")]
public sealed class WebHostLogDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    [Column("date")]
    public DateTime Date { get; set; }
    [Column("url")]
    public string Url { get; set; }
    [Column("path")]
    public string Path { get; set; }
    [Column("user_agent")]
    public string UserAgent { get; set; }
    [Column("status_code")]
    public int StatusCode { get; set; }

    public static implicit operator WebHostLogDao(WebHostLog item)
    {
        return new WebHostLogDao
        {
            Id = ShortGuid.NewGuid(),
            Path = item.Path,
            Date = item.Date,
            Url = item.Url,
            UserAgent = item.UserAgent,
            StatusCode = item.StatusCode,
        };

    }

    public static implicit operator WebHostLog(WebHostLogDao dao)
    {
        if (dao == null) return null;

        return new WebHostLog()
        {
            Path = dao.Path,
            Date = dao.Date,
            Url = dao.Url,
            UserAgent = dao.UserAgent,
            StatusCode = dao.StatusCode,
        };
    }
}