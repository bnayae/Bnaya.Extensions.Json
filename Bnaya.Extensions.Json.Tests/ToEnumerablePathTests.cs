using System;
using System.Linq;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class ToEnumerablePathTests : BaseTests
    {
        #region Ctor

        public ToEnumerablePathTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        #endregion Ctor

        #region JSON_INDENT

        protected override string JSON_INDENT { get; } =
            """
            {
              "name": "bnaya",
              "skills": [
                            "c#", "Typescript", "neo4J", 
                            { "role": [ "architect", "cto" ], "level": 3 },
                            "elasticsearch"
                        ],
              "friends": [
                {
                  "name": "Yaron",
                  "IsSkipper": true
                },
                {
                  "name": "Aviad",
                  "IsSkipper": true
                },
                {
                  "name": "Eyal",
                  "IsSkipper": false
                }
              ]
            }
            """;

        #endregion // JSON_INDENT

        #region ToEnumerable_Path_Test

        [Theory]
        [InlineData("friends.[].name", "Yaron,Aviad,Eyal")]
        [InlineData("friends.*.name", "Yaron,Aviad,Eyal")]
        [InlineData("*.[].name", "Yaron,Aviad,Eyal")]
        [InlineData("friends.[].*", "Yaron,True,Aviad,True,Eyal,False")]
        [InlineData("friends.[1].name", "Aviad")]
        [InlineData("friends.[0].*", "Yaron,True")]
        [InlineData("skills.*.Role.[]", "architect,cto")]
        [InlineData("skills.*.level", "3")]
        [InlineData("skills.[3].role.[]", "architect,cto")]
        [InlineData("skills.[3]", @"{""role"":[""architect"",""cto""],""level"":3}")]
        public void ToEnumerable_Path_Test(string path, string expectedJoined)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            var items = source.ToEnumerable(path).ToArray();

            var results = items.Select(m =>
                m.ValueKind switch
                {
                    JsonValueKind.Number => $"{m.GetInt32()}",
                    JsonValueKind.True => $"True",
                    JsonValueKind.False => $"False",
                    JsonValueKind.Array => string.Join(",", m.EnumerateArray().Select(a => a.GetString())),
                    JsonValueKind.Object => m.AsString(),
                    _ => m.GetString()
                }).ToArray();
            string[] expected = expectedJoined.StartsWith("{") ? new[] { expectedJoined } : expectedJoined.Split(",");
            Assert.True(expected.SequenceEqual(results));
        }

        #endregion // ToEnumerable_Path_Test

        #region ToEnumerable_Path_Array_Test

        [Theory]
        [InlineData("skills.[3].role.[]", JsonValueKind.String, "architect")]
        [InlineData("skills.[3].role", JsonValueKind.Array, "architect,cto")]
        public void ToEnumerable_Path_Array_Test(string path, JsonValueKind expectedKind, string expectedJoined)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            var item = source.ToEnumerable(path).First();
            Assert.Equal(expectedKind, item.ValueKind);
            var res = item.ValueKind switch
            {
                JsonValueKind.Array => string.Join(",", item.EnumerateArray().Select(a => a.GetString())),
                _ => item.GetString()
            };
            Assert.Equal(expectedJoined, res);
        }

        #endregion // ToEnumerable_Path_Array_Test
    }
}
