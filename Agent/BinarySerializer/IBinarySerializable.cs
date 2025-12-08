using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.IO;

namespace BinarySerializer
{
    public interface IBinarySerializable
    {
        Task SerializeAsync(EndianBinaryWriter writer);
        Task DeserializeAsync(EndianBinaryReader reader);
    }
}
