namespace System.Data.Services.Internal
{
    using System;
    using System.ComponentModel;
    using System.Data.Services;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal abstract class ExpandedWrapper<TExpandedElement> : IExpandedResult
    {
        private string description;
        private TExpandedElement expandedElement;
        private string[] propertyNames;
        private string referenceDescription;
        private string[] referencePropertyNames;

        protected ExpandedWrapper()
        {
        }

        public object GetExpandedPropertyValue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this.propertyNames == null)
            {
                throw new InvalidOperationException(Strings.BasicExpandProvider_ExpandedPropertiesNotInitialized);
            }
            int nameIndex = -1;
            for (int i = 0; i < this.propertyNames.Length; i++)
            {
                if (this.propertyNames[i] == name)
                {
                    nameIndex = i;
                    break;
                }
            }
            bool flag = false;
            if (this.referencePropertyNames != null)
            {
                for (int j = 0; j < this.referencePropertyNames.Length; j++)
                {
                    if (this.referencePropertyNames[j] == name)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if ((nameIndex == -1) && (name == "$skiptoken"))
            {
                return null;
            }
            object resource = ProjectedWrapper.ProcessResultInstance(this.InternalGetExpandedPropertyValue(nameIndex));
            if (!flag)
            {
                return ProjectedWrapper.ProcessResultEnumeration(resource);
            }
            return resource;
        }

        protected abstract object InternalGetExpandedPropertyValue(int nameIndex);

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = WebUtil.CheckArgumentNull<string>(value, "value");
                this.propertyNames = WebUtil.StringToSimpleArray(this.description);
            }
        }

        public TExpandedElement ExpandedElement
        {
            get
            {
                return this.expandedElement;
            }
            set
            {
                this.expandedElement = value;
            }
        }

        public string ReferenceDescription
        {
            get
            {
                return this.referenceDescription;
            }
            set
            {
                this.referenceDescription = WebUtil.CheckArgumentNull<string>(value, "value");
                this.referencePropertyNames = WebUtil.StringToSimpleArray(this.referenceDescription);
            }
        }

        object IExpandedResult.ExpandedElement
        {
            get
            {
                return ProjectedWrapper.ProcessResultInstance(this.ExpandedElement);
            }
        }
    }
}

