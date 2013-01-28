using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management
{
	internal static class ADServiceStoreAccessFactory
	{
		internal static ADStoreAccess GetObject ()
		{
			if (OSHelper.IsUnix) {
				return ADDirectoryServiceStoreAccess.GetObject ();
			}
			return ADWebServiceStoreAccess.GetObject ();
		}
	}
}

