namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;

    [Cmdlet("Test", "PSSessionConfigurationFile", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217039"), OutputType(new string[] { "bool" })]
    public class TestPSSessionConfigurationFileCommand : PSCmdlet
    {
        private string path;

        protected override void ProcessRecord()
        {
            ProviderInfo provider = null;
            Collection<string> resolvedProviderPathFromPSPath;
            try
            {
                if (base.Context.EngineSessionState.IsProviderLoaded(base.Context.ProviderNames.FileSystem))
                {
                    resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(this.path, out provider);
                }
                else
                {
                    resolvedProviderPathFromPSPath = new Collection<string> {
                        this.path
                    };
                }
            }
            catch (ItemNotFoundException)
            {
                FileNotFoundException exception = new FileNotFoundException(StringUtil.Format(RemotingErrorIdStrings.PSSessionConfigurationFileNotFound, this.path));
                ErrorRecord errorRecord = new ErrorRecord(exception, "PSSessionConfigurationFileNotFound", ErrorCategory.ResourceUnavailable, this.path);
                base.WriteError(errorRecord);
                return;
            }
            if (!provider.NameEquals(base.Context.ProviderNames.FileSystem))
            {
                throw InterpreterError.NewInterpreterException(this.path, typeof(RuntimeException), null, "FileOpenError", ParserStrings.FileOpenError, new object[] { provider.FullName });
            }
            if ((resolvedProviderPathFromPSPath != null) && (resolvedProviderPathFromPSPath.Count >= 1))
            {
                if (resolvedProviderPathFromPSPath.Count > 1)
                {
                    throw InterpreterError.NewInterpreterException(resolvedProviderPathFromPSPath, typeof(RuntimeException), null, "AmbiguousPath", ParserStrings.AmbiguousPath, new object[0]);
                }
                string path = resolvedProviderPathFromPSPath[0];
                ExternalScriptInfo scriptInfo = null;
                if (System.IO.Path.GetExtension(path).Equals(".pssc", StringComparison.OrdinalIgnoreCase))
                {
                    string str5;
                    scriptInfo = DISCUtils.GetScriptInfoForFile(base.Context, path, out str5);
                    Hashtable table = null;
                    try
                    {
                        table = DISCUtils.LoadConfigFile(base.Context, scriptInfo);
                    }
                    catch (RuntimeException exception3)
                    {
                        base.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCErrorParsingConfigFile, path, exception3.Message));
                        base.WriteObject(false);
                        return;
                    }
                    if (table == null)
                    {
                        base.WriteObject(false);
                    }
                    else
                    {
                        DISCUtils.ExecutionPolicyType = typeof(ExecutionPolicy);
                        base.WriteObject(DISCUtils.VerifyConfigTable(table, this, path));
                    }
                }
                else
                {
                    InvalidOperationException exception4 = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, path));
                    ErrorRecord record3 = new ErrorRecord(exception4, "InvalidPSSessionConfigurationFilePath", ErrorCategory.InvalidArgument, this.path);
                    base.ThrowTerminatingError(record3);
                }
            }
            else
            {
                FileNotFoundException exception2 = new FileNotFoundException(StringUtil.Format(RemotingErrorIdStrings.PSSessionConfigurationFileNotFound, this.path));
                ErrorRecord record2 = new ErrorRecord(exception2, "PSSessionConfigurationFileNotFound", ErrorCategory.ResourceUnavailable, this.path);
                base.WriteError(record2);
            }
        }

        [Parameter(Mandatory=true, ValueFromPipeline=true, Position=0, ValueFromPipelineByPropertyName=true)]
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }
    }
}

