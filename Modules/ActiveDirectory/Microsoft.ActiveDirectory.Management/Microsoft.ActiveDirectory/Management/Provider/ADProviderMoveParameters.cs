using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderMoveParameters : ADProviderCommonParameters
	{
		private string _crossDomain;

		[Parameter]
		public string CrossDomain
		{
			get
			{
				return this._crossDomain;
			}
			set
			{
				this._crossDomain = value;
			}
		}

		public ADProviderMoveParameters()
		{
		}
	}
}