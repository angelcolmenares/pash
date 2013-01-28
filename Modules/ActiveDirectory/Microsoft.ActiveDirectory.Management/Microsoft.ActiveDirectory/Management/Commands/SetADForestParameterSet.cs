using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADForestParameterSet : ADParameterSet
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

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true)]
		[ValidateNotNull]
		public ADForest Identity
		{
			get
			{
				return base["Identity"] as ADForest;
			}
			set
			{
				base["Identity"] = value;
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

		[Parameter]
		[ValidateSetOperationsHashtable(typeof(string))]
		public Hashtable SPNSuffixes
		{
			get
			{
				return base["SPNSuffixes"] as Hashtable;
			}
			set
			{
				base["SPNSuffixes"] = new ADMultivalueHashtableParameter<string>(value);
			}
		}

		[Parameter]
		[ValidateSetOperationsHashtable(typeof(string))]
		public Hashtable UPNSuffixes
		{
			get
			{
				return base["UPNSuffixes"] as Hashtable;
			}
			set
			{
				base["UPNSuffixes"] = new ADMultivalueHashtableParameter<string>(value);
			}
		}

		public SetADForestParameterSet()
		{
		}
	}
}