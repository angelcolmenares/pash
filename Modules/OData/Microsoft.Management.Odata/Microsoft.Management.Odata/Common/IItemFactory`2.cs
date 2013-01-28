using System;

namespace Microsoft.Management.Odata.Common
{
	internal interface IItemFactory<TItem, TUserId>
	{
		TItem Create(TUserId userId, string membershipId);
	}
}