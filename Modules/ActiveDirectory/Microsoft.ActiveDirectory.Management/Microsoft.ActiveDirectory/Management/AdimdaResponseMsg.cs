using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.Management
{
	internal abstract class AdimdaResponseMsg : AdwsResponseMsg
	{
		protected AdimdaResponseMsg()
		{
		}

		protected AdimdaResponseMsg(Message response) : base(response)
		{
		}
	}
}