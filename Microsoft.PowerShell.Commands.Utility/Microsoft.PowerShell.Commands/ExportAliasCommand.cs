namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    [Cmdlet("Export", "Alias", SupportsShouldProcess=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113296"), OutputType(new Type[] { typeof(AliasInfo) })]
    public class ExportAliasCommand : PSCmdlet
    {
        private bool append;
        private ExportAliasFormat asFormat;
        private string description;
        private bool force;
        private bool isLiteralPath;
        private Collection<AliasInfo> matchingAliases = new Collection<AliasInfo>();
        private string[] names = new string[] { "*" };
        private bool noclobber;
        private bool passThru;
        private string path = ".";
        private string scope;

        protected override void EndProcessing()
        {
            StreamWriter writer = null;
            FileInfo readOnlyFileInfo = null;
            try
            {
                if (base.ShouldProcess(this.Path))
                {
                    writer = this.OpenFile(out readOnlyFileInfo);
                }
                if (writer != null)
                {
                    this.WriteHeader(writer);
                }
                foreach (AliasInfo info2 in this.matchingAliases)
                {
                    string aliasLine = null;
                    if (this.As == ExportAliasFormat.Csv)
                    {
                        aliasLine = GetAliasLine(info2, "\"{0}\",\"{1}\",\"{2}\",\"{3}\"");
                    }
                    else
                    {
                        aliasLine = GetAliasLine(info2, "set-alias -Name:\"{0}\" -Value:\"{1}\" -Description:\"{2}\" -Option:\"{3}\"");
                    }
                    if (writer != null)
                    {
                        writer.WriteLine(aliasLine);
                    }
                    if (this.PassThru != 0)
                    {
                        base.WriteObject(info2);
                    }
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                if (readOnlyFileInfo != null)
                {
                    readOnlyFileInfo.Attributes |= System.IO.FileAttributes.ReadOnly;
                }
            }
        }

        private static string GetAliasLine(AliasInfo alias, string formatString)
        {
            return string.Format(CultureInfo.InvariantCulture, formatString, new object[] { alias.Name, alias.Definition, alias.Description, alias.Options });
        }

        private StreamWriter OpenFile(out FileInfo readOnlyFileInfo)
        {
            StreamWriter streamWriter = null;
            FileStream fileStream = null;
            readOnlyFileInfo = null;
            PathUtils.MasterStreamOpen(this, this.Path, "unicode", false, (bool) this.Append, (bool) this.Force, (bool) this.NoClobber, out fileStream, out streamWriter, out readOnlyFileInfo, this.isLiteralPath);
            return streamWriter;
        }

        protected override void ProcessRecord()
        {
            IDictionary<string, AliasInfo> aliasTableAtScope = null;
            if (!string.IsNullOrEmpty(this.scope))
            {
                aliasTableAtScope = base.SessionState.Internal.GetAliasTableAtScope(this.scope);
            }
            else
            {
                aliasTableAtScope = base.SessionState.Internal.GetAliasTable();
            }
            foreach (string str in this.names)
            {
                bool flag = false;
                WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                CommandOrigin commandOrigin = base.MyInvocation.CommandOrigin;
                foreach (KeyValuePair<string, AliasInfo> pair in aliasTableAtScope)
                {
                    if (pattern.IsMatch(pair.Key) && SessionState.IsVisible(commandOrigin, (CommandInfo) pair.Value))
                    {
                        flag = true;
                        this.matchingAliases.Add(pair.Value);
                    }
                }
                if (!flag && !WildcardPattern.ContainsWildcardCharacters(str))
                {
                    ItemNotFoundException replaceParentContainsErrorRecordException = new ItemNotFoundException(str, "AliasNotFound", SessionStateStrings.AliasNotFound);
                    base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                }
            }
        }

        private void ThrowFileOpenError(Exception e, string pathWithError)
        {
            string message = StringUtil.Format(AliasCommandStrings.ExportAliasFileOpenFailed, pathWithError, e.Message);
            ErrorRecord errorRecord = new ErrorRecord(e, "FileOpenFailure", ErrorCategory.OpenError, pathWithError) {
                ErrorDetails = new ErrorDetails(message)
            };
            base.ThrowTerminatingError(errorRecord);
        }

        private static void WriteFormattedResourceString(StreamWriter writer, string resourceId, params object[] args)
        {
            string str = StringUtil.Format(resourceId, args);
            writer.Write("# ");
            writer.WriteLine(str);
        }

        private void WriteHeader(StreamWriter writer)
        {
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderTitle, new object[0]);
            string userName = Environment.UserName;
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderUser, new object[] { userName });
            DateTime now = DateTime.Now;
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderDate, new object[] { now });
            string machineName = Environment.MachineName;
            WriteFormattedResourceString(writer, AliasCommandStrings.ExportAliasHeaderMachine, new object[] { machineName });
            if (this.description != null)
            {
                this.description = this.description.Replace("\n", "\n# ");
                writer.WriteLine("#");
                writer.Write("# ");
                writer.WriteLine(this.description);
            }
        }

        [Parameter]
        public SwitchParameter Append
        {
            get
            {
                return this.append;
            }
            set
            {
                this.append = (bool) value;
            }
        }

        [Parameter]
        public ExportAliasFormat As
        {
            get
            {
                return this.asFormat;
            }
            set
            {
                this.asFormat = value;
            }
        }

        [Parameter]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath"), Alias(new string[] { "PSPath" })]
        public string LiteralPath
        {
            get
            {
                return this.path;
            }
            set
            {
                if (value == null)
                {
                    this.path = ".";
                }
                else
                {
                    this.path = value;
                    this.isLiteralPath = true;
                }
            }
        }

        [Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                if (value == null)
                {
                    this.names = new string[] { "*" };
                }
                else
                {
                    this.names = value;
                }
            }
        }

        [Alias(new string[] { "NoOverwrite" }), Parameter]
        public SwitchParameter NoClobber
        {
            get
            {
                return this.noclobber;
            }
            set
            {
                this.noclobber = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this.passThru;
            }
            set
            {
                this.passThru = (bool) value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="ByPath")]
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (value == null)
                {
                    this.path = ".";
                }
                else
                {
                    this.path = value;
                }
            }
        }

        [Parameter]
        public string Scope
        {
            get
            {
                return this.scope;
            }
            set
            {
                this.scope = value;
            }
        }
    }
}

