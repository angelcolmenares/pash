namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Unblock", "File", DefaultParameterSetName="ByPath", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=217450")]
    public sealed class UnblockFileCommand : PSCmdlet
    {
        private string[] paths;

        private bool IsValidFileForUnblocking(string resolvedpath)
        {
            bool flag = false;
            if (Directory.Exists(resolvedpath))
            {
                return flag;
            }
            if (!File.Exists(resolvedpath))
            {
                ErrorRecord errorRecord = new ErrorRecord(new FileNotFoundException(resolvedpath), "FileNotFound", ErrorCategory.ObjectNotFound, resolvedpath);
                base.WriteError(errorRecord);
                return flag;
            }
            return true;
        }

        protected override void ProcessRecord()
        {
            List<string> list = new List<string>();
            ProviderInfo provider = null;
            if (string.Equals(base.ParameterSetName, "ByLiteralPath", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string str in this.paths)
                {
                    string unresolvedProviderPathFromPSPath = base.Context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str);
                    if (this.IsValidFileForUnblocking(unresolvedProviderPathFromPSPath))
                    {
                        list.Add(unresolvedProviderPathFromPSPath);
                    }
                }
            }
            else
            {
                foreach (string str3 in this.paths)
                {
                    try
                    {
                        foreach (string str4 in base.Context.SessionState.Path.GetResolvedProviderPathFromPSPath(str3, out provider))
                        {
                            if (this.IsValidFileForUnblocking(str4))
                            {
                                list.Add(str4);
                            }
                        }
                    }
                    catch (ItemNotFoundException exception)
                    {
                        if (!WildcardPattern.ContainsWildcardCharacters(str3))
                        {
                            ErrorRecord errorRecord = new ErrorRecord(exception, "FileNotFound", ErrorCategory.ObjectNotFound, str3);
                            base.WriteError(errorRecord);
                        }
                    }
                }
            }
            foreach (string str5 in list)
            {
                if (base.ShouldProcess(str5))
                {
                    AlternateDataStreamUtilities.DeleteFileStream(str5, "Zone.Identifier");
                }
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="ByLiteralPath", ValueFromPipelineByPropertyName=true), Alias(new string[] { "PSPath" })]
        public string[] LiteralPath
        {
            get
            {
                return this.paths;
            }
            set
            {
                this.paths = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="ByPath")]
        public string[] Path
        {
            get
            {
                return this.paths;
            }
            set
            {
                this.paths = value;
            }
        }
    }
}

