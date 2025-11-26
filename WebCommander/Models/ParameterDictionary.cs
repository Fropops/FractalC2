using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer;
using MiscUtil.IO;

namespace TeamServer.UI.Models
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
            // This assumes T has a BinarySerializeAsync method or we handle basic types
            // For now, let's assume basic types or string conversion for simplicity in UI
            // But the user requested this specific signature which implies T is serializable
            // However, in the UI context, we mostly deal with strings. 
            // Let's implement a simple string conversion for now or throw if not supported
            if (item is string s)
            {
                this.Add(id, System.Text.Encoding.UTF8.GetBytes(s));
            }
            else if (item is byte[] b)
            {
                this.Add(id, b);
            }
            else
            {
                // Fallback or error? The user code snippet had: item.BinarySerializeAsync().Result
                // which implies T is IBinarySerializable (or similar).
                // Since I don't have a generic IBinarySerializable<T> yet, I'll stick to byte[] for now
                // and maybe overload for string.
                throw new System.NotImplementedException("Serialization for type " + typeof(T).Name + " not implemented.");
            }
        }

        public void AddParameter(ParameterId id, byte[] item)
        {
            this.Add(id, item);
        }
        
        public void AddParameter(ParameterId id, string item)
        {
            this.Add(id, System.Text.Encoding.UTF8.GetBytes(item));
        }
    }
}
