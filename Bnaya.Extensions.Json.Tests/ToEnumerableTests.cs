using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;
using Xunit.Abstractions;

using static System.Text.Json.TraverseFlow;
using static System.Text.Json.TraverseInstruction;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class ToEnumerableTests : BaseTests
    {
        #region Ctor

        public ToEnumerableTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        #endregion Ctor

        #region JSON_INDENT

        protected override string JSON_INDENT { get; } =
            """
            {
              "projects": [
                {
                    "key": "cloud-d",
                    "duration": 8
                },
                {
                    "key": "cloud-x",
                    "duration": 4
                }
              ],
              "users": [
                {
                    "name": "bnaya",
                    "address":{ "country": "Italy" },
                    "skills": [ "c#", "Typescript", "neo4J", "elasticsearch"],
                    "roles": [ "ARCHITECT", "CTO" ]
                },
                {
                    "name": "mike",
                    "address":{ "country": "China" },
                    "skills": [ "rust", "node.js"],
                    "roles": [ "ARCHITECT", "VP-RND" ],
                    "rank": {
                        "job": 10,
                        "relationship": {
                            "projects": [
                                {
                                    "key": "cloud-d",
                                    "team": 5
                                },
                                {
                                    "key": "cloud-x",
                                    "team": 32
                                }
                            ]
                        }
                    },
                    "education": [{
                        "MBA": {
                            "institute": "UCLA",
                            "year": 2011
                        }
                    }]
                }
              ]
            }
            """;

        #endregion // JSON_INDENT

        #region DrillPattern_Test

        [Fact]
        public void DrillPattern_Test()
        {
            var source = JsonDocument.Parse(JSON_INDENT);

            TraverseInstruction Predicate(JsonElement current, IImmutableList<string> breadcrumbs)
            {
                if (breadcrumbs.Count < 4)
                    return ToChildren;

                if (breadcrumbs[^4] == "relationship" &&
                    breadcrumbs[^3] == "projects" &&
                    breadcrumbs[^1] == "key")
                {
                    return new TraverseInstruction(Stop, TraverseAction.Take);
                }

                return ToChildren;
            }
            var items = source.ToEnumerable(Predicate);
            var results = items.Select(m => m.GetString()).ToArray();
            Assert.Equal(2, results.Length);
            string[] expected = { "cloud-d", "cloud-x" };
            Assert.True(expected.SequenceEqual(results));
        }

        #endregion // DrillPattern_Test
    }
}
