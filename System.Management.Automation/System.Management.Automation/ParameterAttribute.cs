namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=true)]
    public sealed class ParameterAttribute : ParsingBaseAttribute
    {
        public const string AllParameterSets = "__AllParameterSets";
        private string helpMessage;
        private string helpMessageBaseName;
        private string helpMessageResourceId;
        private bool mandatory;
        private string parameterSetName = "__AllParameterSets";
        private int position = -2147483648;
        private bool valueFromPipeline;
        private bool valueFromPipelineByPropertyName;
        private bool valueFromRemainingArguments;

        public string HelpMessage
        {
            get
            {
                return this.helpMessage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentException("value");
                }
                this.helpMessage = value;
            }
        }

        public string HelpMessageBaseName
        {
            get
            {
                return this.helpMessageBaseName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentException("value");
                }
                this.helpMessageBaseName = value;
            }
        }

        public string HelpMessageResourceId
        {
            get
            {
                return this.helpMessageResourceId;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentException("value");
                }
                this.helpMessageResourceId = value;
            }
        }

        public bool Mandatory
        {
            get
            {
                return this.mandatory;
            }
            set
            {
                this.mandatory = value;
            }
        }

        public string ParameterSetName
        {
            get
            {
                return this.parameterSetName;
            }
            set
            {
                this.parameterSetName = value;
                if (string.IsNullOrEmpty(this.parameterSetName))
                {
                    this.parameterSetName = "__AllParameterSets";
                }
            }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }

        public bool ValueFromPipeline
        {
            get
            {
                return this.valueFromPipeline;
            }
            set
            {
                this.valueFromPipeline = value;
            }
        }

        public bool ValueFromPipelineByPropertyName
        {
            get
            {
                return this.valueFromPipelineByPropertyName;
            }
            set
            {
                this.valueFromPipelineByPropertyName = value;
            }
        }

        public bool ValueFromRemainingArguments
        {
            get
            {
                return this.valueFromRemainingArguments;
            }
            set
            {
                this.valueFromRemainingArguments = value;
            }
        }
    }
}

