namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;

    public sealed class ListControlEntry
    {
        private List<string> _entrySelectedBy;
        private List<ListControlEntryItem> _items;

        public ListControlEntry()
        {
            this._items = new List<ListControlEntryItem>();
            this._entrySelectedBy = new List<string>();
        }

        public ListControlEntry(IEnumerable<ListControlEntryItem> listItems)
        {
            this._items = new List<ListControlEntryItem>();
            this._entrySelectedBy = new List<string>();
            if (listItems == null)
            {
                throw PSTraceSource.NewArgumentNullException("listItems");
            }
            foreach (ListControlEntryItem item in listItems)
            {
                this._items.Add(item);
            }
        }

        internal ListControlEntry(ListControlEntryDefinition entrydefn)
        {
            this._items = new List<ListControlEntryItem>();
            this._entrySelectedBy = new List<string>();
            if (entrydefn.appliesTo != null)
            {
                foreach (TypeOrGroupReference reference in entrydefn.appliesTo.referenceList)
                {
                    this._entrySelectedBy.Add(reference.name);
                }
            }
            foreach (ListControlItemDefinition definition in entrydefn.itemDefinitionList)
            {
                this._items.Add(new ListControlEntryItem(definition));
            }
        }

        public ListControlEntry(IEnumerable<ListControlEntryItem> listItems, IEnumerable<string> selectedBy)
        {
            this._items = new List<ListControlEntryItem>();
            this._entrySelectedBy = new List<string>();
            if (listItems == null)
            {
                throw PSTraceSource.NewArgumentNullException("listItems");
            }
            if (selectedBy == null)
            {
                throw PSTraceSource.NewArgumentNullException("selectedBy");
            }
            foreach (string str in selectedBy)
            {
                this._entrySelectedBy.Add(str);
            }
            foreach (ListControlEntryItem item in listItems)
            {
                this._items.Add(item);
            }
        }

        public List<ListControlEntryItem> Items
        {
            get
            {
                return this._items;
            }
            internal set
            {
                this._items = value;
            }
        }

        public List<string> SelectedBy
        {
            get
            {
                return this._entrySelectedBy;
            }
            internal set
            {
                this._entrySelectedBy = value;
            }
        }
    }
}

