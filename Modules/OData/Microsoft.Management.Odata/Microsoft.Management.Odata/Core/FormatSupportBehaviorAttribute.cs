using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Management.Odata.Core
{
	[AttributeUsage(AttributeTargets.Class)]
	internal sealed class FormatSupportBehaviorAttribute : Attribute, IServiceBehavior
	{
		public FormatSupportBehaviorAttribute()
		{
		}

		void System.ServiceModel.Description.IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		void System.ServiceModel.Description.IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher endpoint in serviceHostBase.ChannelDispatchers)
			{
				IEnumerator<EndpointDispatcher> enumerator = endpoint.Endpoints.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						EndpointDispatcher endpointDispatcher = enumerator.Current;
						endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new FormatSupportInspector());
					}
				}
			}
		}

		void System.ServiceModel.Description.IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}