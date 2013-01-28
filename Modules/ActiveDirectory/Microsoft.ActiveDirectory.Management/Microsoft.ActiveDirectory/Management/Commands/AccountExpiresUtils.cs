using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class AccountExpiresUtils
	{
		public AccountExpiresUtils()
		{
		}

		public static void ToDirectoryAccountExpirationDate(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData.Count != 0 && extendedData[0] != null)
			{
				DateTime item = (DateTime)extendedData[0];
				if (item.Ticks != 0x89f7ff5f7b58000L)
				{
					AttributeConverters.ToDirectoryDateTime(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
					return;
				}
			}
			directoryObj.SetValue(directoryAttributes[0], 0x7fffffffffffffffL);
		}

		public static void ToExtendedAccountExpirationDate(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (directoryObj[directoryAttributes[0]].Value != null)
			{
				long value = (long)directoryObj[directoryAttributes[0]].Value;
				if (value == (long)0 || value == 0x7fffffffffffffffL)
				{
					userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
					return;
				}
				else
				{
					AttributeConverters.ToExtendedDateTimeFromLong(extendedAttribute, directoryAttributes, userObj, directoryObj, cmdletSessionInfo);
					return;
				}
			}
			else
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
				return;
			}
		}

		public static IADOPathNode ToSearchAccountExpirationDate(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode == null)
				{
					throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
				}
				else
				{
					ObjectADOPathNode objectADOPathNode = new ObjectADOPathNode(rightNode.DataObject);
					objectADOPathNode.EncodeAsteriskChar = rightNode.EncodeAsteriskChar;
					IADOPathNode searchDateTimeUsingSchemaInfo = new BinaryADOPathNode(binaryADOPathNode.Operator, propertyADOPathNode, objectADOPathNode);
					searchDateTimeUsingSchemaInfo = SearchConverters.ToSearchDateTimeUsingSchemaInfo(extendedAttribute, directoryAttributes, searchDateTimeUsingSchemaInfo, cmdletSessionInfo);
					bool flag1 = true;
					if (binaryADOPathNode.Operator != ADOperator.Eq)
					{
						if (binaryADOPathNode.Operator == ADOperator.Like)
						{
							if (rightNode.DataObject as string == null)
							{
								flag = false;
							}
							else
							{
								flag = ADOPathUtil.IsValueAllAsterisk((string)rightNode.DataObject);
							}
							flag1 = flag;
						}
					}
					else
					{
						flag1 = false;
					}
					if (!flag1)
					{
						return searchDateTimeUsingSchemaInfo;
					}
					else
					{
						IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
						aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, directoryAttributes[0], 0);
						aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, directoryAttributes[0], 0x7fffffffffffffffL);
						IADOPathNode aDOPathNode = ADOPathUtil.CreateNotClause(ADOPathUtil.CreateOrClause(aDOPathNodeArray));
						IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
						aDOPathNodeArray1[0] = searchDateTimeUsingSchemaInfo;
						aDOPathNodeArray1[1] = aDOPathNode;
						return ADOPathUtil.CreateAndClause(aDOPathNodeArray1);
					}
				}
			}
		}
	}
}