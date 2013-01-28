namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [OutputType(new Type[] { typeof(PSModuleInfo) }), Cmdlet("Test", "ModuleManifest", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141557")]
    public sealed class TestModuleManifestCommand : ModuleCmdletBase
    {
        private string _path;

        protected override void ProcessRecord()
        {
            ProviderInfo provider = null;
            Collection<string> resolvedProviderPathFromPSPath;
            try
            {
                if (base.Context.EngineSessionState.IsProviderLoaded(base.Context.ProviderNames.FileSystem))
                {
                    resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(this._path, out provider);
                }
                else
                {
                    resolvedProviderPathFromPSPath = new Collection<string> {
                        this._path
                    };
                }
            }
            catch (ItemNotFoundException)
            {
                FileNotFoundException exception = new FileNotFoundException(StringUtil.Format(Modules.ModuleNotFound, this._path));
                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, this._path);
                base.WriteError(errorRecord);
                return;
            }
            if (!provider.NameEquals(base.Context.ProviderNames.FileSystem))
            {
                throw InterpreterError.NewInterpreterException(this._path, typeof(RuntimeException), null, "FileOpenError", ParserStrings.FileOpenError, new object[] { provider.FullName });
            }
            if ((resolvedProviderPathFromPSPath != null) && (resolvedProviderPathFromPSPath.Count >= 1))
            {
                if (resolvedProviderPathFromPSPath.Count > 1)
                {
                    throw InterpreterError.NewInterpreterException(resolvedProviderPathFromPSPath, typeof(RuntimeException), null, "AmbiguousPath", ParserStrings.AmbiguousPath, new object[0]);
                }
                string path = resolvedProviderPathFromPSPath[0];
                ExternalScriptInfo scriptInfo = null;
                if (System.IO.Path.GetExtension(path).Equals(".psd1", StringComparison.OrdinalIgnoreCase))
                {
                    string str5;
                    scriptInfo = base.GetScriptInfoForFile(path, out str5, false);
                    PSModuleInfo sendToPipeline = base.LoadModuleManifest(scriptInfo, ModuleCmdletBase.ManifestProcessingFlags.WriteWarnings | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, null, null);
                    if (sendToPipeline != null)
                    {
                        base.WriteObject(sendToPipeline);
                    }
                }
                else
                {
                    InvalidOperationException exception3 = new InvalidOperationException(StringUtil.Format(Modules.InvalidModuleManifestPath, path));
                    ErrorRecord record3 = new ErrorRecord(exception3, "Modules_InvalidModuleManifestPath", ErrorCategory.InvalidArgument, this._path);
                    base.ThrowTerminatingError(record3);
                }
            }
            else
            {
                FileNotFoundException exception2 = new FileNotFoundException(StringUtil.Format(Modules.ModuleNotFound, this._path));
                ErrorRecord record2 = new ErrorRecord(exception2, "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, this._path);
                base.WriteError(record2);
            }
        }

        [Parameter(Mandatory=true, ValueFromPipeline=true, Position=0, ValueFromPipelineByPropertyName=true)]
        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }
    }
}

