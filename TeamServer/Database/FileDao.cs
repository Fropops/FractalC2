using Shared;
using SQLite;

namespace TeamServer.Database;

[Table("files")]
public sealed class FileDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    [Column("file_name")]
    public string FileName { get; set; }
    [Column("data")]
    public byte[] Data { get; set; }
    [Column("source")]
    public string Source { get; set; }
    [Column("path")]
    public string Path { get; set; }

    public static implicit operator FileDao(DownloadFile file)
    {
        return new FileDao
        {
            Id = file.Id,
            FileName = file.FileName,
            Data = file.Data,
            Source = file.Source,
            Path = file.Path
        };

    }

    public static implicit operator DownloadFile(FileDao dao)
    {
        if (dao == null) return null;

        return new DownloadFile()
        {
            Id = dao.Id,
            FileName = dao.FileName,
            Data = dao.Data,
            Source = dao.Source,
            Path = dao.Path
        };
    }
}