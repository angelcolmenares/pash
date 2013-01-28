namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ValidationMetadataException : MetadataException
    {
        private bool _swallowException;
        internal const string InvalidValueFailure = "InvalidValueFailure";
        internal const string ValidateCountMaxLengthFailure = "ValidateCountMaxLengthFailure";
        internal const string ValidateCountMaxLengthSmallerThanMinLength = "ValidateCountMaxLengthSmallerThanMinLength";
        internal const string ValidateCountMinLengthFailure = "ValidateCountMinLengthFailure";
        internal const string ValidateCountNotInArray = "ValidateCountNotInArray";
        internal const string ValidateFailureResult = "ValidateFailureResult";
        internal const string ValidateLengthMaxLengthFailure = "ValidateLengthMaxLengthFailure";
        internal const string ValidateLengthMaxLengthSmallerThanMinLength = "ValidateLengthMaxLengthSmallerThanMinLength";
        internal const string ValidateLengthMinLengthFailure = "ValidateLengthMinLengthFailure";
        internal const string ValidateLengthNotString = "ValidateLengthNotString";
        internal const string ValidatePatternFailure = "ValidatePatternFailure";
        internal const string ValidateRangeElementType = "ValidateRangeElementType";
        internal const string ValidateRangeGreaterThanMaxRangeFailure = "ValidateRangeGreaterThanMaxRangeFailure";
        internal const string ValidateRangeMaxRangeSmallerThanMinRange = "ValidateRangeMaxRangeSmallerThanMinRange";
        internal const string ValidateRangeMinRangeMaxRangeType = "ValidateRangeMinRangeMaxRangeType";
        internal const string ValidateRangeNotIComparable = "ValidateRangeNotIComparable";
        internal const string ValidateRangeSmallerThanMinRangeFailure = "ValidateRangeSmallerThanMinRangeFailure";
        internal const string ValidateScriptFailure = "ValidateScriptFailure";
        internal const string ValidateSetFailure = "ValidateSetFailure";
        internal const string ValidateVersionFailure = "ValidateVersionFailure";

        public ValidationMetadataException() : base(typeof(ValidationMetadataException).FullName)
        {
        }

        public ValidationMetadataException(string message) : this(message, false)
        {
        }

        protected ValidationMetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal ValidationMetadataException(string message, bool swallowException) : base(message)
        {
            this._swallowException = swallowException;
        }

        public ValidationMetadataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ValidationMetadataException(string errorId, Exception innerException, string resourceStr, params object[] arguments) : base(errorId, innerException, resourceStr, arguments)
        {
        }

        internal bool SwallowException
        {
            get
            {
                return this._swallowException;
            }
        }
    }
}

