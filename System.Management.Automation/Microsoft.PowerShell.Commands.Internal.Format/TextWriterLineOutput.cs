namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.IO;

    internal class TextWriterLineOutput : LineOutput
    {
        private int columns;
        private TextWriter writer;

        internal TextWriterLineOutput(TextWriter writer, int columns)
        {
            this.writer = writer;
            this.columns = columns;
        }

        internal override void WriteLine(string s)
        {
            base.CheckStopProcessing();
            this.writer.WriteLine(s);
        }

        internal override int ColumnNumber
        {
            get
            {
                base.CheckStopProcessing();
                return this.columns;
            }
        }

        internal override int RowNumber
        {
            get
            {
                base.CheckStopProcessing();
                return -1;
            }
        }
    }
}

