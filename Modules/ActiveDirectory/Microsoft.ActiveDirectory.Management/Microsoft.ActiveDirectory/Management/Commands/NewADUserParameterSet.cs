using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADUserParameterSet : ADParameterSet
	{
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public DateTime? AccountExpirationDate
		{
			get
			{
				return (DateTime?)(base["AccountExpirationDate"] as DateTime?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AccountExpirationDate");
					return;
				}
				else
				{
					base["AccountExpirationDate"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? AccountNotDelegated
		{
			get
			{
				return (bool?)(base["AccountNotDelegated"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AccountNotDelegated");
					return;
				}
				else
				{
					base["AccountNotDelegated"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public SecureString AccountPassword
		{
			get
			{
				return base["AccountPassword"] as SecureString;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AccountPassword");
					return;
				}
				else
				{
					base["AccountPassword"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? AllowReversiblePasswordEncryption
		{
			get
			{
				return (bool?)(base["AllowReversiblePasswordEncryption"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AllowReversiblePasswordEncryption");
					return;
				}
				else
				{
					base["AllowReversiblePasswordEncryption"] = value;
					return;
				}
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? CannotChangePassword
		{
			get
			{
				return (bool?)(base["CannotChangePassword"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("CannotChangePassword");
					return;
				}
				else
				{
					base["CannotChangePassword"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateMultivalueArray]
		public X509Certificate[] Certificates
		{
			get
			{
				return base["Certificates"] as X509Certificate[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Certificates");
					return;
				}
				else
				{
					base["Certificates"] = new ADMultivalueArrayParameter<X509Certificate>(value);
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? ChangePasswordAtLogon
		{
			get
			{
				return (bool?)(base["ChangePasswordAtLogon"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ChangePasswordAtLogon");
					return;
				}
				else
				{
					base["ChangePasswordAtLogon"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string City
		{
			get
			{
				return base["City"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("City");
					return;
				}
				else
				{
					base["City"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Company
		{
			get
			{
				return base["Company"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Company");
					return;
				}
				else
				{
					base["Company"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
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
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("CompoundIdentitySupported");
					return;
				}
				else
				{
					base["CompoundIdentitySupported"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Country
		{
			get
			{
				return base["Country"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Country");
					return;
				}
				else
				{
					base["Country"] = value;
					return;
				}
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Department
		{
			get
			{
				return base["Department"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Department");
					return;
				}
				else
				{
					base["Department"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Description
		{
			get
			{
				return base["Description"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Description");
					return;
				}
				else
				{
					base["Description"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string DisplayName
		{
			get
			{
				return base["DisplayName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("DisplayName");
					return;
				}
				else
				{
					base["DisplayName"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Division
		{
			get
			{
				return base["Division"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Division");
					return;
				}
				else
				{
					base["Division"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string EmailAddress
		{
			get
			{
				return base["EmailAddress"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("EmailAddress");
					return;
				}
				else
				{
					base["EmailAddress"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string EmployeeID
		{
			get
			{
				return base["EmployeeID"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("EmployeeID");
					return;
				}
				else
				{
					base["EmployeeID"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string EmployeeNumber
		{
			get
			{
				return base["EmployeeNumber"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("EmployeeNumber");
					return;
				}
				else
				{
					base["EmployeeNumber"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? Enabled
		{
			get
			{
				return (bool?)(base["Enabled"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Enabled");
					return;
				}
				else
				{
					base["Enabled"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Fax
		{
			get
			{
				return base["Fax"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Fax");
					return;
				}
				else
				{
					base["Fax"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string GivenName
		{
			get
			{
				return base["GivenName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("GivenName");
					return;
				}
				else
				{
					base["GivenName"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string HomeDirectory
		{
			get
			{
				return base["HomeDirectory"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("HomeDirectory");
					return;
				}
				else
				{
					base["HomeDirectory"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string HomeDrive
		{
			get
			{
				return base["HomeDrive"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("HomeDrive");
					return;
				}
				else
				{
					base["HomeDrive"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string HomePage
		{
			get
			{
				return base["HomePage"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("HomePage");
					return;
				}
				else
				{
					base["HomePage"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string HomePhone
		{
			get
			{
				return base["HomePhone"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("HomePhone");
					return;
				}
				else
				{
					base["HomePhone"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Initials
		{
			get
			{
				return base["Initials"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Initials");
					return;
				}
				else
				{
					base["Initials"] = value;
					return;
				}
			}
		}

		[Parameter]
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public ADKerberosEncryptionType KerberosEncryptionType
		{
			get
			{
				return (ADKerberosEncryptionType)base["KerberosEncryptionType"];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("KerberosEncryptionType");
					return;
				}
				else
				{
					base["KerberosEncryptionType"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string LogonWorkstations
		{
			get
			{
				return base["LogonWorkstations"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("LogonWorkstations");
					return;
				}
				else
				{
					base["LogonWorkstations"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public ADUser Manager
		{
			get
			{
				return base["Manager"] as ADUser;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Manager");
					return;
				}
				else
				{
					base["Manager"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string MobilePhone
		{
			get
			{
				return base["MobilePhone"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("MobilePhone");
					return;
				}
				else
				{
					base["MobilePhone"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get
			{
				return base["Name"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Name");
					return;
				}
				else
				{
					base["Name"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Office
		{
			get
			{
				return base["Office"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Office");
					return;
				}
				else
				{
					base["Office"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string OfficePhone
		{
			get
			{
				return base["OfficePhone"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("OfficePhone");
					return;
				}
				else
				{
					base["OfficePhone"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Organization
		{
			get
			{
				return base["Organization"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Organization");
					return;
				}
				else
				{
					base["Organization"] = value;
					return;
				}
			}
		}

		[Parameter]
		[ValidateAttributeValueHashtable]
		[ValidateNotNullOrEmpty]
		public Hashtable OtherAttributes
		{
			get
			{
				return base["OtherAttributes"] as Hashtable;
			}
			set
			{
				base["OtherAttributes"] = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string OtherName
		{
			get
			{
				return base["OtherName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("OtherName");
					return;
				}
				else
				{
					base["OtherName"] = value;
					return;
				}
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? PasswordNeverExpires
		{
			get
			{
				return (bool?)(base["PasswordNeverExpires"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("PasswordNeverExpires");
					return;
				}
				else
				{
					base["PasswordNeverExpires"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? PasswordNotRequired
		{
			get
			{
				return (bool?)(base["PasswordNotRequired"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("PasswordNotRequired");
					return;
				}
				else
				{
					base["PasswordNotRequired"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get
			{
				return base["Path"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Path");
					return;
				}
				else
				{
					base["Path"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string POBox
		{
			get
			{
				return base["POBox"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("POBox");
					return;
				}
				else
				{
					base["POBox"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string PostalCode
		{
			get
			{
				return base["PostalCode"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("PostalCode");
					return;
				}
				else
				{
					base["PostalCode"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADPrincipal[] PrincipalsAllowedToDelegateToAccount
		{
			get
			{
				return base["PrincipalsAllowedToDelegateToAccount"] as ADPrincipal[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("PrincipalsAllowedToDelegateToAccount");
					return;
				}
				else
				{
					base["PrincipalsAllowedToDelegateToAccount"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string ProfilePath
		{
			get
			{
				return base["ProfilePath"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ProfilePath");
					return;
				}
				else
				{
					base["ProfilePath"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string SamAccountName
		{
			get
			{
				return base["SamAccountName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SamAccountName");
					return;
				}
				else
				{
					base["SamAccountName"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string ScriptPath
		{
			get
			{
				return base["ScriptPath"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ScriptPath");
					return;
				}
				else
				{
					base["ScriptPath"] = value;
					return;
				}
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateMultivalueArray]
		public string[] ServicePrincipalNames
		{
			get
			{
				return base["ServicePrincipalNames"] as string[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ServicePrincipalNames");
					return;
				}
				else
				{
					base["ServicePrincipalNames"] = new ADMultivalueArrayParameter<string>(value);
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? SmartcardLogonRequired
		{
			get
			{
				return (bool?)(base["SmartcardLogonRequired"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SmartcardLogonRequired");
					return;
				}
				else
				{
					base["SmartcardLogonRequired"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string State
		{
			get
			{
				return base["State"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("State");
					return;
				}
				else
				{
					base["State"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string StreetAddress
		{
			get
			{
				return base["StreetAddress"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("StreetAddress");
					return;
				}
				else
				{
					base["StreetAddress"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Surname
		{
			get
			{
				return base["Surname"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Surname");
					return;
				}
				else
				{
					base["Surname"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Title
		{
			get
			{
				return base["Title"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Title");
					return;
				}
				else
				{
					base["Title"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool? TrustedForDelegation
		{
			get
			{
				return (bool?)(base["TrustedForDelegation"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("TrustedForDelegation");
					return;
				}
				else
				{
					base["TrustedForDelegation"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Type
		{
			get
			{
				return base["Type"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Type");
					return;
				}
				else
				{
					base["Type"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string UserPrincipalName
		{
			get
			{
				return base["UserPrincipalName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("UserPrincipalName");
					return;
				}
				else
				{
					base["UserPrincipalName"] = value;
					return;
				}
			}
		}

		public NewADUserParameterSet()
		{
		}
	}
}