using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class QbeMatcher : SAMMatcher
	{
		private QbeFilterDescription propertiesToMatch;

		private static object[,] filterPropertiesTableRaw;

		private static Hashtable filterPropertiesTable;

		static QbeMatcher()
		{
			object[,] matcherDelegate = new object[22, 3];
			matcherDelegate[0, 0] = typeof(DescriptionFilter);
			matcherDelegate[0, 1] = "Description";
			matcherDelegate[0, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.StringMatcher);
			matcherDelegate[1, 0] = typeof(DisplayNameFilter);
			matcherDelegate[1, 1] = "FullName";
			matcherDelegate[1, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.StringMatcher);
			matcherDelegate[2, 0] = typeof(SidFilter);
			matcherDelegate[2, 1] = "objectSid";
			matcherDelegate[2, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.SidMatcher);
			matcherDelegate[3, 0] = typeof(SamAccountNameFilter);
			matcherDelegate[3, 1] = "Name";
			matcherDelegate[3, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.SamAccountNameMatcher);
			matcherDelegate[4, 0] = typeof(AuthPrincEnabledFilter);
			matcherDelegate[4, 1] = "UserFlags";
			matcherDelegate[4, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[5, 0] = typeof(PermittedWorkstationFilter);
			matcherDelegate[5, 1] = "LoginWorkstations";
			matcherDelegate[5, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.MultiStringMatcher);
			matcherDelegate[6, 0] = typeof(PermittedLogonTimesFilter);
			matcherDelegate[6, 1] = "LoginHours";
			matcherDelegate[6, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.BinaryMatcher);
			matcherDelegate[7, 0] = typeof(ExpirationDateFilter);
			matcherDelegate[7, 1] = "AccountExpirationDate";
			matcherDelegate[7, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.ExpirationDateMatcher);
			matcherDelegate[8, 0] = typeof(SmartcardLogonRequiredFilter);
			matcherDelegate[8, 1] = "UserFlags";
			matcherDelegate[8, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[9, 0] = typeof(DelegationPermittedFilter);
			matcherDelegate[9, 1] = "UserFlags";
			matcherDelegate[9, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[10, 0] = typeof(HomeDirectoryFilter);
			matcherDelegate[10, 1] = "HomeDirectory";
			matcherDelegate[10, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.StringMatcher);
			matcherDelegate[11, 0] = typeof(HomeDriveFilter);
			matcherDelegate[11, 1] = "HomeDirDrive";
			matcherDelegate[11, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.StringMatcher);
			matcherDelegate[12, 0] = typeof(ScriptPathFilter);
			matcherDelegate[12, 1] = "LoginScript";
			matcherDelegate[12, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.StringMatcher);
			matcherDelegate[13, 0] = typeof(PasswordNotRequiredFilter);
			matcherDelegate[13, 1] = "UserFlags";
			matcherDelegate[13, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[14, 0] = typeof(PasswordNeverExpiresFilter);
			matcherDelegate[14, 1] = "UserFlags";
			matcherDelegate[14, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[15, 0] = typeof(CannotChangePasswordFilter);
			matcherDelegate[15, 1] = "UserFlags";
			matcherDelegate[15, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[16, 0] = typeof(AllowReversiblePasswordEncryptionFilter);
			matcherDelegate[16, 1] = "UserFlags";
			matcherDelegate[16, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.UserFlagsMatcher);
			matcherDelegate[17, 0] = typeof(GroupScopeFilter);
			matcherDelegate[17, 1] = "groupType";
			matcherDelegate[17, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.GroupTypeMatcher);
			matcherDelegate[18, 0] = typeof(ExpiredAccountFilter);
			matcherDelegate[18, 1] = "AccountExpirationDate";
			matcherDelegate[18, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.DateTimeMatcher);
			matcherDelegate[19, 0] = typeof(LastLogonTimeFilter);
			matcherDelegate[19, 1] = "LastLogin";
			matcherDelegate[19, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.DateTimeMatcher);
			matcherDelegate[20, 0] = typeof(PasswordSetTimeFilter);
			matcherDelegate[20, 1] = "PasswordAge";
			matcherDelegate[20, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.DateTimeMatcher);
			matcherDelegate[21, 0] = typeof(BadLogonCountFilter);
			matcherDelegate[21, 1] = "BadPasswordAttempts";
			matcherDelegate[21, 2] = new QbeMatcher.MatcherDelegate(QbeMatcher.IntMatcher);
			QbeMatcher.filterPropertiesTableRaw = matcherDelegate;
			QbeMatcher.filterPropertiesTable = null;
			QbeMatcher.filterPropertiesTable = new Hashtable();
			for (int i = 0; i < QbeMatcher.filterPropertiesTableRaw.GetLength(0); i++)
			{
				Type type = QbeMatcher.filterPropertiesTableRaw[i, 0] as Type;
				string str = QbeMatcher.filterPropertiesTableRaw[i, 1] as string;
				QbeMatcher.MatcherDelegate matcherDelegate1 = QbeMatcher.filterPropertiesTableRaw[i, 2] as QbeMatcher.MatcherDelegate;
				QbeMatcher.FilterPropertyTableEntry filterPropertyTableEntry = new QbeMatcher.FilterPropertyTableEntry();
				filterPropertyTableEntry.winNTPropertyName = str;
				filterPropertyTableEntry.matcher = matcherDelegate1;
				QbeMatcher.filterPropertiesTable[type] = filterPropertyTableEntry;
			}
		}

		internal QbeMatcher(QbeFilterDescription propertiesToMatch)
		{
			this.propertiesToMatch = propertiesToMatch;
		}

		private static bool BinaryMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			byte[] value = (byte[])filter.Value;
			if (value != null)
			{
				if (de.Properties.Contains(winNTPropertyName))
				{
					byte[] numArray = (byte[])de.Properties[winNTPropertyName].Value;
					if (numArray != null && Utils.AreBytesEqual(numArray, value))
					{
						return true;
					}
				}
			}
			else
			{
				if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0 || de.Properties[winNTPropertyName].Value == null)
				{
					return true;
				}
			}
			return false;
		}

		private static bool DateTimeMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			DateTime value;
			QbeMatchType qbeMatchType = (QbeMatchType)filter.Value;
			if (qbeMatchType.Value != null)
			{
				if (de.Properties.Contains(winNTPropertyName) && de.Properties[winNTPropertyName].Value != null)
				{
					if (winNTPropertyName != "PasswordAge")
					{
						value = (DateTime)de.Properties[winNTPropertyName].Value;
					}
					else
					{
						PropertyValueCollection item = de.Properties["PasswordAge"];
						if (item.Count == 0)
						{
							return false;
						}
						else
						{
							int num = (int)item[0];
							value = DateTime.UtcNow - new TimeSpan(0, 0, num);
						}
					}
					int num1 = DateTime.Compare(value, (DateTime)qbeMatchType.Value);
					bool flag = true;
					MatchType match = qbeMatchType.Match;
					switch (match)
					{
						case MatchType.Equals:
						{
							flag = num1 == 0;
							break;
						}
						case MatchType.NotEquals:
						{
							flag = num1 != 0;
							break;
						}
						case MatchType.GreaterThan:
						{
							flag = num1 > 0;
							break;
						}
						case MatchType.GreaterThanOrEquals:
						{
							flag = num1 >= 0;
							break;
						}
						case MatchType.LessThan:
						{
							flag = num1 < 0;
							break;
						}
						case MatchType.LessThanOrEquals:
						{
							flag = num1 <= 0;
							break;
						}
						default:
						{
							flag = false;
							break;
						}
					}
					return flag;
				}
			}
			else
			{
				if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0 || de.Properties[winNTPropertyName].Value == null)
				{
					return true;
				}
			}
			return false;
		}

		private static bool ExpirationDateMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			DateTime? value = (DateTime?)filter.Value;
			if (value.HasValue)
			{
				if (de.Properties.Contains(winNTPropertyName) && de.Properties[winNTPropertyName].Value != null)
				{
					DateTime dateTime = (DateTime)de.Properties[winNTPropertyName].Value;
					if (dateTime.Equals(value.Value))
					{
						return true;
					}
				}
			}
			else
			{
				if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0 || de.Properties[winNTPropertyName].Value == null)
				{
					return true;
				}
			}
			return false;
		}

		private static bool GroupTypeMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			GroupScope value = (GroupScope)filter.Value;
			if (value != GroupScope.Local)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private static bool IntMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			QbeMatchType value = (QbeMatchType)filter.Value;
			bool flag = false;
			if (value.Value != null)
			{
				if (de.Properties.Contains(winNTPropertyName))
				{
					int num = (int)de.Properties[winNTPropertyName].Value;
					int value1 = (int)value.Value;
					MatchType match = value.Match;
					switch (match)
					{
						case MatchType.Equals:
						{
							flag = num == value1;
							break;
						}
						case MatchType.NotEquals:
						{
							flag = num != value1;
							break;
						}
						case MatchType.GreaterThan:
						{
							flag = num > value1;
							break;
						}
						case MatchType.GreaterThanOrEquals:
						{
							flag = num >= value1;
							break;
						}
						case MatchType.LessThan:
						{
							flag = num < value1;
							break;
						}
						case MatchType.LessThanOrEquals:
						{
							flag = num <= value1;
							break;
						}
						default:
						{
							flag = false;
							break;
						}
					}
				}
			}
			else
			{
				if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0 || de.Properties[winNTPropertyName].Value == null)
				{
					flag = true;
				}
			}
			return flag;
		}

		internal override bool Matches(DirectoryEntry de)
		{
			bool flag;
			if (de.Properties["objectSid"] == null || de.Properties["objectSid"].Count == 0)
			{
				return false;
			}
			else
			{
				IEnumerator enumerator = this.propertiesToMatch.FiltersToApply.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						FilterBase current = (FilterBase)enumerator.Current;
						QbeMatcher.FilterPropertyTableEntry item = (QbeMatcher.FilterPropertyTableEntry)QbeMatcher.filterPropertiesTable[current.GetType()];
						if (item != null)
						{
							if (item.matcher(current, item.winNTPropertyName, de))
							{
								continue;
							}
							flag = false;
							return flag;
						}
						else
						{
							object[] externalForm = new object[1];
							externalForm[0] = PropertyNamesExternal.GetExternalForm(current.PropertyName);
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPropertyForQuery, externalForm));
						}
					}
					return true;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return flag;
			}
		}

		private static bool MultiStringMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			bool flag;
			string value = (string)filter.Value;
			if (value != null)
			{
				if (de.Properties.Contains(winNTPropertyName) && de.Properties[winNTPropertyName].Count != 0)
				{
					IEnumerator enumerator = de.Properties[winNTPropertyName].GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string current = (string)enumerator.Current;
							if (current == null)
							{
								continue;
							}
							flag = QbeMatcher.WildcardStringMatch(filter, value, current);
							return flag;
						}
						return false;
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
					return flag;
				}
			}
			else
			{
				if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0 || ((string)de.Properties[winNTPropertyName].Value).Length == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static bool SamAccountNameMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			string str;
			string value = (string)filter.Value;
			int num = value.IndexOf('\\');
			if (num != value.Length - 1)
			{
				if (num != -1)
				{
					str = value.Substring(num + 1);
				}
				else
				{
					str = value;
				}
				string str1 = str;
				if (de.Properties["Name"].Count <= 0 || de.Properties["Name"].Value == null)
				{
					return false;
				}
				else
				{
					return QbeMatcher.WildcardStringMatch(filter, str1, (string)de.Properties["Name"].Value);
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.StoreCtxNT4IdentityClaimWrongForm);
			}
		}

		private static bool SidMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			byte[] byteArray = Utils.StringToByteArray((string)filter.Value);
			if (byteArray != null)
			{
				if (de.Properties["objectSid"].Count <= 0 || de.Properties["objectSid"].Value == null)
				{
					return false;
				}
				else
				{
					return Utils.AreBytesEqual(byteArray, (byte[])de.Properties["objectSid"].Value);
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.StoreCtxSecurityIdentityClaimBadFormat);
			}
		}

		private static bool StringMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			string value = (string)filter.Value;
			if (value != null)
			{
				if (de.Properties.Contains(winNTPropertyName))
				{
					string str = (string)de.Properties[winNTPropertyName].Value;
					if (str != null)
					{
						return QbeMatcher.WildcardStringMatch(filter, value, str);
					}
				}
			}
			else
			{
				if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0 || ((string)de.Properties[winNTPropertyName].Value).Length == 0)
				{
					return true;
				}
			}
			return false;
		}

		private static bool UserFlagsMatcher(FilterBase filter, string winNTPropertyName, DirectoryEntry de)
		{
			bool value = (bool)filter.Value;
			if (!de.Properties.Contains(winNTPropertyName) || de.Properties[winNTPropertyName].Count == 0)
			{
				return false;
			}
			else
			{
				int num = (int)de.Properties[winNTPropertyName].Value;
				string propertyName = filter.PropertyName;
				string str = propertyName;
				if (propertyName != null)
				{
					switch (str)
					{
						case "AuthenticablePrincipal.Enabled":
						{
							return (num & 2) != 0 ^ value;
						}
						case "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired":
						{
							return !((num & 0x40000) != 0 ^ value);
						}
						case "AuthenticablePrincipal.AccountInfo.DelegationPermitted":
						{
							return (num & 0x100000) != 0 ^ value;
						}
						case "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired":
						{
							return !((num & 32) != 0 ^ value);
						}
						case "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires":
						{
							return !((num & 0x10000) != 0 ^ value);
						}
						case "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword":
						{
							return !((num & 64) != 0 ^ value);
						}
						case "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption":
						{
							return !((num & 128) != 0 ^ value);
						}
					}
				}
				return false;
			}
		}

		private static bool WildcardStringMatch(FilterBase filter, string wildcardFilter, string property)
		{
			Regex extra = filter.Extra as Regex;
			if (extra == null)
			{
				extra = new Regex(SAMUtils.PAPIQueryToRegexString(wildcardFilter), RegexOptions.Singleline);
				filter.Extra = extra;
			}
			Match match = extra.Match(property);
			return match.Success;
		}

		private class FilterPropertyTableEntry
		{
			internal string winNTPropertyName;

			internal QbeMatcher.MatcherDelegate matcher;

			public FilterPropertyTableEntry()
			{
			}
		}

		private delegate bool MatcherDelegate(FilterBase filter, string winNTPropertyName, DirectoryEntry de);
	}
}