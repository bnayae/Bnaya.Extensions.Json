using Xunit;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class AsStringTests
    {
        private const string JSON = "{\"A\":12,\"B\":{\"C\":\"Z\"}}";

        private const string JSON_INDENT =
@"{
  ""A"": 12,
  ""B"": {
    ""C"": ""Z""
  }
}";
        private static readonly JsonWriterOptions OPT_INDENT =
            new JsonWriterOptions { Indented = true };



        [Fact]
        public void AsString_Default_Test()
        {
            var json = JsonDocument.Parse(JSON);
            string result = json.AsString();
            Assert.Equal(JSON, result);
        }

        [Fact]
        public void AsString_Default_To_Indent_Test()
        {
            var json = JsonDocument.Parse(JSON);
            string result = json.AsString(OPT_INDENT);
            Assert.Equal(JSON_INDENT, result);
        }

        [Fact]
        public void AsString_Indent_To_Default_Test()
        {
            var json = JsonDocument.Parse(JSON_INDENT);
            string result = json.AsString();
            Assert.Equal(JSON, result);
        }

        [Fact]
        public void AsString_Indent_To_Indent_Test()
        {
            var json = JsonDocument.Parse(JSON_INDENT);
            string result = json.AsString(OPT_INDENT);
            Assert.Equal(JSON_INDENT, result);
        }
    }
}
