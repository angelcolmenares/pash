using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Transfer;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WSMan
{
   public class ClientContext<T> : IDisposable
   {
      private readonly T _channel;
      private readonly OperationContextScope _scope;
	  	
      public ClientContext(Uri endpointUri, AddressingVersion addressingVersion, IChannelFactory<T> proxyFactory, AddressHeaderCreatorDelegate addressHeaderCreatorDelegate)
      {
         EndpointAddressBuilder builder = new EndpointAddressBuilder();
         addressHeaderCreatorDelegate(builder.Headers);
		 builder.Identity = new X509CertificateEndpointIdentity (new X509Certificate2 ("powershell.pfx", "mono"));
		 builder.Uri = endpointUri;
		 var endpoint = builder.ToEndpointAddress();
		 var realProxy = proxyFactory as ChannelFactory<T>;
		 if (!realProxy.Endpoint.Behaviors.Contains (typeof(OperationContextBehavior))) realProxy.Endpoint.Behaviors.Add (new OperationContextBehavior());
         _channel = proxyFactory.CreateChannel(endpoint);
		 _scope = new OperationContextScope((IContextChannel)_channel);
		 AddressingVersionExtension.Activate(addressingVersion);
		 endpoint.Headers.WriteAddressHeaders ();
      }

      public T Channel
      {
         get { return _channel; }
      }

      public void Dispose()
      {
		 /*
         _scope.Dispose();

         ICommunicationObject comm = (ICommunicationObject)_channel;
         if (comm != null)
         {
            try
            {
               if (comm.State != CommunicationState.Faulted)
               {
                  comm.Close();
               }
               else
               {
                  comm.Abort();
               }
            }
            catch (CommunicationException)
            {
               comm.Abort();
            }
            catch (TimeoutException)
            {
               comm.Abort();
            }
            catch (Exception)
            {
               comm.Abort();
               throw;
            }
         }
         */
      }
   }
}