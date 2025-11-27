using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer;
using MiscUtil.IO;

namespace WebCommander.Models
{
    public class ParameterDictionary : Dictionary<ParameterId, byte[]>, IBinarySerializable
    {
        public async Task DeserializeAsync(EndianBinaryReader reader)
        {

            int count = reader.ReadInt32();
            for(int i = 0; i < count; i++)
            {
                var paramId = (ParameterId)reader.ReadByte();
                byte[] paramVal = null;
                if(reader.ReadBoolean())
                {
                    int length = reader.ReadInt32();
                    paramVal = new byte[length];
                    reader.Read(paramVal, 0, length);
                }
                this.Add(paramId, paramVal);
            }
        }

        public async Task SerializeAsync(EndianBinaryWriter writer)
        {
            writer.Write(this.Count);
            foreach (var key in this.Keys)
            {
                writer.Write((byte)key);
                var val = this[key];
                writer.Write(val != null);
                if (val != null)
                {
                    writer.Write(val.Length);
                    writer.Write(val);
                }
            }

        }

        public void AddParameter<T>(ParameterId id, T item)
        {
            Console.WriteLine($"Into AddParameter<T> {id}");
            if(item == null)
                return;
            if(item is string && string.IsNullOrEmpty(item as string))
                return;
            this.Add(id, item.BinarySerializeAsync().Result);
        }

        public void AddParameter(ParameterId id, byte[] item)
        {
            Console.WriteLine($"Into AddParameter {id}");
            if(item == null)
                return;
            this.Add(id, item);
        }
    }
}
