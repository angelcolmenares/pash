namespace System.Data.Services.Client
{
    using System;

    internal abstract class OperationParameter
    {
        private string parameterName;
        private object parameterValue;

        protected OperationParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Strings.Context_MissingOperationParameterName);
            }
            this.parameterName = name;
            this.parameterValue = value;
        }

        public string Name
        {
            get
            {
                return this.parameterName;
            }
        }

        public object Value
        {
            get
            {
                return this.parameterValue;
            }
        }
    }
}

