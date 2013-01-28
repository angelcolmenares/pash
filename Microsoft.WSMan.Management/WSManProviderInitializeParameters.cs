using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProviderInitializeParameters
	{
		private string _paramname;

		private string _paramvalue;

		[Parameter(Mandatory=true, Position=0)]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Param")]
		[ValidateNotNullOrEmpty]
		public string ParamName
		{
			get
			{
				return this._paramname;
			}
			set
			{
				this._paramname = value;
			}
		}

		[Parameter(Mandatory=true, Position=1)]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Param")]
		[ValidateNotNullOrEmpty]
		public string ParamValue
		{
			get
			{
				return this._paramvalue;
			}
			set
			{
				this._paramvalue = value;
			}
		}

		public WSManProviderInitializeParameters()
		{
		}
	}
}