namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;

    public sealed class TableControlColumnHeader
    {
        private System.Management.Automation.Alignment _alignment;
        private string _label;
        private int _width;

        internal TableControlColumnHeader()
        {
        }

        internal TableControlColumnHeader(TableColumnHeaderDefinition colheaderdefinition)
        {
            if (colheaderdefinition.label != null)
            {
                this._label = colheaderdefinition.label.text;
            }
            this._alignment = (System.Management.Automation.Alignment) colheaderdefinition.alignment;
            this._width = colheaderdefinition.width;
        }

        public TableControlColumnHeader(string label, int width, System.Management.Automation.Alignment alignment)
        {
            if (width < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("width", width);
            }
            this._label = label;
            this._width = width;
            this._alignment = alignment;
        }

        public System.Management.Automation.Alignment Alignment
        {
            get
            {
                return this._alignment;
            }
            internal set
            {
                this._alignment = value;
            }
        }

        public string Label
        {
            get
            {
                return this._label;
            }
            internal set
            {
                this._label = value;
            }
        }

        public int Width
        {
            get
            {
                return this._width;
            }
            internal set
            {
                this._width = value;
            }
        }
    }
}

