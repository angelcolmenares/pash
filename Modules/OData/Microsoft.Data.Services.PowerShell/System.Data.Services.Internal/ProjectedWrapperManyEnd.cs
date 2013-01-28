namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ProjectedWrapperManyEnd : ProjectedWrapperMany
    {
        protected override object InternalGetProjectedPropertyValue(int propertyIndex)
        {
            throw Error.NotSupported();
        }
    }
}

