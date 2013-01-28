namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(PSTraceSource) }), Cmdlet("Set", "TraceSource", DefaultParameterSetName="optionsSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113400")]
    public class SetTraceSourceCommand : TraceListenerCommandBase
    {
        private bool passThru;
        private string[] removeFileListeners = new string[] { "*" };
        private string[] removeListeners = new string[] { "*" };

        protected override void ProcessRecord()
        {
            Collection<PSTraceSource> sendToPipeline = null;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "optionsSet"))
                {
                    if (!(parameterSetName == "removeAllListenersSet"))
                    {
                        if (parameterSetName == "removeFileListenersSet")
                        {
                            TraceListenerCommandBase.RemoveListenersByName(base.GetMatchingTraceSource(this.Name, true), this.RemoveFileListener, true);
                        }
                        return;
                    }
                }
                else
                {
                    Collection<PSTraceSource> preconfiguredSources = null;
                    sendToPipeline = base.ConfigureTraceSource(this.Name, true, out preconfiguredSources);
                    if (this.PassThru != 0)
                    {
                        base.WriteObject(sendToPipeline, true);
                        base.WriteObject(preconfiguredSources, true);
                    }
                    return;
                }
                TraceListenerCommandBase.RemoveListenersByName(base.GetMatchingTraceSource(this.Name, true), this.RemoveListener, false);
            }
        }

        [Parameter(ParameterSetName="optionsSet")]
        public SwitchParameter Debugger
        {
            get
            {
                return base.DebuggerListener;
            }
            set
            {
                base.DebuggerListener = (bool) value;
            }
        }

        [Parameter(ParameterSetName="optionsSet"), Alias(new string[] { "PSPath" })]
        public string FilePath
        {
            get
            {
                return base.FileListener;
            }
            set
            {
                base.FileListener = value;
            }
        }

        [Parameter(ParameterSetName="optionsSet")]
        public SwitchParameter Force
        {
            get
            {
                return base.ForceWrite;
            }
            set
            {
                base.ForceWrite = (bool) value;
            }
        }

        [Parameter(ParameterSetName="optionsSet")]
        public TraceOptions ListenerOption
        {
            get
            {
                return base.ListenerOptionsInternal;
            }
            set
            {
                base.ListenerOptionsInternal = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public string[] Name
        {
            get
            {
                return base.NameInternal;
            }
            set
            {
                base.NameInternal = value;
            }
        }

        [Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="optionsSet")]
        public PSTraceSourceOptions Option
        {
            get
            {
                return base.OptionsInternal;
            }
            set
            {
                base.OptionsInternal = value;
            }
        }

        [Parameter(ParameterSetName="optionsSet")]
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

        [Parameter(ParameterSetName="optionsSet")]
        public SwitchParameter PSHost
        {
            get
            {
                return base.PSHostListener;
            }
            set
            {
                base.PSHostListener = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="removeFileListenersSet")]
        public string[] RemoveFileListener
        {
            get
            {
                return this.removeFileListeners;
            }
            set
            {
                this.removeFileListeners = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="removeAllListenersSet")]
        public string[] RemoveListener
        {
            get
            {
                return this.removeListeners;
            }
            set
            {
                this.removeListeners = value;
            }
        }
    }
}

