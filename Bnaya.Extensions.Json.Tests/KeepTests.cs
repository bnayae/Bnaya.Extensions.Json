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
    }
}
