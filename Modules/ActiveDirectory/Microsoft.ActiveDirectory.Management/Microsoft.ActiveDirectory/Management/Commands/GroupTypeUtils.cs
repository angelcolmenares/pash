using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class GroupTypeUtils
	{
		public GroupTypeUtils()
		{
		}

		public static int GetDirectoryGroupTypeValue(ADGroupScope groupScope)
		{
			ADGroupScope aDGroupScope = groupScope;
			switch (aDGroupScope)
			{
				case ADGroupScope.DomainLocal:
				{
					return 4;
				}
				case ADGroupScope.Global:
				{
					return 2;
				}
				case ADGroupScope.Universal:
				{
					return 8;
				}
			}
			return 0;
		}

		public static int GetDirectoryGroupTypeValue(ADGroupCategory groupCategory)
		{
			ADGroupCategory aDGroupCategory = groupCategory;
			if (aDGroupCategory != ADGroupCategory.Security)
			{
				return 0;
			}
			else
			{
				return -2147483648;
			}
		}

		public static void ToDirectoryGroupCategory(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADGroupCategory aDGroupCategory = ADGroupCategory.Distribution;
			if (extendedData != null)
			{
				if (extendedData.Value != null)
				{
					ADGroupCategory value = (ADGroupCategory)extendedData.Value;
					int directoryGroupTypeValue = GroupTypeUtils.GetDirectoryGroupTypeValue(value);
					if (!directoryObj.Contains(directoryAttributes[0]))
					{
						directoryObj.Add(directoryAttributes[0], directoryGroupTypeValue);
					}
					else
					{
						int num = (int)directoryObj[directoryAttributes[0]].Value;
						if (GroupTypeUtils.TryGetExtendedGroupCategoryValue(num, out aDGroupCategory))
						{
							int directoryGroupTypeValue1 = GroupTypeUtils.GetDirectoryGroupTypeValue(aDGroupCategory);
							num = num & ~directoryGroupTypeValue1;
						}
						directoryObj[directoryAttributes[0]].Value = num + directoryGroupTypeValue;
						return;
					}
				}
				else
				{
					return;
				}
			}
		}

		public static void ToDirectoryGroupScope(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADGroupScope aDGroupScope = ADGroupScope.DomainLocal;
			if (extendedData != null)
			{
				if (extendedData.Value != null)
				{
					ADGroupScope value = (ADGroupScope)extendedData.Value;
					int directoryGroupTypeValue = GroupTypeUtils.GetDirectoryGroupTypeValue(value);
					if (!directoryObj.Contains(directoryAttributes[0]))
					{
						directoryObj.Add(directoryAttributes[0], directoryGroupTypeValue);
					}
					else
					{
						int num = (int)directoryObj[directoryAttributes[0]].Value;
						if (GroupTypeUtils.TryGetExtendedGroupScopeValue(num, out aDGroupScope))
						{
							int directoryGroupTypeValue1 = GroupTypeUtils.GetDirectoryGroupTypeValue(aDGroupScope);
							num = num & ~directoryGroupTypeValue1;
						}
						directoryObj[directoryAttributes[0]].Value = num + directoryGroupTypeValue;
						return;
					}
				}
				else
				{
					return;
				}
			}
		}

		public static void ToExtendedGroupCategory(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
				return;
			}
			else
			{
				int value = (int)directoryObj[directoryAttributes[0]].Value;
				if ((value & -2147483648) != -2147483648)
				{
					userObj.Add(extendedAttribute, ADGroupCategory.Distribution);
					return;
				}
				else
				{
					userObj.Add(extendedAttribute, ADGroupCategory.Security);
					return;
				}
			}
		}

		public static void ToExtendedGroupScope(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
			}
			else
			{
				int value = (int)directoryObj[directoryAttributes[0]].Value;
				if ((value & 4) != 4)
				{
					if ((value & 2) != 2)
					{
						if ((value & 8) == 8)
						{
							userObj.Add(extendedAttribute, ADGroupScope.Universal);
							return;
						}
					}
					else
					{
						userObj.Add(extendedAttribute, ADGroupScope.Global);
						return;
					}
				}
				else
				{
					userObj.Add(extendedAttribute, ADGroupScope.DomainLocal);
					return;
				}
			}
		}

		public static IADOPathNode ToSearchGroupCategory(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			ADGroupCategory aDGroupCategory = ADGroupCategory.Distribution;
			IADOPathNode binaryADOPathNode;
			BinaryADOPathNode binaryADOPathNode1 = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode1 == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				if (binaryADOPathNode1.Operator == ADOperator.Eq || binaryADOPathNode1.Operator == ADOperator.Ne)
				{
					IDataNode rightNode = binaryADOPathNode1.RightNode as IDataNode;
					if (rightNode == null)
					{
						throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
					}
					else
					{
						PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
						ObjectADOPathNode objectADOPathNode = new ObjectADOPathNode(null);
						if (!Utils.TryParseEnum<ADGroupCategory>(rightNode.DataObject.ToString(), out aDGroupCategory))
						{
							object[] str = new object[2];
							str[0] = rightNode.DataObject.ToString();
							str[1] = extendedAttributeName;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterInvalidValue, str));
						}
						else
						{
							objectADOPathNode.DataObject = -2147483648;
							if (aDGroupCategory != ADGroupCategory.Security)
							{
								binaryADOPathNode = ADOPathUtil.CreateNotClause(new BinaryADOPathNode(ADOperator.Band, propertyADOPathNode, objectADOPathNode));
							}
							else
							{
								binaryADOPathNode = new BinaryADOPathNode(ADOperator.Band, propertyADOPathNode, objectADOPathNode);
							}
							if (binaryADOPathNode1.Operator != ADOperator.Eq)
							{
								return ADOPathUtil.CreateNotClause(binaryADOPathNode);
							}
							else
							{
								return binaryADOPathNode;
							}
						}
					}
				}
				else
				{
					object[] objArray = new object[2];
					ADOperator[] aDOperatorArray = new ADOperator[2];
					aDOperatorArray[1] = ADOperator.Ne;
					objArray[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
					objArray[1] = extendedAttributeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, objArray));
				}
			}
		}

		public static IADOPathNode ToSearchGroupScope(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			ADGroupScope aDGroupScope = ADGroupScope.DomainLocal;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				if (binaryADOPathNode.Operator == ADOperator.Eq || binaryADOPathNode.Operator == ADOperator.Ne)
				{
					IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
					if (rightNode == null)
					{
						throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
					}
					else
					{
						if (!Utils.TryParseEnum<ADGroupScope>(rightNode.DataObject.ToString(), out aDGroupScope))
						{
							object[] str = new object[2];
							str[0] = rightNode.DataObject.ToString();
							str[1] = extendedAttributeName;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterInvalidValue, str));
						}
						else
						{
							PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
							ObjectADOPathNode objectADOPathNode = new ObjectADOPathNode((object)GroupTypeUtils.GetDirectoryGroupTypeValue(aDGroupScope));
							IADOPathNode aDOPathNode = new BinaryADOPathNode(ADOperator.Band, propertyADOPathNode, objectADOPathNode);
							if (binaryADOPathNode.Operator != ADOperator.Eq)
							{
								return ADOPathUtil.CreateNotClause(aDOPathNode);
							}
							else
							{
								return aDOPathNode;
							}
						}
					}
				}
				else
				{
					object[] objArray = new object[2];
					ADOperator[] aDOperatorArray = new ADOperator[2];
					aDOperatorArray[1] = ADOperator.Ne;
					objArray[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
					objArray[1] = extendedAttributeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, objArray));
				}
			}
		}

		public static bool TryGetExtendedGroupCategoryValue(int ldapgroupTypeValue, out ADGroupCategory groupCategory)
		{
			if ((ldapgroupTypeValue & -2147483648) != -2147483648)
			{
				groupCategory = ADGroupCategory.Distribution;
				return true;
			}
			else
			{
				groupCategory = ADGroupCategory.Security;
				return true;
			}
		}

		public static bool TryGetExtendedGroupScopeValue(int ldapgroupTypeValue, out ADGroupScope groupScope)
		{
			if ((ldapgroupTypeValue & 4) != 4)
			{
				if ((ldapgroupTypeValue & 2) != 2)
				{
					if ((ldapgroupTypeValue & 8) != 8)
					{
						groupScope = ADGroupScope.DomainLocal;
						return false;
					}
					else
					{
						groupScope = ADGroupScope.Universal;
						return true;
					}
				}
				else
				{
					groupScope = ADGroupScope.Global;
					return true;
				}
			}
			else
			{
				groupScope = ADGroupScope.DomainLocal;
				return true;
			}
		}
	}
}