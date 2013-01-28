using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class SearchUtility
	{
		private SearchUtility()
		{
		}

		internal static ADObjectSearcher BuildSearcher(ADSessionInfo session, string searchRoot, ADSearchScope searchScope)
		{
			return SearchUtility.BuildSearcher(session, searchRoot, searchScope, false);
		}

		internal static ADObjectSearcher BuildSearcher(ADSessionInfo session, string searchRoot, ADSearchScope searchScope, bool showDeleted)
		{
			ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(session);
			aDObjectSearcher.PageSize = 0x100;
			aDObjectSearcher.SearchRoot = searchRoot;
			aDObjectSearcher.Scope = searchScope;
			if (showDeleted)
			{
				aDObjectSearcher.ShowDeleted = true;
				aDObjectSearcher.ShowDeactivatedLink = true;
			}
			return aDObjectSearcher;
		}
	}
}