using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.ResultObjects;
using WebCommander.Helpers;
using WebCommander.Models;

namespace WebCommander.Services
{
    public class TaskResultFormatterService
    {
        #region Process Tree (ps / powerpick)

        public List<TerminalLine> FormatProcessTreeLines(List<ListProcessResult> processes, int? currentProcessId = null)
        {
            var lines = new List<TerminalLine>();
            if (processes == null || processes.Count == 0)
                return lines;

            // Header line
            lines.Add(new TerminalLine
            {
                Text = string.Format("{0,-6} {1,-6} {2,-30} {3,-10} {4,-20} {5}", "PID", "PPID", "Name", "Arch", "Owner", "Session"),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsPsHeader", "true" } }
            });

            // Separator
            lines.Add(new TerminalLine
            {
                Text = new string('-', 100),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsPsHeader", "false" } }
            });

            // Build process tree
            var processDict = processes.ToDictionary(p => p.Id, p => p);
            var rootProcesses = processes.Where(p => !processDict.ContainsKey(p.ParentId)).OrderBy(p => p.Id).ToList();

            foreach (var rootProcess in rootProcesses)
            {
                AppendProcessTreeNode(lines, rootProcess, processDict, 0, currentProcessId);
            }

            return lines;
        }

        private void AppendProcessTreeNode(List<TerminalLine> lines, ListProcessResult process, Dictionary<int, ListProcessResult> processDict, int depth, int? currentProcessId)
        {
            var indent = new string(' ', depth * 2);
            var maxNameLength = 30;
            var indentedName = indent + process.Name;

            if (indentedName.Length > maxNameLength)
            {
                indentedName = indentedName.Substring(0, maxNameLength);
            }

            var lineText = string.Format("{0,-6} {1,-6} {2,-30} {3,-10} {4,-20} {5}",
                process.Id,
                process.ParentId,
                indentedName,
                process.Arch ?? "",
                process.Owner ?? "",
                process.SessionId);

            var line = new TerminalLine
            {
                Text = lineText,
                Type = TerminalLineType.Normal,
                Metadata = new Dictionary<string, string>
                {
                    { "IsPsRow", "true" },
                    { "ProcessId", process.Id.ToString() },
                    { "ProcessName", process.Name }
                }
            };

            if (currentProcessId.HasValue && currentProcessId.Value == process.Id)
            {
                line.Metadata["IsCurrentProcess"] = "true";
            }

            lines.Add(line);

            var children = processDict.Values.Where(p => p.ParentId == process.Id).OrderBy(p => p.Id).ToList();
            foreach (var child in children)
            {
                AppendProcessTreeNode(lines, child, processDict, depth + 1, currentProcessId);
            }
        }

        public string FormatProcessTreeText(List<ListProcessResult> processes)
        {
            if (processes == null || processes.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0,-6} {1,-6} {2,-30} {3,-10} {4,-20} {5}", "PID", "PPID", "Name", "Arch", "Owner", "Session"));
            sb.AppendLine(new string('-', 100));

            var processDict = processes.ToDictionary(p => p.Id, p => p);
            var rootProcesses = processes.Where(p => !processDict.ContainsKey(p.ParentId)).OrderBy(p => p.Id).ToList();

            foreach (var rootProcess in rootProcesses)
            {
                AppendProcessTreeNodeText(sb, rootProcess, processDict, 0);
            }

            return sb.ToString();
        }

        private void AppendProcessTreeNodeText(StringBuilder sb, ListProcessResult process, Dictionary<int, ListProcessResult> processDict, int depth)
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

            var children = processDict.Values.Where(p => p.ParentId == process.Id).OrderBy(p => p.Id).ToList();
            foreach (var child in children)
            {
                AppendProcessTreeNodeText(sb, child, processDict, depth + 1);
            }
        }

        #endregion

        #region Directory Listing (ls / dir)

