using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IADServiceStoreAccess : IADSession, IADSyncOperations, IADAccountManagement, IADTopologyManagement
	{

	}
}

