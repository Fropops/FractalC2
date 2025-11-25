using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public class AgentTask
    {
        [FieldOrder(0)]
        public string Id { get; set; }
        [FieldOrder(1)]
        public CommandId CommandId { get; set; }
        [FieldOrder(2)]
        public ParameterDictionary Parameters { get; set; } = new ParameterDictionary();


        public bool HasParameter(ParameterId id)
        {
            if(Parameters == null)
                return false;
            return Parameters.ContainsKey(id);
        }
        public T GetParameter<T>(ParameterId id)
        {
            if (Parameters == null)
                return default(T);
            if (!Parameters.ContainsKey(id))
                return default(T);
            return Parameters[id].BinaryDeserializeAsync<T>().Result;
        }

        public byte[] GetParameter(ParameterId id)
        {
            if (Parameters == null)
                return null;
            if (!Parameters.ContainsKey(id))
                return null;
            return Parameters[id];
        }

        public void ThrowIfParameterMissing(ParameterId id, string errorMessage)
        {
            if(!this.HasParameter(id))
                throw new ArgumentException(errorMessage);
        }

        public void ThrowIfParameterMissing(ParameterId id)
        {
            if (!this.HasParameter(id))
                throw new ArgumentException($"{id} is mandatory!");
        }
    }
}
