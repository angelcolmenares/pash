namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class DefaultSettingsSection
    {
        private bool? _multilineTables;
        internal List<EnumerableExpansionDirective> enumerableExpansionDirectiveList = new List<EnumerableExpansionDirective>();
        internal FormatErrorPolicy formatErrorPolicy = new FormatErrorPolicy();
        internal ShapeSelectionDirectives shapeSelectionDirectives = new ShapeSelectionDirectives();

        internal bool MultilineTables
        {
            get
            {
                if (this._multilineTables.HasValue)
                {
                    return this._multilineTables.Value;
                }
                return false;
            }
            set
            {
                if (!this._multilineTables.HasValue)
                {
                    this._multilineTables = new bool?(value);
                }
            }
        }
    }
}

