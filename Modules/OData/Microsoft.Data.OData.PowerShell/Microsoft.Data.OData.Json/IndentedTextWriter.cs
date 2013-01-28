namespace Microsoft.Data.OData.Json
{
    using System;
    using System.IO;
    using System.Text;

    internal sealed class IndentedTextWriter : TextWriter
    {
        private readonly bool enableIndentation;
        private bool indentationPending;
        private const string IndentationString = "  ";
        private int indentLevel;
        private readonly TextWriter writer;

        public IndentedTextWriter(TextWriter writer, bool enableIndentation) : base(writer.FormatProvider)
        {
            this.writer = writer;
            this.enableIndentation = enableIndentation;
        }

        public override void Close()
        {
            InternalCloseOrDispose();
        }

        public void DecreaseIndentation()
        {
            if (this.indentLevel < 1)
            {
                this.indentLevel = 0;
            }
            else
            {
                this.indentLevel--;
            }
        }

        public override void Flush()
        {
            this.writer.Flush();
        }

        public void IncreaseIndentation()
        {
            this.indentLevel++;
        }

        private static void InternalCloseOrDispose()
        {
            throw new NotImplementedException();
        }

        public override void Write(char value)
        {
            this.WriteIndentation();
            this.writer.Write(value);
        }

        public override void Write(string s)
        {
            this.WriteIndentation();
            this.writer.Write(s);
        }

        private void WriteIndentation()
        {
            if (this.enableIndentation && this.indentationPending)
            {
                for (int i = 0; i < this.indentLevel; i++)
                {
                    this.writer.Write("  ");
                }
                this.indentationPending = false;
            }
        }

        public override void WriteLine()
        {
            if (this.enableIndentation)
            {
                base.WriteLine();
            }
            this.indentationPending = true;
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this.writer.Encoding;
            }
        }

        public override string NewLine
        {
            get
            {
                return this.writer.NewLine;
            }
        }
    }
}

