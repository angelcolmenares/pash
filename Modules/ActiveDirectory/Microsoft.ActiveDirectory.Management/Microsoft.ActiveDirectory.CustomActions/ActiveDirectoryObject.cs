using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryObject", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	[KnownType(typeof(ActiveDirectoryPartition))]
	[KnownType(typeof(ActiveDirectoryDomain))]
	[KnownType(typeof(ActiveDirectoryPrincipal))]
	[KnownType(typeof(ActiveDirectoryGroup))]
	internal class ActiveDirectoryObject : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string DistinguishedNameField;

		private string NameField;

		private string ObjectClassField;

		private Guid ObjectGuidField;

		private string[] ObjectTypesField;

		private string ReferenceServerField;

		[DataMember(IsRequired=true)]
		internal string DistinguishedName
		{
			get
			{
				return this.DistinguishedNameField;
			}
			set
			{
				this.DistinguishedNameField = value;
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
		internal string ObjectClass
		{
			get
			{
				return this.ObjectClassField;
			}
			set
			{
				this.ObjectClassField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal Guid ObjectGuid
		{
			get
			{
				return this.ObjectGuidField;
			}
			set
			{
				this.ObjectGuidField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] ObjectTypes
		{
			get
			{
				return this.ObjectTypesField;
			}
			set
			{
				this.ObjectTypesField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string ReferenceServer
		{
			get
			{
				return this.ReferenceServerField;
			}
			set
			{
				this.ReferenceServerField = value;
			}
		}

		public ActiveDirectoryObject()
		{
		}
	}
}