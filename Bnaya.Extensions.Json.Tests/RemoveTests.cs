using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class RemoveTests : BaseTests
    {
        #region Ctor

        public RemoveTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        #endregion Ctor

        #region RemovePath_Test

        [Theory]
        [InlineData("B.[]", """
                                        {"A":10,"B":[],"C":[],"Note":"Re-shape json"}
                                        """)]
        [InlineData("B.*.val", """
                                        {"A":10,"B":[{"Factor":20}],"C":[0,25,50,100],"Note":"Re-shape json"}
                                        """)]
        [InlineData("B.[1]", """
                                        {"A":10,"B":[{"Val":40},{"Factor":20}],"C":[0,50,100],"Note":"Re-shape json"}
                                        """)]
        [InlineData("B", """
                                        {"A":10,"C":[0,25,50,100],"Note":"Re-shape json"}
                                        """)]
        [InlineData("B.[1].val", """
                                        {"A":10,"B":[{"Val":40},{"Factor":20}],"C":[0,25,50,100],"Note":"Re-shape json"}
                                        """)]

        public void RemovePath_Test(string path, string expected, bool caseSensitive = false)
        {
            _outputHelper.WriteLine(path);
            var source = JsonDocument.Parse(JSON_INDENT);
            var target = source.RootElement.Remove(path, caseSensitive);

            Write(source, target);
            Assert.Equal(
                expected.ToJson().AsString(),
                target.AsString());
        }

        #endregion // RemovePath_Test
    }
}