        public List<TerminalLine> FormatDirectoryListLines(ListDirectoryResult result, OsType osType = OsType.Windows)
        {
            var lines = new List<TerminalLine>();
            if (result == null)
                return lines;

            // Header
            lines.Add(new TerminalLine
            {
                Text = string.Format("{0,-10} {1,-15} {2}", "Type", "Size", "Name"),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsLsHeader", "true" } }
            });

            // Separator
            lines.Add(new TerminalLine
            {
                Text = new string('-', 80),
                Type = TerminalLineType.Info
            });

            var separator = osType == OsType.Linux ? '/' : '\\';
            var directory = (result.Directory ?? string.Empty).TrimEnd(separator);

            foreach (var item in result.Lines)
            {
                var type = item.IsFile ? "[FILE]" : "[DIR] ";
                var size = item.IsFile ? ResultObjectHelper.FormatFileSize(item.Length) : "";
                var lineText = string.Format("{0,-10} {1,-15} {2}", type, size, item.Name);
                var fullPath = string.IsNullOrEmpty(directory) ? item.Name : $"{directory}{separator}{item.Name}";

                lines.Add(new TerminalLine
                {
                    Text = lineText,
                    Type = TerminalLineType.Normal,
                    Metadata = new Dictionary<string, string>
                    {
                        { "IsLsRow", "true" },
                        { "Name", fullPath },
                        { "IsFile", item.IsFile.ToString().ToLower() }
                    }
                });
            }

            return lines;
        }

        public string FormatDirectoryListText(ListDirectoryResult result)
        {
            if (result == null)
                return string.Empty;

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(result.Directory))
            {
                sb.AppendLine($"Directory: {result.Directory}");
                sb.AppendLine();
            }

            sb.AppendLine(string.Format("{0,-10} {1,-15} {2}", "Type", "Size", "Name"));
            sb.AppendLine(new string('-', 80));

            foreach (var item in result.Lines)
            {
                var type = item.IsFile ? "[FILE]" : "[DIR] ";
                var size = item.IsFile ? ResultObjectHelper.FormatFileSize(item.Length) : "";
                sb.AppendLine(string.Format("{0,-10} {1,-15} {2}", type, size, item.Name));
            }

