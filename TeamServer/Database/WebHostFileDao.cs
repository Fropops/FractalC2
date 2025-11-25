using System;
using System.Threading.Tasks;
using BinarySerializer;
using Common.APIModels.WebHost;
using Common.Models;
using Shared;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("web_host_file")]
public sealed class WebHostFileDao : TeamServerDao
{
    [PrimaryKey, Column("path")]
    public string Path { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("is_powershell")]
    public bool IsPowershell { get; set; }
    [Column("data")]
    public byte[] Data { get; set; }
    
    

    public static implicit operator WebHostFileDao(FileWebHost item)
    {
        return new WebHostFileDao
        {
            Path = item.Path,
            Description = item.Description,
            IsPowershell = item.IsPowershell,
            Data = item.Data,
        };

    }

    public static implicit operator FileWebHost(WebHostFileDao dao)
    {
        if (dao == null) return null;

        return new FileWebHost()
        {
            Path = dao.Path,
            Description = dao.Description,
            IsPowershell = dao.IsPowershell,
            Data = dao.Data,
        };
    }
}