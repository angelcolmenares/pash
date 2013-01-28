using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan
{
	public static class MessageHeadersExtensions
	{
		public static void WriteAddressHeaders (this AddressHeaderCollection col)
		{
			var headers = OperationContext.Current.OutgoingMessageHeaders;
			foreach (var header in col) {
				headers.Add(new AddressMessageHeader(header));
			}
		}
	}
}

