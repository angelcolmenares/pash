using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProviderNewItemSecurityParameters
	{
		private string _sddl;

		[Parameter(Mandatory=true)]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Sddl")]
		public string Sddl
		{
			get
			{
				return this._sddl;
			}
			set
			{
				this._sddl = value;
			}
		}

		public WSManProviderNewItemSecurityParameters()
		{
		}
	}
}