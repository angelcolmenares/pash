namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    internal abstract class CommandRedirection
    {
        protected CommandRedirection(RedirectionStream from)
        {
            this.FromStream = from;
        }

        internal abstract void Bind(PipelineProcessor pipelineProcessor, CommandProcessorBase commandProcessor, ExecutionContext context);
        internal void UnbindForExpression(FunctionContext funcContext, Pipe[] pipes)
        {
            if (pipes != null)
            {
                ExecutionContext context = funcContext._executionContext;
                switch (this.FromStream)
                {
                    case RedirectionStream.All:
                        funcContext._outputPipe = pipes[1];
                        context.ShellFunctionErrorOutputPipe = pipes[2];
                        context.ExpressionWarningOutputPipe = pipes[3];
                        context.ExpressionVerboseOutputPipe = pipes[4];
                        context.ExpressionDebugOutputPipe = pipes[5];
                        return;

                    case RedirectionStream.Output:
                        funcContext._outputPipe = pipes[1];
                        return;

                    case RedirectionStream.Error:
                        context.ShellFunctionErrorOutputPipe = pipes[(int) this.FromStream];
                        return;

                    case RedirectionStream.Warning:
                        context.ExpressionWarningOutputPipe = pipes[(int) this.FromStream];
                        return;

                    case RedirectionStream.Verbose:
                        context.ExpressionVerboseOutputPipe = pipes[(int) this.FromStream];
                        return;

                    case RedirectionStream.Debug:
                        context.ExpressionDebugOutputPipe = pipes[(int) this.FromStream];
                        return;

                    case RedirectionStream.Host:
                        return;
                }
            }
        }

        internal RedirectionStream FromStream { get; private set; }
    }
}

