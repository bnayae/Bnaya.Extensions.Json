using System.Collections.Immutable;
using System.Text.RegularExpressions;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class ReplaceTests : BaseTests
    {
        private static readonly Regex WHITE_SPACES = new Regex(@"\s*");

        #region Ctor

        public ReplaceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        #endregion Ctor

        #region Replace_Test

        [Theory]
        [InlineData("B.[1].*", """
                                 {
                                   "A": 10,
                                   "B": [
                                     { "Val": 40 },
                                     { "Val": 30 },
                                     { "Factor": 20 } 
                                   ],
                                   "C": [0, 25, 50, 100],
                                   "Note": "Re-shape json"
                                 }
                                 """)]
        [InlineData("B.[1].val", """
                                     {
                                       "A": 10,
                                       "B": [
                                         { "Val": 40 },
                                         { "Val": 30 },
                                         { "Factor": 20 } 
                                       ],
                                       "C": [0, 25, 50, 100],
                                       "Note": "Re-shape json"
                                     }
                                     """)]
        [InlineData("B.*.val", """
                                    {
                                      "A": 10,
                                      "B": [
                                        { "Val": 50 },
                                        { "Val": 30 },
                                        { "Factor": 20 } 
                                      ],
                                      "C": [0, 25, 50, 100],
                                      "Note": "Re-shape json"
                                    }
                                    """)]
        [InlineData("B.[].val", """
                                     {
                                       "A": 10,
                                       "B": [
                                         { "Val": 50 },
                                         { "Val": 30 },
                                         { "Factor": 20 } 
                                       ],
                                       "C": [0, 25, 50, 100],
                                       "Note": "Re-shape json"
                                     }
                                    """)]
        [InlineData("B.[].Factor", """
                                        {
                                          "A": 10,
                                          "B": [
                                            { "Val": 40 },
                                            { "Val": 20 },
                                            { "Factor": 30 } 
                                          ],
                                          "C": [0, 25, 50, 100],
                                          "Note": "Re-shape json"
                                        }
                                        """, true)]
        public void Replace_Test(string path, string expected, bool caseSensitive = false)
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
                return current;
            }

            var target = source.RootElement.Replace(path, onMatch, caseSensitive);

            Write(source, target);
            var expectedTrim = WHITE_SPACES.Replace(expected, "");
            var targetTrim = WHITE_SPACES.Replace(target.AsString(), "");
            Assert.Equal(
                expectedTrim,
                targetTrim);
        }

        #endregion // Replace_Test
    }
}
