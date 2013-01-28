namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class CommandParameterInfo
    {
        private ReadOnlyCollection<string> aliases;
        private ReadOnlyCollection<Attribute> attributes;
        private string helpMessage = string.Empty;
        private bool isDynamic;
        private bool isMandatory;
        private string name = string.Empty;
        private Type parameterType;
        private int position = -2147483648;
        private bool valueFromPipeline;
        private bool valueFromPipelineByPropertyName;
        private bool valueFromRemainingArguments;

        internal CommandParameterInfo(CompiledCommandParameter parameter, int parameterSetFlag)
        {
            if (parameter == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameter");
            }
            this.name = parameter.Name;
            this.parameterType = parameter.Type;
            this.isDynamic = parameter.IsDynamic;
            this.aliases = new ReadOnlyCollection<string>(parameter.Aliases);
            this.SetAttributes(parameter.CompiledAttributes);
            this.SetParameterSetData(parameter.GetParameterSetData(parameterSetFlag));
        }

        private void SetAttributes(IList<Attribute> attributeMetadata)
        {
            Collection<Attribute> list = new Collection<Attribute>();
            foreach (Attribute attribute in attributeMetadata)
            {
                list.Add(attribute);
            }
            this.attributes = new ReadOnlyCollection<Attribute>(list);
        }

        private void SetParameterSetData(ParameterSetSpecificMetadata parameterMetadata)
        {
            this.isMandatory = parameterMetadata.IsMandatory;
            this.position = parameterMetadata.Position;
            this.valueFromPipeline = parameterMetadata.valueFromPipeline;
            this.valueFromPipelineByPropertyName = parameterMetadata.valueFromPipelineByPropertyName;
            this.valueFromRemainingArguments = parameterMetadata.ValueFromRemainingArguments;
            this.helpMessage = parameterMetadata.HelpMessage;
        }

        public ReadOnlyCollection<string> Aliases
        {
            get
            {
                return this.aliases;
            }
        }

        public ReadOnlyCollection<Attribute> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public string HelpMessage
        {
            get
            {
                return this.helpMessage;
            }
        }

        public bool IsDynamic
        {
            get
            {
                return this.isDynamic;
            }
        }

        public bool IsMandatory
        {
            get
            {
                return this.isMandatory;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Type ParameterType
        {
            get
            {
                return this.parameterType;
            }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
        }

        public bool ValueFromPipeline
        {
            get
            {
                return this.valueFromPipeline;
            }
        }

        public bool ValueFromPipelineByPropertyName
        {
            get
            {
                return this.valueFromPipelineByPropertyName;
            }
        }

        public bool ValueFromRemainingArguments
        {
            get
            {
                return this.valueFromRemainingArguments;
            }
        }
    }
}

