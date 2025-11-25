using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander.Terminal
{
    public class CommandHistory
    {
        private List<CommandDetail> History { get; set; } = new List<CommandDetail>();

        private int CurrentIndex = 0;

        public void Register(CommandDetail cmd)
        {
            this.History.Add(cmd);
            this.CurrentIndex = this.History.Count - 1;
        }

        public void Clear()
        {
            this.History.Clear();
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
