using System.Collections.Immutable;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class KeepTests : BaseTests
    {
        #region Ctor

        public KeepTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        #endregion Ctor

        #region Keep_Test

        [Theory]
        [InlineData("B.[1]", """
                                 {"B":[{"Val":20}]}
                                 """)]
        [InlineData("B.[1].val", """
                                     {"B":[{"Val":20}]}
                                     """)]
        [InlineData("B.*.val", """
                                    {"B":[{"Val":40},{"Val":20}]}
                                    """)]
        [InlineData("B.[]", """
                                 {"B":[{"Val":40},{"Val":20},{"Factor":20}]}
                                 """)]
        [InlineData("B.[].val", """
                                    {"B":[{"Val":40},{"Val":20}]}
                                    """)]
        [InlineData("B.[].val", """
                                     {}
                                     """, true)]
        [InlineData("B.[].Factor", """
                                        {"B":[{"Factor":20}]}
                                        """, true)]
        public void Keep_Test(string path, string expected, bool caseSensitive = false)
        {
            _outputHelper.WriteLine(path);
            var source = JsonDocument.Parse(JSON_INDENT);
            var target = source.RootElement.Keep(path, caseSensitive);

            Write(source, target);
            Assert.Equal(
                expected,
                target.AsString());
        }

        #endregion // Keep_Test

        #region Keep_Replace_Test

        [Theory]
        [InlineData("B.[1].*", """
                                 {"B":[{"Val":30}]}
                                 """)]
        [InlineData("B.[1].val", """
                                     {"B":[{"Val":30}]}
                                     """)]
        [InlineData("B.*.val", """
                                    {"B":[{"Val":50},{"Val":30}]}
                                    """)]
        [InlineData("B.[].val", """
                                    {"B":[{"Val":50},{"Val":30}]}
                                    """)]
        [InlineData("B.[].Factor", """
                                        {"B":[{"Factor":30}]}
                                        """, true)]
        public void Keep_Replace_Test(string path, string expected, bool caseSensitive = false)
        {
            _outputHelper.WriteLine(path);
            var source = JsonDocument.Parse(JSON_INDENT);

            JsonElement? onMatch(JsonElement current, IImmutableList<string> breadcrumbs)
            {
                if (current.ValueKind == JsonValueKind.Number)
                {
                    var v = current.GetDecimal();
                    return (v + 10).ToJson();
                }
                return null;
            }

            var target = source.RootElement.Keep(path, caseSensitive, onMatch);

            Write(source, target);
            Assert.Equal(
                expected,
                target.AsString());
        }

        #endregion // Keep_Replace_Test
    }
}
