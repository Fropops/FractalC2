using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer
{
    [System.Serializable]
    public class BinarySerializationException : Exception
    {

        public const string NothingToSerializeExceptionMessage = "No member found to serialize on the Type {0}";
        public const string NotSupportedSerializeExceptionMessage = "Serializaton not supported for Type {0}";
        public BinarySerializationException() { }
        public BinarySerializationException(string message) : base(message) { }
        public BinarySerializationException(string message, Exception inner) : base(message, inner) { }
    }

    public class NothingToSerializeException : BinarySerializationException
    {
        public NothingToSerializeException(Type type) : base(string.Format(NothingToSerializeExceptionMessage, type)) { }

    }

    public class NotSupportedSerializeException : BinarySerializationException
    {
        public NotSupportedSerializeException(Type type) : base(string.Format(NotSupportedSerializeExceptionMessage, type)) { }

    }
}