            return sb.ToString();
        }

        #endregion

        #region Jobs (job)

        public List<TerminalLine> FormatJobListLines(List<Job> jobs)
        {
            var lines = new List<TerminalLine>();
            if (jobs == null || jobs.Count == 0)
                return lines;

            // Header
            lines.Add(new TerminalLine
            {
                Text = string.Format("{0,-6} {1,-15} {2,-20} {3,-10} {4}", "ID", "Type", "Name", "PID", "TaskID"),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsJobHeader", "true" } }
            });

            // Separator
            lines.Add(new TerminalLine
            {
                Text = new string('-', 80),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsJobHeader", "false" } }
            });

            foreach (var job in jobs)
            {
                var jobTypeStr = job.JobType.ToString().Length > 15 ? job.JobType.ToString().Substring(0, 15) : job.JobType.ToString();
                var pidStr = job.ProcessId.HasValue ? job.ProcessId.Value.ToString() : "-";
                var lineText = string.Format("{0,-6} {1,-15} {2,-20} {3,-10} {4}",
                    job.Id,
                    jobTypeStr,
                    job.Name.Length > 20 ? job.Name.Substring(0, 17) + "..." : job.Name,
                    pidStr,
                    job.TaskId);

                lines.Add(new TerminalLine
                {
                    Text = lineText,
                    Type = TerminalLineType.Normal,
                    Metadata = new Dictionary<string, string>
                    {
                        { "IsJobRow", "true" },
                        { "JobId", job.Id.ToString() },
                        { "JobName", job.Name }
                    }
                });
            }

            return lines;
        }

        public string FormatJobListText(List<Job> jobs)
        {
            if (jobs == null || jobs.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0,-6} {1,-15} {2,-20} {3,-10} {4}", "ID", "Type", "Name", "PID", "TaskID"));
            sb.AppendLine(new string('-', 80));

            foreach (var job in jobs)
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

        #endregion

        #region Links (link)

        public List<TerminalLine> FormatLinkListLines(List<LinkInfo> links)
        {
            var lines = new List<TerminalLine>();
            if (links == null || links.Count == 0)
                return lines;

            // Header
            lines.Add(new TerminalLine
            {
                Text = string.Format("{0,-20} {1,-20} {2}", "ParentID", "ChildID", "Binding"),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsLinkHeader", "true" } }
            });

            // Separator
            lines.Add(new TerminalLine
            {
                Text = new string('-', 80),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsLinkHeader", "false" } }
            });

            foreach (var link in links)
            {
                var lineText = string.Format("{0,-20} {1,-20} {2}", link.ParentId, link.ChildId, link.Binding);
                lines.Add(new TerminalLine
                {
                    Text = lineText,
                    Type = TerminalLineType.Normal,
                    Metadata = new Dictionary<string, string>
                    {
                        { "IsLinkRow", "true" },
                        { "ChildId", link.ChildId },
                        { "Binding", link.Binding }
                    }
                });
            }

            return lines;
        }

        public string FormatLinkListText(List<LinkInfo> links)
        {
            if (links == null || links.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0,-20} {1,-20} {2}", "ParentID", "ChildID", "Binding"));
            sb.AppendLine(new string('-', 80));

            foreach (var link in links)
            {
                sb.AppendLine(string.Format("{0,-20} {1,-20} {2}", link.ParentId, link.ChildId, link.Binding));
            }

            return sb.ToString();
        }

        #endregion

        #region Reverse Port Forward (rportfwd)

        public List<TerminalLine> FormatReversePortForwardLines(List<ReversePortForwarResult> results)
        {
            var lines = new List<TerminalLine>();
            if (results == null || results.Count == 0)
                return lines;

            // Header
            lines.Add(new TerminalLine
            {
                Text = string.Format("{0,-10} {1,-30} {2}", "Port", "DestHost", "DestPort"),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsRportFwdHeader", "true" } }
            });

            // Separator
            lines.Add(new TerminalLine
            {
                Text = new string('-', 60),
                Type = TerminalLineType.Info,
                Metadata = new Dictionary<string, string> { { "IsRportFwdHeader", "false" } }
            });

            foreach (var res in results)
            {
                var lineText = string.Format("{0,-10} {1,-30} {2}", res.Port, res.DestHost, res.DestPort);
                lines.Add(new TerminalLine
                {
                    Text = lineText,
                    Type = TerminalLineType.Normal,
                    Metadata = new Dictionary<string, string>
                    {
                        { "IsRportFwdRow", "true" },
                        { "Port", res.Port.ToString() },
                        { "DestHost", res.DestHost },
                        { "DestPort", res.DestPort.ToString() }
                    }
                });
            }

            return lines;
        }

        public string FormatReversePortForwardText(List<ReversePortForwarResult> results)
        {
            if (results == null || results.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0,-10} {1,-30} {2}", "Port", "DestHost", "DestPort"));
            sb.AppendLine(new string('-', 60));

            foreach (var res in results)
            {
                sb.AppendLine(string.Format("{0,-10} {1,-30} {2}", res.Port, res.DestHost, res.DestPort));
            }

            return sb.ToString();
        }

        #endregion

        #region Unified Async Formatters

        public async Task<List<TerminalLine>?> FormatResultObjectsLinesAsync(CommandId commandId, byte[]? data, OsType osType = OsType.Windows, int? currentProcessId = null)
        {
            if (data == null || data.Length == 0) return null;

            switch (commandId)
            {
                case CommandId.Ls:
                    var listResult = await ResultObjectHelper.DeserializeResult<ListDirectoryResult>(data);
                    return listResult != null && listResult.Lines.Count > 0 ? FormatDirectoryListLines(listResult, osType) : null;

                case CommandId.ListProcess:
                    var processResults = await ResultObjectHelper.DeserializeResult<List<ListProcessResult>>(data);
                    return processResults != null && processResults.Count > 0 ? FormatProcessTreeLines(processResults, currentProcessId) : null;

                case CommandId.Job:
                    var jobResults = await ResultObjectHelper.DeserializeResult<List<Job>>(data);
                    return jobResults != null && jobResults.Count > 0 ? FormatJobListLines(jobResults) : null;

                case CommandId.Link:
                    var linkResults = await ResultObjectHelper.DeserializeResult<List<LinkInfo>>(data);
                    return linkResults != null && linkResults.Count > 0 ? FormatLinkListLines(linkResults) : null;

                case CommandId.RportFwd:
                    var rportfwdResults = await ResultObjectHelper.DeserializeResult<List<ReversePortForwarResult>>(data);
                    return rportfwdResults != null && rportfwdResults.Count > 0 ? FormatReversePortForwardLines(rportfwdResults) : null;

                default:
                    return null;
            }
        }

        public async Task<List<TerminalLine>?> FormatResultObjectsLinesAsync(string command, byte[]? data, OsType osType = OsType.Windows, int? currentProcessId = null)
        {
            if (data == null || data.Length == 0 || string.IsNullOrWhiteSpace(command)) return null;

            var cmd = command.Trim().ToLowerInvariant();
            if (cmd.StartsWith("ls") || cmd.StartsWith("dir"))
            {
                var listResult = await ResultObjectHelper.DeserializeResult<ListDirectoryResult>(data);
                return listResult != null && listResult.Lines.Count > 0 ? FormatDirectoryListLines(listResult, osType) : null;
            }
            if (cmd.StartsWith("ps") || cmd.StartsWith("powerpick"))
            {
                var processResults = await ResultObjectHelper.DeserializeResult<List<ListProcessResult>>(data);
                return processResults != null && processResults.Count > 0 ? FormatProcessTreeLines(processResults, currentProcessId) : null;
            }
            if (cmd.StartsWith("job"))
            {
                var jobResults = await ResultObjectHelper.DeserializeResult<List<Job>>(data);
                return jobResults != null && jobResults.Count > 0 ? FormatJobListLines(jobResults) : null;
            }
            if (cmd.StartsWith("link"))
            {
                var linkResults = await ResultObjectHelper.DeserializeResult<List<LinkInfo>>(data);
                return linkResults != null && linkResults.Count > 0 ? FormatLinkListLines(linkResults) : null;
            }
            if (cmd.StartsWith("rportfwd"))
            {
                var rportfwdResults = await ResultObjectHelper.DeserializeResult<List<ReversePortForwarResult>>(data);
                return rportfwdResults != null && rportfwdResults.Count > 0 ? FormatReversePortForwardLines(rportfwdResults) : null;
            }

            return null;
        }

        public async Task<string?> FormatResultObjectsTextAsync(string command, byte[]? data, OsType osType = OsType.Windows)
        {
            if (data == null || data.Length == 0 || string.IsNullOrWhiteSpace(command)) return null;

            var cmd = command.Trim().ToLowerInvariant();
            if (cmd.StartsWith("ls") || cmd.StartsWith("dir"))
            {
                var listResult = await ResultObjectHelper.DeserializeResult<ListDirectoryResult>(data);
                return listResult != null && listResult.Lines.Count > 0 ? FormatDirectoryListText(listResult) : null;
            }
            if (cmd.StartsWith("ps") || cmd.StartsWith("powerpick"))
            {
                var processResults = await ResultObjectHelper.DeserializeResult<List<ListProcessResult>>(data);
                return processResults != null && processResults.Count > 0 ? FormatProcessTreeText(processResults) : null;
            }
            if (cmd.StartsWith("job"))
            {
                var jobResults = await ResultObjectHelper.DeserializeResult<List<Job>>(data);
                return jobResults != null && jobResults.Count > 0 ? FormatJobListText(jobResults) : null;
            }
            if (cmd.StartsWith("link"))
            {
                var linkResults = await ResultObjectHelper.DeserializeResult<List<LinkInfo>>(data);
                return linkResults != null && linkResults.Count > 0 ? FormatLinkListText(linkResults) : null;
            }
            if (cmd.StartsWith("rportfwd"))
            {
                var rportfwdResults = await ResultObjectHelper.DeserializeResult<List<ReversePortForwarResult>>(data);
                return rportfwdResults != null && rportfwdResults.Count > 0 ? FormatReversePortForwardText(rportfwdResults) : null;
            }

            return null;
        }

        #endregion
    }
}
