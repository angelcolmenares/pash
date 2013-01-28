using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class ResourceClient : ClientBase<Resource>, Resource
	{
		public ResourceClient()
		{
		}

		public ResourceClient(string endpointConfigurationName) : base(endpointConfigurationName)
		{
		}

		public ResourceClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public ResourceClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public ResourceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public Message Delete(Message request)
		{
			return base.Channel.Delete(request);
		}

		public Message Get(Message request)
		{
			return base.Channel.Get(request);
		}

		public Message Put(Message request)
		{
			return base.Channel.Put(request);
		}
	}
}