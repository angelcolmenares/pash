using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IADSyncOperations
	{
		void AbandonSearch(ADSessionHandle handle, ADSearchRequest request);

		ADAddResponse Add(ADSessionHandle handle, ADAddRequest request);

		ADDeleteResponse Delete(ADSessionHandle handle, ADDeleteRequest request);

		ADModifyResponse Modify(ADSessionHandle handle, ADModifyRequest request);

		ADModifyDNResponse ModifyDN(ADSessionHandle handle, ADModifyDNRequest request);

		ADSearchResponse Search(ADSessionHandle handle, ADSearchRequest request);
	}
}