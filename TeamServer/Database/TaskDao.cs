using System;
using System.Threading.Tasks;
using BinarySerializer;
using Common.Models;
using Shared;
using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("tasks")]
public sealed class TaskDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    [Column("agent_id")]
    public string AgentId { get; set; }
    [Column("command_id")]
    public byte CommandId { get; set; }
    [Column("command_label")]
    public string Command { get; set; }
    [Column("date")]
    public DateTime RequestDate { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    public static implicit operator TaskDao(TeamServerAgentTask task)
    {
        return new TaskDao
        {
            Id = task.Id,
            AgentId = task.AgentId,
            Command = task.Command,
            CommandId = (byte)task.CommandId,
            RequestDate = task.RequestDate
        };

    }

    public static implicit operator TeamServerAgentTask(TaskDao dao)
    {
        if (dao == null) return null;

        return new TeamServerAgentTask()
        {
            Id = dao.Id,
            AgentId = dao.AgentId,
            Command = dao.Command,
            CommandId = (CommandId)dao.CommandId,
            RequestDate = dao.RequestDate
        };
    }
}