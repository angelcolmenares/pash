using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class CimCustomOptionsDictionary
	{
		private readonly IDictionary<string, object> _dict;

		private readonly object _dictModificationLock;

		private readonly static ConditionalWeakTable<CimInstance, CimCustomOptionsDictionary> CimInstanceToCustomOptions;

		static CimCustomOptionsDictionary()
		{
			CimCustomOptionsDictionary.CimInstanceToCustomOptions = new ConditionalWeakTable<CimInstance, CimCustomOptionsDictionary>();
		}

		private CimCustomOptionsDictionary(IEnumerable<KeyValuePair<string, object>> wrappedDictionary)
		{
			this._dictModificationLock = new object();
			this._dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, object> value in wrappedDictionary)
			{
				this._dict[value.Key] = value.Value;
			}
		}

		internal void Apply(CimOperationOptions cimOperationOptions)
		{
			CimOperationOptionsHelper.SetCustomOptions(cimOperationOptions, this.GetSnapshot());
		}

		internal static void AssociateCimInstanceWithCustomOptions(CimInstance cimInstance, CimCustomOptionsDictionary newCustomOptions)
		{
            bool flag = false;
			if (newCustomOptions != null)
			{
				lock (newCustomOptions._dictModificationLock)
				{
					if (newCustomOptions._dict.Count == 0)
					{
						return;
					}
				}
				CimCustomOptionsDictionary value = CimCustomOptionsDictionary.CimInstanceToCustomOptions.GetValue(cimInstance, (CimInstance param0) => {
					flag = false;
					return newCustomOptions;
				}
				);
				if (flag)
				{
					lock (value._dictModificationLock)
					{
						foreach (KeyValuePair<string, object> snapshot in newCustomOptions.GetSnapshot())
						{
							value._dict[snapshot.Key] = snapshot.Value;
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal static CimCustomOptionsDictionary Create(IEnumerable<KeyValuePair<string, object>> wrappedDictionary)
		{
			return new CimCustomOptionsDictionary(wrappedDictionary);
		}

		private IEnumerable<KeyValuePair<string, object>> GetSnapshot()
		{
			IEnumerable<KeyValuePair<string, object>> list;
			lock (this._dictModificationLock)
			{
				list = this._dict.ToList<KeyValuePair<string, object>>();
			}
			return list;
		}

		internal static CimCustomOptionsDictionary MergeOptions(CimCustomOptionsDictionary optionsFromCommandLine, CimInstance instanceRelatedToThisOperation)
		{
			CimCustomOptionsDictionary cimCustomOptionsDictionary = null;
			if (!CimCustomOptionsDictionary.CimInstanceToCustomOptions.TryGetValue(instanceRelatedToThisOperation, out cimCustomOptionsDictionary) || cimCustomOptionsDictionary == null)
			{
				return optionsFromCommandLine;
			}
			else
			{
				IEnumerable<KeyValuePair<string, object>> snapshot = cimCustomOptionsDictionary.GetSnapshot();
				IEnumerable<KeyValuePair<string, object>> keyValuePairs = optionsFromCommandLine.GetSnapshot();
				IEnumerable<KeyValuePair<string, object>> keyValuePairs1 = snapshot.Concat<KeyValuePair<string, object>>(keyValuePairs);
				return new CimCustomOptionsDictionary(keyValuePairs1);
			}
		}

		internal static CimCustomOptionsDictionary MergeOptions(CimCustomOptionsDictionary optionsFromCommandLine, IEnumerable<CimInstance> instancesRelatedToThisOperation)
		{
			CimCustomOptionsDictionary cimCustomOptionsDictionary = optionsFromCommandLine;
			if (instancesRelatedToThisOperation != null)
			{
				foreach (CimInstance cimInstance in instancesRelatedToThisOperation)
				{
					cimCustomOptionsDictionary = CimCustomOptionsDictionary.MergeOptions(cimCustomOptionsDictionary, cimInstance);
				}
			}
			return cimCustomOptionsDictionary;
		}
	}
}