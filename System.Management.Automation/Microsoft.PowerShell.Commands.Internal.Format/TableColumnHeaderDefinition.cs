namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class TableColumnHeaderDefinition
    {
        internal int alignment;
        internal TextToken label;
        internal int width {
			get { return _width; }
			set {
				_width = value;
			}
		}

		private int _width;
    }
}

