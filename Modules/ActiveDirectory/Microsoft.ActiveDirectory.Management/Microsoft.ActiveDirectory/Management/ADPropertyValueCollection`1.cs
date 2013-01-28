using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADPropertyValueCollection<T> : CollectionBase
	{
		private const string _debugCategory = "ADPropertyValueCollection<T>";

		private List<T> _addedValues;

		private List<T> _deletedValues;

		private List<T> _replacedValues;

		private bool _isValuesCleared;

		private bool _trackChanges;

		internal List<T> AddedValues
		{
			get
			{
				return this._addedValues;
			}
		}

		internal List<T> DeletedValues
		{
			get
			{
				return this._deletedValues;
			}
		}

		internal bool IsValuesCleared
		{
			get
			{
				return this._isValuesCleared;
			}
		}

		public T this[int index]
		{
			get
			{
				return (T)base.List[index];
			}
			set
			{
				base.List[index] = value;
			}
		}

		internal List<T> ReplacedValues
		{
			get
			{
				return this._replacedValues;
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
					this._addedValues.Clear();
					this._deletedValues.Clear();
					this._replacedValues.Clear();
					this._isValuesCleared = false;
					this._trackChanges = value;
				}
			}
		}

		public object Value
		{
			get
			{
				if (base.List.Count != 0)
				{
					if (base.List.Count != 1)
					{
						T[] tArray = new T[base.List.Count];
						base.List.CopyTo(tArray, 0);
						return tArray;
					}
					else
					{
						return (T)base.List[0];
					}
				}
				else
				{
					return null;
				}
			}
			set
			{
				base.List.Clear();
				this.BoxValue(value);
			}
		}

		public ADPropertyValueCollection()
		{
			this._addedValues = new List<T>();
			this._deletedValues = new List<T>();
			this._replacedValues = new List<T>();
		}

		public ADPropertyValueCollection(T value)
		{
			this._addedValues = new List<T>();
			this._deletedValues = new List<T>();
			this._replacedValues = new List<T>();
			this.Add(value);
		}

		public ADPropertyValueCollection(T[] values)
		{
			this._addedValues = new List<T>();
			this._deletedValues = new List<T>();
			this._replacedValues = new List<T>();
			this.AddRange(values);
		}

		public int Add(T value)
		{
			return base.List.Add(value);
		}

		public void AddRange(T[] value)
		{
			if (value != null)
			{
				for (int i = 0; i < (int)value.Length; i++)
				{
					base.List.Add(value[i]);
				}
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADPropertyValueCollection<T>", "AddRange(T[]): null value");
				throw new ArgumentNullException("value");
			}
		}

		public void AddRange(ADPropertyValueCollection<T> value)
		{
			if (value != null)
			{
				int count = value.Count;
				for (int i = 0; i < count; i++)
				{
					base.List.Add(value[i]);
				}
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADPropertyValueCollection<T>", "AddRange(ADPropertyValueCollection): null value");
				throw new ArgumentNullException("value");
			}
		}

		protected internal void BoxValue(object value)
		{
			if (value != null)
			{
				if (value as ADPropertyValueCollection<T> == null)
				{
					if (!(value is T))
					{
						if (!(value is T[]))
						{
							DebugLogger.LogWarning("ADPropertyValueCollection<T>", string.Concat("BoxValue: invalid value type ", value.GetType().ToString()));
							object[] type = new object[1];
							type[0] = value.GetType();
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, type), "value");
						}
						else
						{
							this.AddRange((T[])value);
							return;
						}
					}
					else
					{
						base.List.Add(value);
						return;
					}
				}
				else
				{
					this.AddRange((ADPropertyValueCollection<T>)value);
					return;
				}
			}
			else
			{
				return;
			}
		}

		public bool Contains(T value)
		{
			return base.List.Contains(value);
		}

		public void CopyTo(T[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		internal void ForceRemove(object value)
		{
			if (!base.List.Contains(value))
			{
				if (this._trackChanges)
				{
					this.OnRemoveComplete(0, value);
				}
				return;
			}
			else
			{
				base.List.Remove(value);
				return;
			}
		}

		public int IndexOf(T value)
		{
			return base.List.IndexOf(value);
		}

		public void Insert(int index, T value)
		{
			base.List.Insert(index, value);
		}

		internal bool IsChanged()
		{
			if (!this._trackChanges || this._addedValues.Count <= 0 && this._deletedValues.Count <= 0 && this._replacedValues.Count <= 0 && !this._isValuesCleared)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		protected override void OnClearComplete()
		{
			if (this._trackChanges)
			{
				this._addedValues.Clear();
				this._deletedValues.Clear();
				this._replacedValues.Clear();
				this._isValuesCleared = true;
			}
		}

		protected void OnInsertComplete(int index, T value)
		{
			if (this._trackChanges)
			{
				if (!this._isValuesCleared)
				{
					if (this._replacedValues.Count <= 0)
					{
						if (!this._deletedValues.Contains(value))
						{
							this._addedValues.Add(value);
						}
						else
						{
							this._deletedValues.Remove(value);
							return;
						}
					}
					else
					{
						this._replacedValues.Add(value);
						return;
					}
				}
				else
				{
					this._replacedValues.Add(value);
					this._isValuesCleared = false;
					return;
				}
			}
		}

		protected void OnRemoveComplete(int index, T value)
		{
			if (this._trackChanges)
			{
				if (this._replacedValues.Count <= 0)
				{
					if (!this._isValuesCleared)
					{
						if (!this._addedValues.Contains(value))
						{
							this._deletedValues.Add(value);
						}
						else
						{
							this._addedValues.Remove(value);
							return;
						}
					}
				}
				else
				{
					this._replacedValues.Remove(value);
					return;
				}
			}
		}

		protected void OnSetComplete(int index, T oldValue, T newValue)
		{
			if (this._trackChanges)
			{
				if (!this._isValuesCleared)
				{
					if (this._replacedValues.Count <= 0)
					{
						this._deletedValues.Add(oldValue);
						this._addedValues.Remove(oldValue);
						this._addedValues.Add(newValue);
						this._deletedValues.Remove(newValue);
					}
					else
					{
						this._replacedValues.Remove(oldValue);
						this._replacedValues.Add(newValue);
						return;
					}
				}
				else
				{
					this._replacedValues.Add(newValue);
					this._isValuesCleared = false;
					return;
				}
			}
		}

		public void Remove(T value)
		{
			this.ForceRemove(value);
		}
	}
}