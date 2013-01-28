using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal delegate ADObjectSearcher IdentityResolverDelegate(object identityObject, string searchRoot, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter);
}