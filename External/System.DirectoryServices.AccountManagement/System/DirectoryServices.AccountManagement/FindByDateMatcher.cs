using System;
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class FindByDateMatcher : SAMMatcher
	{
		private FindByDateMatcher.DateProperty propertyToMatch;

		private MatchType matchType;

		private DateTime valueToMatch;

		internal FindByDateMatcher(FindByDateMatcher.DateProperty property, MatchType matchType, DateTime value)
		{
			this.propertyToMatch = property;
			this.matchType = matchType;
			this.valueToMatch = value;
		}

		internal override bool Matches(DirectoryEntry de)
		{
			if (de.Properties["objectSid"] == null || de.Properties["objectSid"].Count == 0)
			{
				return false;
			}
			else
			{
				FindByDateMatcher.DateProperty dateProperty = this.propertyToMatch;
				switch (dateProperty)
				{
					case FindByDateMatcher.DateProperty.LogonTime:
					{
						return this.MatchOnLogonTime(de);
					}
					case FindByDateMatcher.DateProperty.PasswordSetTime:
					{
						return this.MatchOnPasswordSetTime(de);
					}
					case FindByDateMatcher.DateProperty.AccountExpirationTime:
					{
						return this.MatchOnAccountExpirationTime(de);
					}
				}
				return false;
			}
		}

		private bool MatchOnAccountExpirationTime(DirectoryEntry de)
		{
			PropertyValueCollection item = de.Properties["AccountExpirationDate"];
			DateTime? nullable = null;
			if (item.Count > 0)
			{
				nullable = (DateTime?)item[0];
			}
			return this.TestForMatch(nullable);
		}

		private bool MatchOnLogonTime(DirectoryEntry de)
		{
			PropertyValueCollection item = de.Properties["LastLogin"];
			DateTime? nullable = null;
			if (item.Count > 0)
			{
				nullable = (DateTime?)item[0];
			}
			return this.TestForMatch(nullable);
		}

		private bool MatchOnPasswordSetTime(DirectoryEntry de)
		{
			PropertyValueCollection item = de.Properties["PasswordAge"];
			DateTime? nullable = null;
			if (item.Count != 0)
			{
				int num = (int)item[0];
				nullable = new DateTime?(DateTime.UtcNow - new TimeSpan(0, 0, num));
			}
			return this.TestForMatch(nullable);
		}

		private bool TestForMatch(DateTime? nullableStoreValue)
		{
			if (nullableStoreValue.HasValue)
			{
				DateTime value = nullableStoreValue.Value;
				MatchType matchType = this.matchType;
				switch (matchType)
				{
					case MatchType.Equals:
					{
						return value == this.valueToMatch;
					}
					case MatchType.NotEquals:
					{
						return value != this.valueToMatch;
					}
					case MatchType.GreaterThan:
					{
						return value > this.valueToMatch;
					}
					case MatchType.GreaterThanOrEquals:
					{
						return value >= this.valueToMatch;
					}
					case MatchType.LessThan:
					{
						return value < this.valueToMatch;
					}
					case MatchType.LessThanOrEquals:
					{
						return value <= this.valueToMatch;
					}
				}
				return false;
			}
			else
			{
				if (this.matchType == MatchType.NotEquals)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		internal enum DateProperty
		{
			LogonTime,
			PasswordSetTime,
			AccountExpirationTime
		}
	}
}