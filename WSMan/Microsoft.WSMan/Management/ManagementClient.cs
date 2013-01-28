using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Transfer;

namespace Microsoft.WSMan.Management
{
   public class ManagementClient
   {      
      public T Get<T>(string resourceUri, string fragmentTransferExpression, params Selector[] selectors)
      {
         return Get<T>(resourceUri, fragmentTransferExpression, (IEnumerable<Selector>)selectors);
      }

      public T Get<T>(string resourceUri, string fragmentTransferExpression, IEnumerable<Selector> selectors)
      {
         return _transferClient.Get<T>(x =>
                                          {
                                             x.Add(new SelectorSetHeader(selectors));
                                             x.Add(new ResourceUriHeader(resourceUri));
                                          }, 
                                       x => x.Add(new FragmentTransferHeader(fragmentTransferExpression)));
      }

      public T Put<T>(string resourceUri, string fragmentTransferExpression, object payload, params Selector[] selectors)
      {
         return Put<T>(resourceUri, fragmentTransferExpression, payload, (IEnumerable<Selector>) selectors);
      }

      public T Put<T>(string resourceUri, string fragmentTransferExpression, object payload, IEnumerable<Selector> selectors)
      {
         return _transferClient.Put<T>(x =>
                                          {
                                             x.Add(new SelectorSetHeader(selectors));
                                             x.Add(new ResourceUriHeader(resourceUri));
                                          },
                                       x => x.Add(new FragmentTransferHeader(fragmentTransferExpression)),payload);
      }

      public EndpointAddress Create(string resourceUri, object payload)
      {
         return _transferClient.Create(x => x.Add(new ResourceUriHeader(resourceUri)),
                                       x => {}, payload);
      }

      public void Delete(string resourceUri, params Selector[] selectors)
      {
         Delete(resourceUri, (IEnumerable<Selector>)selectors);
      }

      public void Delete(string resourceUri, IEnumerable<Selector> selectors)
      {
         _transferClient.Delete(x =>
                                   {
                                      x.Add(new SelectorSetHeader(selectors));
                                      x.Add(new ResourceUriHeader(resourceUri));
                                   }, x =>{});
      }

      public ManagementClient(Uri endpointUri, IChannelFactory<IWSTransferContract> proxyFactory, MessageVersion version, IWSTransferFaultHandler faultHandler)
      {
         _transferClient = new TransferClient(endpointUri, proxyFactory, version, faultHandler);
      }

	  public string UserName {
			get; internal set;
	  }

      public ManagementClient(Uri endpointUri, IChannelFactory<IWSTransferContract> proxyFactory, MessageVersion version)
         : this(endpointUri, proxyFactory, version, new ManagementWSTransferFaultHandler())
      {         
      }


      private readonly TransferClient _transferClient;

   }
}