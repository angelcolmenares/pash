using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class QbeMatchType
	{
		private object @value;

		private MatchType matchType;

		internal MatchType Match
		{
			get
			{
				return this.matchType;
			}
			set
			{
				this.matchType = value;
			}
		}

		internal object Value
		{
			get
			{
				return this.@value;
			}
			set
			{
				this.@value = value;
			}
		}

		internal QbeMatchType(object value, MatchType matchType)
		{
			this.@value = value;
			this.matchType = matchType;
		}
	}
}