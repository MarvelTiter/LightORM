using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Implements;

internal class DefaultJsonHelper(
    Func<object, string> serializer
    , Func<string, object> deserializer
    , Func<byte[], object> deserializerBytes) : ILightJsonHelper
{
    public T? Deserialize<T>(string json) => (T)deserializer.Invoke(json);

    public T? Deserialize<T>(byte[] json) => (T)deserializerBytes.Invoke(json);

    public string Serialize<T>(T value) => serializer.Invoke(value!);
}
