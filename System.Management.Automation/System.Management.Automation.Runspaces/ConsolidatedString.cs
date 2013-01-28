namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal class ConsolidatedString : Collection<string>
    {
        internal static readonly ConsolidatedString Empty = new ConsolidatedString(new string[0]);

        public ConsolidatedString(IEnumerable<string> strings) : base(strings.ToList<string>())
        {
            foreach (string str in this)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw PSTraceSource.NewArgumentException("strings");
                }
            }
            this.UpdateKey();
        }

        public ConsolidatedString(ConsolidatedString other) : base(new List<string>(other))
        {
            this.Key = other.Key;
        }

        internal ConsolidatedString(IEnumerable<string> strings, bool interned) : base(interned ? ((IList<string>) new ReadOnlyCollection<string>(strings.ToList<string>())) : ((IList<string>) strings.ToList<string>()))
        {
            this.UpdateKey();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            this.UpdateKey();
        }

        protected override void InsertItem(int index, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                throw PSTraceSource.NewArgumentException("item");
            }
            base.InsertItem(index, item);
            this.UpdateKey();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this.UpdateKey();
        }

        protected override void SetItem(int index, string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                throw PSTraceSource.NewArgumentException("item");
            }
            base.SetItem(index, item);
            this.UpdateKey();
        }

        private void UpdateKey()
        {
            this.Key = string.Join("@@@", this);
        }

        internal bool IsReadOnly
        {
            get
            {
                return true; //TODO: Review: this.IsReadOnly;
            }
        }

        internal string Key { get; private set; }
    }
}

