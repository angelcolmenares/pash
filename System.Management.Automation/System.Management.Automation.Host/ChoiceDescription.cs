namespace System.Management.Automation.Host
{
    using System;
    using System.Management.Automation;

    public sealed class ChoiceDescription
    {
        private string helpMessage;
        private readonly string label;
        private const string NullOrEmptyErrorTemplateResource = "NullOrEmptyErrorTemplate";
        private const string StringsBaseName = "DescriptionsStrings";

        public ChoiceDescription(string label)
        {
            this.helpMessage = "";
            if (string.IsNullOrEmpty(label))
            {
                throw PSTraceSource.NewArgumentException("label", "DescriptionsStrings", "NullOrEmptyErrorTemplate", new object[] { "label" });
            }
            this.label = label;
        }

        public ChoiceDescription(string label, string helpMessage)
        {
            this.helpMessage = "";
            if (string.IsNullOrEmpty(label))
            {
                throw PSTraceSource.NewArgumentException("label", "DescriptionsStrings", "NullOrEmptyErrorTemplate", new object[] { "label" });
            }
            if (helpMessage == null)
            {
                throw PSTraceSource.NewArgumentNullException("helpMessage");
            }
            this.label = label;
            this.helpMessage = helpMessage;
        }

        public string HelpMessage
        {
            get
            {
                return this.helpMessage;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.helpMessage = value;
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
        }
    }
}

