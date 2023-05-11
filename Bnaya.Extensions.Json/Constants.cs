using System.Buffers;
using System.Text.Json.Serialization;

namespace System.Text.Json.Extension
{
    /// <summary>
    /// Json related
    /// </summary>
    public static class Constants
    {
        private static readonly JsonStringEnumConverter EnumConvertor = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);


        #region CreateEmptyJsonElement

        /// <summary>
        /// Create Empty Json Element
        /// </summary>
        /// <returns></returns>
        public static JsonElement CreateEmptyJsonElement()
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }

            var reader = new Utf8JsonReader(buffer.WrittenSpan);
            JsonDocument result = JsonDocument.ParseValue(ref reader);
            return result.RootElement;
        }

        #endregion // CreateEmptyJsonElement

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="Constants"/> class.
        /// </summary>
        static Constants()
        {
            SerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { EnumConvertor, JsonMemoryBytesConverterFactory.Default }
            };
            SerializerOptionsWithoutConverters = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { EnumConvertor }
            };
        }

        #endregion // Ctor

        /// <summary>
        /// Gets the serializer options with indent.
        /// </summary>
        public static JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// Gets the serializer options with indent.
        /// </summary>
        public static JsonSerializerOptions SerializerOptionsWithoutConverters { get; }
    }

}
