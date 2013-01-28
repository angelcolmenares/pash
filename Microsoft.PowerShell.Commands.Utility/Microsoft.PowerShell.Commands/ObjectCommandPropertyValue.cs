namespace Microsoft.PowerShell.Commands
{
    using System;

    internal class ObjectCommandPropertyValue
    {
        internal static readonly ObjectCommandPropertyValue ExistingNullProperty = new ObjectCommandPropertyValue(null);
        private bool isExistingProperty;
        internal static readonly ObjectCommandPropertyValue NonExistingProperty = new ObjectCommandPropertyValue();
        private object propertyValue;

        private ObjectCommandPropertyValue()
        {
        }

        internal ObjectCommandPropertyValue(object propVal)
        {
            this.propertyValue = propVal;
            this.isExistingProperty = true;
        }

        internal bool IsExistingProperty
        {
            get
            {
                return this.isExistingProperty;
            }
        }

        internal object PropertyValue
        {
            get
            {
                return this.propertyValue;
            }
        }
    }
}

