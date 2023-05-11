using Xunit;

using static System.Text.Json.Extension.Constants;

namespace System.Text.Json.Extension.Extensions.Tests
{
    record RecTest(int id, string name, ConsoleColor color);

    public class SerializeTests
    {
        private static JsonElement Empty = CreateEmptyJsonElement();

        [Fact]
        public void Serialize_Test()
        {
            var rec = new RecTest(10, "John", ConsoleColor.Cyan);
            string json = rec.Serialize();
#pragma warning disable xUnit2000 // Constants and literals should be the expected argument
            Assert.Equal(json, @"{
  ""id"": 10,
  ""name"": ""John"",
  ""color"": ""cyan""
}");
#pragma warning restore xUnit2000 // Constants and literals should be the expected argument
        }

        [Fact]
        public void DeSerialize_Test()
        {
            var rec = new RecTest(10, "John", ConsoleColor.Cyan);
            var j = Empty.Merge(rec);
            var result = j.Deserialize<RecTest>(Constants.SerializerOptions);
            Assert.Equal(rec.ToJson().AsString(), result.ToJson().AsString());
        }
    }
}
