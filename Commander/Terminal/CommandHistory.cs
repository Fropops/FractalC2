using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Terminal
{
    public class CommandHistory
    {
        private const string HistoryFileName = "command_history.txt";

        private List<CommandDetail> History { get; set; } = new List<CommandDetail>();

        private int CurrentIndex = 0;

        public CommandHistory()
        {
            if (File.Exists(HistoryFileName))
            {
                try
                {
                    var lines = File.ReadAllLines(HistoryFileName);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            this.History.Add(new CommandDetail(0, "$> ", line));
                        }
                    }
                    this.CurrentIndex = this.History.Count;
                }
                catch
                {
                    //Best effort
                }
            }
        }

        public void Register(CommandDetail cmd)
        {
            this.History.Add(cmd);
            this.CurrentIndex = this.History.Count - 1;

            try
            {
                File.AppendAllText(HistoryFileName, cmd.Value + Environment.NewLine);
            }
            catch
            {
                //Best effort
            }
        }

        public void Clear()
        {
            this.History.Clear();
            if(File.Exists(HistoryFileName))
                File.Delete(HistoryFileName);
        }

        public CommandDetail Previous()
        {
            if (this.CurrentIndex == 0)
                return null;

            this.CurrentIndex--;
            return Current();
        }

        public CommandDetail Next()
        {
            if (this.CurrentIndex >= this.History.Count - 1)
                return null;

            this.CurrentIndex++;
            return Current();
        }

        public CommandDetail Current()
        {
            return this.History[this.CurrentIndex];
        }

        public bool IsMostRecent(CommandDetail cmd)
        {
            if(this.History.Count == 0)
                return false;
            return this.History[this.History.Count - 1] == cmd;
        }

        public CommandDetail Pop()
        {
            if (this.History.Count == 0)
                return null;
            var cmd = this.History[this.History.Count -1];
            this.History.Remove(cmd);
            return cmd;
        }
    }
}
