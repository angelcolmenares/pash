namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;

    internal class MergingRedirection : CommandRedirection
    {
        internal MergingRedirection(RedirectionStream from, RedirectionStream to) : base(from)
        {
            if (to != RedirectionStream.Output)
            {
                throw PSTraceSource.NewArgumentException("to");
            }
        }

        internal override void Bind(PipelineProcessor pipelineProcessor, CommandProcessorBase commandProcessor, ExecutionContext context)
        {
            Pipe outputPipe = commandProcessor.CommandRuntime.OutputPipe;
            switch (base.FromStream)
            {
                case RedirectionStream.All:
                    commandProcessor.CommandRuntime.ErrorMergeTo = MshCommandRuntime.MergeDataStream.Output;
                    commandProcessor.CommandRuntime.WarningOutputPipe = outputPipe;
                    commandProcessor.CommandRuntime.VerboseOutputPipe = outputPipe;
                    commandProcessor.CommandRuntime.DebugOutputPipe = outputPipe;
                    return;

                case RedirectionStream.Output:
                case RedirectionStream.Host:
                    break;

                case RedirectionStream.Error:
                    commandProcessor.CommandRuntime.ErrorMergeTo = MshCommandRuntime.MergeDataStream.Output;
                    return;

                case RedirectionStream.Warning:
                    commandProcessor.CommandRuntime.WarningOutputPipe = outputPipe;
                    return;

                case RedirectionStream.Verbose:
                    commandProcessor.CommandRuntime.VerboseOutputPipe = outputPipe;
                    return;

                case RedirectionStream.Debug:
                    commandProcessor.CommandRuntime.DebugOutputPipe = outputPipe;
                    break;

                default:
                    return;
            }
        }

        internal Pipe[] BindForExpression(ExecutionContext context, FunctionContext funcContext)
        {
            Pipe[] pipeArray = new Pipe[7];
            Pipe pipe = funcContext._outputPipe;
            switch (base.FromStream)
            {
                case RedirectionStream.All:
                    pipeArray[1] = funcContext._outputPipe;
                    pipeArray[2] = context.ShellFunctionErrorOutputPipe;
                    context.ShellFunctionErrorOutputPipe = pipe;
                    pipeArray[3] = context.ExpressionWarningOutputPipe;
                    context.ExpressionWarningOutputPipe = pipe;
                    pipeArray[4] = context.ExpressionVerboseOutputPipe;
                    context.ExpressionVerboseOutputPipe = pipe;
                    pipeArray[5] = context.ExpressionDebugOutputPipe;
                    context.ExpressionDebugOutputPipe = pipe;
                    return pipeArray;

                case RedirectionStream.Output:
                    pipeArray[1] = funcContext._outputPipe;
                    return pipeArray;

                case RedirectionStream.Error:
                    pipeArray[(int) base.FromStream] = context.ShellFunctionErrorOutputPipe;
                    context.ShellFunctionErrorOutputPipe = pipe;
                    return pipeArray;

                case RedirectionStream.Warning:
                    pipeArray[(int) base.FromStream] = context.ExpressionWarningOutputPipe;
                    context.ExpressionWarningOutputPipe = pipe;
                    return pipeArray;

                case RedirectionStream.Verbose:
                    pipeArray[(int) base.FromStream] = context.ExpressionVerboseOutputPipe;
                    context.ExpressionVerboseOutputPipe = pipe;
                    return pipeArray;

                case RedirectionStream.Debug:
                    pipeArray[(int) base.FromStream] = context.ExpressionDebugOutputPipe;
                    context.ExpressionDebugOutputPipe = pipe;
                    return pipeArray;

                case RedirectionStream.Host:
                    return pipeArray;
            }
            return pipeArray;
        }

        public override string ToString()
        {
            if (base.FromStream != RedirectionStream.All)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}>&1", new object[] { (int) base.FromStream });
            }
            return "*>&1";
        }
    }
}

