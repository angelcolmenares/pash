using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADServiceAccountParameterSet : ADParameterSet
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="RestrictedToSingleComputer")]
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
		public string[] Certificates
		{
			get
			{
				return base["Certificates"] as string[];
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
					base["Certificates"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Group")]
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Group")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
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
		public ADServiceAccount Instance
		{
			get
			{
				return base["Instance"] as ADServiceAccount;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Group")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public int? ManagedPasswordIntervalInDays
		{
			get
			{
				return (int?)(base["ManagedPasswordIntervalInDays"] as int?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ManagedPasswordIntervalInDays");
					return;
				}
				else
				{
					base["ManagedPasswordIntervalInDays"] = value;
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Group")]
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

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Group")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADPrincipal[] PrincipalsAllowedToRetrieveManagedPassword
		{
			get
			{
				return base["PrincipalsAllowedToRetrieveManagedPassword"] as ADPrincipal[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("PrincipalsAllowedToRetrieveManagedPassword");
					return;
				}
				else
				{
					base["PrincipalsAllowedToRetrieveManagedPassword"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="RestrictedToOutboundAuthenticationOnly")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "true" })]
		public SwitchParameter RestrictToOutboundAuthenticationOnly
		{
			get
			{
				return base.GetSwitchParameter("RestrictToOutboundAuthenticationOnly");
			}
			set
			{
				base["RestrictToOutboundAuthenticationOnly"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="RestrictedToSingleComputer")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "true" })]
		public SwitchParameter RestrictToSingleComputer
		{
			get
			{
				return base.GetSwitchParameter("RestrictToSingleComputer");
			}
			set
			{
				base["RestrictToSingleComputer"] = value;
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

		public NewADServiceAccountParameterSet()
		{
		}
	}
}