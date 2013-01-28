using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADClaimTypeParameterSet : ADParameterSet
	{
		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNull]
		public string[] AppliesToClasses
		{
			get
			{
				return base["AppliesToClasses"] as string[];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("AppliesToClasses");
					return;
				}
				else
				{
					base["AppliesToClasses"] = value;
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
		public ADClaimType Instance
		{
			get
			{
				return base["Instance"] as ADClaimType;
			}
			set
			{
				base["Instance"] = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool IsSingleValued
		{
			get
			{
				return (bool)base["IsSingleValued"];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("IsSingleValued");
					return;
				}
				else
				{
					base["IsSingleValued"] = value;
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public bool RestrictValues
		{
			get
			{
				return (bool)base["RestrictValues"];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("RestrictValues");
					return;
				}
				else
				{
					base["RestrictValues"] = value;
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="SourceAttribute")]
		public string SourceAttribute
		{
			get
			{
				return base["SourceAttribute"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SourceAttribute");
					return;
				}
				else
				{
					base["SourceAttribute"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="SourceOID")]
		public string SourceOID
		{
			get
			{
				return base["SourceOID"] as string;
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SourceOID");
					return;
				}
				else
				{
					base["SourceOID"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="SourceTransformPolicy")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter SourceTransformPolicy
		{
			get
			{
				return base.GetSwitchParameter("SourceTransformPolicy");
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("SourceTransformPolicy");
					return;
				}
				else
				{
					base["SourceTransformPolicy"] = value;
					return;
				}
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="SourceAttribute")]
		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="SourceTransformPolicy")]
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="SourceTransformPolicy")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public ADClaimValueType ValueType
		{
			get
			{
				return (ADClaimValueType)base["ValueType"];
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("ValueType");
					return;
				}
				else
				{
					base["ValueType"] = value;
					return;
				}
			}
		}

		public NewADClaimTypeParameterSet()
		{
		}
	}
}