namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateSetAttribute : ValidateEnumeratedArgumentsAttribute
    {
        private bool ignoreCase = true;
        private string[] validValues;

        public ValidateSetAttribute(params string[] validValues)
        {
            if (validValues == null)
            {
                throw PSTraceSource.NewArgumentNullException("validValues");
            }
            if (validValues.Length == 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("validValues", validValues);
            }
            this.validValues = validValues;
        }

        private string SetAsString()
        {
            string validateSetSeparator = Metadata.ValidateSetSeparator;
            StringBuilder builder = new StringBuilder();
            if (this.validValues.Length > 0)
            {
                foreach (string str2 in this.validValues)
                {
                    builder.Append(str2);
                    builder.Append(validateSetSeparator);
                }
                builder.Remove(builder.Length - validateSetSeparator.Length, validateSetSeparator.Length);
            }
            return builder.ToString();
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullFailure, new object[0]);
            }
            string strB = element.ToString();
            for (int i = 0; i < this.validValues.Length; i++)
            {
                string strA = this.validValues[i];
                if (string.Compare(strA, strB, this.ignoreCase, CultureInfo.InvariantCulture) == 0)
                {
                    return;
                }
            }
            throw new ValidationMetadataException("ValidateSetFailure", null, Metadata.ValidateSetFailure, new object[] { element.ToString(), this.SetAsString() });
        }

        public bool IgnoreCase
        {
            get
            {
                return this.ignoreCase;
            }
            set
            {
                this.ignoreCase = value;
            }
        }

        public IList<string> ValidValues
        {
            get
            {
                return this.validValues;
            }
        }
    }
}

