using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	public class WSManProviderNewItemResourceParameters
	{
		private Uri _resourceuri;

		private object[] _capability;

		[Parameter(Mandatory=true)]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[ValidateNotNullOrEmpty]
		public object[] Capability
		{
			get
			{
				return this._capability;
			}
			set
			{
				this._capability = value;
			}
		}

		[Parameter(Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public Uri ResourceUri
		{
			get
			{
				return this._resourceuri;
			}
			set
			{
				this._resourceuri = value;
			}
		}

		public WSManProviderNewItemResourceParameters()
		{
		}
	}
}