using System.IO;

using Xunit;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class ToStreamTests
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
        public void ToStream_Default_Test()
        {
            var json = JsonDocument.Parse(JSON);
            var srm = json.ToStream() as MemoryStream;
            string result = Encoding.UTF8.GetString(srm.ToArray());
            Assert.Equal(JSON, result);
        }

        [Fact]
        public void ToStream_Default_To_Indent_Test()
        {
            var json = JsonDocument.Parse(JSON);
            var srm = json.ToStream(OPT_INDENT) as MemoryStream;
            string result = Encoding.UTF8.GetString(srm.ToArray());
            Assert.Equal(JSON_INDENT, result);
        }

        [Fact]
        public void ToStream_Indent_To_Default_Test()
        {
            var json = JsonDocument.Parse(JSON_INDENT);
            var srm = json.ToStream() as MemoryStream;
            string result = Encoding.UTF8.GetString(srm.ToArray());
            Assert.Equal(JSON, result);
        }

        [Fact]
        public void ToStream_Indent_To_Indent_Test()
        {
            var json = JsonDocument.Parse(JSON_INDENT);
            var srm = json.ToStream(OPT_INDENT) as MemoryStream;
            string result = Encoding.UTF8.GetString(srm.ToArray());
            Assert.Equal(JSON_INDENT, result);
        }
    }
}
