namespace System.Management.Automation
{
    using System;

    public class CompletionResult
    {
        private string completionText;
        private string listItemText;
        private static readonly CompletionResult NullInstance = new CompletionResult();
        private CompletionResultType resultType;
        private string toolTip;

        private CompletionResult()
        {
        }

        public CompletionResult(string completionText) : this(completionText, completionText, CompletionResultType.Text, completionText)
        {
        }

        public CompletionResult(string completionText, string listItemText, CompletionResultType resultType, string toolTip)
        {
            if (string.IsNullOrEmpty(completionText))
            {
                throw PSTraceSource.NewArgumentNullException("completionText");
            }
            if (string.IsNullOrEmpty(listItemText))
            {
                throw PSTraceSource.NewArgumentNullException("listItemText");
            }
            if ((resultType < CompletionResultType.Text) || (resultType > CompletionResultType.Type))
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("resultType", resultType);
            }
            if (string.IsNullOrEmpty(toolTip))
            {
                throw PSTraceSource.NewArgumentNullException("toolTip");
            }
            this.completionText = completionText;
            this.listItemText = listItemText;
            this.toolTip = toolTip;
            this.resultType = resultType;
        }

        public string CompletionText
        {
            get
            {
                if (this == NullInstance)
                {
                    throw PSTraceSource.NewInvalidOperationException("TabCompletionStrings", "NoAccessToProperties", new object[0]);
                }
                return this.completionText;
            }
        }

        public string ListItemText
        {
            get
            {
                if (this == NullInstance)
                {
                    throw PSTraceSource.NewInvalidOperationException("TabCompletionStrings", "NoAccessToProperties", new object[0]);
                }
                return this.listItemText;
            }
        }

        internal static CompletionResult Null
        {
            get
            {
                return NullInstance;
            }
        }

        public CompletionResultType ResultType
        {
            get
            {
                if (this == NullInstance)
                {
                    throw PSTraceSource.NewInvalidOperationException("TabCompletionStrings", "NoAccessToProperties", new object[0]);
                }
                return this.resultType;
            }
        }

        public string ToolTip
        {
            get
            {
                if (this == NullInstance)
                {
                    throw PSTraceSource.NewInvalidOperationException("TabCompletionStrings", "NoAccessToProperties", new object[0]);
                }
                return this.toolTip;
            }
        }
    }
}

