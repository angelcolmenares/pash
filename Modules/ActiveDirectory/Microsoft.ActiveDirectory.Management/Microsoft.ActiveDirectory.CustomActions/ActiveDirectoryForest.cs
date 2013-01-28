using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryForest", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class ActiveDirectoryForest : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string[] ApplicationPartitionsField;

		private string[] CrossForestReferencesField;

		private string DomainNamingMasterField;

		private string[] DomainsField;

		private int ForestModeField;

		private string[] GlobalCatalogsField;

		private string NameField;

		private string RootDomainField;

		private string[] SPNSuffixesField;

		private string SchemaMasterField;

		private string[] SitesField;

		private string[] UPNSuffixesField;

		[DataMember(IsRequired=true)]
		internal string[] ApplicationPartitions
		{
			get
			{
				return this.ApplicationPartitionsField;
			}
			set
			{
				this.ApplicationPartitionsField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] CrossForestReferences
		{
			get
			{
				return this.CrossForestReferencesField;
			}
			set
			{
				this.CrossForestReferencesField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string DomainNamingMaster
		{
			get
			{
				return this.DomainNamingMasterField;
			}
			set
			{
				this.DomainNamingMasterField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] Domains
		{
			get
			{
				return this.DomainsField;
			}
			set
			{
				this.DomainsField = value;
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
		internal int ForestMode
		{
			get
			{
				return this.ForestModeField;
			}
			set
			{
				this.ForestModeField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] GlobalCatalogs
		{
			get
			{
				return this.GlobalCatalogsField;
			}
			set
			{
				this.GlobalCatalogsField = value;
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
		internal string RootDomain
		{
			get
			{
				return this.RootDomainField;
			}
			set
			{
				this.RootDomainField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string SchemaMaster
		{
			get
			{
				return this.SchemaMasterField;
			}
			set
			{
				this.SchemaMasterField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] Sites
		{
			get
			{
				return this.SitesField;
			}
			set
			{
				this.SitesField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] SPNSuffixes
		{
			get
			{
				return this.SPNSuffixesField;
			}
			set
			{
				this.SPNSuffixesField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string[] UPNSuffixes
		{
			get
			{
				return this.UPNSuffixesField;
			}
			set
			{
				this.UPNSuffixesField = value;
			}
		}

		public ActiveDirectoryForest()
		{
		}
	}
}