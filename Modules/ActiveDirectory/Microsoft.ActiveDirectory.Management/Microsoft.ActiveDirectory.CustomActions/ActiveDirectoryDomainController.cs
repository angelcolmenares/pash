using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryDomainController", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class ActiveDirectoryDomainController : ActiveDirectoryDirectoryServer
	{
		private string ComputerDNField;

		private string DomainField;

		private bool EnabledField;

		private string ForestField;

		private bool IsGlobalCatalogField;

		private bool IsReadOnlyField;

		private string OSHotFixField;

		private string OSNameField;

		private string OSServicepackField;

		private string OSVersionField;

		[DataMember(IsRequired=true)]
		internal string ComputerDN
		{
			get
			{
				return this.ComputerDNField;
			}
			set
			{
				this.ComputerDNField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string Domain
		{
			get
			{
				return this.DomainField;
			}
			set
			{
				this.DomainField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal bool Enabled
		{
			get
			{
				return this.EnabledField;
			}
			set
			{
				this.EnabledField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string Forest
		{
			get
			{
				return this.ForestField;
			}
			set
			{
				this.ForestField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal bool IsGlobalCatalog
		{
			get
			{
				return this.IsGlobalCatalogField;
			}
			set
			{
				this.IsGlobalCatalogField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal bool IsReadOnly
		{
			get
			{
				return this.IsReadOnlyField;
			}
			set
			{
				this.IsReadOnlyField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string OSHotFix
		{
			get
			{
				return this.OSHotFixField;
			}
			set
			{
				this.OSHotFixField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string OSName
		{
			get
			{
				return this.OSNameField;
			}
			set
			{
				this.OSNameField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string OSServicepack
		{
			get
			{
				return this.OSServicepackField;
			}
			set
			{
				this.OSServicepackField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal string OSVersion
		{
			get
			{
				return this.OSVersionField;
			}
			set
			{
				this.OSVersionField = value;
			}
		}

		public ActiveDirectoryDomainController()
		{
		}
	}
}