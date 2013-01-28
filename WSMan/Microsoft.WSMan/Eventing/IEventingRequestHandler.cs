using System;
using System.Linq;
using System.ServiceModel;

namespace Microsoft.WSMan.Eventing
{
   public interface IEventingRequestHandler<T> : IEventingRequestHandler
   {
   }

   public interface IEventingRequestHandler
   {
      void Bind(IEventingRequestHandlerContext context, EndpointAddressBuilder susbcriptionManagerEndpointAddress);
      void Unbind(IEventingRequestHandlerContext context);
   }
}