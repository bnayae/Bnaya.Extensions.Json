using System.Collections.Immutable;

using Xunit;
using Xunit.Abstractions;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class TraverseInstructionTests
    {
        private readonly ITestOutputHelper _outputHelper;

        #region Ctor

        public TraverseInstructionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion Ctor


        #region TraverseInstruction_Test

        [Fact]
        public void TraverseInstruction_Test()
        {
            TraverseInstruction instruction = TraverseFlow.Sibling;
            Assert.Equal(TraverseFlow.Sibling, instruction.Next);
            Assert.Equal(TraverseAction.None, instruction.Marked);
        }

        #endregion // TraverseInstruction_Test
    }
}
