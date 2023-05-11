using System.Collections.Immutable;

using FakeItEasy;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public abstract class BaseTests
    {
        protected readonly ITestOutputHelper _outputHelper;

        #region Ctor

        public BaseTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion Ctor


        protected virtual string JSON_INDENT { get; } =
                                """
                                {
                                  "A": 10,
                                  "B": [
                                    { "Val": 40 },
                                    { "Val": 20 },
                                    { "Factor": 20 } 
                                  ],
                                  "C": [0, 25, 50, 100],
                                  "Note": "Re-shape json"
                                }
                                """;

        #region Write

        protected void Write(JsonDocument source, JsonElement target)
        {
            _outputHelper.WriteLine("Source:-----------------");
            _outputHelper.WriteLine(source.RootElement.AsString());
            _outputHelper.WriteLine("Target:-----------------");
            _outputHelper.WriteLine(target.AsString());
        }

        #endregion // Write
    }
}
