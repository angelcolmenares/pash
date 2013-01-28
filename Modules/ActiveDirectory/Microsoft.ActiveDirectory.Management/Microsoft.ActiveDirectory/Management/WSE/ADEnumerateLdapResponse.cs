using Microsoft.ActiveDirectory.Management;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADEnumerateLdapResponse : ADEnumerateResponse
	{
		public ADEnumerateLdapResponse()
		{
		}

		public ADEnumerateLdapResponse(Message response) : base(response)
		{
		}

		public static ADEnumerateLdapResponse CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADEnumerateLdapResponse>(response);
		}
	}
}