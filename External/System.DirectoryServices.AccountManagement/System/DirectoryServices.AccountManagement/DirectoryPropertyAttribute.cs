using System;
using System.Runtime;

namespace System.DirectoryServices.AccountManagement
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
	public sealed class DirectoryPropertyAttribute : Attribute
	{
		private string schemaAttributeName;

		private ContextType? context;

		public ContextType? Context
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.context;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.context = value;
			}
		}

		public string SchemaAttributeName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.schemaAttributeName;
			}
		}

		public DirectoryPropertyAttribute(string schemaAttributeName)
		{
			this.schemaAttributeName = schemaAttributeName;
			this.context = null;
		}
	}
}