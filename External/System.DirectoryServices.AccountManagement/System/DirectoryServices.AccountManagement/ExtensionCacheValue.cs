using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExtensionCacheValue
	{
		private object[] @value;

		private bool filterOnly;

		private Type type;

		private MatchType matchType;

		internal bool Filter
		{
			get
			{
				return this.filterOnly;
			}
		}

		internal MatchType MatchType
		{
			get
			{
				return this.matchType;
			}
		}

		internal Type Type
		{
			get
			{
				return this.type;
			}
		}

		internal object[] Value
		{
			get
			{
				return this.@value;
			}
		}

		internal ExtensionCacheValue(object[] value)
		{
			this.@value = value;
			this.filterOnly = false;
		}

		internal ExtensionCacheValue(object[] value, Type type, MatchType matchType)
		{
			this.@value = value;
			this.type = type;
			this.matchType = matchType;
			this.filterOnly = true;
		}
	}
}