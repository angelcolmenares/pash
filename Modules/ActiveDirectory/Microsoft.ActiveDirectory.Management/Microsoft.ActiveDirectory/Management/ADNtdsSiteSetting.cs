using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADNtdsSiteSetting : ADObject
	{
		public ADNtdsSiteSetting()
		{
		}

		public ADNtdsSiteSetting(string identity) : base(identity)
		{
		}

		public ADNtdsSiteSetting(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADNtdsSiteSetting(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}