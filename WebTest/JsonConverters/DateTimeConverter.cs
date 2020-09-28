using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebTest.JsonConverters
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private string _formatString = "yyyy-MM-dd HH:mm:ss";
        
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var date = DateTime.Parse(reader.GetString());
            return date;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_formatString));
        }
    }
}
