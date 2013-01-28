namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    internal class FileRedirection : CommandRedirection, IDisposable
    {
        private bool _disposed;

        internal FileRedirection(RedirectionStream from, bool appending, string file) : base(from)
        {
            this.File = file;
            this.Appending = appending;
        }

        internal override void Bind(System.Management.Automation.Internal.PipelineProcessor pipelineProcessor, CommandProcessorBase commandProcessor, ExecutionContext context)
        {
            Pipe redirectionPipe = this.GetRedirectionPipe(context, pipelineProcessor);
            switch (base.FromStream)
            {
                case RedirectionStream.All:
                    commandProcessor.CommandRuntime.OutputPipe = redirectionPipe;
                    commandProcessor.CommandRuntime.ErrorOutputPipe = redirectionPipe;
                    commandProcessor.CommandRuntime.WarningOutputPipe = redirectionPipe;
                    commandProcessor.CommandRuntime.VerboseOutputPipe = redirectionPipe;
                    commandProcessor.CommandRuntime.DebugOutputPipe = redirectionPipe;
                    return;

                case RedirectionStream.Output:
                    commandProcessor.CommandRuntime.OutputPipe = redirectionPipe;
                    return;

                case RedirectionStream.Error:
                    commandProcessor.CommandRuntime.ErrorOutputPipe = redirectionPipe;
                    return;

                case RedirectionStream.Warning:
                    commandProcessor.CommandRuntime.WarningOutputPipe = redirectionPipe;
                    return;

                case RedirectionStream.Verbose:
                    commandProcessor.CommandRuntime.VerboseOutputPipe = redirectionPipe;
                    return;

                case RedirectionStream.Debug:
                    commandProcessor.CommandRuntime.DebugOutputPipe = redirectionPipe;
                    break;

                case RedirectionStream.Host:
                    break;

                default:
                    return;
            }
        }

        internal Pipe[] BindForExpression(FunctionContext funcContext)
        {
            ExecutionContext context = funcContext._executionContext;
            Pipe redirectionPipe = this.GetRedirectionPipe(context, null);
            Pipe[] pipeArray = new Pipe[7];
            switch (base.FromStream)
            {
                case RedirectionStream.All:
                    pipeArray[1] = funcContext._outputPipe;
                    pipeArray[2] = context.ShellFunctionErrorOutputPipe;
                    pipeArray[3] = context.ExpressionWarningOutputPipe;
                    pipeArray[4] = context.ExpressionVerboseOutputPipe;
                    pipeArray[5] = context.ExpressionDebugOutputPipe;
                    funcContext._outputPipe = redirectionPipe;
                    context.ShellFunctionErrorOutputPipe = redirectionPipe;
                    context.ExpressionWarningOutputPipe = redirectionPipe;
                    context.ExpressionVerboseOutputPipe = redirectionPipe;
                    context.ExpressionDebugOutputPipe = redirectionPipe;
                    return pipeArray;

                case RedirectionStream.Output:
                    pipeArray[1] = funcContext._outputPipe;
                    funcContext._outputPipe = redirectionPipe;
                    return pipeArray;

                case RedirectionStream.Error:
                    pipeArray[(int) base.FromStream] = context.ShellFunctionErrorOutputPipe;
                    context.ShellFunctionErrorOutputPipe = redirectionPipe;
                    return pipeArray;

                case RedirectionStream.Warning:
                    pipeArray[(int) base.FromStream] = context.ExpressionWarningOutputPipe;
                    context.ExpressionWarningOutputPipe = redirectionPipe;
                    return pipeArray;

                case RedirectionStream.Verbose:
                    pipeArray[(int) base.FromStream] = context.ExpressionVerboseOutputPipe;
                    context.ExpressionVerboseOutputPipe = redirectionPipe;
                    return pipeArray;

                case RedirectionStream.Debug:
                    pipeArray[(int) base.FromStream] = context.ExpressionDebugOutputPipe;
                    context.ExpressionDebugOutputPipe = redirectionPipe;
                    return pipeArray;

                case RedirectionStream.Host:
                    return pipeArray;
            }
            return pipeArray;
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
                if (disposing && (this.PipelineProcessor != null))
                {
                    this.PipelineProcessor.Dispose();
                }
                this._disposed = true;
            }
        }

        internal Pipe GetRedirectionPipe(ExecutionContext context, System.Management.Automation.Internal.PipelineProcessor parentPipelineProcessor)
        {
            if (string.IsNullOrWhiteSpace(this.File))
            {
                return new Pipe { NullPipe = true };
            }
            CommandProcessorBase commandProcessor = context.CreateCommand("out-file", false);
            CommandParameterInternal parameter = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, "Encoding", "-Encoding:", PositionUtilities.EmptyExtent, "Unicode", false);
            commandProcessor.AddParameter(parameter);
            if (this.Appending)
            {
                parameter = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, "Append", "-Append:", PositionUtilities.EmptyExtent, true, false);
                commandProcessor.AddParameter(parameter);
            }
            parameter = CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, "Filepath", "-Filepath:", PositionUtilities.EmptyExtent, this.File, false);
            commandProcessor.AddParameter(parameter);
            this.PipelineProcessor = new System.Management.Automation.Internal.PipelineProcessor();
            this.PipelineProcessor.Add(commandProcessor);
            try
            {
                this.PipelineProcessor.StartStepping(true);
            }
            catch (RuntimeException exception)
            {
                if (exception.ErrorRecord.Exception is ArgumentException)
                {
                    throw InterpreterError.NewInterpreterExceptionWithInnerException(null, typeof(RuntimeException), null, "RedirectionFailed", ParserStrings.RedirectionFailed, exception.ErrorRecord.Exception, new object[] { this.File, exception.ErrorRecord.Exception.Message });
                }
                throw;
            }
            if (parentPipelineProcessor != null)
            {
                parentPipelineProcessor.AddRedirectionPipe(this.PipelineProcessor);
            }
            return new Pipe(context, this.PipelineProcessor);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}> {1}", new object[] { (base.FromStream == RedirectionStream.All) ? "*" : ((int) base.FromStream).ToString(CultureInfo.InvariantCulture), this.File });
        }

        internal bool Appending { get; private set; }

        internal string File { get; private set; }

        private System.Management.Automation.Internal.PipelineProcessor PipelineProcessor { get; set; }
    }
}

