using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class GPLinkUtil
	{
		private const string _debugCategory = "GPLinkUtil";

		private readonly static string GpLinkRegExDNGroup;

		private readonly static Regex GpLinkRegEx;

		private readonly static char[] GpLinkSplitChars;

		private readonly static string GpLinkFilterPrefix;

		private readonly static string GpLinkFilterSuffix;

		static GPLinkUtil()
		{
			GPLinkUtil.GpLinkRegExDNGroup = "DNGroup";
			GPLinkUtil.GpLinkRegEx = new Regex(string.Concat("LDAP://(?'", GPLinkUtil.GpLinkRegExDNGroup, "'[^;]+);[^]]*\\]$"), RegexOptions.Compiled | RegexOptions.Singleline);
			char[] chrArray = new char[1];
			chrArray[0] = '[';
			GPLinkUtil.GpLinkSplitChars = chrArray;
			GPLinkUtil.GpLinkFilterPrefix = "*[LDAP://";
			GPLinkUtil.GpLinkFilterSuffix = ";*]*";
		}

		private static IADOPathNode BuildGPLinkFilter(string extendedAttribute, string directoryAttribute, IADOPathNode filterClause)
		{
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					string dataObject = rightNode.DataObject as string;
					if (dataObject != null)
					{
						ADOperator @operator = binaryADOPathNode.Operator;
						if (@operator == ADOperator.Eq || @operator == ADOperator.Ne || @operator == ADOperator.Like || @operator == ADOperator.NotLike)
						{
							if (@operator != ADOperator.Eq)
							{
								if (@operator == ADOperator.Ne)
								{
									dataObject = ADOPathUtil.LdapSearchEncodeString(dataObject, true);
									@operator = ADOperator.NotLike;
								}
							}
							else
							{
								dataObject = ADOPathUtil.LdapSearchEncodeString(dataObject, true);
								@operator = ADOperator.Like;
							}
							dataObject = string.Concat(GPLinkUtil.GpLinkFilterPrefix, dataObject, GPLinkUtil.GpLinkFilterSuffix);
							return ADOPathUtil.CreateFilterClause(@operator, directoryAttribute, dataObject);
						}
						else
						{
							object[] str = new object[2];
							ADOperator[] aDOperatorArray = new ADOperator[4];
							aDOperatorArray[1] = ADOperator.Ne;
							aDOperatorArray[2] = ADOperator.Like;
							aDOperatorArray[3] = ADOperator.NotLike;
							str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
							str[1] = extendedAttribute;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
						}
					}
					else
					{
						object[] type = new object[2];
						type[0] = rightNode.DataObject.GetType();
						type[1] = extendedAttribute;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterRHSInvalidType, type));
					}
				}
				else
				{
					throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
				}
			}
			else
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
		}

		private static ADPropertyValueCollection ConvertLinkedGroupPolicyObjects(string rawGPLink)
		{
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
			if (rawGPLink != null && rawGPLink.Length > 0)
			{
				string[] strArrays = rawGPLink.Split(GPLinkUtil.GpLinkSplitChars, StringSplitOptions.RemoveEmptyEntries);
				if (strArrays != null && (int)strArrays.Length > 0)
				{
					string[] strArrays1 = strArrays;
					for (int i = 0; i < (int)strArrays1.Length; i++)
					{
						string str = strArrays1[i];
						Match match = GPLinkUtil.GpLinkRegEx.Match(str);
						if (match.Success)
						{
							aDPropertyValueCollection.Add(match.Groups[GPLinkUtil.GpLinkRegExDNGroup].Value);
						}
					}
				}
			}
			return aDPropertyValueCollection;
		}

		internal static void ToExtendedGPLink(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				string value = directoryObj[directoryAttributes[0]].Value as string;
				userObj.Add(extendedAttribute, GPLinkUtil.ConvertLinkedGroupPolicyObjects(value));
				return;
			}
		}

		internal static IADOPathNode ToSearchGPLink(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return GPLinkUtil.BuildGPLinkFilter(extendedAttribute, directoryAttributes[0], filterClause);
		}
	}
}