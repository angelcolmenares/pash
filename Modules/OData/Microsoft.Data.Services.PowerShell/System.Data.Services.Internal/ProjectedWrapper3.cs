namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ProjectedWrapper3 : ProjectedWrapper
    {
        protected override object InternalGetProjectedPropertyValue(int propertyIndex)
        {
            switch (propertyIndex)
            {
                case 0:
                    return this.ProjectedProperty0;

                case 1:
                    return this.ProjectedProperty1;

                case 2:
                    return this.ProjectedProperty2;
            }
            throw Error.NotSupported();
        }

        public object ProjectedProperty0 { get; set; }

        public object ProjectedProperty1 { get; set; }

        public object ProjectedProperty2 { get; set; }
    }
}

