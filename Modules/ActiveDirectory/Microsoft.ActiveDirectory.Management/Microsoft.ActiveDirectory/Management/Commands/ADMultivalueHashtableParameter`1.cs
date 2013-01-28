using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADMultivalueHashtableParameter<T> : IADCustomParameter
	{
		private Hashtable _hash;

		internal ADMultivalueHashtableParameter(Hashtable hash)
		{
			this._hash = hash;
		}

		public ADPropertyValueCollection ConvertToADPropertyValueCollection(string parameterName)
		{
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
			aDPropertyValueCollection.TrackChanges = true;
			string str = null;
			string str1 = null;
			string str2 = null;
			if (this._hash != null)
			{
				foreach (object key in this._hash.Keys)
				{
					if (string.Compare((string)key, "Add", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (string.Compare((string)key, "Remove", StringComparison.OrdinalIgnoreCase) != 0)
						{
							if (string.Compare((string)key, "Replace", StringComparison.OrdinalIgnoreCase) != 0)
							{
								continue;
							}
							str2 = (string)key;
						}
						else
						{
							str1 = (string)key;
						}
					}
					else
					{
						str = (string)key;
					}
				}
				if (str2 != null)
				{
					if (this._hash[str2] != null)
					{
						aDPropertyValueCollection.Clear();
						this.processMultivalueElement(PropertyModifyOp.Replace, aDPropertyValueCollection, this._hash[str2], parameterName);
					}
					else
					{
						aDPropertyValueCollection.Value = null;
					}
				}
				else
				{
					if (str != null)
					{
						this.processMultivalueElement(PropertyModifyOp.Add, aDPropertyValueCollection, this._hash[str], parameterName);
					}
					if (str1 != null)
					{
						this.processMultivalueElement(PropertyModifyOp.Remove, aDPropertyValueCollection, this._hash[str1], parameterName);
					}
				}
			}
			else
			{
				aDPropertyValueCollection.Value = null;
			}
			return aDPropertyValueCollection;
		}

		public object GetOriginalValue()
		{
			return this._hash;
		}

		private void processMultivalueElement(PropertyModifyOp operation, ADPropertyValueCollection collection, object value, string parameterName)
		{
			object baseObject;
			if (value as object[] == null)
			{
				if (value.GetType() == typeof(PSObject))
				{
					value = ((PSObject)value).BaseObject;
				}
				if (operation != PropertyModifyOp.Add)
				{
					if (operation != PropertyModifyOp.Remove)
					{
						if (operation != PropertyModifyOp.Replace)
						{
							throw new ArgumentException();
						}
						else
						{
							collection.Add(value);
							return;
						}
					}
					else
					{
						collection.ForceRemove(value);
						return;
					}
				}
				else
				{
					collection.Add(value);
					return;
				}
			}
			else
			{
				object[] objArray = (object[])value;
				for (int i = 0; i < (int)objArray.Length; i++)
				{
					object obj = objArray[i];
					if (obj.GetType() != typeof(PSObject))
					{
						baseObject = obj;
					}
					else
					{
						baseObject = ((PSObject)obj).BaseObject;
					}
					if (operation != PropertyModifyOp.Add)
					{
						if (operation != PropertyModifyOp.Remove)
						{
							if (operation != PropertyModifyOp.Replace)
							{
								throw new ArgumentException();
							}
							else
							{
								collection.Add(baseObject);
							}
						}
						else
						{
							collection.ForceRemove(baseObject);
						}
					}
					else
					{
						collection.Add(baseObject);
					}
				}
				return;
			}
		}
	}
}