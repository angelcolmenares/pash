using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADResourcePropertyParameterSet : ADParameterSet
	{
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string[] AppliesToResourceTypes
		{
			get
			{
				return base["AppliesToResourceTypes"] as string[];
			}
			set
			{
				base["AppliesToResourceTypes"] = value;
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true)]
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
		public bool Enabled
		{
			get
			{
				return (bool)base["Enabled"];
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
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string ID
		{
			get
			{
				return base["ID"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ID");
					return;
				}
				else
				{
					base["ID"] = value;
					return;
				}
			}
		}

		[Parameter]
		[ValidateNotNull]
		public ADResourceProperty Instance
		{
			get
			{
				return base["Instance"] as ADResourceProperty;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool IsSecured
		{
			get
			{
				return (bool)base["IsSecured"];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("IsSecured");
					return;
				}
				else
				{
					base["IsSecured"] = value;
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
		public bool? ProtectedFromAccidentalDeletion
		{
			get
			{
				return (bool?)(base["ProtectedFromAccidentalDeletion"] as bool?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ProtectedFromAccidentalDeletion");
					return;
				}
				else
				{
					base["ProtectedFromAccidentalDeletion"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADResourcePropertyValueType ResourcePropertyValueType
		{
			get
			{
				return base["ResourcePropertyValueType"] as ADResourcePropertyValueType;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ResourcePropertyValueType");
					return;
				}
				else
				{
					base["ResourcePropertyValueType"] = value;
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
		public ADClaimType SharesValuesWith
		{
			get
			{
				return base["SharesValuesWith"] as ADClaimType;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SharesValuesWith");
					return;
				}
				else
				{
					base["SharesValuesWith"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public ADSuggestedValueEntry[] SuggestedValues
		{
			get
			{
				return base["SuggestedValues"] as ADSuggestedValueEntry[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SuggestedValues");
					return;
				}
				else
				{
					base["SuggestedValues"] = value;
					return;
				}
			}
		}

		public NewADResourcePropertyParameterSet()
		{
		}
	}
}