using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.AgentCommands;
using Common.CommandLine.Core;
using Common.CommandLine.Execution;
using Common.Models;
using Shared.ResultObjects;
using WebCommander.Models;

namespace WebCommander.Services
{
    public class DirectoryItemInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsFile { get; set; }
    }

    public class TerminalCompletionService
    {
        private readonly CommandService _commandService;
        private readonly Dictionary<string, List<DirectoryItemInfo>> _agentDirectoryCache = new(StringComparer.OrdinalIgnoreCase);

        private CompletionCycle? _currentCycle;

        public TerminalCompletionService(CommandService commandService)
        {
            _commandService = commandService;
        }

        public void Reset()
        {
            _currentCycle = null;
        }

        public void UpdateDirectoryCache(string agentId, ListDirectoryResult? result)
        {
            if (string.IsNullOrWhiteSpace(agentId) || result?.Lines == null)
                return;

            var items = result.Lines
                .Select(l => new DirectoryItemInfo
                {
                    Name = l.Name ?? string.Empty,
                    IsFile = l.IsFile
                })
                .Where(i => !string.IsNullOrEmpty(i.Name))
                .DistinctBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _agentDirectoryCache[agentId] = items;
        }

        public void UpdateDirectoryCacheFromHistory(string agentId, IEnumerable<TerminalLine> historyLines)
        {
            if (string.IsNullOrWhiteSpace(agentId) || historyLines == null)
                return;

            var items = new Dictionary<string, DirectoryItemInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in historyLines)
            {
                if (line.Metadata != null &&
                    line.Metadata.TryGetValue("IsLsRow", out var isLsRow) &&
                    isLsRow == "true" &&
                    line.Metadata.TryGetValue("Name", out var rawName))
                {
                    var isFile = line.Metadata.TryGetValue("IsFile", out var isFileVal) &&
                                 bool.TryParse(isFileVal, out var parsedIsFile) && parsedIsFile;

                    var name = rawName.Trim();
                    // If rawName contains path separators, extract just the file/dir name
                    var sepIndex = name.LastIndexOfAny(new[] { '/', '\\' });
                    if (sepIndex >= 0 && sepIndex < name.Length - 1)
                    {
                        name = name.Substring(sepIndex + 1);
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        items[name] = new DirectoryItemInfo
                        {
                            Name = name,
                            IsFile = isFile
                        };
                    }
                }
            }

            if (items.Count > 0)
            {
                _agentDirectoryCache[agentId] = items.Values.ToList();
            }
        }

        public string? GetNextCompletion(string currentInput, Agent? agent, bool reverse = false)
        {
            if (_currentCycle != null && _currentCycle.Matches.Count > 0)
            {
                // Advance or retreat in active matches
                if (reverse)
                {
                    _currentCycle.CurrentIndex = (_currentCycle.CurrentIndex - 1 + _currentCycle.Matches.Count) % _currentCycle.Matches.Count;
                }
                else
                {
                    _currentCycle.CurrentIndex = (_currentCycle.CurrentIndex + 1) % _currentCycle.Matches.Count;
                }

                return FormatCompletionResult(_currentCycle);
            }

            // Start a new completion cycle
            var trimmedStart = currentInput.TrimStart();
            var leadingWhitespace = currentInput.Substring(0, currentInput.Length - trimmedStart.Length);

            // Determine if completing command name or argument
            var firstSpaceIndex = trimmedStart.IndexOf(' ');

            if (firstSpaceIndex == -1)
            {
                // Completing command name
                var prefix = trimmedStart;
                var candidates = GetAvailableCommandNames(agent);

                var matches = candidates
                    .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c)
                    .ToList();

                if (matches.Count == 0)
                    return null;

                _currentCycle = new CompletionCycle
                {
                    LeadingWhitespace = leadingWhitespace,
                    Prefix = prefix,
                    IsCommandCompletion = true,
                    Matches = matches,
                    CurrentIndex = 0
                };

                return FormatCompletionResult(_currentCycle);
            }
            else
            {
                // Completing argument
                var commandName = trimmedStart.Substring(0, firstSpaceIndex).Trim();
                var afterCommand = trimmedStart.Substring(firstSpaceIndex);

                // Find the argument prefix to complete
                string prefixBeforeArg;
                string argPrefix;
                bool isQuoted = false;

                // Check if currently inside quotes or ending with an open quote
                var lastQuoteIndex = afterCommand.LastIndexOf('"');
                if (lastQuoteIndex >= 0)
                {
                    var countQuotes = afterCommand.Count(c => c == '"');
                    if (countQuotes % 2 != 0)
                    {
                        // Open quote: argument starts after quote
                        isQuoted = true;
                        prefixBeforeArg = trimmedStart.Substring(0, firstSpaceIndex + lastQuoteIndex + 1);
                        argPrefix = afterCommand.Substring(lastQuoteIndex + 1);
                    }
                    else
                    {
                        // Even quotes: check after the last quote
                        var lastSpaceAfterQuote = afterCommand.LastIndexOf(' ');
                        if (lastSpaceAfterQuote >= lastQuoteIndex)
                        {
                            prefixBeforeArg = trimmedStart.Substring(0, firstSpaceIndex + lastSpaceAfterQuote + 1);
                            argPrefix = afterCommand.Substring(lastSpaceAfterQuote + 1);
                        }
                        else
                        {
                            // Cursor right after closed quote, nothing to complete
                            return null;
                        }
                    }
                }
                else
                {
                    var lastSpace = afterCommand.LastIndexOf(' ');
                    prefixBeforeArg = trimmedStart.Substring(0, firstSpaceIndex + lastSpace + 1);
                    argPrefix = afterCommand.Substring(lastSpace + 1);
                }

                var matches = GetArgumentMatches(agent, commandName, argPrefix);
                if (matches.Count == 0)
                    return null;

                _currentCycle = new CompletionCycle
                {
                    LeadingWhitespace = leadingWhitespace,
                    PrefixBeforeArg = prefixBeforeArg,
                    Prefix = argPrefix,
                    IsCommandCompletion = false,
                    IsQuoted = isQuoted,
                    Matches = matches,
                    CurrentIndex = 0
                };

                return FormatCompletionResult(_currentCycle);
            }
        }

        private List<string> GetAvailableCommandNames(Agent? agent)
        {
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentAgentOs = agent?.Metadata?.OsType;

            var registeredCommands = _commandService.GetCommands();
            if (registeredCommands != null)
            {
                foreach (var cmdDef in registeredCommands)
                {
                    if (cmdDef?.CommandType == null)
                        continue;

                    // OS check if it's an AgentCommandBase
                    if (currentAgentOs.HasValue && typeof(AgentCommandBase).IsAssignableFrom(cmdDef.CommandType))
                    {
                        try
                        {
                            var cmd = (AgentCommandBase?)Activator.CreateInstance(cmdDef.CommandType);
                            if (cmd != null && !cmd.SupportedOs.Contains(currentAgentOs.Value))
                                continue;
                        }
                        catch
                        {
                            // If instantiation fails, ignore filter
                        }
                    }

                    var attr = cmdDef.CommandType.GetCustomAttribute<CommandAttribute>();
                    if (attr != null)
                    {
                        if (!string.IsNullOrWhiteSpace(attr.Name))
                            candidates.Add(attr.Name.ToLowerInvariant());

                        if (attr.Aliases != null)
                        {
                            foreach (var alias in attr.Aliases)
                            {
                                if (!string.IsNullOrWhiteSpace(alias))
                                    candidates.Add(alias.ToLowerInvariant());
                            }
                        }
                    }
                }
            }

            // Add client-side terminal commands
            candidates.Add("clear");
            candidates.Add("cls");

            return candidates.ToList();
        }

        private List<string> GetArgumentMatches(Agent? agent, string commandName, string argPrefix)
        {
            if (agent == null || !_agentDirectoryCache.TryGetValue(agent.Id, out var items) || items.Count == 0)
                return new List<string>();

            var cmdLower = commandName.ToLowerInvariant();
            var matches = new List<DirectoryItemInfo>();

            // If cd or rmdir, prioritize directories
            if (cmdLower == "cd" || cmdLower == "rmdir")
            {
                matches = items
                    .Where(i => !i.IsFile && i.Name.StartsWith(argPrefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.Name)
                    .ToList();
            }
            // If download, del, cat, upload, prioritize files
            else if (cmdLower == "download" || cmdLower == "del" || cmdLower == "cat" || cmdLower == "upload")
            {
                matches = items
                    .Where(i => i.IsFile && i.Name.StartsWith(argPrefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.Name)
                    .ToList();

                // If no file matches found, also include directories as fallback
                if (matches.Count == 0)
                {
                    matches = items
                        .Where(i => i.Name.StartsWith(argPrefix, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(i => i.Name)
                        .ToList();
                }
            }
            else
            {
                // Default: all files and directories matching prefix
                matches = items
                    .Where(i => i.Name.StartsWith(argPrefix, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.Name)
                    .ToList();
            }

            return matches.Select(m => m.Name).ToList();
        }

        private string FormatCompletionResult(CompletionCycle cycle)
        {
            var matched = cycle.Matches[cycle.CurrentIndex];

            if (cycle.IsCommandCompletion)
            {
                // If only 1 match, append space to allow typing arguments immediately
                var suffix = cycle.Matches.Count == 1 ? " " : "";
                return cycle.LeadingWhitespace + matched + suffix;
            }
            else
            {
                // For arguments: if matched string contains spaces, wrap in quotes
                string formattedArg;
                if (cycle.IsQuoted)
                {
                    // Already inside quote: append closing quote
                    formattedArg = matched + "\"";
                }
                else if (matched.Contains(' '))
                {
                    formattedArg = $"\"{matched}\"";
                }
                else
                {
                    formattedArg = matched;
                }

                return cycle.LeadingWhitespace + cycle.PrefixBeforeArg + formattedArg;
            }
        }

        private class CompletionCycle
        {
            public string LeadingWhitespace { get; set; } = string.Empty;
            public string PrefixBeforeArg { get; set; } = string.Empty;
            public string Prefix { get; set; } = string.Empty;
            public bool IsCommandCompletion { get; set; }
            public bool IsQuoted { get; set; }
            public List<string> Matches { get; set; } = new();
            public int CurrentIndex { get; set; }
        }
    }
}
