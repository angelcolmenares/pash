using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADComputerParameterSet : ADParameterSet
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
		public string DNSHostName
		{
			get
			{
				return base["DNSHostName"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("DNSHostName");
					return;
				}
				else
				{
					base["DNSHostName"] = value;
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

		[Parameter]
		[ValidateNotNull]
		public ADComputer Instance
		{
			get
			{
				return base["Instance"] as ADComputer;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
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
		public string Location
		{
			get
			{
				return base["Location"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("Location");
					return;
				}
				else
				{
					base["Location"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public ADPrincipal ManagedBy
		{
			get
			{
				return base["ManagedBy"] as ADPrincipal;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ManagedBy");
					return;
				}
				else
				{
					base["ManagedBy"] = value;
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
		public string OperatingSystem
		{
			get
			{
				return base["OperatingSystem"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("OperatingSystem");
					return;
				}
				else
				{
					base["OperatingSystem"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string OperatingSystemHotfix
		{
			get
			{
				return base["OperatingSystemHotfix"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("OperatingSystemHotfix");
					return;
				}
				else
				{
					base["OperatingSystemHotfix"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string OperatingSystemServicePack
		{
			get
			{
				return base["OperatingSystemServicePack"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("OperatingSystemServicePack");
					return;
				}
				else
				{
					base["OperatingSystemServicePack"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string OperatingSystemVersion
		{
			get
			{
				return base["OperatingSystemVersion"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("OperatingSystemVersion");
					return;
				}
				else
				{
					base["OperatingSystemVersion"] = value;
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
		public string SAMAccountName
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

		public NewADComputerParameterSet()
		{
		}
	}
}