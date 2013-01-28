namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Text;

    public sealed class ParameterSetMetadata
    {
        private string helpMessage;
        private string helpMessageBaseName;
        private const string HelpMessageFormat = "{0}HelpMessage='{1}'";
        private string helpMessageResourceId;
        private bool isMandatory;
        private const string MandatoryFormat = "{0}Mandatory=$true";
        private int position;
        private const string PositionFormat = "{0}Position={1}";
        private bool valueFromPipeline;
        private bool valueFromPipelineByPropertyName;
        private const string ValueFromPipelineByPropertyNameFormat = "{0}ValueFromPipelineByPropertyName=$true";
        private const string ValueFromPipelineFormat = "{0}ValueFromPipeline=$true";
        private bool valueFromRemainingArguments;
        private const string ValueFromRemainingArgumentsFormat = "{0}ValueFromRemainingArguments=$true";

        internal ParameterSetMetadata(ParameterSetMetadata other)
        {
            if (other == null)
            {
                throw PSTraceSource.NewArgumentNullException("other");
            }
            this.helpMessage = other.helpMessage;
            this.helpMessageBaseName = other.helpMessageBaseName;
            this.helpMessageResourceId = other.helpMessageResourceId;
            this.isMandatory = other.isMandatory;
            this.position = other.position;
            this.valueFromPipeline = other.valueFromPipeline;
            this.valueFromPipelineByPropertyName = other.valueFromPipelineByPropertyName;
            this.valueFromRemainingArguments = other.valueFromRemainingArguments;
        }

        internal ParameterSetMetadata(ParameterSetSpecificMetadata psMD)
        {
            this.Initialize(psMD);
        }

        internal ParameterSetMetadata(int position, ParameterFlags flags, string helpMessage)
        {
            this.Position = position;
            this.Flags = flags;
            this.HelpMessage = helpMessage;
        }

        internal bool Equals(ParameterSetMetadata second)
        {
            return ((((this.isMandatory == second.isMandatory) && (this.position == second.position)) && ((this.valueFromPipeline == second.valueFromPipeline) && (this.valueFromPipelineByPropertyName == second.valueFromPipelineByPropertyName))) && (((this.valueFromRemainingArguments == second.valueFromRemainingArguments) && !(this.helpMessage != second.helpMessage)) && (!(this.helpMessageBaseName != second.helpMessageBaseName) && !(this.helpMessageResourceId != second.helpMessageResourceId))));
        }

        internal string GetProxyParameterData()
        {
            StringBuilder builder = new StringBuilder();
            string str = "";
            if (this.isMandatory)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}Mandatory=$true", new object[] { str });
                str = ", ";
            }
            if (this.position != -2147483648)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}Position={1}", new object[] { str, this.position });
                str = ", ";
            }
            if (this.valueFromPipeline)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}ValueFromPipeline=$true", new object[] { str });
                str = ", ";
            }
            if (this.valueFromPipelineByPropertyName)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}ValueFromPipelineByPropertyName=$true", new object[] { str });
                str = ", ";
            }
            if (this.valueFromRemainingArguments)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}ValueFromRemainingArguments=$true", new object[] { str });
                str = ", ";
            }
            if (!string.IsNullOrEmpty(this.helpMessage))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}HelpMessage='{1}'", new object[] { str, CommandMetadata.EscapeSingleQuotedString(this.helpMessage) });
                str = ", ";
            }
            return builder.ToString();
        }

        internal void Initialize(ParameterSetSpecificMetadata psMD)
        {
            this.isMandatory = psMD.IsMandatory;
            this.position = psMD.Position;
            this.valueFromPipeline = psMD.ValueFromPipeline;
            this.valueFromPipelineByPropertyName = psMD.ValueFromPipelineByPropertyName;
            this.valueFromRemainingArguments = psMD.ValueFromRemainingArguments;
            this.helpMessage = psMD.HelpMessage;
            this.helpMessageBaseName = psMD.HelpMessageBaseName;
            this.helpMessageResourceId = psMD.HelpMessageResourceId;
        }

        internal ParameterFlags Flags
        {
            get
            {
                ParameterFlags flags = 0;
                if (this.IsMandatory)
                {
                    flags |= ParameterFlags.Mandatory;
                }
                if (this.ValueFromPipeline)
                {
                    flags |= ParameterFlags.ValueFromPipeline;
                }
                if (this.ValueFromPipelineByPropertyName)
                {
                    flags |= ParameterFlags.ValueFromPipelineByPropertyName;
                }
                if (this.ValueFromRemainingArguments)
                {
                    flags |= ParameterFlags.ValueFromRemainingArguments;
                }
                return flags;
            }
            set
            {
                this.IsMandatory = ParameterFlags.Mandatory == (value & ParameterFlags.Mandatory);
                this.ValueFromPipeline = ParameterFlags.ValueFromPipeline == (value & ParameterFlags.ValueFromPipeline);
                this.ValueFromPipelineByPropertyName = ParameterFlags.ValueFromPipelineByPropertyName == (value & ParameterFlags.ValueFromPipelineByPropertyName);
                this.ValueFromRemainingArguments = ParameterFlags.ValueFromRemainingArguments == (value & ParameterFlags.ValueFromRemainingArguments);
            }
        }

        public string HelpMessage
        {
            get
            {
                return this.helpMessage;
            }
            set
            {
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
                this.helpMessageResourceId = value;
            }
        }

        public bool IsMandatory
        {
            get
            {
                return this.isMandatory;
            }
            set
            {
                this.isMandatory = value;
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

        [Flags]
        internal enum ParameterFlags : int
        {
            Mandatory = 1,
            ValueFromPipeline = 2,
            ValueFromPipelineByPropertyName = 4,
            ValueFromRemainingArguments = 8
        }
    }
}

