using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Implements;

internal class DefaultJsonHelper(
    Func<object, string> serializer,
    Func<string, Type, object> deserializer,
    Func<byte[], Type, object> deserializerBytes) : ILightJsonHelper
{
    public object? Deserialize(string json, Type type) => deserializer.Invoke(json, type);

    public object? Deserialize(byte[] json, Type type) => deserializerBytes.Invoke(json, type);

    public string Serialize<T>(T value) => serializer.Invoke(value!);
}