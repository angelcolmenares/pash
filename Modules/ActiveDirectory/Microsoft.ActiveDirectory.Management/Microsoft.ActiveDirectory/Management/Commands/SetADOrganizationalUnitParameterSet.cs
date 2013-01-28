using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADOrganizationalUnitParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Add
		{
			get
			{
				return base["Add"] as Hashtable;
			}
			set
			{
				base["Add"] = value;
			}
		}

		[Parameter]
		public ADAuthType AuthType
		{
			get
			{
				return (ADAuthType)base["AuthType"];
			}
			set
			{
				base["AuthType"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string City
		{
			get
			{
				return base["City"] as string;
			}
			set
			{
				base["City"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNullOrEmpty]
		public string[] Clear
		{
			get
			{
				return base["Clear"] as string[];
			}
			set
			{
				base["Clear"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Country
		{
			get
			{
				return base["Country"] as string;
			}
			set
			{
				base["Country"] = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return base["Credential"] as PSCredential;
			}
			set
			{
				base["Credential"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Description
		{
			get
			{
				return base["Description"] as string;
			}
			set
			{
				base["Description"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string DisplayName
		{
			get
			{
				return base["DisplayName"] as string;
			}
			set
			{
				base["DisplayName"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		public ADOrganizationalUnit Identity
		{
			get
			{
				return base["Identity"] as ADOrganizationalUnit;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADOrganizationalUnit Instance
		{
			get
			{
				return base["Instance"] as ADOrganizationalUnit;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADPrincipal ManagedBy
		{
			get
			{
				return base["ManagedBy"] as ADPrincipal;
			}
			set
			{
				base["ManagedBy"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNullOrEmpty]
		public string Partition
		{
			get
			{
				return base["Partition"] as string;
			}
			set
			{
				base["Partition"] = value;
			}
		}

		[Parameter]
		[ValidateNotNull]
		public SwitchParameter PassThru
		{
			get
			{
				return base.GetSwitchParameter("PassThru");
			}
			set
			{
				base["PassThru"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string PostalCode
		{
			get
			{
				return base["PostalCode"] as string;
			}
			set
			{
				base["PostalCode"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? ProtectedFromAccidentalDeletion
		{
			get
			{
				return (bool?)(base["ProtectedFromAccidentalDeletion"] as bool?);
			}
			set
			{
				base["ProtectedFromAccidentalDeletion"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Remove
		{
			get
			{
				return base["Remove"] as Hashtable;
			}
			set
			{
				base["Remove"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable Replace
		{
			get
			{
				return base["Replace"] as Hashtable;
			}
			set
			{
				base["Replace"] = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return base["Server"] as string;
			}
			set
			{
				base["Server"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string State
		{
			get
			{
				return base["State"] as string;
			}
			set
			{
				base["State"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string StreetAddress
		{
			get
			{
				return base["StreetAddress"] as string;
			}
			set
			{
				base["StreetAddress"] = value;
			}
		}

		public SetADOrganizationalUnitParameterSet()
		{
		}
	}
}