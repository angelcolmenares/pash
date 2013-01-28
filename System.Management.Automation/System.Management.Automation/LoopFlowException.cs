namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class LoopFlowException : FlowControlException
    {
        protected LoopFlowException(string label)
        {
            this.Label = label ?? "";
        }

        internal bool MatchLabel(string loopLabel)
        {
            return MatchLoopLabel(this.Label, loopLabel);
        }

        internal static bool MatchLoopLabel(string flowLabel, string loopLabel)
        {
            if (!string.IsNullOrEmpty(flowLabel))
            {
                return flowLabel.Equals(loopLabel, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        internal string Label { get; set; }
    }
}

