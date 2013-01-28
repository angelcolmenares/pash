namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class ShapeSelectionDirectives
    {
        private int? _propertyCountForTable = null;
        internal List<FormatShapeSelectionOnType> formatShapeSelectionOnTypeList = new List<FormatShapeSelectionOnType>();

        internal int PropertyCountForTable
        {
            get
            {
                if (this._propertyCountForTable.HasValue)
                {
                    return this._propertyCountForTable.Value;
                }
                return 4;
            }
            set
            {
                if (!this._propertyCountForTable.HasValue)
                {
                    this._propertyCountForTable = new int?(value);
                }
            }
        }
    }
}

