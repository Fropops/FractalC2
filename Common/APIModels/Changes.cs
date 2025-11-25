using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.APIModels
{
    public class Change
    {
        public ChangingElement Element { get; set; }
        public string Id { get; set; }

        public Change(ChangingElement elem, string id)
        {
            this.Element = elem;
            this.Id = id;
        }
    }

    public enum ChangingElement
    {
        Listener = 0,
        Agent,
        Task,
        Result,
        Metadata,
    }
}
