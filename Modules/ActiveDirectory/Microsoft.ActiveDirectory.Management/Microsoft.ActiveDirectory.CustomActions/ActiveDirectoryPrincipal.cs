using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryPrincipal", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	[KnownType(typeof(ActiveDirectoryGroup))]
	internal class ActiveDirectoryPrincipal : ActiveDirectoryObject
	{
		private byte[] SIDField;

		private string SamAccountNameField;

		[DataMember(IsRequired=true)]
		internal string SamAccountName
		{
			get
			{
				return this.SamAccountNameField;
			}
			set
			{
				this.SamAccountNameField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal byte[] SID
		{
			get
			{
				return this.SIDField;
			}
			set
			{
				this.SIDField = value;
			}
		}

		public ActiveDirectoryPrincipal()
		{
		}
	}
}