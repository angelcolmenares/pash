using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryGroup", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class ActiveDirectoryGroup : ActiveDirectoryPrincipal
	{
		private ActiveDirectoryGroupScope GroupScopeField;

		private ActiveDirectoryGroupType GroupTypeField;

		[DataMember(IsRequired=true)]
		internal ActiveDirectoryGroupScope GroupScope
		{
			get
			{
				return this.GroupScopeField;
			}
			set
			{
				this.GroupScopeField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal ActiveDirectoryGroupType GroupType
		{
			get
			{
				return this.GroupTypeField;
			}
			set
			{
				this.GroupTypeField = value;
			}
		}

		public ActiveDirectoryGroup()
		{
		}
	}
}