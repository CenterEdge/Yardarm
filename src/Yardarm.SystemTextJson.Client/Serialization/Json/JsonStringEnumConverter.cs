using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RootNamespace.Serialization.Json
{
    internal class JsonStringEnumConverter<T> : JsonConverter<T>
        where T : Enum
    {
        private static readonly TypeCode _enumTypeCode = Type.GetTypeCode(typeof(T));

        private static readonly Dictionary<T, JsonEncodedText> _enumToString = new();
        private static readonly Dictionary<string, T> _stringToEnum = new();

        static JsonStringEnumConverter()
        {
            var type = typeof(T);

            foreach (string name in Enum.GetNames(type))
            {
                var enumMember = type.GetField(name);
                T enumValue = (T) enumMember.GetValue(null);

                var attribute = (EnumMemberAttribute?) enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault();
                var stringValue = attribute is not null ? attribute.Value : name;

                _stringToEnum.Add(stringValue, enumValue);
                _enumToString.Add(enumValue, JsonEncodedText.Encode(stringValue));
            }
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var strValue = reader.GetString()!;
                    if (_stringToEnum.TryGetValue(strValue, out var enumValue))
                    {
                        return enumValue;
                    }
                    break;

                case JsonTokenType.Number:
                    switch (_enumTypeCode)
                    {
                        case TypeCode.Int32:
                            if (reader.TryGetInt32(out int int32))
                            {
                                return Unsafe.As<int, T>(ref int32);
                            }
                            break;
                        case TypeCode.UInt32:
                            if (reader.TryGetUInt32(out uint uint32))
                            {
                                return Unsafe.As<uint, T>(ref uint32);
                            }
                            break;
                        case TypeCode.UInt64:
                            if (reader.TryGetUInt64(out ulong uint64))
                            {
                                return Unsafe.As<ulong, T>(ref uint64);
                            }
                            break;
                        case TypeCode.Int64:
                            if (reader.TryGetInt64(out long int64))
                            {
                                return Unsafe.As<long, T>(ref int64);
                            }
                            break;
                        case TypeCode.SByte:
                            if (reader.TryGetSByte(out sbyte byte8))
                            {
                                return Unsafe.As<sbyte, T>(ref byte8);
                            }
                            break;
                        case TypeCode.Byte:
                            if (reader.TryGetByte(out byte ubyte8))
                            {
                                return Unsafe.As<byte, T>(ref ubyte8);
                            }
                            break;
                        case TypeCode.Int16:
                            if (reader.TryGetInt16(out short int16))
                            {
                                return Unsafe.As<short, T>(ref int16);
                            }
                            break;
                        case TypeCode.UInt16:
                            if (reader.TryGetUInt16(out ushort uint16))
                            {
                                return Unsafe.As<ushort, T>(ref uint16);
                            }
                            break;
                    }

                    throw new JsonException();
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (_enumToString.TryGetValue(value, out JsonEncodedText str))
            {
                writer.WriteStringValue(str);
                return;
            }

            switch (_enumTypeCode)
            {
                case TypeCode.Int32:
                    writer.WriteNumberValue(Unsafe.As<T, int>(ref value));
                    break;
                case TypeCode.UInt32:
                    writer.WriteNumberValue(Unsafe.As<T, uint>(ref value));
                    break;
                case TypeCode.UInt64:
                    writer.WriteNumberValue(Unsafe.As<T, ulong>(ref value));
                    break;
                case TypeCode.Int64:
                    writer.WriteNumberValue(Unsafe.As<T, long>(ref value));
                    break;
                case TypeCode.Int16:
                    writer.WriteNumberValue(Unsafe.As<T, short>(ref value));
                    break;
                case TypeCode.UInt16:
                    writer.WriteNumberValue(Unsafe.As<T, ushort>(ref value));
                    break;
                case TypeCode.Byte:
                    writer.WriteNumberValue(Unsafe.As<T, byte>(ref value));
                    break;
                case TypeCode.SByte:
                    writer.WriteNumberValue(Unsafe.As<T, sbyte>(ref value));
                    break;
                default:
                    throw new JsonException();
            }
        }
    }
}
