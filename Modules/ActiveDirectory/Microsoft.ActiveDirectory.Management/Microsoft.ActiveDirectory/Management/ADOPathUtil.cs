using Microsoft.ActiveDirectory;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADOPathUtil
	{
		public ADOPathUtil()
		{
		}

		internal static string ChangeNodeToWhereFilterSyntax(IADOPathNode node)
		{
			if (node != null)
			{
				if (node as UnaryADOPathNode == null)
				{
					if (node as BinaryADOPathNode == null)
					{
						if (node as TextDataADOPathNode == null)
						{
							if (node as VariableADOPathNode == null)
							{
								if (node as CompositeADOPathNode == null)
								{
									if (node as IDataNode == null)
									{
										object[] type = new object[1];
										type[0] = node.GetType();
										throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Node type: {0} not supported", type));
									}
									else
									{
										IDataNode dataNode = (IDataNode)node;
										return dataNode.DataObject.ToString();
									}
								}
								else
								{
									CompositeADOPathNode compositeADOPathNode = (CompositeADOPathNode)node;
									StringBuilder stringBuilder = new StringBuilder("( ");
									int num = 0;
									foreach (IADOPathNode childNode in compositeADOPathNode.ChildNodes)
									{
										if (num > 0)
										{
											stringBuilder.Append(" -");
											stringBuilder.Append(compositeADOPathNode.Operator);
											stringBuilder.Append(" ");
										}
										stringBuilder.Append(ADOPathUtil.ChangeNodeToWhereFilterSyntax(childNode));
										num++;
									}
									stringBuilder.Append(" )");
									return stringBuilder.ToString();
								}
							}
							else
							{
								VariableADOPathNode variableADOPathNode = (VariableADOPathNode)node;
								return string.Concat("$", variableADOPathNode.VariableExpression);
							}
						}
						else
						{
							TextDataADOPathNode textDataADOPathNode = (TextDataADOPathNode)node;
							return string.Concat("\"", textDataADOPathNode.TextValue, "\"");
						}
					}
					else
					{
						BinaryADOPathNode binaryADOPathNode = (BinaryADOPathNode)node;
						if (binaryADOPathNode.Operator == ADOperator.Approx || binaryADOPathNode.Operator == ADOperator.RecursiveMatch)
						{
							object[] str = new object[1];
							str[0] = binaryADOPathNode.Operator.ToString();
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Operator type: {0} is not supported", str));
						}
						else
						{
							StringBuilder stringBuilder1 = new StringBuilder("( ");
							if (binaryADOPathNode.LeftNode as VariableADOPathNode == null)
							{
								stringBuilder1.Append("$_.");
								stringBuilder1.Append(ADOPathUtil.ChangeNodeToWhereFilterSyntax(binaryADOPathNode.LeftNode));
							}
							else
							{
								IDataNode leftNode = (VariableADOPathNode)binaryADOPathNode.LeftNode;
								stringBuilder1.Append("$_.");
								stringBuilder1.Append(leftNode.DataObject.ToString());
							}
							if (ADOperator.Like != binaryADOPathNode.Operator || string.Compare(binaryADOPathNode.RightNode.GetLdapFilterString(), "*", StringComparison.OrdinalIgnoreCase) != 0)
							{
								stringBuilder1.Append(" -");
								stringBuilder1.Append(binaryADOPathNode.Operator.ToString());
								stringBuilder1.Append(" ");
								stringBuilder1.Append(ADOPathUtil.ChangeNodeToWhereFilterSyntax(binaryADOPathNode.RightNode));
								stringBuilder1.Append(" )");
							}
							else
							{
								stringBuilder1.Append(" -");
								stringBuilder1.Append(ADOperator.Ne.ToString());
								stringBuilder1.Append(" ");
								stringBuilder1.Append("$null");
								stringBuilder1.Append(" )");
							}
							return stringBuilder1.ToString();
						}
					}
				}
				else
				{
					UnaryADOPathNode unaryADOPathNode = (UnaryADOPathNode)node;
					StringBuilder stringBuilder2 = new StringBuilder("-");
					stringBuilder2.Append(unaryADOPathNode.Operator.ToString());
					stringBuilder2.Append("( ");
					stringBuilder2.Append(ADOPathUtil.ChangeNodeToWhereFilterSyntax(unaryADOPathNode.ChildNode));
					stringBuilder2.Append(" )");
					return stringBuilder2.ToString();
				}
			}
			else
			{
				throw new ArgumentNullException("node");
			}
		}

		internal static IADOPathNode CreateAndClause(IADOPathNode[] exprList)
		{
			return new CompositeADOPathNode(ADOperator.And, exprList);
		}

		public static IADOPathNode CreateFilterClause(ADOperator op, string attributeName, object value)
		{
			IADOPathNode objectADOPathNode;
			IADOPathNode propertyADOPathNode = new PropertyADOPathNode(attributeName);
			string str = value as string;
			if (str == null)
			{
				objectADOPathNode = new ObjectADOPathNode(value);
			}
			else
			{
				objectADOPathNode = new TextDataADOPathNode(str);
			}
			return ADOPathUtil.CreateRelationalExpressionNode(op, propertyADOPathNode, objectADOPathNode, null);
		}

		internal static IADOPathNode CreateNotClause(IADOPathNode expr)
		{
			return new UnaryADOPathNode(ADOperator.Not, expr);
		}

		internal static IADOPathNode CreateOrClause(IADOPathNode[] exprList)
		{
			return new CompositeADOPathNode(ADOperator.Or, exprList);
		}

		internal static IADOPathNode CreateRelationalExpressionNode(ADOperator op, IADOPathNode leftExpr, IADOPathNode rightExpr, ConvertSearchFilterDelegate searchFilterConverter)
		{
			if (op == ADOperator.Eq || op == ADOperator.Ne)
			{
				VariableADOPathNode variableADOPathNode = rightExpr as VariableADOPathNode;
				if (variableADOPathNode != null)
				{
					variableADOPathNode.EncodeAsteriskChar = true;
				}
				TextDataADOPathNode textDataADOPathNode = rightExpr as TextDataADOPathNode;
				if (textDataADOPathNode != null)
				{
					textDataADOPathNode.EncodeAsteriskChar = true;
				}
			}
			IADOPathNode binaryADOPathNode = new BinaryADOPathNode(op, leftExpr, rightExpr);
			if (searchFilterConverter != null)
			{
				binaryADOPathNode = searchFilterConverter(binaryADOPathNode);
			}
			return binaryADOPathNode;
		}

		internal static string GetLdapFilterString(ADOperator op)
		{
			ADOperator aDOperator = op;
			switch (aDOperator)
			{
				case ADOperator.Eq:
				{
					return "=";
				}
				case ADOperator.Le:
				{
					return "<=";
				}
				case ADOperator.Ge:
				{
					return ">=";
				}
				case ADOperator.Lt:
				{
					return "NOT_SUPPORTED <";
				}
				case ADOperator.Gt:
				{
					return "NOT_SUPPORTED >";
				}
				case ADOperator.Approx:
				{
					return "~=";
				}
				case ADOperator.RecursiveMatch:
				{
					return ":1.2.840.113556.1.4.1941:=";
				}
				case ADOperator.Ne:
				{
					return "NOT_SUPPORTED !=";
				}
				case ADOperator.Band:
				{
					return ":1.2.840.113556.1.4.803:=";
				}
				case ADOperator.Bor:
				{
					return ":1.2.840.113556.1.4.804:=";
				}
				case ADOperator.Like:
				{
					return "=";
				}
				case ADOperator.NotLike:
				{
					return "NOT_SUPPORTED !=";
				}
				case ADOperator.Not:
				{
					return "!";
				}
				case ADOperator.And:
				{
					return "&";
				}
				case ADOperator.Or:
				{
					return "|";
				}
			}
			object[] str = new object[1];
			str[0] = op.ToString();
			throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.ADFilterOperatorNotSupported, str));
		}

		internal static bool IsOperatorUnary(ADOperator op)
		{
			if (op != ADOperator.Not)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		internal static bool IsValueAllAsterisk(string value)
		{
			if (value == null)
			{
				return false;
			}
			else
			{
				return Regex.Match(value, "^[\\*]+$").Success;
			}
		}

		internal static string LdapSearchEncodeByteArray(byte[] byteArray)
		{
			string str = BitConverter.ToString(byteArray);
			str = str.Replace('-', '\\');
			return string.Concat("\\", str);
		}

		internal static string LdapSearchEncodeObject(object obj, bool encodeAsterisk)
		{
			if (obj as byte[] == null)
			{
				if (!(obj is bool))
				{
					if (!(obj is int))
					{
						if (!(obj is long))
						{
							string str = obj.ToString();
							return ADOPathUtil.LdapSearchEncodeString(str, encodeAsterisk);
						}
						else
						{
							long num = (long)obj;
							return num.ToString(CultureInfo.InvariantCulture);
						}
					}
					else
					{
						int num1 = (int)obj;
						return num1.ToString(CultureInfo.InvariantCulture);
					}
				}
				else
				{
					if ((bool)obj)
					{
						return "TRUE";
					}
					else
					{
						return "FALSE";
					}
				}
			}
			else
			{
				return ADOPathUtil.LdapSearchEncodeByteArray((byte[])obj);
			}
		}

		internal static string LdapSearchEncodeString(string filterTextData, bool encodeAsteriskChar)
		{
			string str;
			StringBuilder stringBuilder = new StringBuilder(filterTextData.Length * 2);
			string str1 = filterTextData;
			for (int i = 0; i < str1.Length; i++)
			{
				char chr = str1[i];
				char chr1 = chr;
				switch (chr1)
				{
					case '(':
					{
						stringBuilder.Append("\\28");
						break;
					}
					case ')':
					{
						stringBuilder.Append("\\29");
						break;
					}
					case '*':
					{
						StringBuilder stringBuilder1 = stringBuilder;
						if (encodeAsteriskChar)
						{
							str = "\\2a";
						}
						else
						{
							str = "*";
						}
						stringBuilder1.Append(str);
						break;
					}
					default:
					{
						if (chr1 == '/')
						{
							stringBuilder.Append("\\2f");
							break;
						}
						else
						{
							stringBuilder.Append(chr);
							break;
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}