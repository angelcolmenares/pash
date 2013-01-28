namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class TypeMatchItem
    {
        private Microsoft.PowerShell.Commands.Internal.Format.AppliesTo _appliesTo;
        private PSObject _currentObject;
        private object _item;

        internal TypeMatchItem(object obj, Microsoft.PowerShell.Commands.Internal.Format.AppliesTo a)
        {
            this._item = obj;
            this._appliesTo = a;
        }

        internal TypeMatchItem(object obj, Microsoft.PowerShell.Commands.Internal.Format.AppliesTo a, PSObject currentObject)
        {
            this._item = obj;
            this._appliesTo = a;
            this._currentObject = currentObject;
        }

        internal Microsoft.PowerShell.Commands.Internal.Format.AppliesTo AppliesTo
        {
            get
            {
                return this._appliesTo;
            }
        }

        internal PSObject CurrentObject
        {
            get
            {
                return this._currentObject;
            }
        }

        internal object Item
        {
            get
            {
                return this._item;
            }
        }
    }
}

