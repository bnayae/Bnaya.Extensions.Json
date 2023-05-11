using System.Collections.Immutable;

using FakeItEasy;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class FilterTests: BaseTests
    {
        #region Ctor

        public FilterTests(ITestOutputHelper outputHelper): base(outputHelper) { }

        #endregion Ctor

        #region Filter_Gt30_Test

        [Fact]
        public void Filter_Gt30_Test()
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            var target = source.RootElement.Filter((e, _, _) =>
                                    e.ValueKind != JsonValueKind.Number || e.GetInt32() > 30 ? TraverseFlowWrite.Drill : TraverseFlowWrite.Skip);

            Write(source, target);
            Assert.Equal(
                @"{""B"":[{""Val"":40}],""C"":[50,100],""Note"":""Re-shape json""}",
                target.AsString());
        }

        #endregion // Filter_Gt30_Test
    }
}
