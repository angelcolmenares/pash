using Microsoft.ActiveDirectory.Management;
using System;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADReleaseResponse : AdwsResponseMsg
	{
		protected override string SupportedAction
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/ReleaseResponse";
			}
		}

		protected override bool SupportsEmptyMessage
		{
			get
			{
				return true;
			}
		}

		public ADReleaseResponse()
		{
		}

		public ADReleaseResponse(Message response) : base(response)
		{
		}

		public static ADReleaseResponse CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADReleaseResponse>(response);
		}
	}
}