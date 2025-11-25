using System;
using System.Threading.Tasks;
using BinarySerializer;
using Common.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Shared;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("results")]
public sealed class ResultDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    [Column("output")]
    public string Output { get; set; }
    [Column("objects")]
    public byte[] Objects { get; set; }
    [Column("error")]
    public string Error { get; set; }
    [Column("info")]
    public string Info { get; set; }
    [Column("status")]
    public byte Status { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    public static implicit operator ResultDao(AgentTaskResult res)
    {
        return new ResultDao
        {
            Id = res.Id,
            Output = res.Output,
            Objects = res.Objects,
            Error = res.Error,
            Info = res.Info,
            Status = (byte) res.Status
        };

    }

    public static implicit operator AgentTaskResult(ResultDao dao)
    {
        if (dao == null) return null;

        return new AgentTaskResult()
        {
            Id = dao.Id,
            Error = dao.Error,
            Info = dao.Info,
            Output = dao.Output,
            Objects = dao.Objects,
            Status = (AgentResultStatus)dao.Status
        };
    }
}