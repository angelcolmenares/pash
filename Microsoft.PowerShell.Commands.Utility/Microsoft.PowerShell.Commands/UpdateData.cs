namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public class UpdateData : PSCmdlet
    {
        private string[] appendPath = new string[0];
        protected const string FileParameterSet = "FileSet";
        private string[] prependPath = new string[0];

        internal static Collection<string> Glob(string[] files, string errorId, PSCmdlet cmdlet)
        {
            Collection<string> collection = new Collection<string>();
            foreach (string str in files)
            {
                Collection<string> resolvedProviderPathFromPSPath;
                ProviderInfo provider = null;
                try
                {
                    resolvedProviderPathFromPSPath = cmdlet.SessionState.Path.GetResolvedProviderPathFromPSPath(str, out provider);
                }
                catch (SessionStateException exception)
                {
                    cmdlet.WriteError(new ErrorRecord(exception, errorId, ErrorCategory.InvalidOperation, str));
                    continue;
                }
                if (!provider.NameEquals(cmdlet.Context.ProviderNames.FileSystem))
                {
                    ReportWrongProviderType(provider.FullName, errorId, cmdlet);
                }
                else
                {
                    foreach (string str2 in resolvedProviderPathFromPSPath)
                    {
                        if (!str2.EndsWith(".ps1xml", StringComparison.OrdinalIgnoreCase))
                        {
                            ReportWrongExtension(str2, "WrongExtension", cmdlet);
                        }
                        else
                        {
                            collection.Add(str2);
                        }
                    }
                }
            }
            return collection;
        }

        private static void ReportWrongExtension(string file, string errorId, PSCmdlet cmdlet)
        {
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException("UpdateDataStrings", "UpdateData_WrongExtension", new object[] { file, "ps1xml" }), errorId, ErrorCategory.InvalidArgument, null);
            cmdlet.WriteError(errorRecord);
        }

        private static void ReportWrongProviderType(string providerId, string errorId, PSCmdlet cmdlet)
        {
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewInvalidOperationException("UpdateDataStrings", "UpdateData_WrongProviderError", new object[] { providerId }), errorId, ErrorCategory.InvalidArgument, null);
            cmdlet.WriteError(errorRecord);
        }

        [Alias(new string[] { "PSPath", "Path" }), ValidateNotNull, Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="FileSet")]
        public string[] AppendPath
        {
            get
            {
                return this.appendPath;
            }
            set
            {
                this.appendPath = value;
            }
        }

        [Parameter(ParameterSetName="FileSet"), ValidateNotNull]
        public string[] PrependPath
        {
            get
            {
                return this.prependPath;
            }
            set
            {
                this.prependPath = value;
            }
        }
    }
}

