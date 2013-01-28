namespace System.Management.Automation
{
    using System;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidatePatternAttribute : ValidateEnumeratedArgumentsAttribute
    {
        private RegexOptions options = RegexOptions.IgnoreCase;
        private string regexPattern;

        public ValidatePatternAttribute(string regexPattern)
        {
            if (string.IsNullOrEmpty(regexPattern))
            {
                throw PSTraceSource.NewArgumentException("regexPattern");
            }
            this.regexPattern = regexPattern;
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullFailure, new object[0]);
            }
            string input = element.ToString();
            Regex regex = null;
            regex = new Regex(this.regexPattern, this.options);
            if (!regex.Match(input).Success)
            {
                throw new ValidationMetadataException("ValidatePatternFailure", null, Metadata.ValidatePatternFailure, new object[] { input, this.regexPattern });
            }
        }

        public RegexOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
            }
        }

        public string RegexPattern
        {
            get
            {
                return this.regexPattern;
            }
        }
    }
}

