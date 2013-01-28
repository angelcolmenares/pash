using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADUserParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
		public DateTime? AccountExpirationDate
		{
			get
			{
				return (DateTime?)(base["AccountExpirationDate"] as DateTime?);
			}
			set
			{
				base["AccountExpirationDate"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? AccountNotDelegated
		{
			get
			{
				return (bool?)(base["AccountNotDelegated"] as bool?);
			}
			set
			{
				base["AccountNotDelegated"] = value;
			}
		}

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

		[Parameter(ParameterSetName="Identity")]
		public bool? AllowReversiblePasswordEncryption
		{
			get
			{
				return (bool?)(base["AllowReversiblePasswordEncryption"] as bool?);
			}
			set
			{
				base["AllowReversiblePasswordEncryption"] = value;
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
		public bool? CannotChangePassword
		{
			get
			{
				return (bool?)(base["CannotChangePassword"] as bool?);
			}
			set
			{
				base["CannotChangePassword"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateSetOperationsHashtable(typeof(X509Certificate))]
		public Hashtable Certificates
		{
			get
			{
				return base["Certificates"] as Hashtable;
			}
			set
			{
				base["Certificates"] = new ADMultivalueHashtableParameter<X509Certificate>(value);
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? ChangePasswordAtLogon
		{
			get
			{
				return (bool?)(base["ChangePasswordAtLogon"] as bool?);
			}
			set
			{
				base["ChangePasswordAtLogon"] = value;
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
		public string Company
		{
			get
			{
				return base["Company"] as string;
			}
			set
			{
				base["Company"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public bool? CompoundIdentitySupported
		{
			get
			{
				return (bool?)(base["CompoundIdentitySupported"] as bool?);
			}
			set
			{
				base["CompoundIdentitySupported"] = value;
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
		public string Department
		{
			get
			{
				return base["Department"] as string;
			}
			set
			{
				base["Department"] = value;
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

		[Parameter(ParameterSetName="Identity")]
		public string Division
		{
			get
			{
				return base["Division"] as string;
			}
			set
			{
				base["Division"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string EmailAddress
		{
			get
			{
				return base["EmailAddress"] as string;
			}
			set
			{
				base["EmailAddress"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string EmployeeID
		{
			get
			{
				return base["EmployeeID"] as string;
			}
			set
			{
				base["EmployeeID"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string EmployeeNumber
		{
			get
			{
				return base["EmployeeNumber"] as string;
			}
			set
			{
				base["EmployeeNumber"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? Enabled
		{
			get
			{
				return (bool?)(base["Enabled"] as bool?);
			}
			set
			{
				base["Enabled"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Fax
		{
			get
			{
				return base["Fax"] as string;
			}
			set
			{
				base["Fax"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string GivenName
		{
			get
			{
				return base["GivenName"] as string;
			}
			set
			{
				base["GivenName"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string HomeDirectory
		{
			get
			{
				return base["HomeDirectory"] as string;
			}
			set
			{
				base["HomeDirectory"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string HomeDrive
		{
			get
			{
				return base["HomeDrive"] as string;
			}
			set
			{
				base["HomeDrive"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string HomePage
		{
			get
			{
				return base["HomePage"] as string;
			}
			set
			{
				base["HomePage"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string HomePhone
		{
			get
			{
				return base["HomePhone"] as string;
			}
			set
			{
				base["HomePhone"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[ValidateNotNull]
		public ADUser Identity
		{
			get
			{
				return base["Identity"] as ADUser;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Initials
		{
			get
			{
				return base["Initials"] as string;
			}
			set
			{
				base["Initials"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
		[ValidateNotNull]
		public ADUser Instance
		{
			get
			{
				return base["Instance"] as ADUser;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADKerberosEncryptionType KerberosEncryptionType
		{
			get
			{
				return (ADKerberosEncryptionType)base["KerberosEncryptionType"];
			}
			set
			{
				base["KerberosEncryptionType"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string LogonWorkstations
		{
			get
			{
				return base["LogonWorkstations"] as string;
			}
			set
			{
				base["LogonWorkstations"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public ADUser Manager
		{
			get
			{
				return base["Manager"] as ADUser;
			}
			set
			{
				base["Manager"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string MobilePhone
		{
			get
			{
				return base["MobilePhone"] as string;
			}
			set
			{
				base["MobilePhone"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Office
		{
			get
			{
				return base["Office"] as string;
			}
			set
			{
				base["Office"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string OfficePhone
		{
			get
			{
				return base["OfficePhone"] as string;
			}
			set
			{
				base["OfficePhone"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Organization
		{
			get
			{
				return base["Organization"] as string;
			}
			set
			{
				base["Organization"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string OtherName
		{
			get
			{
				return base["OtherName"] as string;
			}
			set
			{
				base["OtherName"] = value;
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
		public bool? PasswordNeverExpires
		{
			get
			{
				return (bool?)(base["PasswordNeverExpires"] as bool?);
			}
			set
			{
				base["PasswordNeverExpires"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? PasswordNotRequired
		{
			get
			{
				return (bool?)(base["PasswordNotRequired"] as bool?);
			}
			set
			{
				base["PasswordNotRequired"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string POBox
		{
			get
			{
				return base["POBox"] as string;
			}
			set
			{
				base["POBox"] = value;
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
		public ADPrincipal[] PrincipalsAllowedToDelegateToAccount
		{
			get
			{
				return base["PrincipalsAllowedToDelegateToAccount"] as ADPrincipal[];
			}
			set
			{
				base["PrincipalsAllowedToDelegateToAccount"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string ProfilePath
		{
			get
			{
				return base["ProfilePath"] as string;
			}
			set
			{
				base["ProfilePath"] = value;
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

		[Parameter(ParameterSetName="Identity")]
		[Parameter(ParameterSetName="Instance")]
		public string SamAccountName
		{
			get
			{
				return base["SamAccountName"] as string;
			}
			set
			{
				base["SamAccountName"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string ScriptPath
		{
			get
			{
				return base["ScriptPath"] as string;
			}
			set
			{
				base["ScriptPath"] = value;
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
		[ValidateSetOperationsHashtable(typeof(string))]
		public Hashtable ServicePrincipalNames
		{
			get
			{
				return base["ServicePrincipalNames"] as Hashtable;
			}
			set
			{
				base["ServicePrincipalNames"] = new ADMultivalueHashtableParameter<string>(value);
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? SmartcardLogonRequired
		{
			get
			{
				return (bool?)(base["SmartcardLogonRequired"] as bool?);
			}
			set
			{
				base["SmartcardLogonRequired"] = value;
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

		[Parameter(ParameterSetName="Identity")]
		public string Surname
		{
			get
			{
				return base["Surname"] as string;
			}
			set
			{
				base["Surname"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string Title
		{
			get
			{
				return base["Title"] as string;
			}
			set
			{
				base["Title"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public bool? TrustedForDelegation
		{
			get
			{
				return (bool?)(base["TrustedForDelegation"] as bool?);
			}
			set
			{
				base["TrustedForDelegation"] = value;
			}
		}

		[Parameter(ParameterSetName="Identity")]
		public string UserPrincipalName
		{
			get
			{
				return base["UserPrincipalName"] as string;
			}
			set
			{
				base["UserPrincipalName"] = value;
			}
		}

		public SetADUserParameterSet()
		{
		}
	}
}