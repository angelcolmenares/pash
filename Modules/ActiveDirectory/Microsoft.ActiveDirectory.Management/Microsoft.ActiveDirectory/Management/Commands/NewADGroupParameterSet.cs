using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class NewADGroupParameterSet : ADParameterSet
	{
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
		[ValidateNotNull]
		public ADGroupCategory? GroupCategory
		{
			get
			{
				return (ADGroupCategory?)(base["GroupCategory"] as ADGroupCategory?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("GroupCategory");
					return;
				}
				else
				{
					base["GroupCategory"] = value;
					return;
				}
			}
		}

		[Parameter(Mandatory=true, Position=2, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNull]
		public ADGroupScope? GroupScope
		{
			get
			{
				return (ADGroupScope?)(base["GroupScope"] as ADGroupScope?);
			}
			set
			{
				if (ADParameterUtil.ShouldIgnorePipelineValue(value))
				{
					base.RemoveParameter("GroupScope");
					return;
				}
				else
				{
					base["GroupScope"] = value;
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
		public ADGroup Instance
		{
			get
			{
				return base["Instance"] as ADGroup;
			}
			set
			{
				base["Instance"] = value;
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

		public NewADGroupParameterSet()
		{
		}
	}
}