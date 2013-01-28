using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class ParameterBinder
	{
		private Dictionary<string, HashSet<ParameterDefinitionEntry>> parameterDefinitionEntries;

		private Dictionary<string, ParameterSetEntry> parameterSetEntries;

		private List<string> parametersetNamesList;

		private List<string> parameterNamesList;

		private List<string> parametersetNamesListAtBeginProcess;

		private List<string> parameterNamesListAtBeginProcess;

		internal ParameterBinder(Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters, Dictionary<string, ParameterSetEntry> sets)
		{
			this.parametersetNamesList = new List<string>();
			this.parameterNamesList = new List<string>();
			this.parametersetNamesListAtBeginProcess = new List<string>();
			this.parameterNamesListAtBeginProcess = new List<string>();
			this.CloneParameterEntries(parameters, sets);
		}

		private void CloneParameterEntries(Dictionary<string, HashSet<ParameterDefinitionEntry>> parameters, Dictionary<string, ParameterSetEntry> sets)
		{
			this.parameterDefinitionEntries = parameters;
			this.parameterSetEntries = new Dictionary<string, ParameterSetEntry>();
			foreach (KeyValuePair<string, ParameterSetEntry> set in sets)
			{
				this.parameterSetEntries.Add(set.Key, new ParameterSetEntry(set.Value));
			}
		}

		internal string GetParameterSet()
		{
			DebugHelper.WriteLogEx();
			string item = null;
			string str = null;
			List<string> strs = new List<string>();
			foreach (string key in this.parameterSetEntries.Keys)
			{
				ParameterSetEntry parameterSetEntry = this.parameterSetEntries[key];
				object[] setMandatoryParameterCount = new object[3];
				setMandatoryParameterCount[0] = key;
				setMandatoryParameterCount[1] = parameterSetEntry.SetMandatoryParameterCount;
				setMandatoryParameterCount[2] = parameterSetEntry.MandatoryParameterCount;
				DebugHelper.WriteLogEx("parameterset name = {0}, {1}/{2} mandatory parameters.", 1, setMandatoryParameterCount);
				if (parameterSetEntry.MandatoryParameterCount != 0)
				{
					if (parameterSetEntry.SetMandatoryParameterCount != parameterSetEntry.MandatoryParameterCount || !this.parametersetNamesList.Contains(key))
					{
						continue;
					}
					if (item == null)
					{
						item = key;
					}
					else
					{
						throw new PSArgumentException(Strings.UnableToResolvePareameterSetName);
					}
				}
				else
				{
					if (parameterSetEntry.IsDefaultParameterSet)
					{
						str = key;
					}
					if (!parameterSetEntry.IsValueSet)
					{
						continue;
					}
					strs.Add(key);
				}
			}
			if (item == null)
			{
				if (strs.Count <= 1)
				{
					if (strs.Count == 1)
					{
						item = strs[0];
					}
				}
				else
				{
					throw new PSArgumentException(Strings.UnableToResolvePareameterSetName);
				}
			}
			if (item == null)
			{
				item = str;
			}
			if (item != null)
			{
				return item;
			}
			else
			{
				throw new PSArgumentException(Strings.UnableToResolvePareameterSetName);
			}
		}

		internal void reset()
		{
			foreach (KeyValuePair<string, ParameterSetEntry> parameterSetEntry in this.parameterSetEntries)
			{
				parameterSetEntry.Value.reset();
			}
			this.parametersetNamesList.Clear();
			foreach (string str in this.parametersetNamesListAtBeginProcess)
			{
				this.parametersetNamesList.Add(str);
			}
			this.parameterNamesList.Clear();
			foreach (string str1 in this.parameterNamesListAtBeginProcess)
			{
				this.parameterNamesList.Add(str1);
			}
		}

		internal void SetParameter(string parameterName, bool isBeginProcess)
		{
			object[] objArray = new object[2];
			objArray[0] = parameterName;
			objArray[1] = isBeginProcess;
			DebugHelper.WriteLogEx("ParameterName = {0}, isBeginProcess = {1}", 0, objArray);
			if (!this.parameterNamesList.Contains(parameterName))
			{
				this.parameterNamesList.Add(parameterName);
				if (isBeginProcess)
				{
					this.parameterNamesListAtBeginProcess.Add(parameterName);
				}
				if (this.parametersetNamesList.Count != 0)
				{
					List<string> strs = new List<string>();
					foreach (ParameterDefinitionEntry item in this.parameterDefinitionEntries[parameterName])
					{
						if (!this.parametersetNamesList.Contains(item.ParameterSetName))
						{
							continue;
						}
						strs.Add(item.ParameterSetName);
						if (!item.IsMandatory)
						{
							continue;
						}
						ParameterSetEntry parameterSetEntry = this.parameterSetEntries[item.ParameterSetName];
						ParameterSetEntry setMandatoryParameterCount = parameterSetEntry;
						setMandatoryParameterCount.SetMandatoryParameterCount = setMandatoryParameterCount.SetMandatoryParameterCount + 1;
						if (isBeginProcess)
						{
							ParameterSetEntry setMandatoryParameterCountAtBeginProcess = parameterSetEntry;
							setMandatoryParameterCountAtBeginProcess.SetMandatoryParameterCountAtBeginProcess = setMandatoryParameterCountAtBeginProcess.SetMandatoryParameterCountAtBeginProcess + 1;
						}
						object[] parameterSetName = new object[2];
						parameterSetName[0] = item.ParameterSetName;
						parameterSetName[1] = parameterSetEntry.SetMandatoryParameterCount;
						DebugHelper.WriteLogEx("parameterset name = '{0}'; SetMandatoryParameterCount = '{1}'", 1, parameterSetName);
					}
					if (strs.Count != 0)
					{
						this.parametersetNamesList = strs;
						if (isBeginProcess)
						{
							this.parametersetNamesListAtBeginProcess = strs;
						}
					}
					else
					{
						throw new PSArgumentException(Strings.UnableToResolvePareameterSetName);
					}
				}
				else
				{
					List<string> strs1 = new List<string>();
					foreach (ParameterDefinitionEntry parameterDefinitionEntry in this.parameterDefinitionEntries[parameterName])
					{
						object[] isMandatory = new object[2];
						isMandatory[0] = parameterDefinitionEntry.ParameterSetName;
						isMandatory[1] = parameterDefinitionEntry.IsMandatory;
						DebugHelper.WriteLogEx("parameterset name = '{0}'; mandatory = '{1}'", 1, isMandatory);
						ParameterSetEntry item1 = this.parameterSetEntries[parameterDefinitionEntry.ParameterSetName];
						if (item1 == null)
						{
							continue;
						}
						if (parameterDefinitionEntry.IsMandatory)
						{
							ParameterSetEntry setMandatoryParameterCount1 = item1;
							setMandatoryParameterCount1.SetMandatoryParameterCount = setMandatoryParameterCount1.SetMandatoryParameterCount + 1;
							if (isBeginProcess)
							{
								ParameterSetEntry setMandatoryParameterCountAtBeginProcess1 = item1;
								setMandatoryParameterCountAtBeginProcess1.SetMandatoryParameterCountAtBeginProcess = setMandatoryParameterCountAtBeginProcess1.SetMandatoryParameterCountAtBeginProcess + 1;
							}
							object[] parameterSetName1 = new object[2];
							parameterSetName1[0] = parameterDefinitionEntry.ParameterSetName;
							parameterSetName1[1] = item1.SetMandatoryParameterCount;
							DebugHelper.WriteLogEx("parameterset name = '{0}'; SetMandatoryParameterCount = '{1}'", 1, parameterSetName1);
						}
						if (!item1.IsValueSet)
						{
							item1.IsValueSet = true;
							if (isBeginProcess)
							{
								item1.IsValueSetAtBeginProcess = true;
							}
						}
						strs1.Add(parameterDefinitionEntry.ParameterSetName);
					}
					this.parametersetNamesList = strs1;
					if (isBeginProcess)
					{
						this.parametersetNamesListAtBeginProcess = strs1;
						return;
					}
				}
				return;
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = parameterName;
				DebugHelper.WriteLogEx("ParameterName {0} is already bound ", 1, objArray1);
				return;
			}
		}
	}
}