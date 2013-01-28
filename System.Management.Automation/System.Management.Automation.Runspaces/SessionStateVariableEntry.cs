namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public sealed class SessionStateVariableEntry : ConstrainedSessionStateEntry
    {
        private Collection<Attribute> _attributes;
        private string _description;
        private ScopedItemOptions _options;
        private object _value;

        public SessionStateVariableEntry(string name, object value, string description) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._value = value;
            this._description = description;
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._value = value;
            this._description = description;
            this._options = options;
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Attribute attribute) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._value = value;
            this._description = description;
            this._options = options;
            this._attributes = new Collection<Attribute>();
            this._attributes.Add(attribute);
        }

        public SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Collection<Attribute> attributes) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._value = value;
            this._description = description;
            this._options = options;
            this._attributes = attributes;
        }

        internal SessionStateVariableEntry(string name, object value, string description, ScopedItemOptions options, Collection<Attribute> attributes, SessionStateEntryVisibility visibility) : base(name, visibility)
        {
            this._description = string.Empty;
            this._value = value;
            this._description = description;
            this._options = options;
            this._attributes = new Collection<Attribute>();
            this._attributes = attributes;
        }

        public override InitialSessionStateEntry Clone()
        {
            Collection<Attribute> attributes = null;
            if ((this._attributes != null) && (this._attributes.Count > 0))
            {
                attributes = new Collection<Attribute>(this._attributes);
            }
            return new SessionStateVariableEntry(base.Name, this._value, this._description, this._options, attributes, base.Visibility);
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                if (this._attributes == null)
                {
                    this._attributes = new Collection<Attribute>();
                }
                return this._attributes;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                return this._options;
            }
        }

        public object Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

