using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class ResourceFactoryClient : ClientBase<ResourceFactory>, ResourceFactory
	{
		public ResourceFactoryClient()
		{
		}

		public ResourceFactoryClient(string endpointConfigurationName) : base(endpointConfigurationName)
		{
		}

		public ResourceFactoryClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public ResourceFactoryClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public ResourceFactoryClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public Message Create(Message request)
		{
			return base.Channel.Create(request);
		}
	}
}