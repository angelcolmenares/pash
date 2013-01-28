using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADPropertyCollection
	{
		private const string _debugCategory = "ADPropertyCollection";

		private SortedDictionary<string, ADPropertyValueCollection> _dictionary;

		private ICollection<string> _addedProperties;

		private ICollection<string> _removedProperties;

		private bool _trackChanges;

		public ICollection<string> AddedProperties
		{
			get
			{
				if (this._addedProperties == null)
				{
					this._addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				}
				return this._addedProperties;
			}
		}

		protected SortedDictionary<string, ADPropertyValueCollection> InnerDictionary
		{
			get
			{
				if (this._dictionary == null)
				{
					this._dictionary = new SortedDictionary<string, ADPropertyValueCollection>(StringComparer.OrdinalIgnoreCase);
				}
				return this._dictionary;
			}
		}

		public ADPropertyValueCollection this[string propertyName]
		{
			get
			{
				ADPropertyValueCollection aDPropertyValueCollection = null;
				bool flag = this.InnerDictionary.TryGetValue(propertyName, out aDPropertyValueCollection);
				this.OnGet(propertyName, aDPropertyValueCollection);
				if (!flag || aDPropertyValueCollection == null)
				{
					aDPropertyValueCollection = new ADPropertyValueCollection();
					aDPropertyValueCollection.TrackChanges = true;
					if (!this._trackChanges)
					{
						this.Add(propertyName, aDPropertyValueCollection);
					}
					else
					{
						this._trackChanges = false;
						this.Add(propertyName, aDPropertyValueCollection);
						this._trackChanges = true;
					}
				}
				return aDPropertyValueCollection;
			}
		}

		public ICollection<string> ModifiedProperties
		{
			get
			{
				HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string propertyName in this.PropertyNames)
				{
					if (this.AddedProperties.Contains(propertyName) || this.RemovedProperties.Contains(propertyName) || !this.InnerDictionary[propertyName].IsChanged())
					{
						continue;
					}
					strs.Add(propertyName);
				}
				return strs;
			}
		}

		internal int NonNullCount
		{
			get
			{
				int num = 0;
				foreach (object value in this.Values)
				{
					if (value == null)
					{
						continue;
					}
					ADPropertyValueCollection aDPropertyValueCollection = (ADPropertyValueCollection)value;
					if (aDPropertyValueCollection.Count <= 0)
					{
						continue;
					}
					num++;
				}
				return num;
			}
		}

		public int PropertyCount
		{
			get
			{
				if (this._dictionary == null)
				{
					return 0;
				}
				else
				{
					return this._dictionary.Count;
				}
			}
		}

		public ICollection PropertyNames
		{
			get
			{
				return this.InnerDictionary.Keys;
			}
		}

		public ICollection<string> RemovedProperties
		{
			get
			{
				if (this._removedProperties == null)
				{
					this._removedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				}
				return this._removedProperties;
			}
		}

		internal bool TrackChanges
		{
			get
			{
				return this._trackChanges;
			}
			set
			{
				if (this._trackChanges != value)
				{
					if (this._addedProperties != null)
					{
						this.AddedProperties.Clear();
					}
					if (this._removedProperties != null)
					{
						this.RemovedProperties.Clear();
					}
					foreach (ADPropertyValueCollection aDPropertyValueCollection in this.Values)
					{
						aDPropertyValueCollection.TrackChanges = value;
					}
					this._trackChanges = value;
				}
			}
		}

		internal ICollection Values
		{
			get
			{
				return this.InnerDictionary.Values;
			}
		}

		internal ADPropertyCollection()
		{
		}

		internal void Add(string propertyName, object propertyValue)
		{
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(propertyValue);
			this.DictionaryAdd(propertyName, aDPropertyValueCollection);
		}

		internal void Add(string propertyName, ADPropertyValueCollection propertyValue)
		{
			if (propertyValue != null)
			{
				this.DictionaryAdd(propertyName, propertyValue);
				return;
			}
			else
			{
				this.Add(propertyName, null);
				return;
			}
		}

		internal void Clear()
		{
			this.OnClear();
			this.InnerDictionary.Clear();
			this.OnClearComplete();
		}

		public bool Contains(string propertyName)
		{
			return this.InnerDictionary.ContainsKey(propertyName);
		}

		private void DictionaryAdd(string key, ADPropertyValueCollection value)
		{
			this.OnValidate(key, value);
			this.OnInsert(key, value);
			this.InnerDictionary.Add(key, value);
			try
			{
				this.OnInsertComplete(key, value);
			}
			catch
			{
				this.InnerDictionary.Remove(key);
				DebugLogger.LogWarning("ADPropertyCollection", string.Concat("Add: OnInsertComplete failed for key ", key, ". Value was ", value.ToString()));
				throw;
			}
		}

		private void DictionaryRemove(string key)
		{
			ADPropertyValueCollection aDPropertyValueCollection = null;
			if (this.InnerDictionary.TryGetValue(key, out aDPropertyValueCollection))
			{
				this.OnValidate(key, aDPropertyValueCollection);
				this.OnRemove(key, aDPropertyValueCollection);
				this.InnerDictionary.Remove(key);
				try
				{
					this.OnRemoveComplete(key, aDPropertyValueCollection);
				}
				catch
				{
					this.InnerDictionary.Add(key, aDPropertyValueCollection);
					DebugLogger.LogWarning("ADPropertyCollection", string.Concat("Remove: OnRemoveComplete failed for key ", key));
					throw;
				}
			}
		}

		internal void ForceRemove(string propertyName)
		{
			if (!this.Contains(propertyName))
			{
				if (this._trackChanges)
				{
					this.OnRemoveComplete(propertyName, null);
				}
				return;
			}
			else
			{
				this.DictionaryRemove(propertyName);
				return;
			}
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return this.InnerDictionary.GetEnumerator();
		}

		internal object GetValue(string propertyName)
		{
			ADPropertyValueCollection aDPropertyValueCollection = null;
			if (!this.InnerDictionary.TryGetValue(propertyName, out aDPropertyValueCollection) || aDPropertyValueCollection == null)
			{
				return null;
			}
			else
			{
				return aDPropertyValueCollection.Value;
			}
		}

		protected virtual void OnClear()
		{
		}

		protected virtual void OnClearComplete()
		{
		}

		protected virtual object OnGet(object key, object currentValue)
		{
			return currentValue;
		}

		protected virtual void OnInsert(object key, object value)
		{
		}

		protected virtual void OnInsertComplete(object key, object value)
		{
			if (this._trackChanges)
			{
				this.AddedProperties.Add((string)key);
				this.RemovedProperties.Remove((string)key);
			}
		}

		protected virtual void OnRemove(object key, object value)
		{
		}

		protected virtual void OnRemoveComplete(object key, object value)
		{
			if (this._trackChanges)
			{
				this.RemovedProperties.Add((string)key);
				this.AddedProperties.Remove((string)key);
			}
		}

		protected virtual void OnSet(object key, object oldValue, object newValue)
		{
			DebugLogger.LogWarning("ADPropertyCollection", "OnSet: not supported");
			throw new NotSupportedException();
		}

		protected virtual void OnSetComplete(object key, object oldValue, object newValue)
		{
		}

		protected virtual void OnValidate(object key, object value)
		{
		}

		internal void Remove(string propertyName)
		{
			this.DictionaryRemove(propertyName);
		}

		internal void SetValue(string propertyName, object propertyValue)
		{
			ADPropertyValueCollection aDPropertyValueCollection = null;
			bool flag = this.InnerDictionary.TryGetValue(propertyName, out aDPropertyValueCollection);
			if (!flag || aDPropertyValueCollection == null)
			{
				this.Add(propertyName, propertyValue);
				return;
			}
			else
			{
				aDPropertyValueCollection.Value = propertyValue;
				return;
			}
		}

		internal bool TryGetValue(string propertyName, out ADPropertyValueCollection value)
		{
			return this.InnerDictionary.TryGetValue(propertyName, out value);
		}
	}
}