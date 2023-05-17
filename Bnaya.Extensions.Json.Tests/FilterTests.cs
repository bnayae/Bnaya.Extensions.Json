using System.Collections.Immutable;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class FilterTests : BaseTests
    {
        #region Ctor

        public FilterTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        #endregion Ctor

        #region Filter_Gt30_Test

        [Fact]
        public void Filter_Gt30_Test()
        {
            var source = JsonDocument.Parse(JSON_INDENT);

            TraverseInstruction Strategy(
                            JsonElement e,
                            IImmutableList<string> breadcrumbs)
            { 
                if (e.ValueKind == JsonValueKind.Number)
                {
                    var val = e.GetInt32();
                    if (val > 30)
                        return TraverseInstruction.TakeOrReplace;
                    return TraverseInstruction.SkipToSibling;
                }
                if (e.ValueKind == JsonValueKind.Array || e.ValueKind == JsonValueKind.Object)
                    return TraverseInstruction.ToChildren;
                return TraverseInstruction.TakeOrReplace;
            }

            JsonElement target = source.Filter(Strategy);

            Write(source, target);
            Assert.Equal(
                @"{""B"":[{""Val"":40}],""C"":[50,100],""Note"":""Re-shape json""}",
                target.AsString());
        }

        #endregion // Filter_Gt30_Test
    }
}
