namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Trace", "Command", DefaultParameterSetName="expressionSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113419")]
    public class TraceCommandCommand : TraceListenerCommandBase, IDisposable
    {
        private PSObject _inputObject = AutomationNull.Value;
        private string command;
        private object[] commandArgs;
        private bool disposed;
        private ScriptBlock expression;
        private Collection<PSTraceSource> matchingSources;
        private PipelineProcessor pipeline;

        protected override void BeginProcessing()
        {
            Collection<PSTraceSource> preconfiguredSources = null;
            this.matchingSources = base.ConfigureTraceSource(base.NameInternal, false, out preconfiguredSources);
            base.TurnOnTracing(this.matchingSources, false);
            base.TurnOnTracing(preconfiguredSources, true);
            foreach (PSTraceSource source in preconfiguredSources)
            {
                this.matchingSources.Add(source);
            }
            if (base.ParameterSetName == "commandSet")
            {
                CommandProcessorBase commandProcessor = base.Context.CommandDiscovery.LookupCommandProcessor(this.command, CommandOrigin.Runspace, false);
                ParameterBinderController.AddArgumentsToCommandProcessor(commandProcessor, this.ArgumentList);
                this.pipeline = new PipelineProcessor();
                this.pipeline.Add(commandProcessor);
                this.pipeline.ExternalErrorOutput = new TracePipelineWriter(this, true, this.matchingSources);
                this.pipeline.ExternalSuccessOutput = new TracePipelineWriter(this, false, this.matchingSources);
            }
            base.ResetTracing(this.matchingSources);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                base.ResetTracing(this.matchingSources);
                base.ClearStoredState();
                this.matchingSources = null;
                if (this.pipeline != null)
                {
                    this.pipeline.Dispose();
                    this.pipeline = null;
                }
                if (base.FileStreams != null)
                {
                    foreach (FileStream stream in base.FileStreams)
                    {
                        stream.Flush();
                        stream.Close();
                    }
                }
                GC.SuppressFinalize(this);
            }
        }

        protected override void EndProcessing()
        {
            if (this.pipeline != null)
            {
                base.TurnOnTracing(this.matchingSources, false);
                Array sendToPipeline = this.pipeline.SynchronousExecute(null, null);
                base.ResetTracing(this.matchingSources);
                base.WriteObject(sendToPipeline, true);
            }
            this.Dispose();
        }

        protected override void ProcessRecord()
        {
            base.TurnOnTracing(this.matchingSources, false);
            object obj2 = null;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if (!(parameterSetName == "expressionSet"))
                {
                    if (parameterSetName == "commandSet")
                    {
                        obj2 = this.StepCommand();
                    }
                }
                else
                {
                    obj2 = this.RunExpression();
                }
            }
            base.ResetTracing(this.matchingSources);
            if ((obj2 != null) && !LanguagePrimitives.IsNull(obj2))
            {
                base.WriteObject(obj2, true);
            }
        }

        private object RunExpression()
        {
            object[] input = new object[] { this.InputObject };
            return this.expression.DoInvokeReturnAsIs(false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, this.InputObject, input, AutomationNull.Value, new object[0]);
        }

        private object StepCommand()
        {
            if (this.InputObject != AutomationNull.Value)
            {
                this.pipeline.Step(this.InputObject);
            }
            return null;
        }

        protected override void StopProcessing()
        {
            if (this.pipeline != null)
            {
                this.pipeline.Stop();
            }
        }

        [Parameter(ParameterSetName="commandSet", ValueFromRemainingArguments=true), Alias(new string[] { "Args" })]
        public object[] ArgumentList
        {
            get
            {
                return this.commandArgs;
            }
            set
            {
                this.commandArgs = value;
            }
        }

        [Parameter(Position=1, Mandatory=true, ParameterSetName="commandSet")]
        public string Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }

        [Parameter]
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

        [Parameter(Position=1, Mandatory=true, ParameterSetName="expressionSet")]
        public ScriptBlock Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }
        }

        [Parameter, Alias(new string[] { "PSPath" })]
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

        [Parameter]
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

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this._inputObject;
            }
            set
            {
                this._inputObject = value;
            }
        }

        [Parameter]
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

        [Parameter(Position=0, Mandatory=true)]
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

        [Parameter(Position=2)]
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

        [Parameter]
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
    }
}

