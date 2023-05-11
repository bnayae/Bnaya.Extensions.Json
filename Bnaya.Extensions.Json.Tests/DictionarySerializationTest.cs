using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Xunit;

using static System.Text.Json.Extension.Constants;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class DictionarySerializationTest
    {
        private readonly static Func<Dictionary<ConsoleColor, string>, Dictionary<ConsoleColor, string>, bool> COMPARE_DIC =
            (a, b) => a.Count == b.Count && a.All(p => b[p.Key] == p.Value);
        private readonly static Func<Dictionary<string, object>, Dictionary<string, object>, bool> COMPARE_STR_OBJ_DIC =
            (a, b) => a.Count == b.Count && a.All(p => b[p.Key].Equals(p.Value));
        private readonly static Func<ImmutableDictionary<ConsoleColor, string>, ImmutableDictionary<ConsoleColor, string>, bool> COMPARE_IMM_DIC =
            (a, b) => a.Count == b.Count && a.All(p => b[p.Key] == p.Value);

        #region Dictionary_Test

        [Fact]
        public void Dictionary_Test()
        {
            var source = new Dictionary<ConsoleColor, string>
            {
                [ConsoleColor.Blue] = nameof(ConsoleColor.Blue),
                [ConsoleColor.White] = nameof(ConsoleColor.White)
            };

            source.AssertSerialization(COMPARE_DIC);
        }

        #endregion // Dictionary_Test

        #region Dictionary_WithoutConvertor_Test

        [Fact(Skip = "work on .NET 5")]
        public void Dictionary_WithoutConvertor_Test()
        {
            var source = new Dictionary<ConsoleColor, string>
            {
                [ConsoleColor.Blue] = nameof(ConsoleColor.Blue),
                [ConsoleColor.White] = nameof(ConsoleColor.White)
            };

            try
            {
                source.AssertSerialization(
                    COMPARE_DIC,
                    options: SerializerOptionsWithoutConverters);
                throw new Exception("Unexpected");
            }
            catch (NotSupportedException)
            {
                // expected
            }
        }

        #endregion // Dictionary_WithoutConvertor_Test

        #region Dictionary_String_Object_Test

        [Fact(Skip = "not supported")]
        public void Dictionary_String_Object_Test()
        {
            var source = new Dictionary<string, object>
            {
                ["A"] = nameof(ConsoleColor.Blue),
                ["B"] = new Foo(10, "Bamby", DateTime.Now)
            };

            source.AssertSerialization(COMPARE_STR_OBJ_DIC);
        }

        #endregion // Dictionary_String_Object_Test

        #region Foo_Test

        [Fact]
        public void Foo_Test()
        {
            var source = new Foo(2, "B", DateTime.Now);

            source.AssertSerialization();
        }

        #endregion // Foo_Test

        #region Dictionary_Complex_Key_Without_ConvertorTest

        [Fact]
        public void Dictionary_Complex_Key_Without_ConvertorTest()
        {
            var source = new Dictionary<Foo, string>
            {
                [new Foo(2, "B", DateTime.Now)] = "B",
                [new Foo(3, "Q", DateTime.Now.AddDays(1))] = "Q"
            };

            try
            {
                source.AssertSerialization((a, b) => a.Count == b.Count && a.All(p => b[p.Key] == p.Value));
                throw new Exception("Unexpected");
            }
            catch (NotSupportedException)
            {
                // expected
            }
        }

        #endregion // Dictionary_Complex_Key_Without_ConvertorTest

        #region Dictionary_Complex_Value_Test

        [Fact]
        public void Dictionary_Complex_Value_Test()
        {
            var source = new Dictionary<int, Foo>
            {
                [2] = new Foo(2, "B", DateTime.Now),
                [3] = new Foo(3, "Q", DateTime.Now.AddDays(1))
            };

            source.AssertSerialization((a, b) => a.Count == b.Count && a.All(p => b[p.Key] == p.Value));
        }

        #endregion // Dictionary_Complex_Value_Test

        #region Dictionary_KeyValueStyle_Test

        [Fact]
        public void Dictionary_KeyValueStyle_Test()
        {
            const string json = """
            {
                "kv": 
                    {
                        "X": ["A", "B"],
                        "Y": ["C", "D"] 
                    }           
            }
            """;

            var kvs = JsonSerializer.Deserialize<KeyValueStyle>(json, SerializerOptions);
            var kv = kvs.KV;
            kv["X"].SequenceEqual(new[] { "A", "B" });
            kv["Y"].SequenceEqual(new[] { "C", "D" });
        }

        #endregion // Dictionary_KeyValueStyle_Test

        #region Dictionary_KeyValueStyle_auto_Test

        [Fact]
        public void Dictionary_KeyValueStyle_auto_Test()
        {
            var source = new KeyValueStyle
            {
                KV = ImmutableDictionary<string, string[]>.Empty.Add("X", new[] { "A", "B" })
            };
            source.AssertSerialization((a, b) => true);

        }

        #endregion // Dictionary_KeyValueStyle_auto_Test

        #region FromFile_Test

        [Fact]
        public void FromFile_Test()
        {
            string json = File.ReadAllText("data-v1.json");
            var data = JsonSerializer.Deserialize<FromFile>(json, SerializerOptions);

            validation(data);

            var json1 = JsonSerializer.Serialize(data, SerializerOptions);
            var data1 = JsonSerializer.Deserialize<FromFile>(json1, SerializerOptions);

            validation(data1);

            void validation(FromFile entity)
            {
                Assert.Equal("bnaya", data.Name);
                Assert.Equal("user", data.Role);
                Assert.Equal(5, data.Stars);
                Assert.Equal(ConsoleColor.Red, data.Color);
                Assert.True(entity.Tags.SequenceEqual(new[] { "A", "B" }));
                Assert.True(entity.Headers
                                        .OrderBy(m => m.Key)
                                        .SequenceEqual(new[] {
                                                KeyValuePair.Create("Head1", ConsoleColor.Blue ),
                                                KeyValuePair.Create("Head2", ConsoleColor.Green )
                                        }));
                Assert.True(entity.Map.OrderBy(m => m.Key)
                                        .SequenceEqual(new[] {
                                            KeyValuePair.Create("X", KeyValuePair.Create( "A", ConsoleColor.Cyan)),
                                            KeyValuePair.Create("Y", KeyValuePair.Create( "B", ConsoleColor.DarkBlue))
                                        }));
            }
        }

        #endregion // FromFile_Test

        #region FromFile_Interface_Test

        [Fact]
        public void FromFile_Interface_Test()
        {
            string json = File.ReadAllText("data-v1.json");
            var data = JsonSerializer.Deserialize<FromFileInterface>(json, SerializerOptions);

            validation(data);

            var json1 = JsonSerializer.Serialize(data, SerializerOptions);
            var data1 = JsonSerializer.Deserialize<FromFileInterface>(json1, SerializerOptions);

            validation(data1);

            void validation(FromFileInterface entity)
            {
                Assert.Equal("bnaya", data.Name);
                Assert.Equal("user", data.Role);
                Assert.Equal(5, data.Stars);
                Assert.Equal(ConsoleColor.Red, data.Color);
                Assert.True(entity.Tags.SequenceEqual(new[] { "A", "B" }));
                Assert.True(entity.Headers
                                        .OrderBy(m => m.Key)
                                        .SequenceEqual(new[] {
                                                KeyValuePair.Create("Head1", ConsoleColor.Blue ),
                                                KeyValuePair.Create("Head2", ConsoleColor.Green )
                                        }));
                Assert.True(entity.Map.OrderBy(m => m.Key)
                                        .SequenceEqual(new[] {
                                            KeyValuePair.Create("X", KeyValuePair.Create( "A", ConsoleColor.Cyan)),
                                            KeyValuePair.Create("Y", KeyValuePair.Create( "B", ConsoleColor.DarkBlue))
                                        }));
            }
        }

        #endregion // FromFile_Interface_Test
    }
}
