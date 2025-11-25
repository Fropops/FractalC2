using SQLite;
using TeamServer.Models;

namespace TeamServer.Database;

[Table("http_handlers")]
public sealed class HttpListenerDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("port")]
    public int BindPort { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("secure")]
    public bool Secure { get; set; }

    public static implicit operator HttpListenerDao(HttpListener handler)
    {
        return new HttpListenerDao
        {
            Id = handler.Id,
            Name = handler.Name,
            BindPort = handler.BindPort,
            Address = handler.Ip,
            Secure = handler.Secured,
        };
    }

    public static implicit operator HttpListener(HttpListenerDao dao)
    {
        return new HttpListener(dao.Id, dao.Name, dao.BindPort, dao.Address, dao.Secure);
    }
}
