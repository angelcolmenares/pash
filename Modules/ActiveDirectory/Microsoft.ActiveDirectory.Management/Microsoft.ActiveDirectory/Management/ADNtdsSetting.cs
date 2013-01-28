using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADNtdsSetting : ADObject
	{
		public ADNtdsSetting()
		{
		}

		public ADNtdsSetting(string identity) : base(identity)
		{
		}

		public ADNtdsSetting(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADNtdsSetting(ADObject identity)
		{
			base.Identity = identity;
		}
	}
}