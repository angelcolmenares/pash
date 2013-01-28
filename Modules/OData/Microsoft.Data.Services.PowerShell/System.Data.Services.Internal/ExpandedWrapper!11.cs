namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ExpandedWrapper<TExpandedElement, TProperty0, TProperty1, TProperty2, TProperty3, TProperty4, TProperty5, TProperty6, TProperty7, TProperty8, TProperty9> : ExpandedWrapper<TExpandedElement>
    {
        protected override object InternalGetExpandedPropertyValue(int nameIndex)
        {
            if (nameIndex == 0)
            {
                return this.ProjectedProperty0;
            }
            if (nameIndex == 1)
            {
                return this.ProjectedProperty1;
            }
            if (nameIndex == 2)
            {
                return this.ProjectedProperty2;
            }
            if (nameIndex == 3)
            {
                return this.ProjectedProperty3;
            }
            if (nameIndex == 4)
            {
                return this.ProjectedProperty4;
            }
            if (nameIndex == 5)
            {
                return this.ProjectedProperty5;
            }
            if (nameIndex == 6)
            {
                return this.ProjectedProperty6;
            }
            if (nameIndex == 7)
            {
                return this.ProjectedProperty7;
            }
            if (nameIndex == 8)
            {
                return this.ProjectedProperty8;
            }
            if (nameIndex != 9)
            {
                throw Error.NotSupported();
            }
            return this.ProjectedProperty9;
        }

        public TProperty0 ProjectedProperty0 { get; set; }

        public TProperty1 ProjectedProperty1 { get; set; }

        public TProperty2 ProjectedProperty2 { get; set; }

        public TProperty3 ProjectedProperty3 { get; set; }

        public TProperty4 ProjectedProperty4 { get; set; }

        public TProperty5 ProjectedProperty5 { get; set; }

        public TProperty6 ProjectedProperty6 { get; set; }

        public TProperty7 ProjectedProperty7 { get; set; }

        public TProperty8 ProjectedProperty8 { get; set; }

        public TProperty9 ProjectedProperty9 { get; set; }
    }
}

