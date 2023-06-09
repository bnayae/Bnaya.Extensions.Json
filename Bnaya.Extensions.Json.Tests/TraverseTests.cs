using System.Collections.Immutable;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class TraverseTests
    {
        private readonly ITestOutputHelper _outputHelper;

        #region Ctor

        public TraverseTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion Ctor


        private const string JSON_INDENT =
                """
                {
                  "A": 12,
                  "B": {
                    "B1": "Cool",
                    "B2": {
                        "B21": {
                          "B211": 211
                        },
                        "B22": 22    
                    }
                  },
                  "C": ["C1", "C2"],
                  "D": [{ "D1": 1}, "D2", 3]
                }
                """;

        #region Write

        private void Write(JsonDocument source, JsonElement positive)
        {
            _outputHelper.WriteLine("Source:-----------------");
            _outputHelper.WriteLine(source.RootElement.AsString());
            _outputHelper.WriteLine("Result:-----------------");
            _outputHelper.WriteLine(positive.AsString());
        }

        #endregion // Write

        #region Traverse_Remove_Test

        [Fact]
        public void Traverse_Remove_Test()
        {
            #region string expected = ...

            string expected =
                """
                {
                  "A": 12,
                  "B": {
                    "B1": "Cool",
                    "B2": {
                      "B21": {
                        "B211": 211
                      },
                      "B22": 22
                    }
                  },
                  "D": [
                    {
                      "D1": 1
                    },
                    "D2",
                    3
                  ]
                }
                """;

            #endregion // string expected = ...

            var source = JsonDocument.Parse(JSON_INDENT);

            var result = source.RootElement.Remove("C");

            Write(source, result);
            string actual = result.AsIndentString();
            Assert.Equal(expected, actual);
        }

        #endregion // Traverse_Remove_Test
    }
}
