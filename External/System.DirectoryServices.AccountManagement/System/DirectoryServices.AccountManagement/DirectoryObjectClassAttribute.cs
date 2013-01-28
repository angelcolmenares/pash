using System;
using System.Runtime;

namespace System.DirectoryServices.AccountManagement
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
	public sealed class DirectoryObjectClassAttribute : Attribute
	{
		private string objectClass;

		private ContextType? context;

		public ContextType? Context
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.context;
			}
		}

		public string ObjectClass
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.objectClass;
			}
		}

		public DirectoryObjectClassAttribute(string objectClass)
		{
			this.objectClass = objectClass;
			this.context = null;
		}
	}
}