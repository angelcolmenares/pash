namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Globalization;

    internal sealed class ExceptionHandler
    {
        public readonly int EndIndex;
        public readonly Type ExceptionType;
        public readonly int HandlerEndIndex;
        public readonly int HandlerStartIndex;
        public readonly int LabelIndex;
        internal TryCatchFinallyHandler Parent;
        public readonly int StartIndex;

        internal ExceptionHandler(int start, int end, int labelIndex, int handlerStartIndex, int handlerEndIndex, Type exceptionType)
        {
            this.StartIndex = start;
            this.EndIndex = end;
            this.LabelIndex = labelIndex;
            this.ExceptionType = exceptionType;
            this.HandlerStartIndex = handlerStartIndex;
            this.HandlerEndIndex = handlerEndIndex;
        }

        public bool IsBetterThan(ExceptionHandler other)
        {
            return ((other == null) || (this.HandlerStartIndex < other.HandlerStartIndex));
        }

        internal bool IsInsideCatchBlock(int index)
        {
            return ((index >= this.HandlerStartIndex) && (index < this.HandlerEndIndex));
        }

        internal bool IsInsideFinallyBlock(int index)
        {
            return ((this.Parent.IsFinallyBlockExist && (index >= this.Parent.FinallyStartIndex)) && (index < this.Parent.FinallyEndIndex));
        }

        internal bool IsInsideTryBlock(int index)
        {
            return ((index >= this.StartIndex) && (index < this.EndIndex));
        }

        public bool Matches(Type exceptionType)
        {
            if ((this.ExceptionType != null) && !this.ExceptionType.IsAssignableFrom(exceptionType))
            {
                return false;
            }
            return true;
        }

        internal void SetParent(TryCatchFinallyHandler tryHandler)
        {
            this.Parent = tryHandler;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} [{1}-{2}] [{3}->{4}]", new object[] { this.IsFault ? "fault" : ("catch(" + this.ExceptionType.Name + ")"), this.StartIndex, this.EndIndex, this.HandlerStartIndex, this.HandlerEndIndex });
        }

        public bool IsFault
        {
            get
            {
                return (this.ExceptionType == null);
            }
        }
    }
}

