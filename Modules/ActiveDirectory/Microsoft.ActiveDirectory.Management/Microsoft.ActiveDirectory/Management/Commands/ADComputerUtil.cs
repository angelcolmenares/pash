using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADComputerUtil
	{
		private const string _samAccountNameEnding = "$";

		private const string _debugCategory = "ComputerUtil";

		public const int _samAccountMaxLength = 15;

		internal static List<IADOPathNode> BuildComputerSamAccountNameIdentityFilter(string identity, List<IADOPathNode> baseList)
		{
			if (!identity.EndsWith("$", StringComparison.OrdinalIgnoreCase))
			{
				IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "sAMAccountName", string.Concat(identity, "$"));
				baseList.Add(aDOPathNode);
			}
			return baseList;
		}

		internal static IdentityResolverDelegate GetGenericIdentityResolverWithSamName(string[] identityLdapAttributes)
		{
			return (object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter) => {
				useSearchFilter = true;
				ADObjectSearcher aDObjectSearcher = IdentityResolverMethods.BuildGenericSearcher(identityLdapAttributes, identityObject, searchBase, cmdletSessionInfo, out useSearchFilter);
				if (aDObjectSearcher != null)
				{
					string str = identityObject as string;
					if (str != null && !str.EndsWith("$", StringComparison.OrdinalIgnoreCase))
					{
						IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "sAMAccountName", string.Concat(str, "$"));
						IADOPathNode[] filter = new IADOPathNode[2];
						filter[0] = aDObjectSearcher.Filter;
						filter[1] = aDOPathNode;
						aDObjectSearcher.Filter = ADOPathUtil.CreateOrClause(filter);
					}
				}
				return aDObjectSearcher;
			}
			;
		}

		internal static void ToDirectoryComputerSamAccountName(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				AttributeConverters.ToDirectoryObject(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
				return;
			}
			else
			{
				string value = extendedData.Value as string;
				if (!string.IsNullOrEmpty(value) && !value.EndsWith("$", StringComparison.OrdinalIgnoreCase))
				{
					value = string.Concat(value, "$");
				}
				AttributeConverters.ToDirectoryObject(extendedAttribute, directoryAttributes, new ADPropertyValueCollection(value), directoryObj, cmdletSessionInfo);
				return;
			}
		}

		internal static void ToDirectoryServiceAccountSamAccountName(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			int num;
			if (extendedData == null || extendedData.Value == null)
			{
				AttributeConverters.ToDirectoryObject(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
				return;
			}
			else
			{
				string value = extendedData.Value as string;
				if (!string.IsNullOrEmpty(value))
				{
					bool flag = value.EndsWith("$", StringComparison.OrdinalIgnoreCase);
					int length = value.Length;
					if (flag)
					{
						num = 16;
					}
					else
					{
						num = 15;
					}
					if (length <= num)
					{
						if (!flag)
						{
							value = string.Concat(value, "$");
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = value;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ServiceAccountNameLengthInvalid, objArray));
					}
				}
				AttributeConverters.ToDirectoryObject(extendedAttribute, directoryAttributes, new ADPropertyValueCollection(value), directoryObj, cmdletSessionInfo);
				return;
			}
		}

		internal static IADOPathNode ToSearchComputerSamAccountName(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
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
						bool flag = dataObject.EndsWith("$", StringComparison.OrdinalIgnoreCase);
						bool flag1 = dataObject.EndsWith("*", StringComparison.OrdinalIgnoreCase);
						ADOperator @operator = binaryADOPathNode.Operator;
						if ((@operator == ADOperator.Eq || @operator == ADOperator.Ne) && !flag)
						{
							object[] objArray = new object[3];
							objArray[0] = extendedAttribute;
							objArray[1] = "$";
							objArray[2] = filterClause;
							cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.WarningSamAccountNameClauseLacksDollarSign, objArray));
						}
						else
						{
							if ((@operator == ADOperator.Like || @operator == ADOperator.NotLike) && !flag && !flag1)
							{
								object[] objArray1 = new object[3];
								objArray1[0] = extendedAttribute;
								objArray1[1] = "$";
								objArray1[2] = filterClause;
								cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.WarningSamAccountNameClauseLacksDollarSign, objArray1));
							}
						}
						return SearchConverters.ToSearchObject(extendedAttribute, directoryAttributes, filterClause, cmdletSessionInfo);
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
	}
}