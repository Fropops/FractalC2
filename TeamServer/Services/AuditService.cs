using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using TeamServer.Helper;

namespace TeamServer.Services;

public interface IAuditService
{
    void Record(AuditItem auditItem);
    void Record(AuditType type, AuditCategory category, string source, string target, string message);
    void Record(AuditCategory category, string source, string message);

    void Record(UserContext context, string message);
    void Record(UserContext context, string target, string message);


}

public class AuditItem
{
    public DateTime Date {get; set;}

    public AuditType Type {get; set;}
    public AuditCategory Category { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
    
    public string Message { get; set; }
}

public enum AuditType
{
    Info,
    Warning,
    Success,
    Error,
}

public enum AuditCategory
{
    User,
    Agent,
    Host,
}

public class AuditService : IAuditService
{
    private readonly IConfiguration _configService;

    public string Folder { get; private set; }

    public string FileName { 
        get {
            return Path.Combine(Folder, DateTime.Now.ToString("dd-MM-yyyy"));
        }
    }

    public AuditService(IConfiguration configService)
    {
        _configService = configService;

        Folder = configService.GetValue<string>("AuditFolder");
        if(!Directory.Exists(Folder))
            Directory.CreateDirectory(Folder);
    }
    public void Record(AuditItem auditItem)
    {
        File.AppendAllText(FileName + ".txt", $"{auditItem.Date.ToString("dd/MM/yyyy HH:mm:ss")} | {auditItem.Type} | {auditItem.Category} | {auditItem.Source} | {auditItem.Target} | {auditItem.Message}{Environment.NewLine}");
    }

    public void Record(AuditType type, AuditCategory category, string source, string target, string message)
    {
        if (string.IsNullOrWhiteSpace(source))
            source = "System";
        this.Record(new AuditItem()
        {
            Date = DateTime.Now,
            Type = type,
            Category = category,
            Source = source,
            Target = target,
            Message = message
        });
    }

    public void Record(AuditCategory category, string source, string target, string message)
    {
        this.Record(AuditType.Info, category, source, target, message);
    }

    public void Record(AuditCategory category, string source,string message)
    {
        this.Record(AuditType.Info, category, source, String.Empty, message);
    }

    public void Record(UserContext context, string message)
    {
        this.Record(AuditType.Info, AuditCategory.User, $"{context.User.Id}-{context.Session}", String.Empty, message);
    }

    public void Record(UserContext context, string target, string message)
    {
        this.Record(AuditType.Info, AuditCategory.User, $"{context.User.Id}-{context.Session}", target, message);
    }
}