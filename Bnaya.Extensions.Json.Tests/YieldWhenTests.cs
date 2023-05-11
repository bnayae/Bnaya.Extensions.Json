using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

using static System.Text.Json.TraverseFlowInstruction;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class YieldWhenTests: BaseTests
    {
        #region Ctor

        public YieldWhenTests(ITestOutputHelper outputHelper): base(outputHelper) { }

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

            TraverseFlowInstruction Predicate(JsonElement json, int deep, IImmutableList<string> breadcrumbs)
            { 
                if(breadcrumbs.Count < 4)
                    return Drill;

                if (breadcrumbs[^4] == "relationship" &&
                    breadcrumbs[^3] == "projects" &&
                    breadcrumbs[^1] == "key")
                {
                    return Yield;
                }

                return Drill;
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
