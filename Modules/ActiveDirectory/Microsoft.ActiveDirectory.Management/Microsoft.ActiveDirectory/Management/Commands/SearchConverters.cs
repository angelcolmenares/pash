using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class SearchConverters
	{
		private SearchConverters()
		{
		}

		internal static string ConvertOperatorListToString(ADOperator[] operatorList)
		{
			if (operatorList.Length == 0 || operatorList.Contains (ADOperator.Eq))
			{
				return "";
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("'").Append(operatorList[0].ToString()).Append("'");
				for (int i = 1; i < (int)operatorList.Length; i++)
				{
					stringBuilder.Append(", '").Append(operatorList[i].ToString()).Append("'");
				}
				return stringBuilder.ToString();
			}
		}

		internal static ToSearchFilterDelegate GetDelegateToSearchFlagInInt(int bit, bool isInverted)
		{
			return (string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo) => SearchConverters.ToSearchFlagInInt(bit, isInverted, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchDateTimeUsingSchemaInfo(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			DateTime dateTime;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
				ObjectADOPathNode objectADOPathNode = new ObjectADOPathNode(null);
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					object dataObject = rightNode.DataObject;
					string str = dataObject as string;
					if (str != null && DateTime.TryParse(str, out dateTime))
					{
						dataObject = dateTime;
					}
					ADTypeConverter aDTypeConverter = new ADTypeConverter(cmdletSessionInfo.ADSessionInfo);
					object raw = aDTypeConverter.ConvertToRaw(directoryAttributes[0], dataObject);
					objectADOPathNode.DataObject = raw;
					objectADOPathNode.EncodeAsteriskChar = rightNode.EncodeAsteriskChar;
					return new BinaryADOPathNode(binaryADOPathNode.Operator, propertyADOPathNode, objectADOPathNode);
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

		internal static IADOPathNode ToSearchEnum<T>(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			T t = default(T);
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
				ObjectADOPathNode objectADOPathNode = new ObjectADOPathNode(null);
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode == null)
				{
					throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
				}
				else
				{
					if (!Utils.TryParseEnum<T>(rightNode.DataObject.ToString(), out t))
					{
						object[] objArray = new object[1];
						objArray[0] = extendedAttributeName;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterRHSNotMatchEnumValue, objArray));
					}
					else
					{
						if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
						{
							objectADOPathNode.DataObject = Convert.ToInt64(t, CultureInfo.InvariantCulture);
						}
						else
						{
							objectADOPathNode.DataObject = Convert.ToInt32(t, CultureInfo.InvariantCulture);
						}
						return new BinaryADOPathNode(binaryADOPathNode.Operator, propertyADOPathNode, objectADOPathNode);
					}
				}
			}
		}

		internal static IADOPathNode ToSearchFlagEnumerationInInt<T>(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			T t = default(T);
			object num;
			IADOPathNode item;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					ADOperator @operator = binaryADOPathNode.Operator;
					if (@operator == ADOperator.Eq || @operator == ADOperator.Ne)
					{
						char[] chrArray = new char[1];
						chrArray[0] = ',';
						string[] strArrays = rightNode.DataObject.ToString().Split(chrArray);
						List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>();
						string[] strArrays1 = strArrays;
						int num1 = 0;
						while (num1 < (int)strArrays1.Length)
						{
							string str = strArrays1[num1];
							if (!Utils.TryParseEnum<T>(str, out t))
							{
								object[] objArray = new object[1];
								objArray[0] = extendedAttributeName;
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterRHSNotMatchEnumValue, objArray));
							}
							else
							{
								if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
								{
									num = Convert.ToInt64(t, CultureInfo.InvariantCulture);
								}
								else
								{
									num = Convert.ToInt32(t, CultureInfo.InvariantCulture);
								}
								IADOPathNode aDOPathNode = new BinaryADOPathNode(ADOperator.Bor, new PropertyADOPathNode(directoryAttributes[0]), new ObjectADOPathNode(num));
								aDOPathNodes.Add(aDOPathNode);
								num1++;
							}
						}
						if (aDOPathNodes.Count <= 1)
						{
							if (aDOPathNodes.Count != 1)
							{
								object[] objArray1 = new object[1];
								objArray1[0] = extendedAttributeName;
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterRHSNotMatchEnumValue, objArray1));
							}
							else
							{
								item = aDOPathNodes[0];
							}
						}
						else
						{
							item = ADOPathUtil.CreateAndClause(aDOPathNodes.ToArray());
						}
						if (@operator == ADOperator.Ne)
						{
							item = ADOPathUtil.CreateNotClause(item);
						}
						return item;
					}
					else
					{
						object[] str1 = new object[2];
						ADOperator[] aDOperatorArray = new ADOperator[2];
						aDOperatorArray[1] = ADOperator.Ne;
						str1[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
						str1[1] = extendedAttributeName;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str1));
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

		internal static IADOPathNode ToSearchFlagInInt(int bit, bool isInverted, string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					if (rightNode.DataObject is bool)
					{
						ADOperator @operator = binaryADOPathNode.Operator;
						if (@operator == ADOperator.Eq || @operator == ADOperator.Ne)
						{
							IADOPathNode aDOPathNode = new BinaryADOPathNode(ADOperator.Bor, new PropertyADOPathNode(directoryAttributes[0]), new ObjectADOPathNode((object)bit));
							bool dataObject = !(bool)rightNode.DataObject;
							if (@operator == ADOperator.Ne)
							{
								dataObject = !dataObject;
							}
							if (isInverted)
							{
								dataObject = !dataObject;
							}
							if (dataObject)
							{
								aDOPathNode = ADOPathUtil.CreateNotClause(aDOPathNode);
							}
							return aDOPathNode;
						}
						else
						{
							object[] str = new object[2];
							ADOperator[] aDOperatorArray = new ADOperator[2];
							aDOperatorArray[1] = ADOperator.Ne;
							str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
							str[1] = extendedAttributeName;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
						}
					}
					else
					{
						object[] type = new object[2];
						type[0] = rightNode.DataObject.GetType();
						type[1] = extendedAttributeName;
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

		internal static IADOPathNode ToSearchFromADEntityToAttributeValue<F, O>(string searchBase, string attributeName, string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			ADEntity extendedObjectFromIdentity;
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
						O dataObject = (O)(rightNode.DataObject as O);
						if (dataObject == null)
						{
							string str = rightNode.DataObject as string;
							if (str != null)
							{
								dataObject = Activator.CreateInstance<O>();
								dataObject.Identity = str;
							}
						}
						if (dataObject == null)
						{
							object[] objArray = new object[2];
							objArray[0] = rightNode.DataObject.ToString();
							objArray[1] = extendedAttributeName;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterInvalidValue, objArray));
						}
						else
						{
							F f = Activator.CreateInstance<F>();
							f.SetCmdletSessionInfo(cmdletSessionInfo);
							try
							{
								if (attributeName != null)
								{
									HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
									strs.Add(attributeName);
									extendedObjectFromIdentity = f.GetExtendedObjectFromIdentity(dataObject, searchBase, strs, false);
									objectADOPathNode.DataObject = (string)extendedObjectFromIdentity[attributeName].Value;
								}
								else
								{
									extendedObjectFromIdentity = f.GetDirectoryObjectFromIdentity(dataObject, searchBase);
									objectADOPathNode.DataObject = (string)extendedObjectFromIdentity["DistinguishedName"].Value;
								}
								binaryADOPathNode = new BinaryADOPathNode(binaryADOPathNode1.Operator, propertyADOPathNode, objectADOPathNode);
							}
							catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
							{
								ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
								object[] message = new object[2];
								message[0] = extendedAttributeName;
								message[1] = aDIdentityNotFoundException.Message;
								throw new ADIdentityResolutionException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityInExtendedAttributeCannotBeResolved, message), aDIdentityNotFoundException);
							}
							catch (ADIdentityResolutionException aDIdentityResolutionException1)
							{
								ADIdentityResolutionException aDIdentityResolutionException = aDIdentityResolutionException1;
								object[] message1 = new object[2];
								message1[0] = extendedAttributeName;
								message1[1] = aDIdentityResolutionException.Message;
								throw new ADIdentityResolutionException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityInExtendedAttributeCannotBeResolved, message1), aDIdentityResolutionException);
							}
							return binaryADOPathNode;
						}
					}
				}
				else
				{
					object[] str1 = new object[2];
					ADOperator[] aDOperatorArray = new ADOperator[2];
					aDOperatorArray[1] = ADOperator.Ne;
					str1[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
					str1[1] = extendedAttributeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str1));
				}
			}
		}

		internal static IADOPathNode ToSearchFromADObjectToDN<F, O>(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADObject, new()
		{
			return SearchConverters.ToSearchFromADEntityToAttributeValue<F, O>(cmdletSessionInfo.DefaultPartitionPath, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchGuid(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					ADOperator @operator = binaryADOPathNode.Operator;
					if (@operator == ADOperator.Eq || @operator == ADOperator.Ne)
					{
						byte[] byteArray = null;
						if (!(rightNode.DataObject is Guid))
						{
							if (rightNode.DataObject as byte[] == null)
							{
								if (rightNode.DataObject as string == null)
								{
									object[] type = new object[2];
									type[0] = rightNode.DataObject.GetType();
									type[1] = extendedAttributeName;
									throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterUnrecognizedObjectType, type));
								}
								else
								{
									Guid? nullable = null;
									if (!Utils.TryParseGuid((string)rightNode.DataObject, out nullable))
									{
										return ADOPathUtil.CreateFilterClause(@operator, directoryAttributes[0], rightNode.DataObject);
									}
									else
									{
										Guid value = nullable.Value;
										byteArray = value.ToByteArray();
									}
								}
							}
							else
							{
								byteArray = (byte[])rightNode.DataObject;
							}
						}
						else
						{
							Guid dataObject = (Guid)rightNode.DataObject;
							byteArray = dataObject.ToByteArray();
						}
						return ADOPathUtil.CreateFilterClause(@operator, directoryAttributes[0], byteArray);
					}
					else
					{
						object[] str = new object[2];
						ADOperator[] aDOperatorArray = new ADOperator[2];
						aDOperatorArray[1] = ADOperator.Ne;
						str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
						str[1] = extendedAttributeName;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
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

		internal static IADOPathNode ToSearchInvertBool(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					if (rightNode.DataObject is bool)
					{
						ADOperator @operator = binaryADOPathNode.Operator;
						if (@operator == ADOperator.Eq || @operator == ADOperator.Ne)
						{
							IADOPathNode aDOPathNode = new BinaryADOPathNode(binaryADOPathNode.Operator, new PropertyADOPathNode(directoryAttributes[0]), new ObjectADOPathNode((object)(!(bool)rightNode.DataObject)));
							return SearchConverters.ToSearchObject(extendedAttributeName, directoryAttributes, aDOPathNode, cmdletSessionInfo);
						}
						else
						{
							object[] str = new object[2];
							ADOperator[] aDOperatorArray = new ADOperator[2];
							aDOperatorArray[1] = ADOperator.Ne;
							str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
							str[1] = extendedAttributeName;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
						}
					}
					else
					{
						object[] type = new object[2];
						type[0] = rightNode.DataObject.GetType();
						type[1] = extendedAttributeName;
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

		internal static IADOPathNode ToSearchMultivalueCertificate(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
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
						X509Certificate dataObject = rightNode.DataObject as X509Certificate;
						if (dataObject != null)
						{
							IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(binaryADOPathNode.Operator, directoryAttributes[0], dataObject.GetRawCertData());
							return aDOPathNode;
						}
						else
						{
							throw new ArgumentException(StringResources.SearchConverterInvalidValue);
						}
					}
				}
				else
				{
					object[] str = new object[2];
					ADOperator[] aDOperatorArray = new ADOperator[2];
					aDOperatorArray[1] = ADOperator.Ne;
					str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
					str[1] = extendedAttribute;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
				}
			}
		}

		internal static IADOPathNode ToSearchMultivalueObject(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return SearchConverters.ToSearchObject(extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchNegativeTimeSpan(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			TimeSpan timeSpan;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
				TextDataADOPathNode textDataADOPathNode = new TextDataADOPathNode("");
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					if (!(rightNode.DataObject is TimeSpan))
					{
						if (!(rightNode.DataObject is long) || !(rightNode.DataObject is int))
						{
							long dataObject = (long)rightNode.DataObject;
							long num = -Math.Abs(dataObject);
							textDataADOPathNode.TextValue = num.ToString();
						}
						else
						{
							if (rightNode.DataObject as string == null)
							{
								object[] type = new object[2];
								type[0] = rightNode.DataObject.GetType();
								type[1] = directoryAttributes[0];
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterUnrecognizedObjectType, type));
							}
							else
							{
								if (!TimeSpan.TryParse((string)rightNode.DataObject, out timeSpan))
								{
									textDataADOPathNode.TextValue = (string)rightNode.DataObject;
								}
								else
								{
									long num1 = -Math.Abs(timeSpan.Ticks);
									textDataADOPathNode.TextValue = num1.ToString();
								}
							}
						}
					}
					else
					{
						TimeSpan dataObject1 = (TimeSpan)rightNode.DataObject;
						long num2 = -Math.Abs(dataObject1.Ticks);
						textDataADOPathNode.TextValue = num2.ToString();
					}
					ADOperator @operator = binaryADOPathNode.Operator;
					ADOperator aDOperator = @operator;
					switch (aDOperator)
					{
						case ADOperator.Le:
						{
							@operator = ADOperator.Ge;
							break;
						}
						case ADOperator.Ge:
						{
							@operator = ADOperator.Le;
							break;
						}
						case ADOperator.Lt:
						{
							@operator = ADOperator.Gt;
							break;
						}
						case ADOperator.Gt:
						{
							@operator = ADOperator.Lt;
							break;
						}
					}
					return new BinaryADOPathNode(@operator, propertyADOPathNode, textDataADOPathNode);
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

		internal static IADOPathNode ToSearchNotSupported(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			object[] objArray = new object[1];
			objArray[0] = extendedAttributeName;
			throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterAttributeNotSupported, objArray));
		}

		internal static IADOPathNode ToSearchObject(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					return new BinaryADOPathNode(binaryADOPathNode.Operator, propertyADOPathNode, binaryADOPathNode.RightNode);
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

		internal static IADOPathNode ToSearchObjectClientSideFilter(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return filterClause;
		}

		internal static IADOPathNode ToSearchUsingSchemaInfo(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			ADTypeConverter aDTypeConverter = new ADTypeConverter(cmdletSessionInfo.ADSessionInfo);
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				PropertyADOPathNode propertyADOPathNode = new PropertyADOPathNode(directoryAttributes[0]);
				ObjectADOPathNode objectADOPathNode = new ObjectADOPathNode(null);
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode == null)
				{
					throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
				}
				else
				{
					object raw = aDTypeConverter.ConvertToRaw(directoryAttributes[0], rightNode.DataObject);
					objectADOPathNode.DataObject = raw;
					objectADOPathNode.EncodeAsteriskChar = rightNode.EncodeAsteriskChar;
					return new BinaryADOPathNode(binaryADOPathNode.Operator, propertyADOPathNode, objectADOPathNode);
				}
			}
		}
	}
}