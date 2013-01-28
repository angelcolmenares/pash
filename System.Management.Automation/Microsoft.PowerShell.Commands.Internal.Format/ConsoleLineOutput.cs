namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Specialized;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;

    internal sealed class ConsoleLineOutput : LineOutput
    {
        private Microsoft.PowerShell.Commands.Internal.Format.DisplayCells _displayCellsPSHost;
        private PSHostUserInterface console;
        private bool disableLineWrittenEvent;
        private TerminatingErrorContext errorContext;
        private int fallbackRawConsoleColumnNumber = 80;
        private int fallbackRawConsoleRowNumber = 40;
        private bool forceNewLine = true;
        private long linesWritten;
        private PromptHandler prompt;
        [TraceSource("ConsoleLineOutput", "ConsoleLineOutput")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("ConsoleLineOutput", "ConsoleLineOutput");
        private WriteLineHelper writeLineHelper;

        internal ConsoleLineOutput(PSHostUserInterface hostConsole, bool paging, bool lineWrap, TerminatingErrorContext errorContext)
        {
            if (hostConsole == null)
            {
                throw PSTraceSource.NewArgumentNullException("hostConsole");
            }
            if (errorContext == null)
            {
                throw PSTraceSource.NewArgumentNullException("errorContext");
            }
            this.console = hostConsole;
            this.errorContext = errorContext;
            if (paging)
            {
                tracer.WriteLine("paging is needed", new object[0]);
                string s = StringUtil.Format(FormatAndOut_out_xxx.ConsoleLineOutput_PagingPrompt, new object[0]);
                this.prompt = new PromptHandler(s, this);
            }
            PSHostRawUserInterface rawUI = this.console.RawUI;
            if (rawUI != null)
            {
                tracer.WriteLine("there is a valid raw interface", new object[0]);
                this._displayCellsPSHost = new DisplayCellsPSHost(rawUI);
            }
            WriteLineHelper.WriteCallback wlc = new WriteLineHelper.WriteCallback(this.OnWriteLine);
            WriteLineHelper.WriteCallback wc = new WriteLineHelper.WriteCallback(this.OnWrite);
            if (this.forceNewLine)
            {
                this.writeLineHelper = new WriteLineHelper(lineWrap, wlc, null, this.DisplayCells);
            }
            else
            {
                this.writeLineHelper = new WriteLineHelper(lineWrap, wlc, wc, this.DisplayCells);
            }
        }

        private void LineWrittenEvent()
        {
            if (!this.disableLineWrittenEvent && (this.prompt != null))
            {
                this.linesWritten += 1L;
                if (this.NeedToPrompt)
                {
                    this.disableLineWrittenEvent = true;
                    PromptHandler.PromptResponse response = this.prompt.PromptUser(this.console);
                    this.disableLineWrittenEvent = false;
                    switch (response)
                    {
                        case PromptHandler.PromptResponse.NextPage:
                            this.linesWritten = 0L;
                            return;

                        case PromptHandler.PromptResponse.NextLine:
                            this.linesWritten -= 1L;
                            return;

                        case PromptHandler.PromptResponse.Quit:
                            throw new HaltCommandException();
                    }
                }
            }
        }

        private void OnWrite(string s)
        {
            switch (base.WriteStream)
            {
                case WriteStreamType.Error:
                    this.console.WriteErrorLine(s);
                    break;

                case WriteStreamType.Warning:
                    this.console.WriteWarningLine(s);
                    break;

                case WriteStreamType.Verbose:
                    this.console.WriteVerboseLine(s);
                    break;

                case WriteStreamType.Debug:
                    this.console.WriteDebugLine(s);
                    break;

                default:
                    this.console.Write(s);
                    break;
            }
            this.LineWrittenEvent();
        }

        private void OnWriteLine(string s)
        {
            switch (base.WriteStream)
            {
                case WriteStreamType.Error:
                    this.console.WriteErrorLine(s);
                    break;

                case WriteStreamType.Warning:
                    this.console.WriteWarningLine(s);
                    break;

                case WriteStreamType.Verbose:
                    this.console.WriteVerboseLine(s);
                    break;

                case WriteStreamType.Debug:
                    this.console.WriteDebugLine(s);
                    break;

                default:
                    this.console.WriteLine(s);
                    break;
            }
            this.LineWrittenEvent();
        }

        internal override void WriteLine(string s)
        {
            base.CheckStopProcessing();
            this.writeLineHelper.WriteLine(s, this.ColumnNumber);
        }

        internal override int ColumnNumber
        {
            get
            {
                base.CheckStopProcessing();
                PSHostRawUserInterface rawUI = this.console.RawUI;
                try
                {
                    return (this.forceNewLine ? (rawUI.BufferSize.Width - 1) : rawUI.BufferSize.Width);
                }
                catch (HostException)
                {
                }
                if (!this.forceNewLine)
                {
                    return this.fallbackRawConsoleColumnNumber;
                }
                return (this.fallbackRawConsoleColumnNumber - 1);
            }
        }

        internal override Microsoft.PowerShell.Commands.Internal.Format.DisplayCells DisplayCells
        {
            get
            {
                base.CheckStopProcessing();
                if (this._displayCellsPSHost != null)
                {
                    return this._displayCellsPSHost;
                }
                return this._displayCellsPSHost;
            }
        }

        private bool NeedToPrompt
        {
            get
            {
                if (this.RowNumber <= 0)
                {
                    return false;
                }
                int num2 = this.prompt.ComputePromptLines(this.DisplayCells, this.ColumnNumber);
                int num3 = this.RowNumber - num2;
                if (num3 <= 0)
                {
                    tracer.WriteLine("No available Lines; suppress prompting", new object[0]);
                    return false;
                }
                return (this.linesWritten >= num3);
            }
        }

        internal override int RowNumber
        {
            get
            {
                base.CheckStopProcessing();
                PSHostRawUserInterface rawUI = this.console.RawUI;
                try
                {
                    return rawUI.WindowSize.Height;
                }
                catch (HostException)
                {
                }
                return this.fallbackRawConsoleRowNumber;
            }
        }

        private class PromptHandler
        {
            private StringCollection actualPrompt;
            private ConsoleLineOutput callingCmdlet;
            private string promptString;

            internal PromptHandler(string s, ConsoleLineOutput cmdlet)
            {
                if (string.IsNullOrEmpty(s))
                {
                    throw PSTraceSource.NewArgumentNullException("s");
                }
                this.promptString = s;
                this.callingCmdlet = cmdlet;
            }

            internal int ComputePromptLines(DisplayCells displayCells, int cols)
            {
                this.actualPrompt = StringManipulationHelper.GenerateLines(displayCells, this.promptString, cols, cols);
                return this.actualPrompt.Count;
            }

            internal PromptResponse PromptUser(PSHostUserInterface console)
            {
                char ch = char.MinValue;
                for (int i = 0; i < this.actualPrompt.Count; i++)
                {
                    if (i < (this.actualPrompt.Count - 1))
                    {
                        console.WriteLine(this.actualPrompt[i]);
                    }
                    else
                    {
                        console.Write(this.actualPrompt[i]);
                    }
                }
                do
                {
                    this.callingCmdlet.CheckStopProcessing();
                    switch (console.RawUI.ReadKey(ReadKeyOptions.IncludeKeyUp | ReadKeyOptions.NoEcho).Character)
                    {
                        case 'q':
                        case 'Q':
                            console.WriteLine();
                            return PromptResponse.Quit;

                        case ' ':
                            console.WriteLine();
                            return PromptResponse.NextPage;
                    }
                }
                while (ch != '\r');
                console.WriteLine();
                return PromptResponse.NextLine;
            }

            internal enum PromptResponse
            {
                NextPage,
                NextLine,
                Quit
            }
        }
    }
}

