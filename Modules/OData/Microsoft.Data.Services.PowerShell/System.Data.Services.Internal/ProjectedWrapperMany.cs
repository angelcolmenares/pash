namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ProjectedWrapperMany : ProjectedWrapper
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

                case 3:
                    return this.ProjectedProperty3;

                case 4:
                    return this.ProjectedProperty4;

                case 5:
                    return this.ProjectedProperty5;

                case 6:
                    return this.ProjectedProperty6;

                case 7:
                    return this.ProjectedProperty7;
            }
            if ((this.Next == null) || (propertyIndex < 0))
            {
                throw Error.NotSupported();
            }
            return this.Next.InternalGetProjectedPropertyValue(propertyIndex - 8);
        }

        public ProjectedWrapperMany Next { get; set; }

        public object ProjectedProperty0 { get; set; }

        public object ProjectedProperty1 { get; set; }

        public object ProjectedProperty2 { get; set; }

        public object ProjectedProperty3 { get; set; }

        public object ProjectedProperty4 { get; set; }

        public object ProjectedProperty5 { get; set; }

        public object ProjectedProperty6 { get; set; }

        public object ProjectedProperty7 { get; set; }
    }
}

