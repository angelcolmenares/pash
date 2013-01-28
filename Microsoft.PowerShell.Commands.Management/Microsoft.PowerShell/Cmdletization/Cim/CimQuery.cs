using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cim;
using Microsoft.PowerShell.Cmdletization;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Text;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class CimQuery : QueryBuilder, ISessionBoundQueryBuilder<CimSession>
	{
		private readonly StringBuilder wqlCondition;

		private CimInstance associatedObject;

		private string associationName;

		private string resultRole;

		private string sourceRole;

		internal readonly Dictionary<string, object> queryOptions;

		internal ClientSideQuery ClientSideQuery
		{
			get;
			private set;
		}

		internal CimQuery()
		{
			this.queryOptions = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			this.wqlCondition = new StringBuilder();
			this.ClientSideQuery = new ClientSideQuery();
		}

		public override void AddQueryOption(string optionName, object optionValue)
		{
			if (!string.IsNullOrEmpty(optionName))
			{
				if (optionValue != null)
				{
					this.queryOptions[optionName] = optionValue;
					return;
				}
				else
				{
					throw new ArgumentNullException("optionValue");
				}
			}
			else
			{
				throw new ArgumentNullException("optionName");
			}
		}

		private void AddWqlCondition(string condition)
		{
			string str;
			StringBuilder stringBuilder = this.wqlCondition;
			if (this.wqlCondition.Length != 0)
			{
				str = " AND ";
			}
			else
			{
				str = " WHERE ";
			}
			stringBuilder.Append(str);
			this.wqlCondition.Append('(');
			this.wqlCondition.Append(condition);
			this.wqlCondition.Append(')');
		}

		public override void ExcludeByProperty(string propertyName, IEnumerable excludedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this.ClientSideQuery.ExcludeByProperty(propertyName, excludedPropertyValues, wildcardsEnabled, behaviorOnNoMatch);
			string matchCondition = CimQuery.GetMatchCondition(propertyName, excludedPropertyValues, wildcardsEnabled);
			if (!string.IsNullOrWhiteSpace(matchCondition))
			{
				object[] objArray = new object[1];
				objArray[0] = matchCondition;
				string str = string.Format(CultureInfo.InvariantCulture, "NOT ({0})", objArray);
				this.AddWqlCondition(str);
			}
		}

		public override void FilterByAssociatedInstance(object associatedInstance, string associationName, string sourceRole, string resultRole, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this.ClientSideQuery.FilterByAssociatedInstance(associatedInstance, associationName, sourceRole, resultRole, behaviorOnNoMatch);
			this.associatedObject = associatedInstance as CimInstance;
			this.associationName = associationName;
			this.resultRole = resultRole;
			this.sourceRole = sourceRole;
		}

		public override void FilterByMaxPropertyValue(string propertyName, object maxPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this.ClientSideQuery.FilterByMaxPropertyValue(propertyName, maxPropertyValue, behaviorOnNoMatch);
			string wqlLiteral = CimQuery.ObjectToWqlLiteral(maxPropertyValue);
			if (!string.IsNullOrWhiteSpace(wqlLiteral))
			{
				object[] objArray = new object[2];
				objArray[0] = propertyName;
				objArray[1] = CimQuery.ObjectToWqlLiteral(maxPropertyValue);
				string str = string.Format(CultureInfo.InvariantCulture, "{0} <= {1}", objArray);
				this.AddWqlCondition(str);
			}
		}

		public override void FilterByMinPropertyValue(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this.ClientSideQuery.FilterByMinPropertyValue(propertyName, minPropertyValue, behaviorOnNoMatch);
			string wqlLiteral = CimQuery.ObjectToWqlLiteral(minPropertyValue);
			if (!string.IsNullOrWhiteSpace(wqlLiteral))
			{
				object[] objArray = new object[2];
				objArray[0] = propertyName;
				objArray[1] = wqlLiteral;
				string str = string.Format(CultureInfo.InvariantCulture, "{0} >= {1}", objArray);
				this.AddWqlCondition(str);
			}
		}

		public override void FilterByProperty(string propertyName, IEnumerable allowedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this.ClientSideQuery.FilterByProperty(propertyName, allowedPropertyValues, wildcardsEnabled, behaviorOnNoMatch);
			string matchCondition = CimQuery.GetMatchCondition(propertyName, allowedPropertyValues, wildcardsEnabled);
			if (!string.IsNullOrWhiteSpace(matchCondition))
			{
				this.AddWqlCondition(matchCondition);
			}
		}

		internal IEnumerable<ClientSideQuery.NotFoundError> GenerateNotFoundErrors()
		{
			return this.ClientSideQuery.GenerateNotFoundErrors();
		}

		private static string GetMatchCondition(string propertyName, IEnumerable propertyValues, bool wildcardsEnabled)
		{
			IEnumerable<string> strs = propertyValues.Cast<object>().Select<object, string>((object propertyValue) => {
				if (wildcardsEnabled)
				{
					return CimQuery.GetMatchConditionForLikeOperator(propertyName, propertyValue);
				}
				else
				{
					return CimQuery.GetMatchConditionForEqualityOperator(propertyName, propertyValue);
				}
			}
			);
			List<string> list = strs.Where<string>((string individualCondition) => !string.IsNullOrWhiteSpace(individualCondition)).ToList<string>();
			if (list.Count != 0)
			{
				string str = string.Join(" OR ", list);
				return str;
			}
			else
			{
				return null;
			}
		}

		private static string GetMatchConditionForEqualityOperator(string propertyName, object propertyValue)
		{
			string str;
            if (propertyValue is char)
			{
				string wqlLiteral = CimQuery.ObjectToWqlLiteral(propertyValue);
				if (!string.IsNullOrWhiteSpace(wqlLiteral))
				{
					object[] objArray = new object[2];
					objArray[0] = propertyName;
					objArray[1] = wqlLiteral;
					str = string.Format(CultureInfo.InvariantCulture, "({0} = {1})", objArray);
					return str;
				}
				else
				{
					return null;
				}
			}
			else
			{
				char chr = (char)propertyValue;
				char lowerInvariant = char.ToLowerInvariant(chr);
				char upperInvariant = char.ToUpperInvariant(chr);
				string wqlLiteral1 = CimQuery.ObjectToWqlLiteral(lowerInvariant);
				string str1 = CimQuery.ObjectToWqlLiteral(upperInvariant);
				object[] objArray1 = new object[3];
				objArray1[0] = propertyName;
				objArray1[1] = wqlLiteral1;
				objArray1[2] = str1;
				str = string.Format(CultureInfo.InvariantCulture, "(({0} = {1}) OR ({0} = {2}))", objArray1);
				return str;
			}
		}

		private static string GetMatchConditionForLikeOperator(string propertyName, object propertyValue)
		{
			bool flag = false;
			string str = (string)LanguagePrimitives.ConvertTo(propertyValue, typeof(string), CultureInfo.InvariantCulture);
			WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
			string wqlLikeOperand = CimQuery.WildcardToWqlLikeOperand(wildcardPattern, out flag);
			object[] objArray = new object[2];
			objArray[0] = propertyName;
			objArray[1] = wqlLikeOperand;
			string str1 = string.Format(CultureInfo.InvariantCulture, "({0} LIKE {1})", objArray);
			return str1;
		}

		internal StartableJob GetQueryJob(CimJobContext jobContext)
		{
			if (this.associationName != null)
			{
				return new EnumerateAssociatedInstancesJob(jobContext, this, this.associatedObject, this.associationName, this.resultRole, this.sourceRole);
			}
			else
			{
				return new QueryInstancesJob(jobContext, this, this.wqlCondition.ToString());
			}
		}

		internal bool IsMatchingResult(CimInstance result)
		{
			return this.ClientSideQuery.IsResultMatchingClientSideQuery(result);
		}

		CimSession Microsoft.PowerShell.Cmdletization.ISessionBoundQueryBuilder<Microsoft.Management.Infrastructure.CimSession>.GetTargetSession()
		{
			if (this.associatedObject == null)
			{
				return null;
			}
			else
			{
				return CimCmdletAdapter.GetSessionOfOriginFromCimInstance(this.associatedObject);
			}
		}

		private static string ObjectToWqlLiteral(object o)
		{
			if (!LanguagePrimitives.IsNull(o))
			{
				o = CimValueConverter.ConvertFromDotNetToCim(o);
				PSObject pSObject = PSObject.AsPSObject(o);
				Type type = pSObject.BaseObject.GetType();
				TypeCode typeCode = LanguagePrimitives.GetTypeCode(type);
				if (typeCode != TypeCode.String)
				{
					if (typeCode != TypeCode.Char)
					{
						if (typeCode != TypeCode.DateTime)
						{
							if (type != typeof(TimeSpan))
							{
								if (!LanguagePrimitives.IsNumeric(typeCode))
								{
									if (!LanguagePrimitives.IsBooleanType(type))
									{
										throw CimValueConverter.GetInvalidCastException(null, "InvalidCimQueryCast", o, CmdletizationResources.CimConversion_WqlQuery);
									}
									else
									{
										if (!(bool)LanguagePrimitives.ConvertTo(o, typeof(bool), CultureInfo.InvariantCulture))
										{
											return "FALSE";
										}
										else
										{
											return "TRUE";
										}
									}
								}
								else
								{
									return (string)LanguagePrimitives.ConvertTo(o, typeof(string), CultureInfo.InvariantCulture);
								}
							}
							else
							{
								return null;
							}
						}
						else
						{
							DateTime dateTime = (DateTime)LanguagePrimitives.ConvertTo(o, typeof(DateTime), CultureInfo.InvariantCulture);
							string dmtfDateTime = ManagementDateTimeConverter.ToDmtfDateTime(dateTime);
							return string.Concat("'", dmtfDateTime, "'");
						}
					}
					else
					{
						return CimQuery.ObjectToWqlLiteral(LanguagePrimitives.ConvertTo(o, typeof(string), CultureInfo.InvariantCulture));
					}
				}
				else
				{
					string str = o.ToString();
					str = str.Replace("\\", "\\\\");
					str = str.Replace("'", "\\'");
					return string.Concat("'", str, "'");
				}
			}
			else
			{
				return "null";
			}
		}

		public override string ToString()
		{
			return this.wqlCondition.ToString();
		}

		private static string WildcardToWqlLikeOperand(WildcardPattern wildcardPattern, out bool needsClientSideFiltering)
		{
			string str = WildcardPatternToCimQueryParser.Parse(wildcardPattern, out needsClientSideFiltering);
			return CimQuery.ObjectToWqlLiteral(str);
		}
	}
}