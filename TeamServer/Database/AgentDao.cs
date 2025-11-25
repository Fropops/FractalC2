using System;
using BinarySerializer;
using Shared;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("agents")]
public sealed class AgentDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }

    [Column("implant_id")]
    public string ImplantId { get; set; }

    [Column("first_seen")]
    public DateTime FirstSeen { get; set; }

    [Column("last_seen")]
    public DateTime LastSeen { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("hostname")]
    public string Hostname { get; set; }
    [Column("username")]
    public string UserName { get; set; }
    [Column("process_name")]
    public string ProcessName { get; set; }
    [Column("process_id")]
    public int ProcessId { get; set; }
    [Column("integrity")]
    public byte Integrity { get; set; }
    [Column("architecture")]
    public string Architecture { get; set; }
    [Column("endpoint")]
    public string EndPoint { get; set; }
    [Column("version")]
    public string Version { get; set; }

    [Column("address")]
    public byte[] Address { get; set; }

    [Column("sleep_interval")]
    public int SleepInterval { get; set; }
    [Column("sleep_jitter")]
    public int SleepJitter { get; set; }

    public static implicit operator AgentDao(Agent agent)
    {
        var dao = new AgentDao
        {
            Id = agent.Id,
            FirstSeen = agent.FirstSeen,
            LastSeen = agent.LastSeen,

        };

        if (agent.Metadata != null)
        {
            dao.ImplantId = agent.Metadata.ImplantId;
            dao.Hostname = agent.Metadata.Hostname;
            dao.UserName = agent.Metadata.UserName;
            dao.ProcessId = agent.Metadata.ProcessId;
            dao.Integrity = (byte)agent.Metadata.Integrity;
            dao.EndPoint = agent.Metadata.EndPoint;
            dao.Address = agent.Metadata.Address;
            dao.ProcessName = agent.Metadata.ProcessName;
            dao.Version = agent.Metadata.Version;
            dao.SleepInterval = agent.Metadata.SleepInterval;
            dao.SleepJitter = agent.Metadata.SleepJitter;
            dao.Architecture = agent.Metadata.Architecture;
        }

        return dao;

    }

    public static implicit operator Agent(AgentDao dao)
    {
        if (dao == null) return null;

        var agent = new Agent(dao.Id)
        {
            FirstSeen = dao.FirstSeen,
            LastSeen = dao.LastSeen,
        };

        if (!string.IsNullOrEmpty(dao.EndPoint))
        {
            var metadata = new AgentMetadata()
            {
                Id = dao.Id,
                ImplantId = dao.ImplantId,
                Hostname = dao.Hostname,
                UserName = dao.UserName,
                ProcessId = dao.ProcessId,
                Integrity = (IntegrityLevel)dao.Integrity,
                EndPoint = dao.EndPoint,
                Address = dao.Address,
                ProcessName = dao.ProcessName,
                Version = dao.Version,
                SleepInterval = dao.SleepInterval,
                SleepJitter = dao.SleepJitter,
                Architecture = dao.Architecture
            };
            agent.Metadata = metadata;
        }

        return agent;
    }
}