using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;
using Commander.Models;
using Common.APIModels;
using Common.Models;
using Shared;
using Shared.ResultObjects;
using Spectre.Console;

namespace Commander.Helper
{
    public static class LootOutputFormatter
    {
        public static async Task<byte[]> FormatLootContent(Agent agent, TeamServerAgentTask task, AgentTaskResult result)
        {
            var header = new StringBuilder();
            header.AppendLine("=".PadRight(80, '='));
            header.AppendLine("TASK OUTPUT");
            header.AppendLine("=".PadRight(80, '='));
            header.AppendLine();
            header.AppendLine($"Agent Name:      {agent?.Metadata?.Name ?? "Unknown"}");
            header.AppendLine($"Hostname:        {agent?.Metadata?.Hostname ?? "Unknown"}");
            header.AppendLine($"User:            {agent?.Metadata?.UserName ?? "Unknown"}");
            header.AppendLine($"IP Address:      {IpAsString(agent?.Metadata?.Address)}");
            header.AppendLine($"Process:         {agent?.Metadata?.ProcessName ?? "Unknown"} (PID: {agent?.Metadata?.ProcessId})");
            header.AppendLine();
            header.AppendLine($"Task ID:         {task.Id}");
            header.AppendLine($"Command:         {task.Command}");
            header.AppendLine($"Execution Date:  {task.RequestDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            header.AppendLine($"Status:          {result.Status}");
            header.AppendLine();
            header.AppendLine("=".PadRight(80, '='));
            header.AppendLine("OUTPUT");
            header.AppendLine("=".PadRight(80, '='));
            header.AppendLine();

            string outputContent = result.Output;

            if (string.IsNullOrEmpty(outputContent) && result.Objects != null && result.Objects.Length > 0)
            {
                if (task.Command.StartsWith("ps"))
                {
                    outputContent = await FormatProcessList(result.Objects);
                }
                else if (task.Command.StartsWith("ls") || task.Command.StartsWith("dir"))
                {
                    outputContent = await FormatDirectoryList(result.Objects);
                }
                else if (task.Command.StartsWith("job"))
                {
                    outputContent = await FormatJobList(result.Objects);
                }
                else if (task.Command.StartsWith("link"))
                {
                    outputContent = await FormatLinkList(result.Objects);
                }
            }

            header.Append(outputContent);
            return Encoding.UTF8.GetBytes(header.ToString());
        }

        private static string IpAsString(byte[] ip)
        {
            if (ip == null || ip.Length == 0) return "Unknown";
            try
            {
                return new System.Net.IPAddress(ip).ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        private static async Task<string> FormatProcessList(byte[] data)
        {
            try
            {
                var processResults = await data.BinaryDeserializeAsync<List<ListProcessResult>>();
                if (processResults.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format("{0,-6} {1,-6} {2,-30} {3,-10} {4,-20} {5}", "PID", "PPID", "Name", "Arch", "Owner", "Session"));
                    sb.AppendLine(new string('-', 100));

                    var processDict = processResults.ToDictionary(p => p.Id, p => p);
                    var rootProcesses = processResults.Where(p => !processDict.ContainsKey(p.ParentId)).OrderBy(p => p.Id).ToList();

                    foreach (var rootProcess in rootProcesses)
                    {
                        AppendProcessTreeToLoot(sb, rootProcess, processDict, 0);
                    }

                    return sb.ToString();
                }
            }
            catch { }
            return string.Empty;
        }

        private static void AppendProcessTreeToLoot(StringBuilder sb, ListProcessResult process, Dictionary<int, ListProcessResult> processDict, int depth)
        {
            var indent = new string(' ', depth * 2);
            var maxNameLength = 30;
            var indentedName = indent + process.Name;

            if (indentedName.Length > maxNameLength)
            {
                indentedName = indentedName.Substring(0, maxNameLength);
            }

            sb.AppendLine(string.Format("{0,-6} {1,-6} {2,-30} {3,-10} {4,-20} {5}",
                process.Id,
                process.ParentId,
                indentedName,
                process.Arch ?? "",
                process.Owner ?? "",
                process.SessionId));

            // Add children
            var children = processDict.Values.Where(p => p.ParentId == process.Id).OrderBy(p => p.Id).ToList();
            foreach (var child in children)
            {
                AppendProcessTreeToLoot(sb, child, processDict, depth + 1);
            }
        }

        private static async Task<string> FormatDirectoryList(byte[] data)
        {
            try
            {
                var listResult = await data.BinaryDeserializeAsync<ListDirectoryResult>();
                if (listResult != null && listResult.Lines.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Directory: {listResult.Directory}");
                    sb.AppendLine();
                    sb.AppendLine(string.Format("{0,-10} {1,-15} {2}", "Type", "Size", "Name"));
                    sb.AppendLine(new string('-', 80));

                    foreach (var item in listResult.Lines)
                    {
                        var type = item.IsFile ? "[FILE]" : "[DIR] ";
                        var size = item.IsFile ? FormatFileSize(item.Length) : "";
                        sb.AppendLine(string.Format("{0,-10} {1,-15} {2}", type, size, item.Name));
                    }

                    return sb.ToString();
                }
            }
            catch { }
            return string.Empty;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static async Task<string> FormatJobList(byte[] data)
        {
             try
            {
                var jobResults = await data.BinaryDeserializeAsync<List<Job>>();
                if (jobResults.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format("{0,-6} {1,-15} {2,-20} {3,-10} {4}", "ID", "Type", "Name", "PID", "TaskID"));
                    sb.AppendLine(new string('-', 80));

                    foreach (var job in jobResults)
                    {
                        var jobTypeStr = job.JobType.ToString().Length > 15 ? job.JobType.ToString().Substring(0, 15) : job.JobType.ToString();
                        var pidStr = job.ProcessId.HasValue ? job.ProcessId.Value.ToString() : "-";
                        sb.AppendLine(string.Format("{0,-6} {1,-15} {2,-20} {3,-10} {4}",
                            job.Id,
                            jobTypeStr,
                            job.Name.Length > 20 ? job.Name.Substring(0, 17) + "..." : job.Name,
                            pidStr,
                            job.TaskId));
                    }

                    return sb.ToString();
                }
            }
            catch { }
            return string.Empty;
        }

        private static async Task<string> FormatLinkList(byte[] data)
        {
            try
            {
                var linkResults = await data.BinaryDeserializeAsync<List<LinkInfo>>();
                if (linkResults.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format("{0,-20} {1,-20} {2,-20} {3}", "TaskID", "ParentID", "ChildID", "Binding"));
                    sb.AppendLine(new string('-', 100));

                    foreach (var link in linkResults)
                    {
                        sb.AppendLine(string.Format("{0,-20} {1,-20} {2,-20} {3}",
                            link.TaskId,
                            link.ParentId,
                            link.ChildId,
                            link.Binding));
                    }

                    return sb.ToString();
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
