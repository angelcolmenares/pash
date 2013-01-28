using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class GetADDCCloningExcludedApplicationListParameterSet : ADParameterSet
	{
		[Parameter(ParameterSetName="Xml")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter Force
		{
			get
			{
				return base.GetSwitchParameter("Force");
			}
			set
			{
				base["Force"] = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Xml")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public SwitchParameter GenerateXml
		{
			get
			{
				return base.GetSwitchParameter("GenerateXml");
			}
			set
			{
				base["GenerateXml"] = value;
			}
		}

		[Parameter(ParameterSetName="Xml")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get
			{
				return base["Path"] as string;
			}
			set
			{
				base["Path"] = value;
			}
		}

		public GetADDCCloningExcludedApplicationListParameterSet()
		{
		}
	}
}