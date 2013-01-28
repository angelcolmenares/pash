using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryDirectoryServer", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	[KnownType(typeof(ActiveDirectoryDomainController))]
	internal class ActiveDirectoryDirectoryServer : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string DefaultPartitionField;

		private string HostNameField;

		private Guid InvocationIdField;

		private int LdapPortField;

		private string NTDSSettingsObjectDNField;

		private string NameField;

		private ActiveDirectoryOperationMasterRole[] OperationMasterRoleField;

		private string[] PartitionsField;

		private string ServerObjectDNField;

		private Guid ServerObjectGuidField;

		private string SiteField;

		private int SslPortField;

		[DataMember(IsRequired=true)]
		internal string DefaultPartition
		{
			get
			{
				return this.DefaultPartitionField;
			}
			set
			{
				this.DefaultPartitionField = value;
			}
		}

		public ExtensionDataObject ExtensionData
		{
			get
			{
				return this.extensionDataField;
			}
			set
			{
				this.extensionDataField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string HostName
		{
			get
			{
				return this.HostNameField;
			}
			set
			{
				this.HostNameField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal Guid InvocationId
		{
			get
			{
				return this.InvocationIdField;
			}
			set
			{
				this.InvocationIdField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal int LdapPort
		{
			get
			{
				return this.LdapPortField;
			}
			set
			{
				this.LdapPortField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string Name
		{
			get
			{
				return this.NameField;
			}
			set
			{
				this.NameField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string NTDSSettingsObjectDN
		{
			get
			{
				return this.NTDSSettingsObjectDNField;
			}
			set
			{
				this.NTDSSettingsObjectDNField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal ActiveDirectoryOperationMasterRole[] OperationMasterRole
		{
			get
			{
				return this.OperationMasterRoleField;
			}
			set
			{
				this.OperationMasterRoleField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] Partitions
		{
			get
			{
				return this.PartitionsField;
			}
			set
			{
				this.PartitionsField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string ServerObjectDN
		{
			get
			{
				return this.ServerObjectDNField;
			}
			set
			{
				this.ServerObjectDNField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal Guid ServerObjectGuid
		{
			get
			{
				return this.ServerObjectGuidField;
			}
			set
			{
				this.ServerObjectGuidField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string Site
		{
			get
			{
				return this.SiteField;
			}
			set
			{
				this.SiteField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal int SslPort
		{
			get
			{
				return this.SslPortField;
			}
			set
			{
				this.SslPortField = value;
			}
		}

		public ActiveDirectoryDirectoryServer()
		{
		}
	}
}