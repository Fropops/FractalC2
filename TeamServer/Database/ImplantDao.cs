using System;
using Common.Models;
using Common.Payload;
using Newtonsoft.Json;
using Shared;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("implants")]
public sealed class ImplantDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("data")]
    public byte[] Data { get; set; }
    [Column("config")]
    public string Config { get; set; }
    [Column("listener")]
    public string Listener { get; set; }
    [Column("isDeleted")]
    public bool IsDeleted { get; set; }

    public static implicit operator ImplantDao(Models.Implant implant)
    {
        return new ImplantDao
        {
            Id = implant.Id,
            Name = implant.Name,
            Data = string.IsNullOrEmpty(implant.Data) ? new byte[0] : Convert.FromBase64String(implant.Data),
            Config = JsonConvert.SerializeObject(implant.Config),
            Listener = implant.Listener,
            IsDeleted = implant.IsDeleted
        };

    }

    public static implicit operator Models.Implant(ImplantDao dao)
    {
        if (dao == null) return null;

        return new Models.Implant(dao.Id)
        {
            Name = dao.Name,
            Data = Convert.ToBase64String(dao.Data),
            Config = JsonConvert.DeserializeObject<ImplantConfig>(dao.Config),
            Listener = dao.Listener,
            IsDeleted = dao.IsDeleted
        };
    }
}