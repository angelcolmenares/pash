using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal interface IADCmdletCache
	{
		void ClearSubcache(string category);

		bool ContainsSubcache(string category);

		object GetSubcache(string category);

		void SetSubcache(string category, object subcache);
	}
}