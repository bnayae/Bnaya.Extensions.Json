// credit: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to

namespace System.Text.Json
{
    /// <summary>
    /// Traverse flow instruction, how to proceed.
    /// </summary>
    [Obsolete("deprecated")]
    public record struct TraverseFlowInstruction
    {
        #region Drill

        /// <summary>
        /// Instruct to continue into children.
        /// </summary>
        /// <returns></returns>
        public static TraverseFlowInstruction Drill { get; } = Do(TraverseFlowDeprecated.Drill);

        #endregion // Drill

        #region Skip

        /// <summary>
        /// Instruct to continue on next sibling.
        /// </summary>
        /// <returns></returns>
        public static TraverseFlowInstruction Skip { get; } = Do(TraverseFlowDeprecated.Skip);

        #endregion // Skip

        #region SkipToParent

        /// <summary>
        /// Instruct to skip all sibling.
        /// </summary>
        /// <returns></returns>
        public static TraverseFlowInstruction SkipToParent { get; } = Do(TraverseFlowDeprecated.SkipToParent);

        #endregion // SkipToParent

        #region Yield

        /// <summary>
        /// Instruct to yield and continue on next sibling.
        /// </summary>
        /// <returns></returns>
        public static TraverseFlowInstruction Yield { get; } = Do(true);

        #endregion // Yield

        #region YieldAndSkipToParent

        /// <summary>
        /// Instruct to yield and continue on skip all sibling.
        /// </summary>
        /// <returns></returns>
        public static TraverseFlowInstruction YieldAndSkipToParent { get; } = Do(true, TraverseFlowDeprecated.SkipToParent);

        #endregion // YieldAndSkipToParent

        #region Do

        /// <summary>
        /// Traverse flow instruction, how to proceed.
        /// </summary>
        /// <param name="pick">When true: result with yield</param>
        /// <param name="flow">Instruct how to continue</param>
        /// <returns></returns>
        public static TraverseFlowInstruction Do(
                        bool pick,
                        TraverseFlowDeprecated flow = TraverseFlowDeprecated.SkipWhenMatch) =>
                                                new TraverseFlowInstruction(pick, flow);

        /// <summary>
        /// Traverse flow instruction, how to proceed.
        /// </summary>
        /// <param name="flow">Instruct how to continue</param>
        /// <param name="pick">When true: result with yield</param>
        /// <returns></returns>
        public static TraverseFlowInstruction Do(TraverseFlowDeprecated flow, bool pick = false) =>
                                                new TraverseFlowInstruction(pick, flow);

        #endregion // Do

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TraverseFlowInstruction"/> struct.
        /// </summary>
        /// <param name="pick">if set to <c>true</c> [pick].</param>
        /// <param name="flow">The flow.</param>
        public TraverseFlowInstruction(bool pick, TraverseFlowDeprecated flow)
        {
            this.Pick = pick;
            Flow = flow;
        }

        #endregion // Ctor

        #region Pick

        /// <summary>
        /// When true: result with yield
        /// </summary>
        public bool Pick { get; }

        #endregion // Pick

        #region Flow

        /// <summary>
        /// Instruct how to continue
        /// </summary>
        public TraverseFlowDeprecated Flow { get; }

        #endregion // Flow

        #region Deconstruct

        /// <summary>
        /// De-constructs the specified pick.
        /// </summary>
        /// <param name="pick">When true: result with yield.</param>
        /// <param name="flow">Instruct how to continue.</param>
        /// <returns></returns>
        public void Deconstruct(out bool pick, out TraverseFlowDeprecated flow)
        {
            pick = Pick;
            flow = Flow;
        }

        #endregion // Deconstruct
    }
}
