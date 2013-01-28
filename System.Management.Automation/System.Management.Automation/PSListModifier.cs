namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class PSListModifier
    {
        private Collection<object> _itemsToAdd;
        private Collection<object> _itemsToRemove;
        private Collection<object> _replacementItems;
        internal const string AddKey = "Add";
        internal const string RemoveKey = "Remove";
        internal const string ReplaceKey = "Replace";

        public PSListModifier()
        {
            this._itemsToAdd = new Collection<object>();
            this._itemsToRemove = new Collection<object>();
            this._replacementItems = new Collection<object>();
        }

        public PSListModifier(Hashtable hash)
        {
            if (hash == null)
            {
                throw PSTraceSource.NewArgumentNullException("hash");
            }
            this._itemsToAdd = new Collection<object>();
            this._itemsToRemove = new Collection<object>();
            this._replacementItems = new Collection<object>();
            foreach (DictionaryEntry entry in hash)
            {
                Collection<object> collection;
                if (!(entry.Key is string))
                {
                    throw PSTraceSource.NewArgumentException("hash", "PSListModifierStrings", "ListModifierDisallowedKey", new object[] { entry.Key });
                }
                string key = entry.Key as string;
                bool flag = key.Equals("Add", StringComparison.OrdinalIgnoreCase);
                bool flag2 = key.Equals("Remove", StringComparison.OrdinalIgnoreCase);
                bool flag3 = key.Equals("Replace", StringComparison.OrdinalIgnoreCase);
                if ((!flag && !flag2) && !flag3)
                {
                    throw PSTraceSource.NewArgumentException("hash", "PSListModifierStrings", "ListModifierDisallowedKey", new object[] { key });
                }
                if (flag2)
                {
                    collection = this._itemsToRemove;
                }
                else if (flag)
                {
                    collection = this._itemsToAdd;
                }
                else
                {
                    collection = this._replacementItems;
                }
                IEnumerable enumerable = LanguagePrimitives.GetEnumerable(entry.Value);
                if (enumerable != null)
                {
                    foreach (object obj2 in enumerable)
                    {
                        collection.Add(obj2);
                    }
                }
                else
                {
                    collection.Add(entry.Value);
                }
            }
        }

        public PSListModifier(object replacementItems)
        {
            this._itemsToAdd = new Collection<object>();
            this._itemsToRemove = new Collection<object>();
            if (replacementItems == null)
            {
                this._replacementItems = new Collection<object>();
            }
            else if (replacementItems is Collection<object>)
            {
                this._replacementItems = (Collection<object>) replacementItems;
            }
            else if (replacementItems is IList<object>)
            {
                this._replacementItems = new Collection<object>((IList<object>) replacementItems);
            }
            else if (replacementItems is IList)
            {
                this._replacementItems = new Collection<object>();
                foreach (object obj2 in (IList) replacementItems)
                {
                    this._replacementItems.Add(obj2);
                }
            }
            else
            {
                this._replacementItems = new Collection<object>();
                this._replacementItems.Add(replacementItems);
            }
        }

        public PSListModifier(Collection<object> removeItems, Collection<object> addItems)
        {
            this._itemsToAdd = (addItems != null) ? addItems : new Collection<object>();
            this._itemsToRemove = (removeItems != null) ? removeItems : new Collection<object>();
            this._replacementItems = new Collection<object>();
        }

        public void ApplyTo(IList collectionToUpdate)
        {
            if (collectionToUpdate == null)
            {
                throw PSTraceSource.NewArgumentNullException("collectionToUpdate");
            }
            if (this._replacementItems.Count > 0)
            {
                collectionToUpdate.Clear();
                foreach (object obj2 in this._replacementItems)
                {
                    collectionToUpdate.Add(PSObject.Base(obj2));
                }
            }
            else
            {
                foreach (object obj3 in this._itemsToRemove)
                {
                    collectionToUpdate.Remove(PSObject.Base(obj3));
                }
                foreach (object obj4 in this._itemsToAdd)
                {
                    collectionToUpdate.Add(PSObject.Base(obj4));
                }
            }
        }

        public void ApplyTo(object collectionToUpdate)
        {
            if (collectionToUpdate == null)
            {
                throw new ArgumentNullException("collectionToUpdate");
            }
            collectionToUpdate = PSObject.Base(collectionToUpdate);
            IList list = collectionToUpdate as IList;
            if (list == null)
            {
                throw PSTraceSource.NewInvalidOperationException("PSListModifierStrings", "UpdateFailed", new object[0]);
            }
            this.ApplyTo(list);
        }

        internal Hashtable ToHashtable()
        {
            Hashtable hashtable = new Hashtable(2);
            if (this._itemsToAdd.Count > 0)
            {
                hashtable.Add("Add", this._itemsToAdd);
            }
            if (this._itemsToRemove.Count > 0)
            {
                hashtable.Add("Remove", this._itemsToRemove);
            }
            if (this._replacementItems.Count > 0)
            {
                hashtable.Add("Replace", this._replacementItems);
            }
            return hashtable;
        }

        public Collection<object> Add
        {
            get
            {
                return this._itemsToAdd;
            }
        }

        public Collection<object> Remove
        {
            get
            {
                return this._itemsToRemove;
            }
        }

        public Collection<object> Replace
        {
            get
            {
                return this._replacementItems;
            }
        }
    }
}

