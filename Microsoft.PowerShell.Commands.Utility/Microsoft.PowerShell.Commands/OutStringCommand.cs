namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Text;

    [OutputType(new Type[] { typeof(string) }), Cmdlet("Out", "String", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113368", RemotingCapability=RemotingCapability.None)]
    public class OutStringCommand : FrontEndCommandBase
    {
        private StringBuilder buffer = new StringBuilder();
        private bool stream;
        private int? width = null;
        private StreamingTextWriter writer;

        public OutStringCommand()
        {
            base.implementation = new OutputManagerInner();
        }

        protected override void BeginProcessing()
        {
            OutputManagerInner implementation = (OutputManagerInner) base.implementation;
            implementation.LineOutput = this.InstantiateLineOutputInterface();
            base.BeginProcessing();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            this.writer.Flush();
            this.writer.Close();
            if (!this.stream)
            {
                base.WriteObject(this.buffer.ToString());
            }
        }

        private LineOutput InstantiateLineOutputInterface()
        {
            StreamingTextWriter.WriteLineCallback writeCall = new StreamingTextWriter.WriteLineCallback(this.OnWriteLine);
            this.writer = new StreamingTextWriter(writeCall, base.Host.CurrentCulture);
            int columns = 80;
            if (this.width.HasValue)
            {
                columns = this.width.Value;
            }
            else
            {
                try
                {
                    columns = base.Host.UI.RawUI.BufferSize.Width - 1;
                }
                catch (HostException)
                {
                }
            }
            return new TextWriterLineOutput(this.writer, columns);
        }

        private void OnWriteLine(string s)
        {
            if (this.stream)
            {
                base.WriteObject(s);
            }
            else
            {
                this.buffer.AppendLine(s);
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            this.writer.Flush();
        }

        [Parameter]
        public SwitchParameter Stream
        {
            get
            {
                return this.stream;
            }
            set
            {
                this.stream = (bool) value;
            }
        }

        [ValidateRange(2, 0x7fffffff), Parameter]
        public int Width
        {
            get
            {
                if (!this.width.HasValue)
                {
                    return 0;
                }
                return this.width.Value;
            }
            set
            {
                this.width = new int?(value);
            }
        }
    }
}

