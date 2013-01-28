namespace System.Management.Automation
{
    using System;

    internal class ParameterSetSpecificMetadata
    {
        private ParameterAttribute attribute;
        private string helpMessage;
        private string helpMessageBaseName;
        private string helpMessageResourceId;
        private bool isInAllSets;
        private bool isMandatory;
        private int parameterSetFlag;
        private int position;
        internal bool valueFromPipeline;
        internal bool valueFromPipelineByPropertyName;
        private bool valueFromRemainingArguments;

        internal ParameterSetSpecificMetadata(ParameterAttribute attribute)
        {
            this.position = -2147483648;
            if (attribute == null)
            {
                throw PSTraceSource.NewArgumentNullException("attribute");
            }
            this.attribute = attribute;
            this.isMandatory = attribute.Mandatory;
            this.position = attribute.Position;
            this.valueFromRemainingArguments = attribute.ValueFromRemainingArguments;
            this.valueFromPipeline = attribute.ValueFromPipeline;
            this.valueFromPipelineByPropertyName = attribute.ValueFromPipelineByPropertyName;
            this.helpMessage = attribute.HelpMessage;
            this.helpMessageBaseName = attribute.HelpMessageBaseName;
            this.helpMessageResourceId = attribute.HelpMessageResourceId;
        }

        internal ParameterSetSpecificMetadata(bool isMandatory, int position, bool valueFromRemainingArguments, bool valueFromPipeline, bool valueFromPipelineByPropertyName, string helpMessageBaseName, string helpMessageResourceId, string helpMessage)
        {
            this.position = -2147483648;
            this.isMandatory = isMandatory;
            this.position = position;
            this.valueFromRemainingArguments = valueFromRemainingArguments;
            this.valueFromPipeline = valueFromPipeline;
            this.valueFromPipelineByPropertyName = valueFromPipelineByPropertyName;
            this.helpMessageBaseName = helpMessageBaseName;
            this.helpMessageResourceId = helpMessageResourceId;
            this.helpMessage = helpMessage;
        }

        internal string GetHelpMessage(Cmdlet cmdlet)
        {
            string helpMessage = null;
            bool flag = !string.IsNullOrEmpty(this.HelpMessage);
            bool flag2 = !string.IsNullOrEmpty(this.HelpMessageBaseName);
            bool flag3 = !string.IsNullOrEmpty(this.HelpMessageResourceId);
            if (flag2 ^ flag3)
            {
                throw PSTraceSource.NewArgumentException(flag2 ? "HelpMessageResourceId" : "HelpMessageBaseName");
            }
            if (flag2 && flag3)
            {
                try
                {
                    return cmdlet.GetResourceString(this.HelpMessageBaseName, this.HelpMessageResourceId);
                }
                catch (ArgumentException)
                {
                    if (!flag)
                    {
                        throw;
                    }
                    return this.HelpMessage;
                }
                catch (InvalidOperationException)
                {
                    if (!flag)
                    {
                        throw;
                    }
                    return this.HelpMessage;
                }
            }
            if (flag)
            {
                helpMessage = this.HelpMessage;
            }
            return helpMessage;
        }

        internal string HelpMessage
        {
            get
            {
                return this.helpMessage;
            }
        }

        internal string HelpMessageBaseName
        {
            get
            {
                return this.helpMessageBaseName;
            }
        }

        internal string HelpMessageResourceId
        {
            get
            {
                return this.helpMessageResourceId;
            }
        }

        internal bool IsInAllSets
        {
            get
            {
                return this.isInAllSets;
            }
            set
            {
                this.isInAllSets = value;
            }
        }

        internal bool IsMandatory
        {
            get
            {
                return this.isMandatory;
            }
        }

        internal bool IsPositional
        {
            get
            {
                return (this.position != -2147483648);
            }
        }

        internal int ParameterSetFlag
        {
            get
            {
                return this.parameterSetFlag;
            }
            set
            {
                this.parameterSetFlag = value;
            }
        }

        internal int Position
        {
            get
            {
                return this.position;
            }
        }

        internal bool ValueFromPipeline
        {
            get
            {
                return this.valueFromPipeline;
            }
        }

        internal bool ValueFromPipelineByPropertyName
        {
            get
            {
                return this.valueFromPipelineByPropertyName;
            }
        }

        internal bool ValueFromRemainingArguments
        {
            get
            {
                return this.valueFromRemainingArguments;
            }
        }
    }
}

