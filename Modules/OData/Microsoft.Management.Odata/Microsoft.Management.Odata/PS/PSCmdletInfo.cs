using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.Management.Odata.PS
{
	internal class PSCmdletInfo
	{
		public string CmdletName
		{
			get;
			private set;
		}

		public Dictionary<string, string> FieldParameterMapping
		{
			get;
			private set;
		}

		public Dictionary<string, string> ImmutableParameters
		{
			get;
			private set;
		}

		public HashSet<string> Options
		{
			get;
			private set;
		}

		public List<PSParameterSet> ParameterSets
		{
			get;
			private set;
		}

		public PSCmdletInfo(string cmdletName)
		{
			cmdletName.ThrowIfNullOrEmpty("cmdletName", Resources.EmptyCmdletName, new object[0]);
			this.CmdletName = cmdletName;
			this.ParameterSets = new List<PSParameterSet>();
			this.FieldParameterMapping = new Dictionary<string, string>();
			this.Options = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this.ImmutableParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public PSCmdletInfo(PSCmdletInfo other)
		{
			this.CmdletName = other.CmdletName;
			this.ParameterSets = new List<PSParameterSet>(other.ParameterSets);
			this.FieldParameterMapping = new Dictionary<string, string>(other.FieldParameterMapping);
			this.Options = new HashSet<string>(other.Options, StringComparer.OrdinalIgnoreCase);
			this.ImmutableParameters = new Dictionary<string, string>(other.ImmutableParameters, StringComparer.OrdinalIgnoreCase);
		}

		internal PSParameterInfo FindParameterInfo(string parameter)
		{
			IEnumerable<PSParameterSet> pSParameterSets = this.ParameterSets.FindAll((PSParameterSet item) => item.Parameters.ContainsKey(parameter));
			object[] cmdletName = new object[2];
			cmdletName[0] = parameter;
			cmdletName[1] = this.CmdletName;
			ExceptionHelpers.ThrowArgumentExceptionIf("parameter", pSParameterSets.Count<PSParameterSet>() == 0, Resources.ParameterNotFoundInCommand, cmdletName);
			PSParameterInfo pSParameterInfo = null;
			foreach (PSParameterSet pSParameterSet in pSParameterSets)
			{
				PSParameterInfo pSParameterInfo1 = pSParameterSet.Parameters[parameter];
				if (pSParameterInfo != null)
				{
					if (!(pSParameterInfo.Type != pSParameterInfo1.Type) && pSParameterInfo.IsSwitch == pSParameterInfo1.IsSwitch)
					{
						continue;
					}
					TraceHelper.Current.DebugMessage(string.Concat("Parameter ", parameter, " is found in multiple parameter set and has either different Type or IsSwitch value. So FindParametersInfo returns null"));
					pSParameterInfo = null;
					break;
				}
				else
				{
					pSParameterInfo = pSParameterInfo1;
				}
			}
			return pSParameterInfo;
		}

		internal ulong FindParameterSets(string parameter)
		{
			long count = ~((long)-1 << (this.ParameterSets.Count & 63));
			ulong num = 0;
			for (int i = 0; i < this.ParameterSets.Count; i++)
			{
				if (this.ParameterSets[i].Parameters.ContainsKey(parameter))
				{
					if (!string.Equals(this.ParameterSets[i].Name, "__AllParameterSets", StringComparison.OrdinalIgnoreCase))
					{
						num = num + ((ulong)1 << (i & 63));
					}
					else
					{
						return (ulong)count;
					}
				}
			}
			return num;
		}

		private PSParameterSet GetAllParameterSet()
		{
			List<PSParameterSet> parameterSets = this.ParameterSets;
			return parameterSets.FirstOrDefault<PSParameterSet>((PSParameterSet it) => string.Equals(it.Name, "__AllParameterSets", StringComparison.OrdinalIgnoreCase));
		}

		private List<int> GetParameterSetsFromBitmask(ulong parameterSetsBitmask)
		{
			List<int> nums = new List<int>();
			int num = 0;
			while (parameterSetsBitmask != (long)0)
			{
				if ((parameterSetsBitmask & (long)1) == (long)1)
				{
					nums.Add(num);
				}
				num++;
				parameterSetsBitmask = parameterSetsBitmask >> 1;
			}
			return nums;
		}

		internal string GetParameterType(string parameter)
		{
			return this.GetParameterType(parameter, this.FindParameterSets(parameter));
		}

		internal string GetParameterType(string parameter, ulong parameterSetsBitmask)
		{
			if (this.IsAllParameterSet(parameterSetsBitmask))
			{
				PSParameterSet allParameterSet = this.GetAllParameterSet();
				if (allParameterSet != null)
				{
					PSParameterInfo pSParameterInfo = null;
					if (allParameterSet.Parameters.TryGetValue(parameter, out pSParameterInfo))
					{
						return pSParameterInfo.Type;
					}
				}
			}
			HashSet<string> parameterTypes = this.GetParameterTypes(parameter, parameterSetsBitmask);
			if (parameterTypes.Count != 1)
			{
				return null;
			}
			else
			{
				return parameterTypes.ElementAt<string>(0);
			}
		}

		private HashSet<string> GetParameterTypes(string parameter, ulong parameterSetsBitmask)
		{
			HashSet<string> strs = new HashSet<string>();
			foreach (int parameterSetsFromBitmask in this.GetParameterSetsFromBitmask(parameterSetsBitmask))
			{
				strs.Add(this.ParameterSets[parameterSetsFromBitmask].Parameters[parameter].Type);
			}
			return strs;
		}

		private bool IsAllParameterSet(ulong parameterSetMask)
		{
			int num = 0;
			num = 0;
			while (parameterSetMask != (long)0)
			{
				num++;
				parameterSetMask = parameterSetMask >> 1;
			}
			return num == this.ParameterSets.Count;
		}

		internal bool IsMandatory(string parameterName)
		{
			PSParameterInfo pSParameterInfo = this.FindParameterInfo(parameterName);
			if (pSParameterInfo == null)
			{
				return false;
			}
			else
			{
				return pSParameterInfo.IsMandatory;
			}
		}

		internal bool IsSwitch(string parameterName)
		{
			PSParameterInfo pSParameterInfo = this.FindParameterInfo(parameterName);
			if (pSParameterInfo == null)
			{
				return false;
			}
			else
			{
				return pSParameterInfo.IsSwitch;
			}
		}

		internal bool IsValidOption(string parameterName)
		{
			return this.Options.Contains(parameterName);
		}

		public void ThrowIfInvalidState(ResourceType resourceType)
		{
			List<string> list = this.FieldParameterMapping.Keys.Where<string>((string item) => resourceType.Properties.FirstOrDefault<ResourceProperty>((ResourceProperty property) => {
				if (property.Name != item)
				{
					return false;
				}
				else
				{
					return property.Kind == ResourcePropertyKind.Collection;
				}
			}
			) != null).ToList<string>();
			IEnumerable<KeyValuePair<string, string>> keyValuePairs = this.FieldParameterMapping.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> keyValuePair) => list.Contains(keyValuePair.Key));
			List<string> strs = keyValuePairs.Select<KeyValuePair<string, string>, string>((KeyValuePair<string, string> item) => item.Value).ToList<string>();
			HashSet<string> strs1 = new HashSet<string>();
			strs.ForEach((string item) => strs1.Add(item));
			List<string> list1 = strs1.Where<string>((string item) => {
				if (this.GetParameterType(item) == null)
				{
					return true;
				}
				else
				{
					return Type.GetType(this.GetParameterType(item)) == null;
				}
			}
			).ToList<string>();
			if (TraceHelper.IsEnabled(5))
			{
				HashSet<string> strs2 = new HashSet<string>();
				this.ParameterSets.ForEach((PSParameterSet paramSet) => paramSet.Parameters.Keys.ToList<string>().Where<string>((string parameterName) => {
					if (paramSet.Parameters[parameterName].Type == null)
					{
						return false;
					}
					else
					{
						return !strs1.Contains(parameterName);
					}
				}
				).ToList<string>().ForEach((string item) => strs2.Add(item)));
				if (strs2.Count > 0)
				{
					string str = string.Concat("Cmdlet ", this.CmdletName, " has the following parameters which has a Type associated with them that is not used : ");
					strs2.ToList<string>().ForEach((string item) => {

						str = string.Concat(str, item, " ");
					}
					);
					TraceHelper.Current.DebugMessage(str);
				}
			}
			List<string> list2 = this.FieldParameterMapping.Values.ToList<string>().FindAll((string item) => this.FindParameterSets(item) == (long)0).ToList<string>();
			List<string> list3 = this.Options.ToList<string>().FindAll((string item) => this.FindParameterSets(item) == (long)0).ToList<string>();
			List<string> strs3 = this.ImmutableParameters.Keys.ToList<string>().FindAll((string item) => this.FindParameterSets(item) == (long)0).ToList<string>();
			if (list1.Count<string>() > 0 || list2.Count<string>() > 0 || list3.Count<string>() > 0 || strs3.Count<string>() > 0)
			{
				string empty = string.Empty;
				if (list1.Count<string>() > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					list1.ToList<string>().ForEach((string item) => stringBuilder.Append(string.Concat(item, " ")));
					stringBuilder.AppendLine();
					object[] objArray = new object[1];
					objArray[0] = stringBuilder.ToString();
					empty = string.Format(CultureInfo.CurrentCulture, Resources.InvalidParametersTypeForCollectionFields, objArray);
					TraceHelper.Current.DebugMessage(empty);
				}
				string empty1 = string.Empty;
				if (list2.Count<string>() > 0)
				{
					StringBuilder stringBuilder1 = new StringBuilder();
					list2.ForEach((string item) => stringBuilder1.Append(string.Concat(item, " ")));
					stringBuilder1.AppendLine();
					object[] str1 = new object[1];
					str1[0] = stringBuilder1.ToString();
					empty1 = string.Format(CultureInfo.CurrentCulture, Resources.FieldParamNotPresentInParameterSets, str1);
					TraceHelper.Current.DebugMessage(empty1);
				}
				string empty2 = string.Empty;
				if (list3.Count<string>() > 0)
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					list3.ForEach((string item) => stringBuilder2.Append(string.Concat(item, " ")));
					stringBuilder2.AppendLine();
					object[] objArray1 = new object[1];
					objArray1[0] = stringBuilder2.ToString();
					empty2 = string.Format(CultureInfo.CurrentCulture, Resources.OptionsNotPresentInParameterSets, objArray1);
					TraceHelper.Current.DebugMessage(empty2);
				}
				string str2 = string.Empty;
				if (strs3.Count<string>() > 0)
				{
					StringBuilder stringBuilder3 = new StringBuilder();
					strs3.ForEach((string item) => stringBuilder3.Append(string.Concat(item, " ")));
					stringBuilder3.AppendLine();
					object[] objArray2 = new object[1];
					objArray2[0] = stringBuilder3.ToString();
					str2 = string.Format(CultureInfo.CurrentCulture, Resources.ImmutableParametersNotPresentInParameterSets, objArray2);
					TraceHelper.Current.DebugMessage(str2);
				}
				throw new InvalidOperationException(string.Concat(empty, empty1, empty2, str2));
			}
			else
			{
				return;
			}
		}

		public StringBuilder ToTraceMessage(string message, StringBuilder builder)
		{
			builder.AppendLine(message);
			builder.AppendLine(string.Concat("CmdletName = ", this.CmdletName));
			builder.AppendLine("Field parameter mapping ");
			builder = this.FieldParameterMapping.ToTraceMessage(builder);
			builder.AppendLine(string.Concat("Options\nCount = ", this.Options.Count));
			this.Options.ToList<string>().ForEach((string item) => builder.AppendLine(item));
			builder.AppendLine("\nParameter sets ");
			this.ParameterSets.ForEach((PSParameterSet item) => builder = item.ToTraceMessage(builder));
			return builder;
		}

		public void VerifyMandatoryParameterAdded(IEnumerable<string> parameters, ulong parameterSetMask)
		{
			if (parameters.Count<string>() == 0)
			{
				parameterSetMask = (ulong)(~((long)-1 << (this.ParameterSets.Count & 63)));
			}
			if (parameterSetMask != 0)
			{
				List<int>.Enumerator enumerator = this.GetParameterSetsFromBitmask(parameterSetMask).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						int current = enumerator.Current;
						PSParameterSet item = this.ParameterSets[current];
						bool flag = false;
						Dictionary<string, PSParameterInfo>.KeyCollection.Enumerator enumerator1 = item.Parameters.Keys.GetEnumerator();
						try
						{
							while (enumerator1.MoveNext())
							{
								string str = enumerator1.Current;
								if (!item.Parameters[str].IsMandatory || parameters.Contains<string>(str))
								{
									continue;
								}
								flag = true;
								break;
							}
						}
						finally
						{
							enumerator1.Dispose();
						}
						if (flag)
						{
							continue;
						}
						return;
					}
					object[] cmdletName = new object[1];
					cmdletName[0] = this.CmdletName;
					throw new DataServiceException(0x190, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.BadRequest, Resources.MandatoryParameterMissing, cmdletName));
				}
				finally
				{
					enumerator.Dispose();
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}