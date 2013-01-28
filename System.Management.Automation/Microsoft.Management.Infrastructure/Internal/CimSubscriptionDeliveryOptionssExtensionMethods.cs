using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class CimSubscriptionDeliveryOptionssExtensionMethods
	{
		internal static SubscriptionDeliveryOptionsHandle GetSubscriptionDeliveryOptionsHandle(this CimSubscriptionDeliveryOptions deliveryOptions)
		{
			if (deliveryOptions != null)
			{
				return deliveryOptions.SubscriptionDeliveryOptionsHandle;
			}
			else
			{
				return null;
			}
		}
	}
}