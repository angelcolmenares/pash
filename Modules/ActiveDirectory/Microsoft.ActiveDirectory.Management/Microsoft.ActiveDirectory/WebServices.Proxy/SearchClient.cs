using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class SearchClient : ClientBase<Search>, Search
	{
		public SearchClient()
		{
		}

		public SearchClient(string endpointConfigurationName) : base(endpointConfigurationName)
		{
		}

		public SearchClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public SearchClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public SearchClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public Message Enumerate(Message request)
		{
			return base.Channel.Enumerate(request);
		}

		public Message GetStatus(Message request)
		{
			return base.Channel.GetStatus(request);
		}

		public Message Pull(Message request)
		{
			return base.Channel.Pull(request);
		}

		public Message Release(Message request)
		{
			return base.Channel.Release(request);
		}

		public Message Renew(Message request)
		{
			return base.Channel.Renew(request);
		}
	}
}