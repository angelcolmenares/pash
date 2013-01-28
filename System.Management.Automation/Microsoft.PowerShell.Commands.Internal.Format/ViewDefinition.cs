namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class ViewDefinition
    {
        private Guid _instanceId = Guid.NewGuid();
        internal AppliesTo appliesTo = new AppliesTo();
        internal FormatControlDefinitionHolder formatControlDefinitionHolder = new FormatControlDefinitionHolder();
        internal GroupBy groupBy;
        internal DatabaseLoadingInfo loadingInfo;
        internal ControlBase mainControl;
        internal string name;
        internal bool outOfBand;

        internal ViewDefinition()
        {
        }

        internal Guid InstanceId
        {
            get
            {
                return this._instanceId;
            }
        }
    }
}

