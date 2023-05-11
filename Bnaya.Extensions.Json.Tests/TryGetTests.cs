using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class TryGetTests: BaseTests
    {
        #region Ctor

        public TryGetTests(ITestOutputHelper outputHelper): base(outputHelper) { }

        #endregion Ctor

        protected override string JSON_INDENT { get;  } =
                                    """
                                    {
                                      "B": {
                                        "B1": ["Very", "Cool"],
                                        "B2": {
                                            "B21": {
                                              "B211": "OK"
                                            },
                                            "B22": 22,   
                                            "B23": true,
                                            "B24": 1.8,
                                            "B25": "2007-09-01T10:35:01",
                                            "B26": "2007-09-01T10:35:01+02"
                                        }
                                      }
                                    }
                                    """;

        #region TryGetValue_Test

        [Theory]
        [InlineData("B.B1", """
                                 ["Very","Cool"]
                                 """)]
        [InlineData("B.B1.*", """
                                 Very
                                 """)]
        [InlineData("B.B2.B21", """{"B211":"OK"}""")]
        public void TryGetValue_Test(string path, string expected)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            Assert.True(source.TryGetValue(out JsonElement value, path));
            string actual = value.ToString()
                                 .Replace(" ", "")
                                 .Replace(Environment.NewLine, "");
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetValue_Test

        #region TryGetString_Test

        [Theory]
        [InlineData("B.B1", """
                                 ["Very","Cool"]
                                 """,
                        false, "Array cannot be mapped to a string" )]
        [InlineData("B.B1.*", """
                                 Very
                                 """)]
        [InlineData("B.B2.*.B211", """OK""")]
        public void TryGetString_Test(string path, string expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetString(out var actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetString_Test

        #region TryGetDateTime_Test

        [Theory]
        [InlineData("B.B1", "2001-01-01T01:01:01",
                        false, "Array cannot be mapped to a date" )]
        [InlineData("B.*.B25", "2007-09-01T10:35:01")]
        //[InlineData("B.*.B26", "2007-09-01T10:35:01+localtime")]
        public void TryGetDateTime_Test(string path, DateTime expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetDateTime(out var actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetDateTime_Test

        #region TryGetDateTimeOffset_Test

        [Theory]
        [InlineData("B.B1", "2001-01-01T01:01:01",
                        false, "Array cannot be mapped to a date" )]
        [InlineData("B.*.B25", "2007-09-01T10:35:01")]
        [InlineData("B.*.B26", "2007-09-01T10:35:01+02")]
        public void TryGetDateTimeOffset_Test(string path, DateTimeOffset expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetDateTimeOffset(out var actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetDateTimeOffset_Test

        #region TryGetBoolean_Test

        [Theory]
        [InlineData("B.B2.B23", true)]
        [InlineData("B.B2.*.B211", false, false, "string doesn't mapped to a bool")]
        public void TryGetBoolean_Test(string path, bool expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetBoolean(out var actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetBoolean_Test

        #region TryGetDouble_Test

        [Theory]
        [InlineData("B.B2.B22", 22 )]
        [InlineData("B.B2.B24", 1.8 )]
        [InlineData("B.B2.B23", 0,  false, "not a double")]
        public void TryGetDouble_Test(string path, double expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out double actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetDouble_Test

        #region TryGetInt_Test

        [Theory]
        [InlineData("B.B2.B22", 22 )]
        [InlineData("B.B2.B24", 0, false, "it is a double" )]
        [InlineData("B.B2.B23", 0,  false, "not a number")]
        public void TryGetInt_Test(string path, int expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out int actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetInt_Test

        #region TryGetUInt_Test

        [Theory]
        [InlineData("B.B2.B22", 22 )]
        [InlineData("B.B2.B24", 0, false, "it is a double" )]
        [InlineData("B.B2.B23", 0,  false, "not a number")]
        public void TryGetUInt_Test(string path, uint expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out uint actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetUInt_Test

        #region TryGetDecimal_Test

        [Theory]
        [InlineData("B.B2.B22", 22 )]
        [InlineData("B.B2.B24", 1.8 )]
        [InlineData("B.B2.B23", 0,  false, "not a double")]
        public void TryGetDecimal_Test(string path, decimal expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out decimal actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetDecimal_Test

        #region TryGetFloat_Test

        [Theory]
        [InlineData("B.B2.B22", 22 )]
        [InlineData("B.B2.B24", 1.8 )]
        [InlineData("B.B2.B23", 0,  false, "not a double")]
        public void TryGetFloat_Test(string path, float expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out float actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetFloat_Test

        #region TryGetSByte_Test

        [Theory]
        [InlineData("B.B2.B22", 22)]
        [InlineData("B.B2.B24", 0, false, "it is a double")]
        [InlineData("B.B2.B23", 0, false, "not a number")]
        public void TryGetSByte_Test(string path, sbyte expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out sbyte actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetSByte_Test

        #region TryGetLong_Test

        [Theory]
        [InlineData("B.B2.B22", 22)]
        [InlineData("B.B2.B24", 0, false, "it is a double")]
        [InlineData("B.B2.B23", 0, false, "not a number")]
        public void TryGetLong_Test(string path, long expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out long actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetLong_Test

        #region TryGetULong_Test

        [Theory]
        [InlineData("B.B2.B22", 22)]
        [InlineData("B.B2.B24", 0, false, "it is a double")]
        [InlineData("B.B2.B23", 0, false, "not a number")]
        public void TryGetULong_Test(string path, ulong expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out ulong actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetULong_Test

        #region TryGetShort_Test

        [Theory]
        [InlineData("B.B2.B22", 22)]
        [InlineData("B.B2.B24", 0, false, "it is a double")]
        [InlineData("B.B2.B23", 0, false, "not a number")]
        public void TryGetShort_Test(string path, short expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out short actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetShort_Test

        #region TryGetUShort_Test

        [Theory]
        [InlineData("B.B2.B22", 22)]
        [InlineData("B.B2.B24", 0, false, "it is a double")]
        [InlineData("B.B2.B23", 0, false, "not a number")]
        public void TryGetUShort_Test(string path, short expected, bool shouldSucceed = true, string? reason = null)
        {
            var source = JsonDocument.Parse(JSON_INDENT);
            bool succeed = source.TryGetNumber(out short actual, path);
            _outputHelper.WriteLine($"""
                                Succeed: {succeed}
                                Result: 
                                {actual}
                                Expected:
                                {expected}
                                Reason:
                                {reason}
                                """);
            if (!shouldSucceed)
            {
                Assert.False(succeed);
                return;
            }
            Assert.True(succeed);
            Assert.Equal(expected, actual);
        }

        #endregion // TryGetUShort_Test
    }
}
