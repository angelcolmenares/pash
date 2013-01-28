namespace System.Management.Automation
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateLengthAttribute : ValidateEnumeratedArgumentsAttribute
    {
        private int maxLength;
        private int minLength;

        public ValidateLengthAttribute(int minLength, int maxLength)
        {
            if (minLength < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("minLength", minLength);
            }
            if (maxLength <= 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("maxLength", maxLength);
            }
            if (maxLength < minLength)
            {
                throw new ValidationMetadataException("ValidateLengthMaxLengthSmallerThanMinLength", null, Metadata.ValidateLengthMaxLengthSmallerThanMinLength, new object[0]);
            }
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        protected override void ValidateElement(object element)
        {
            string str = element as string;
            if (str == null)
            {
                throw new ValidationMetadataException("ValidateLengthNotString", null, Metadata.ValidateLengthNotString, new object[0]);
            }
            int length = str.Length;
            if (length < this.minLength)
            {
                throw new ValidationMetadataException("ValidateLengthMinLengthFailure", null, Metadata.ValidateLengthMinLengthFailure, new object[] { this.minLength, length });
            }
            if (length > this.maxLength)
            {
                throw new ValidationMetadataException("ValidateLengthMaxLengthFailure", null, Metadata.ValidateLengthMaxLengthFailure, new object[] { this.maxLength, length });
            }
        }

        public int MaxLength
        {
            get
            {
                return this.maxLength;
            }
        }

        public int MinLength
        {
            get
            {
                return this.minLength;
            }
        }
    }
}

