using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryPartition", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	[KnownType(typeof(ActiveDirectoryDomain))]
	internal class ActiveDirectoryPartition : ActiveDirectoryObject
	{
		private string DNSRootField;

		private string DeletedObjectsContainerField;

		private string LostAndFoundContainerField;

		private string QuotasContainerField;

		private string[] ReadOnlyReplicaDirectoryServerField;

		private string[] ReplicaDirectoryServerField;

		private string[] SubordinateReferencesField;

		[DataMember(IsRequired=true)]
		internal string DeletedObjectsContainer
		{
			get
			{
				return this.DeletedObjectsContainerField;
			}
			set
			{
				this.DeletedObjectsContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string DNSRoot
		{
			get
			{
				return this.DNSRootField;
			}
			set
			{
				this.DNSRootField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string LostAndFoundContainer
		{
			get
			{
				return this.LostAndFoundContainerField;
			}
			set
			{
				this.LostAndFoundContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string QuotasContainer
		{
			get
			{
				return this.QuotasContainerField;
			}
			set
			{
				this.QuotasContainerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] ReadOnlyReplicaDirectoryServer
		{
			get
			{
				return this.ReadOnlyReplicaDirectoryServerField;
			}
			set
			{
				this.ReadOnlyReplicaDirectoryServerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] ReplicaDirectoryServer
		{
			get
			{
				return this.ReplicaDirectoryServerField;
			}
			set
			{
				this.ReplicaDirectoryServerField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] SubordinateReferences
		{
			get
			{
				return this.SubordinateReferencesField;
			}
			set
			{
				this.SubordinateReferencesField = value;
			}
		}

		public ActiveDirectoryPartition()
		{
		}
	}
}