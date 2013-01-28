using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IADOPathNode
	{
		string GetLdapFilterString();
	}
}