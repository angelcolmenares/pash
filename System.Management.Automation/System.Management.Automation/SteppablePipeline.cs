namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    public sealed class SteppablePipeline : IDisposable
    {
        private ExecutionContext _context;
        private bool _disposed;
        private bool _expectInput;
        private PipelineProcessor _pipeline;

        internal SteppablePipeline(ExecutionContext context, PipelineProcessor pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException("pipeline");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._pipeline = pipeline;
            this._context = context;
        }

        public void Begin(bool expectInput)
        {
            this.Begin(expectInput, (ICommandRuntime) null);
        }

        public void Begin(InternalCommand command)
        {
            if ((command == null) || (command.MyInvocation == null))
            {
                throw new ArgumentNullException("command");
            }
            this.Begin(command.MyInvocation.ExpectingInput, command.commandRuntime);
        }

        public void Begin(bool expectInput, EngineIntrinsics contextToRedirectTo)
        {
            if (contextToRedirectTo == null)
            {
                throw new ArgumentNullException("contextToRedirectTo");
            }
            CommandProcessorBase currentCommandProcessor = contextToRedirectTo.SessionState.Internal.ExecutionContext.CurrentCommandProcessor;
            ICommandRuntime commandRuntime = (currentCommandProcessor == null) ? null : currentCommandProcessor.CommandRuntime;
            this.Begin(expectInput, commandRuntime);
        }

        private void Begin(bool expectInput, ICommandRuntime commandRuntime)
        {
            try
            {
                this._pipeline.ExecutionScope = this._context.EngineSessionState.CurrentScope;
                this._context.PushPipelineProcessor(this._pipeline);
                this._expectInput = expectInput;
                MshCommandRuntime runtime = commandRuntime as MshCommandRuntime;
                if (runtime != null)
                {
                    if (runtime.OutputPipe != null)
                    {
                        this._pipeline.LinkPipelineSuccessOutput(runtime.OutputPipe);
                    }
                    if (runtime.ErrorOutputPipe != null)
                    {
                        this._pipeline.LinkPipelineErrorOutput(runtime.ErrorOutputPipe);
                    }
                }
                this._pipeline.StartStepping(this._expectInput);
            }
            finally
            {
                this._context.PopPipelineProcessor(true);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._pipeline.Dispose();
                }
                this._disposed = true;
            }
        }

        public Array End()
        {
            Array array;
            try
            {
                this._context.PushPipelineProcessor(this._pipeline);
                array = this._pipeline.DoComplete();
            }
            finally
            {
                this._context.PopPipelineProcessor(true);
                this._pipeline.Dispose();
            }
            return array;
        }

        ~SteppablePipeline()
        {
            this.Dispose(false);
        }

        public Array Process()
        {
            Array array;
            try
            {
                this._context.PushPipelineProcessor(this._pipeline);
                array = this._pipeline.Step(AutomationNull.Value);
            }
            finally
            {
                this._context.PopPipelineProcessor(true);
            }
            return array;
        }

        public Array Process(PSObject input)
        {
            Array array;
            try
            {
                this._context.PushPipelineProcessor(this._pipeline);
                if (this._expectInput)
                {
                    return this._pipeline.Step(input);
                }
                array = this._pipeline.Step(AutomationNull.Value);
            }
            finally
            {
                this._context.PopPipelineProcessor(true);
            }
            return array;
        }

        public Array Process(object input)
        {
            Array array;
            try
            {
                this._context.PushPipelineProcessor(this._pipeline);
                if (this._expectInput)
                {
                    return this._pipeline.Step(input);
                }
                array = this._pipeline.Step(AutomationNull.Value);
            }
            finally
            {
                this._context.PopPipelineProcessor(true);
            }
            return array;
        }
    }
}

