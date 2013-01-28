using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADClaimTypeParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Identity")]
		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceAttribute")]
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

		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceAttribute")]
		[Parameter(ParameterSetName="Identity")]
		[ValidateNotNull]
		public string[] AppliesToClasses
		{
			get
			{
				return base["AppliesToClasses"] as string[];
			}
			set
			{
				base["AppliesToClasses"] = value;
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

		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceAttribute")]
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

		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="Identity")]
		[Parameter(ParameterSetName="SourceAttribute")]
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
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceAttribute")]
		[Parameter(ParameterSetName="SourceOID")]
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

		[Parameter(ParameterSetName="SourceAttribute")]
		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="Identity")]
		public bool Enabled
		{
			get
			{
				return (bool)base["Enabled"];
			}
			set
			{
				base["Enabled"] = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="SourceAttribute")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="SourceTransformPolicy")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="Identity")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ParameterSetName="SourceOID")]
		[ValidateNotNull]
		public ADClaimType Identity
		{
			get
			{
				return base["Identity"] as ADClaimType;
			}
			set
			{
				base["Identity"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Instance")]
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
		[Parameter(ParameterSetName="SourceAttribute")]
		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
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
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceAttribute")]
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
		[Parameter(ParameterSetName="SourceOID")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceAttribute")]
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
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		[Parameter(ParameterSetName="SourceAttribute")]
		[Parameter(ParameterSetName="SourceOID")]
		public bool RestrictValues
		{
			get
			{
				return (bool)base["RestrictValues"];
			}
			set
			{
				base["RestrictValues"] = value;
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

		[Parameter(Mandatory=true, ParameterSetName="SourceAttribute")]
		public string SourceAttribute
		{
			get
			{
				return base["SourceAttribute"] as string;
			}
			set
			{
				base["SourceAttribute"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="SourceOID")]
		public string SourceOID
		{
			get
			{
				return base["SourceOID"] as string;
			}
			set
			{
				base["SourceOID"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="SourceTransformPolicy")]
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
				base["SourceTransformPolicy"] = value;
			}
		}

		[Parameter(ParameterSetName="SourceAttribute")]
		[Parameter(ParameterSetName="Identity")]
		[Parameter(ParameterSetName="SourceTransformPolicy")]
		public ADSuggestedValueEntry[] SuggestedValues
		{
			get
			{
				return base["SuggestedValues"] as ADSuggestedValueEntry[];
			}
			set
			{
				base["SuggestedValues"] = value;
			}
		}

		public SetADClaimTypeParameterSet()
		{
		}
	}
}