namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public sealed class PropertySetData
    {
        private string _name;
        private Collection<string> _referencedProperties;

        public PropertySetData(IEnumerable<string> referencedProperties)
        {
            if (referencedProperties == null)
            {
                throw PSTraceSource.NewArgumentNullException("referencedProperties");
            }
            this._referencedProperties = new Collection<string>();
            foreach (string str in referencedProperties)
            {
                this._referencedProperties.Add(str);
            }
        }

        internal PropertySetData Copy()
        {
            return new PropertySetData(this.ReferencedProperties) { Name = this.Name };
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public Collection<string> ReferencedProperties
        {
            get
            {
                return this._referencedProperties;
            }
        }
    }
}

