using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.WSMan.Enumeration
{
   public class FilterMapExtensionServiceBehaviorAttribute : Attribute, IServiceBehavior
   {
      public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
      {         
         ServiceHost host = serviceHostBase as ServiceHost;
         if (host == null)
         {
            throw new NotSupportedException("ServiceHost derived host is required.");
         }
         IFilterMapProvider provider = host.SingletonInstance as IFilterMapProvider;
         if (provider == null)
         {
            throw new NotSupportedException(
               "Service must be configured in singleton mode and singleton instance must implement IFilterMapProvider interface.");
         }
      }

      public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
      {
      }

      public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
      {
         ServiceHost host = (ServiceHost) serviceHostBase;
         IFilterMapProvider provider = (IFilterMapProvider)host.SingletonInstance;
         foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
         {
            foreach (EndpointDispatcher endpointDispatcher in dispatcher.Endpoints)
            {
               var inspector = new MessageInspector(provider.ProvideFilterMap());
               endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
            }
         }            
      }

      private class MessageInspector : IDispatchMessageInspector
      {         
         public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
         {
            FilterMapExtension.Activate(_map);
            return null;
         }

         public void BeforeSendReply(ref Message reply, object correlationState)
         {

         }

         public MessageInspector(FilterMap map)
         {
            _map = map;
         }

         private readonly FilterMap _map;
      }
   }
}