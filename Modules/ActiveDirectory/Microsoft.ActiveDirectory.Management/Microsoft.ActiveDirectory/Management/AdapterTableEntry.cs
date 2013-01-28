using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.ActiveDirectory.Management
{
	internal class AdapterTableEntry
	{
		private const string _debugCategory = "ADEntityAdapter";

		private IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> _attributeTable;

		private Dictionary<string, PropertyInfo> _properties;

		private HashSet<string> _methodNames;

		internal AdapterTableEntry(Type type, IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> attributeTable)
		{
			this._methodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._properties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
			if (attributeTable != null)
			{
				this._attributeTable = new Dictionary<ADServerType, MappingTable<AttributeConverterEntry>>();
				foreach (KeyValuePair<ADServerType, MappingTable<AttributeConverterEntry>> key in _attributeTable)
				{
					MappingTable<AttributeConverterEntry> mappingTable = new MappingTable<AttributeConverterEntry>();
					Dictionary<string, AttributeConverterEntry>.KeyCollection.Enumerator enumerator = key.Value.Keys.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string str = enumerator.Current;
							mappingTable.Add(str, key.Value[str]);
						}
					}
					finally
					{
						enumerator.Dispose();
					}
					this._attributeTable.Add(key.Key, mappingTable);
				}
			}
			else
			{
				this._attributeTable = null;
			}
			PropertyInfo[] properties = type.GetProperties();
			for (int i = 0; i < (int)properties.Length; i++)
			{
				PropertyInfo propertyInfo = properties[i];
				this._properties.Add(propertyInfo.Name, propertyInfo);
			}
			MethodInfo[] methods = type.GetMethods();
			for (int j = 0; j < (int)methods.Length; j++)
			{
				MethodInfo methodInfo = methods[j];
				this._methodNames.Add(methodInfo.Name);
			}
		}

		internal AttributeConverterEntry GetConverterEntry(string propertyName, ADServerType serverType)
		{
			if (this._attributeTable == null || !this._attributeTable.ContainsKey(serverType) || !this._attributeTable[serverType].ContainsKey(propertyName))
			{
				return null;
			}
			else
			{
				return this._attributeTable[serverType][propertyName];
			}
		}

		internal PropertyInfo GetProperty(string propertyName)
		{
			if (!this._properties.ContainsKey(propertyName))
			{
				return null;
			}
			else
			{
				return this._properties[propertyName];
			}
		}

		internal bool HasMethod(string methodName)
		{
			return this._methodNames.Contains(methodName);
		}

		internal void MergeAdapterTableEntry(IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> attributeTable)
		{
			foreach (ADServerType key in attributeTable.Keys)
			{
				if (!this._attributeTable.ContainsKey(key))
				{
					this._attributeTable[key] = attributeTable[key];
				}
				else
				{
					foreach (string str in attributeTable[key].Keys)
					{
						if (this._attributeTable[key].ContainsKey(str))
						{
							continue;
						}
						this._attributeTable[key].Add(str, attributeTable[key][str]);
					}
				}
			}
		}
	}
}