using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebTest.JsonConverters
{
    public class IntStrConverter : JsonConverter<Int32>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int o = 0;
            if(reader.TokenType == JsonTokenType.Number)
            {
                reader.TryGetInt32(out o);
            }
            else
            {
                var s = reader.GetString();
                int.TryParse(s, out o);
            }
            return o;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
