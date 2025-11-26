using System;
using Common.Models;
using Common.Payload;
using Shared;
using SQLite;
using TeamServer.Models.Implant;

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
    [Column("Type")]
    public int Type { get; set; }
    [Column("extension")]
    public string Extension { get; set; }
    [Column("listener")]
    public string Listener { get; set; }
    [Column("endpoint")]
    public string Endpoint { get; set; }
    [Column("isDeleted")]
    public bool IsDeleted { get; set; }

    public static implicit operator ImplantDao(Implant implant)
    {
        return new ImplantDao
        {
            Id = implant.Id,
            Name = implant.Name,
            Data = Convert.FromBase64String(implant.Data),
            Type = (int)implant.Type,
            Extension = implant.Extension,
            Listener = implant.Listener,
            Endpoint = implant.Endpoint,
            IsDeleted = implant.IsDeleted
        };

    }

    public static implicit operator Implant(ImplantDao dao)
    {
        if (dao == null) return null;

        return new Implant(dao.Id)
        {
            Name = dao.Name,
            Data = Convert.ToBase64String(dao.Data),
            Type = (PayloadType)dao.Type,
            Extension = dao.Extension,
            Listener = dao.Listener,
            Endpoint = dao.Endpoint,
            IsDeleted = dao.IsDeleted
        };
    }
}