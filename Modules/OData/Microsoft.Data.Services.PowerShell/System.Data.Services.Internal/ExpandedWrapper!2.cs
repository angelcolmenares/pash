namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ExpandedWrapper<TExpandedElement, TProperty0> : ExpandedWrapper<TExpandedElement>
    {
        protected override object InternalGetExpandedPropertyValue(int nameIndex)
        {
            if (nameIndex != 0)
            {
                throw Error.NotSupported();
            }
            return this.ProjectedProperty0;
        }

        public TProperty0 ProjectedProperty0 { get; set; }
    }
}

