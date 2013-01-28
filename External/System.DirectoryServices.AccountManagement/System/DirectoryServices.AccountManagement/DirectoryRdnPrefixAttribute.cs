using System;
using System.Runtime;

namespace System.DirectoryServices.AccountManagement
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public sealed class DirectoryRdnPrefixAttribute : Attribute
	{
		private string rdnPrefix;

		private ContextType? context;

		public ContextType? Context
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.context;
			}
		}

		public string RdnPrefix
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.rdnPrefix;
			}
		}

		public DirectoryRdnPrefixAttribute(string rdnPrefix)
		{
			this.rdnPrefix = rdnPrefix;
			this.context = null;
		}
	}
}