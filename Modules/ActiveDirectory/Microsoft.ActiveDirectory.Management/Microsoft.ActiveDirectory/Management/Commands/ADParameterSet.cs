using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADParameterSet
	{
		private Dictionary<string, object> _dictionary;

		internal object this[string key]
		{
			get
			{
				if (this._dictionary.ContainsKey(key))
				{
					IADCustomParameter item = this._dictionary[key] as IADCustomParameter;
					if (item == null)
					{
						return this._dictionary[key];
					}
					else
					{
						return item.GetOriginalValue();
					}
				}
				else
				{
					return null;
				}
			}
			set
			{
				this._dictionary[key] = value;
			}
		}

		internal ADParameterSet()
		{
			this._dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		internal bool Contains(string parameterName)
		{
			return this._dictionary.ContainsKey(parameterName);
		}

		internal bool ContainsKey(string parameterName)
		{
			return this._dictionary.ContainsKey(parameterName);
		}

		internal IDictionary<string, ADPropertyValueCollection> GetADPVCDictionary()
		{
			Dictionary<string, ADPropertyValueCollection> strs = new Dictionary<string, ADPropertyValueCollection>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, object> keyValuePair in this._dictionary)
			{
				IADCustomParameter value = keyValuePair.Value as IADCustomParameter;
				if (value == null)
				{
					strs.Add(keyValuePair.Key, new ADPropertyValueCollection(keyValuePair.Value));
				}
				else
				{
					strs.Add(keyValuePair.Key, value.ConvertToADPropertyValueCollection(keyValuePair.Key));
				}
			}
			return strs;
		}

		internal SwitchParameter GetSwitchParameter(string parameterName)
		{
			if (!this._dictionary.ContainsKey(parameterName))
			{
				return new SwitchParameter(false);
			}
			else
			{
				return (SwitchParameter)this._dictionary[parameterName];
			}
		}

		internal bool GetSwitchParameterBooleanValue(string parameterName)
		{
			SwitchParameter switchParameter = this.GetSwitchParameter(parameterName);
			return switchParameter.ToBool();
		}

		internal bool RemoveParameter(string parameterName)
		{
			return this._dictionary.Remove(parameterName);
		}
	}
}