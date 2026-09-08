using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AotOracleTest;

[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserProfile))]
[JsonSerializable(typeof(IList<User>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class JsonContext : JsonSerializerContext
{
    private static readonly JsonSerializerOptions options = new ()
    {
        TypeInfoResolver = Default,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public static JsonSerializerOptions JsonOptions => options;
}
