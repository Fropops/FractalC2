using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace BinarySerializer
{
    public class Serializer
    {

        internal static readonly Dictionary<Type, Func<object, object>> TypeConverters =
            new Dictionary<Type, Func<object, object>>
            {
                {typeof(char), o => Convert.ToChar(o)},
                {typeof(byte), o => Convert.ToByte(o)},
                {typeof(sbyte), o => Convert.ToSByte(o)},
                {typeof(bool), o => Convert.ToBoolean(o)},
                {typeof(short), o => Convert.ToInt16(o)},
                {typeof(int), o => Convert.ToInt32(o)},
                {typeof(long), o => Convert.ToInt64(o)},
                {typeof(ushort), o => Convert.ToUInt16(o)},
                {typeof(uint), o => Convert.ToUInt32(o)},
                {typeof(ulong), o => Convert.ToUInt64(o)},
                {typeof(float), o => Convert.ToSingle(o)},
                {typeof(double), o => Convert.ToDouble(o)},
                {typeof(string), Convert.ToString}
            };

        internal static readonly Dictionary<Type, Action<EndianBinaryWriter, object>> PrimitveWriter =
           new Dictionary<Type, Action<EndianBinaryWriter, object>>
           {
                {typeof(char), (writer, o) => writer.Write(Convert.ToChar(o))},
                {typeof(byte), (writer, o) => writer.Write(Convert.ToByte(o))},
                {typeof(sbyte), (writer, o) => writer.Write(Convert.ToSByte(o))},
                {typeof(bool), (writer, o) => writer.Write(Convert.ToBoolean(o))},
                {typeof(Int16), (writer, o) => writer.Write(Convert.ToInt16(o))},
                {typeof(Int32), (writer, o) => writer.Write(Convert.ToInt32(o))},
                {typeof(Int64), (writer, o) => writer.Write(Convert.ToInt64(o))},
                {typeof(UInt16), (writer, o) => writer.Write(Convert.ToUInt16(o))},
                {typeof(UInt32), (writer, o) => writer.Write(Convert.ToUInt32(o))},
                {typeof(UInt64), (writer, o) => writer.Write(Convert.ToUInt64(o))},
                {typeof(float), (writer, o) => writer.Write(Convert.ToSingle(o))},
                {typeof(double), (writer, o) => writer.Write(Convert.ToDouble(o))},
           };

        internal static readonly Dictionary<Type, Func<EndianBinaryReader, object>> PrimitveReader =
           new Dictionary<Type, Func<EndianBinaryReader, object>>
           {
                {typeof(char), reader => (char)reader.ReadByte()},
                {typeof(byte), reader => reader.ReadByte()},
                {typeof(sbyte), reader => reader.ReadSByte()},
                {typeof(bool), reader => reader.ReadBoolean()},
                {typeof(Int16), reader => reader.ReadInt16()},
               // {typeof(short), reader => reader.ReadInt16()},
                {typeof(Int32), reader => reader.ReadInt32()},
                //{typeof(int), reader => reader.ReadInt32()},
                {typeof(Int64), reader => reader.ReadInt64()},
                //{typeof(long), reader => reader.ReadInt64()},
                //{typeof(ushort), reader => reader.ReadUInt16()},
                {typeof(UInt16), reader => reader.ReadUInt16()},
               // {typeof(uint), reader => reader.ReadUInt32()},
                {typeof(UInt32), reader => reader.ReadUInt32()},
               // {typeof(ulong), reader => reader.ReadUInt64()},
                {typeof(UInt64), reader => reader.ReadUInt64()},
                {typeof(float), reader => reader.ReadSingle()},
                {typeof(double), reader => reader.ReadDouble()},
           };




        public async Task<byte[]> SerializeAsync<T>(T item)
        {
            using (var stream = new MemoryStream())
            {
                await SerializeAsync<T>(stream, item);
                return stream.ToArray();
            }
        }

        public async Task SerializeAsync<T>(Stream stream, T value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (value == null)
            {
                return;
            }

            using (var writer = new EndianBinaryWriter(new BigEndianBitConverter(), stream))
                await SerializeAsync(writer, value);

            await stream.FlushAsync();
            return;
        }


        public async Task SerializeAsync<T>(EndianBinaryWriter writer, T value, Type forcedType = null)
        {
            Type type = forcedType ?? typeof(T);

            //            Console.WriteLine($"Serializing Type {type}");

            if (type.IsPrimitive)
            {
                PrimitveWriter[type](writer, value);
                return;
            }

            if (type.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                PrimitveWriter[underlyingType](writer, value);
                return;
            }

            writer.Write(value != null);
            if (value == null)
                return;

            if (typeof(IBinarySerializable).IsAssignableFrom(type))
            {
                var tmp = value as IBinarySerializable;
                await tmp.SerializeAsync(writer);
                return;
            }

            if (type == typeof(string))
            {
                var tmp = value as string;
                writer.Write(tmp);
                return;
            }

            if (type == typeof(byte[]))
            {
                if (value != null)
                {
                    var tmp = value as byte[];
                    writer.Write(tmp.Length);
                    writer.Write(tmp);
                }
                return;
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                var tmp = value as IList;
                writer.Write(tmp.Count);
                foreach (var item in tmp)
                    await SerializeAsync(writer, item, item.GetType());

                return;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value != null)
                {
                    // Retrieve the underlying value type of the nullable type
                    Type underlyingType = Nullable.GetUnderlyingType(type);
                    PrimitveWriter[underlyingType](writer, value);
                }
                return;
            }

            // Get all members of the type
            MemberInfo[] members = type.GetMembers();

            // Filter members by the presence of MyAttribute
            IEnumerable<MemberInfo> orderedMembers = members
                .Where(m => m.GetCustomAttribute<FieldOrderAttribute>() != null)
                .OrderBy(m => m.GetCustomAttribute<FieldOrderAttribute>().Order);


            if (orderedMembers.Count() == 0)
                throw new NothingToSerializeException(type);


            // Iterate over the filtered members
            foreach (MemberInfo member in orderedMembers)
            {
                //                Console.WriteLine($"Serializing Member {member.Name}");
                PropertyInfo propertyInfo = (PropertyInfo)member;
                object propertyValue = propertyInfo.GetValue(value);

                await this.SerializeAsync(writer, propertyValue, propertyInfo.PropertyType);
            }
        }

        public async Task<object> DeserializeAsync(EndianBinaryReader reader, Type forcedType)
        {
            Type type = forcedType;

            //            Console.WriteLine($"Deserializing Type {type}");

            if (type.IsPrimitive)
                return PrimitveReader[type](reader);

            if (type.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                return PrimitveReader[underlyingType](reader);
            }

            var isNull = !reader.ReadBoolean();
            if (isNull)
                return null;

            if (typeof(IBinarySerializable).IsAssignableFrom(type))
            {
                var inst = Activator.CreateInstance(type);
                var tmp = inst as IBinarySerializable;
                await tmp.DeserializeAsync(reader);
                return inst;
            }

            if (type == typeof(string))
            {
                return reader.ReadString();
            }

            if (type == typeof(byte[]))
            {
                var length = reader.ReadInt32();
                return reader.ReadBytes(length);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Retrieve the underlying value type of the nullable type
                Type underlyingType = Nullable.GetUnderlyingType(type);

                var item = PrimitveReader[underlyingType](reader);
                return item;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Retrieve the type argument of the generic List
                Type elementType = type.GetGenericArguments()[0];

                var list = Activator.CreateInstance(type) as IList;
                var length = reader.ReadInt32();
                for (int i = 0; i < length; ++i)
                {
                    var item = await DeserializeAsync(reader, elementType);
                    list.Add(item);
                }
                return list;
            }

            var ret = Activator.CreateInstance(type);

            // Get all members of the type
            MemberInfo[] members = type.GetMembers();

            // Filter members by the presence of MyAttribute
            IEnumerable<MemberInfo> orderedMembers = members
                .Where(m => m.GetCustomAttribute<FieldOrderAttribute>() != null)
                .OrderBy(m => m.GetCustomAttribute<FieldOrderAttribute>().Order);


            if (orderedMembers.Count() == 0)
                throw new NothingToSerializeException(type);


            // Iterate over the filtered members
            foreach (MemberInfo member in orderedMembers)
            {
                //                Console.WriteLine($"Deserializing Member {member.Name}");
                PropertyInfo propertyInfo = (PropertyInfo)member;
                var val = await this.DeserializeAsync(reader, propertyInfo.PropertyType);
                propertyInfo.SetValue(ret, val, null);
            }

            return ret;
        }


        public async Task<T> DeserializeAsync<T>(Stream stream)
        {
            var res = await DeserializeAsync(new EndianBinaryReader(new BigEndianBitConverter(), stream), typeof(T));
            return (T)res;
        }

        public Task<T> DeserializeAsync<T>(byte[] data)
        {
            return DeserializeAsync<T>(new MemoryStream(data));
        }

    }
}
