using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer
{
    public static class Extensions
    {
        public static async Task<byte[]> BinarySerializeAsync<T>(this T item)
        {
            return await new Serializer().SerializeAsync(item);
        }

        public static async Task<T> BinaryDeserializeAsync<T>(this byte[] data)
        {
            return await new Serializer().DeserializeAsync<T>(data);
        }

    }
}
