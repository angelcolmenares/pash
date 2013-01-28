namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class StreamingTextWriter : TextWriter
    {
        [TraceSource("StreamingTextWriter", "StreamingTextWriter")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("StreamingTextWriter", "StreamingTextWriter");
        private WriteLineCallback writeCall;

        internal StreamingTextWriter(WriteLineCallback writeCall, CultureInfo culture) : base(culture)
        {
            if (writeCall == null)
            {
                throw PSTraceSource.NewArgumentNullException("writeCall");
            }
            this.writeCall = writeCall;
        }

        public override void WriteLine(string s)
        {
            this.writeCall(s);
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return new UnicodeEncoding();
            }
        }

        internal delegate void WriteLineCallback(string s);
    }
}

