namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ProjectedWrapper1 : ProjectedWrapper
    {
        protected override object InternalGetProjectedPropertyValue(int propertyIndex)
        {
            if (propertyIndex != 0)
            {
                throw Error.NotSupported();
            }
            return this.ProjectedProperty0;
        }

        public object ProjectedProperty0 { get; set; }
    }
}

