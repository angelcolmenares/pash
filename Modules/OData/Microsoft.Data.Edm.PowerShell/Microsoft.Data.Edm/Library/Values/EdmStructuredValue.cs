using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmStructuredValue : EdmValue, IEdmStructuredValue, IEdmValue, IEdmElement
	{
		private readonly IEnumerable<IEdmPropertyValue> propertyValues;

		private readonly Cache<EdmStructuredValue, Dictionary<string, IEdmPropertyValue>> propertiesDictionaryCache;

		private readonly static Func<EdmStructuredValue, Dictionary<string, IEdmPropertyValue>> ComputePropertiesDictionaryFunc;

		private Dictionary<string, IEdmPropertyValue> PropertiesDictionary
		{
			get
			{
				if (this.propertiesDictionaryCache == null)
				{
					return null;
				}
				else
				{
					return this.propertiesDictionaryCache.GetValue(this, EdmStructuredValue.ComputePropertiesDictionaryFunc, null);
				}
			}
		}

		public IEnumerable<IEdmPropertyValue> PropertyValues
		{
			get
			{
				return this.propertyValues;
			}
		}

		public override EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.Structured;
			}
		}

		static EdmStructuredValue()
		{
			EdmStructuredValue.ComputePropertiesDictionaryFunc = (EdmStructuredValue me) => me.ComputePropertiesDictionary();
		}

		public EdmStructuredValue(IEdmStructuredTypeReference type, IEnumerable<IEdmPropertyValue> propertyValues) : base(type)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmPropertyValue>>(propertyValues, "propertyValues");
			this.propertyValues = propertyValues;
			if (propertyValues != null)
			{
				int num = 0;
				foreach (IEdmPropertyValue propertyValue in propertyValues)
				{
					num++;
					if (num <= 5)
					{
						continue;
					}
					this.propertiesDictionaryCache = new Cache<EdmStructuredValue, Dictionary<string, IEdmPropertyValue>>();
					break;
				}
			}
		}

		private Dictionary<string, IEdmPropertyValue> ComputePropertiesDictionary()
		{
			Dictionary<string, IEdmPropertyValue> strs = new Dictionary<string, IEdmPropertyValue>();
			foreach (IEdmPropertyValue propertyValue in this.propertyValues)
			{
				strs[propertyValue.Name] = propertyValue;
			}
			return strs;
		}

		public IEdmPropertyValue FindPropertyValue(string propertyName)
		{
			IEdmPropertyValue edmPropertyValue = null;
			IEdmPropertyValue edmPropertyValue1;
			Dictionary<string, IEdmPropertyValue> propertiesDictionary = this.PropertiesDictionary;
			if (propertiesDictionary == null)
			{
				IEnumerator<IEdmPropertyValue> enumerator = this.propertyValues.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						IEdmPropertyValue current = enumerator.Current;
						if (current.Name != propertyName)
						{
							continue;
						}
						edmPropertyValue1 = current;
						return edmPropertyValue1;
					}
					return null;
				}
				return edmPropertyValue1;
			}
			else
			{
				propertiesDictionary.TryGetValue(propertyName, out edmPropertyValue);
				return edmPropertyValue;
			}
		}
	}
}