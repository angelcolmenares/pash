using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADPropertyValueCollection : CollectionBase
	{
		private const string _debugCategory = "ADPropertyValueCollection";

		private ArrayList _addedValues;

		private ArrayList _deletedValues;

		private ArrayList _replacedValues;

		private bool _isValuesCleared;

		private bool _trackChanges;

		private bool _doValidation;

		internal ArrayList AddedValues
		{
			get
			{
				if (this._addedValues == null)
				{
					this._addedValues = new ArrayList();
				}
				return this._addedValues;
			}
		}

		internal ArrayList DeletedValues
		{
			get
			{
				if (this._deletedValues == null)
				{
					this._deletedValues = new ArrayList();
				}
				return this._deletedValues;
			}
		}

		internal bool DoValidation
		{
			get
			{
				return this._doValidation;
			}
			set
			{
				this._doValidation = value;
			}
		}

		internal bool IsValuesCleared
		{
			get
			{
				return this._isValuesCleared;
			}
		}

		public object this[int index]
		{
			get
			{
				return base.List[index];
			}
			set
			{
				base.List[index] = value;
			}
		}

		internal ArrayList ReplacedValues
		{
			get
			{
				if (this._replacedValues == null)
				{
					this._replacedValues = new ArrayList();
				}
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
					if (this._addedValues != null)
					{
						this.AddedValues.Clear();
					}
					if (this._deletedValues != null)
					{
						this.DeletedValues.Clear();
					}
					if (this._replacedValues != null)
					{
						this.ReplacedValues.Clear();
					}
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
						Array arrays = Array.CreateInstance(base.List[0].GetType(), base.List.Count);
						base.List.CopyTo(arrays, 0);
						return arrays;
					}
					else
					{
						return base.List[0];
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

		public object ValueList
		{
			get
			{
				if (base.List.Count != 0)
				{
					Array arrays = Array.CreateInstance(base.List[0].GetType(), base.List.Count);
					base.List.CopyTo(arrays, 0);
					return arrays;
				}
				else
				{
					return null;
				}
			}
		}

		public ADPropertyValueCollection()
		{
			this._doValidation = true;
		}

		public ADPropertyValueCollection(int capacity) : base(capacity)
		{
			this._doValidation = true;
		}

		public ADPropertyValueCollection(ADPropertyValueCollection collection)
		{
			this._doValidation = true;
			if (collection != null)
			{
				base.Capacity = collection.Capacity;
				this.BoxValue(collection);
			}
		}

		public ADPropertyValueCollection(object value)
		{
			this._doValidation = true;
			this.BoxValue(value);
		}

		public int Add(object value)
		{
			return base.List.Add(value);
		}

		public void AddRange(object[] value)
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
				DebugLogger.LogWarning("ADPropertyValueCollection", "AddRange(object[]): null value");
				throw new ArgumentNullException("value");
			}
		}

		public void AddRange(ADPropertyValueCollection value)
		{
			if (value != null)
			{
				base.InnerList.AddRange(value);
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADPropertyValueCollection", "AddRange(ADPropertyValueCollection): null value");
				throw new ArgumentNullException("value");
			}
		}

		protected internal void BoxValue(object value)
		{
			if (value != null)
			{
				if (value as ADPropertyValueCollection == null)
				{
					if (value as Array == null)
					{
						base.List.Add(value);
						return;
					}
					else
					{
						if (value as byte[] == null)
						{
							if (value as object[] == null)
							{
								object[] objArray = new object[((Array)value).Length];
								((Array)value).CopyTo(objArray, 0);
								this.AddRange(objArray);
								return;
							}
							else
							{
								this.AddRange((object[])value);
								return;
							}
						}
						else
						{
							base.List.Add(value);
							return;
						}
					}
				}
				else
				{
					this.AddRange((ADPropertyValueCollection)value);
					return;
				}
			}
			else
			{
				return;
			}
		}

		public bool Contains(object value)
		{
			return base.List.Contains(value);
		}

		public void CopyTo(object[] array, int index)
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

		public int IndexOf(object value)
		{
			return base.List.IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			base.List.Insert(index, value);
		}

		internal bool IsChanged()
		{
			if (!this._trackChanges || (this._addedValues == null || this.AddedValues.Count <= 0) && (this._deletedValues == null || this.DeletedValues.Count <= 0) && (this._replacedValues == null || this.ReplacedValues.Count <= 0) && !this._isValuesCleared)
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
				this.AddedValues.Clear();
				this.DeletedValues.Clear();
				this.ReplacedValues.Clear();
				this._isValuesCleared = true;
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (!this._doValidation || value == null || base.List.Count <= 0 || !(base.List[0].GetType() != value.GetType()) || base.List[0].GetType().IsAssignableFrom(value.GetType()) || value.GetType().IsAssignableFrom(base.List[0].GetType()))
			{
				base.OnInsert(index, value);
				return;
			}
			else
			{
				object[] type = new object[2];
				type[0] = value.GetType();
				type[1] = base.List[0].GetType();
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.NoMixedType, type), "value");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			if (this._trackChanges)
			{
				if (!this._isValuesCleared)
				{
					if (this.ReplacedValues.Count <= 0)
					{
						if (!this.DeletedValues.Contains(value))
						{
							this.AddedValues.Add(value);
						}
						else
						{
							this.DeletedValues.Remove(value);
							return;
						}
					}
					else
					{
						this.ReplacedValues.Add(value);
						return;
					}
				}
				else
				{
					this.ReplacedValues.Add(value);
					this._isValuesCleared = false;
					return;
				}
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (this._trackChanges)
			{
				if (this.ReplacedValues.Count <= 0)
				{
					if (!this._isValuesCleared)
					{
						if (!this.AddedValues.Contains(value))
						{
							this.DeletedValues.Add(value);
						}
						else
						{
							this.AddedValues.Remove(value);
							return;
						}
					}
				}
				else
				{
					this.ReplacedValues.Remove(value);
					return;
				}
			}
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			if (this._trackChanges)
			{
				if (!this._isValuesCleared)
				{
					if (this.ReplacedValues.Count <= 0)
					{
						this.DeletedValues.Add(oldValue);
						this.AddedValues.Remove(oldValue);
						this.AddedValues.Add(newValue);
						this.DeletedValues.Remove(newValue);
					}
					else
					{
						this.ReplacedValues.Remove(oldValue);
						this.ReplacedValues.Add(newValue);
						return;
					}
				}
				else
				{
					this.ReplacedValues.Add(newValue);
					this._isValuesCleared = false;
					return;
				}
			}
		}

		public void Remove(object value)
		{
			this.ForceRemove(value);
		}
	}
}