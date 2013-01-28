namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Net;
    using System.Text;

    [Cmdlet("ConvertTo", "Html", DefaultParameterSetName="Page", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113290", RemotingCapability=RemotingCapability.None)]
    public sealed class ConvertToHtmlCommand : PSCmdlet
    {
        private string _as = "Table";
        private Uri _cssuri;
        private SwitchParameter _fragment;
        private string[] _postContent;
        private string[] _preContent;
        private string[] body;
        private bool cssuriSpecified;
        private string[] head;
        private PSObject inputObject;
        private bool isTHWritten;
        private int numberObjects;
        private object[] property;
        private StringCollection propertyCollector;
        private List<MshParameter> propertyMshParameterList;
        private List<MshParameter> resolvedNameMshParameters;
        private string title = "HTML TABLE";

        protected override void BeginProcessing()
        {
            if (this.cssuriSpecified && string.IsNullOrEmpty(this._cssuri.OriginalString.Trim()))
            {
                ArgumentException exception = new ArgumentException(StringUtil.Format(UtilityCommonStrings.EmptyCSSUri, "CSSUri"));
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArgumentException", ErrorCategory.InvalidArgument, "CSSUri");
                base.ThrowTerminatingError(errorRecord);
            }
            this.propertyMshParameterList = this.ProcessParameter(this.property);
            if (!string.IsNullOrEmpty(this.title))
            {
                WebUtility.HtmlEncode(this.title);
            }
            if (this._fragment == 0)
            {
                base.WriteObject("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"  \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
                base.WriteObject("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                base.WriteObject("<head>");
                base.WriteObject(this.head ?? new string[] { ("<title>" + this.title + "</title>") }, true);
                if (this.cssuriSpecified)
                {
                    base.WriteObject("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + this._cssuri + "\" />");
                }
                base.WriteObject("</head><body>");
                if (this.body != null)
                {
                    base.WriteObject(this.body, true);
                }
            }
            if (this._preContent != null)
            {
                base.WriteObject(this._preContent, true);
            }
            base.WriteObject("<table>");
            this.isTHWritten = false;
            this.propertyCollector = new StringCollection();
        }

        private static Hashtable CreateAuxPropertyHT(string label, string alignment, string width)
        {
            Hashtable hashtable = new Hashtable();
            if (label != null)
            {
                hashtable.Add("label", label);
            }
            if (alignment != null)
            {
                hashtable.Add("alignment", alignment);
            }
            if (width != null)
            {
                hashtable.Add("width", width);
            }
            return hashtable;
        }

        protected override void EndProcessing()
        {
            base.WriteObject("</table>");
            if (this._postContent != null)
            {
                base.WriteObject(this._postContent, true);
            }
            if (this._fragment == 0)
            {
                base.WriteObject("</body></html>");
            }
        }

        private void InitializeResolvedNameMshParameters()
        {
            ArrayList list = new ArrayList();
            foreach (MshParameter parameter in this.propertyMshParameterList)
            {
                string entry = parameter.GetEntry("label") as string;
                string alignment = parameter.GetEntry("alignment") as string;
                string width = parameter.GetEntry("width") as string;
                MshExpression expression = parameter.GetEntry("expression") as MshExpression;
                List<MshExpression> list2 = expression.ResolveNames(this.inputObject);
                if (list2.Count == 1)
                {
                    Hashtable hashtable = CreateAuxPropertyHT(entry, alignment, width);
                    if (expression.Script != null)
                    {
                        hashtable.Add("expression", expression.Script);
                    }
                    else
                    {
                        hashtable.Add("expression", expression.ToString());
                    }
                    list.Add(hashtable);
                }
                else
                {
                    foreach (MshExpression expression2 in list2)
                    {
                        Hashtable hashtable2 = CreateAuxPropertyHT(entry, alignment, width);
                        hashtable2.Add("expression", expression2.ToString());
                        list.Add(hashtable2);
                    }
                }
            }
            this.resolvedNameMshParameters = this.ProcessParameter(list.ToArray());
        }

        private List<MshParameter> ProcessParameter(object[] properties)
        {
            TerminatingErrorContext invocationContext = new TerminatingErrorContext(this);
            ParameterProcessor processor = new ParameterProcessor(new ConvertHTMLExpressionParameterDefinition());
            if (properties == null)
            {
                properties = new object[] { "*" };
            }
            return processor.ProcessParameters(properties, invocationContext);
        }

        protected override void ProcessRecord()
        {
            if ((this.inputObject != null) && (this.inputObject != AutomationNull.Value))
            {
                this.numberObjects++;
                if (!this.isTHWritten)
                {
                    this.InitializeResolvedNameMshParameters();
                    if ((this.resolvedNameMshParameters == null) || (this.resolvedNameMshParameters.Count == 0))
                    {
                        return;
                    }
                    if (this._as.Equals("List", StringComparison.OrdinalIgnoreCase))
                    {
                        if (this.numberObjects > 1)
                        {
                            base.WriteObject("<tr><td><hr></td></tr>");
                        }
                        this.WriteListEntry();
                    }
                    else
                    {
                        this.WriteColumns(this.resolvedNameMshParameters);
                        StringBuilder tHtag = new StringBuilder("<tr>");
                        this.WriteTableHeader(tHtag, this.resolvedNameMshParameters);
                        tHtag.Append("</tr>");
                        base.WriteObject(tHtag.ToString());
                        this.isTHWritten = true;
                    }
                }
                if (this._as.Equals("Table", StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder tRtag = new StringBuilder("<tr>");
                    this.WriteTableRow(tRtag, this.resolvedNameMshParameters);
                    tRtag.Append("</tr>");
                    base.WriteObject(tRtag.ToString());
                }
            }
        }

        private static string SafeToString(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return obj.ToString();
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
            return "";
        }

        private void WriteColumns(List<MshParameter> mshParams)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<colgroup>");
            foreach (MshParameter parameter in mshParams)
            {
                builder.Append("<col");
                string entry = parameter.GetEntry("width") as string;
                if (entry != null)
                {
                    builder.Append(" width = \"");
                    builder.Append(entry);
                    builder.Append("\"");
                }
                string str2 = parameter.GetEntry("alignment") as string;
                if (str2 != null)
                {
                    builder.Append(" align = \"");
                    builder.Append(str2);
                    builder.Append("\"");
                }
                builder.Append("/>");
            }
            builder.Append("</colgroup>");
            base.WriteObject(builder.ToString());
        }

        private void WriteListEntry()
        {
            foreach (MshParameter parameter in this.resolvedNameMshParameters)
            {
                StringBuilder listtag = new StringBuilder();
                listtag.Append("<tr><td>");
                this.WritePropertyName(listtag, parameter);
                listtag.Append(":");
                listtag.Append("</td>");
                listtag.Append("<td>");
                this.WritePropertyValue(listtag, parameter);
                listtag.Append("</td></tr>");
                base.WriteObject(listtag.ToString());
            }
        }

        private void WritePropertyName(StringBuilder Listtag, MshParameter p)
        {
            string entry = p.GetEntry("label") as string;
            if (entry != null)
            {
                Listtag.Append(entry);
            }
            else
            {
                Listtag.Append((p.GetEntry("expression") as MshExpression).ToString());
            }
        }

        private void WritePropertyValue(StringBuilder Listtag, MshParameter p)
        {
            MshExpression entry = p.GetEntry("expression") as MshExpression;
            foreach (MshExpressionResult result in entry.GetValues(this.inputObject))
            {
                if (result.Result != null)
                {
                    string str = WebUtility.HtmlEncode(SafeToString(result.Result));
                    Listtag.Append(str);
                }
                Listtag.Append(", ");
            }
            if (Listtag.ToString().EndsWith(", ", StringComparison.Ordinal))
            {
                Listtag.Remove(Listtag.Length - 2, 2);
            }
        }

        private void WriteTableHeader(StringBuilder THtag, List<MshParameter> resolvedNameMshParameters)
        {
            foreach (MshParameter parameter in resolvedNameMshParameters)
            {
                THtag.Append("<th>");
                this.WritePropertyName(THtag, parameter);
                THtag.Append("</th>");
            }
        }

        private void WriteTableRow(StringBuilder TRtag, List<MshParameter> resolvedNameMshParameters)
        {
            foreach (MshParameter parameter in resolvedNameMshParameters)
            {
                TRtag.Append("<td>");
                this.WritePropertyValue(TRtag, parameter);
                TRtag.Append("</td>");
            }
        }

        [ValidateNotNullOrEmpty, ValidateSet(new string[] { "Table", "List" }), Parameter]
        public string As
        {
            get
            {
                return this._as;
            }
            set
            {
                this._as = value;
            }
        }

        [Parameter(ParameterSetName="Page", Position=3)]
        public string[] Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }

        [Alias(new string[] { "cu", "uri" }), ValidateNotNullOrEmpty, Parameter(ParameterSetName="Page")]
        public Uri CssUri
        {
            get
            {
                return this._cssuri;
            }
            set
            {
                this._cssuri = value;
                this.cssuriSpecified = true;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="Fragment")]
        public SwitchParameter Fragment
        {
            get
            {
                return this._fragment;
            }
            set
            {
                this._fragment = value;
            }
        }

        [Parameter(ParameterSetName="Page", Position=1)]
        public string[] Head
        {
            get
            {
                return this.head;
            }
            set
            {
                this.head = value;
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string[] PostContent
        {
            get
            {
                return this._postContent;
            }
            set
            {
                this._postContent = value;
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public string[] PreContent
        {
            get
            {
                return this._preContent;
            }
            set
            {
                this._preContent = value;
            }
        }

        [Parameter(Position=0)]
        public object[] Property
        {
            get
            {
                return this.property;
            }
            set
            {
                this.property = value;
            }
        }

        [Parameter(ParameterSetName="Page", Position=2), ValidateNotNullOrEmpty]
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        internal class ConvertHTMLExpressionParameterDefinition : CommandParameterDefinition
        {
            protected override void SetEntries()
            {
                base.hashEntries.Add(new ExpressionEntryDefinition());
                base.hashEntries.Add(new HashtableEntryDefinition("label", new Type[] { typeof(string) }));
                base.hashEntries.Add(new HashtableEntryDefinition("alignment", new Type[] { typeof(string) }));
                base.hashEntries.Add(new HashtableEntryDefinition("width", new Type[] { typeof(string) }));
            }
        }

        internal static class ConvertHTMLParameterDefinitionKeys
        {
            internal const string AlignmentEntryKey = "alignment";
            internal const string LabelEntryKey = "label";
            internal const string WidthEntryKey = "width";
        }
    }
}

