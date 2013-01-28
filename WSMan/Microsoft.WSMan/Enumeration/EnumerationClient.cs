using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationClient : IDisposable
   {      
      public void BindFilterDialect(string dialect, Type implementationType)
      {
         _filterMap.Bind(dialect, implementationType);
      }

      public IEnumerable<EndpointAddress> EnumerateEPR(Uri resourceUri, Filter filter, int maxElements, params Selector[] selectors)
      {
         return EnumerateEPR(resourceUri, filter, maxElements, (IEnumerable<Selector>)selectors);
      }

      public int EstimateCount(Uri resourceUri, Filter filter, params Selector[] selectors)
      {
         return EstimateCount(resourceUri, filter, (IEnumerable<Selector>) selectors);
      }

      public int EstimateCount(Uri resourceUri, Filter filter, IEnumerable<Selector> selectors)
      {         
         using (ClientContext<IWSEnumerationContract> ctx =
            new ClientContext<IWSEnumerationContract>(_endpointUri, _binding.MessageVersion.Addressing, _proxyFactory,
               mx =>
               {                  
                  mx.Add(new ResourceUriHeader(resourceUri.ToString()));
                  mx.Add(new SelectorSetHeader(selectors));
               }))
         {
            OperationContextProxy.Current.AddHeader(new RequestTotalItemsCountEstimate());
            FilterMapExtension.Activate(_filterMap);
            EnumerationModeExtension.Activate(EnumerationMode.EnumerateEPR, null);
			
            ctx.Channel.Enumerate(new EnumerateRequest
            {
               EnumerationMode = EnumerationMode.EnumerateEPR,
               OptimizeEnumeration = _optimize ? new OptimizeEnumeration() : null,
               Filter = filter,
            });
			
            TotalItemsCountEstimate totalItemsCountEstimate =
               OperationContextProxy.Current.FindHeader<TotalItemsCountEstimate>();               
            return totalItemsCountEstimate.Value;
         }
      }

      public IEnumerable<EndpointAddress> EnumerateEPR(Uri resourceUri, Filter filter, int maxElements, IEnumerable<Selector> selectors)
      {
         EnumerateResponse response;
         using (ClientContext<IWSEnumerationContract> ctx = 
            new ClientContext<IWSEnumerationContract>(_endpointUri, _binding.MessageVersion.Addressing,  _proxyFactory,
               mx =>
                  {
                     mx.Add(new ResourceUriHeader(resourceUri.ToString()));
                     mx.Add(new SelectorSetHeader(selectors));
                  }))
         {
            FilterMapExtension.Activate(_filterMap);
            EnumerationModeExtension.Activate(EnumerationMode.EnumerateEPR, null);
            response = ctx.Channel.Enumerate(new EnumerateRequest
                                     {
                                        EnumerationMode = EnumerationMode.EnumerateEPR,
                                        OptimizeEnumeration = _optimize ? new OptimizeEnumeration() : null,
                                        Filter = filter,
                                        MaxElements = new MaxElements(maxElements)
                                     });            
         }
         if (response.Items != null)
         {
            foreach (EnumerationItem item in response.Items.Items)
            {
               yield return item.EprValue;
            }
         }
         EnumerationContextKey context = response.EnumerationContext;
         bool endOfSequence = response.EndOfSequence != null;
         while (!endOfSequence)
         {
            PullResponse pullResponse = PullNextEPRBatch(context, resourceUri.ToString(), maxElements, selectors);            
            foreach (EnumerationItem item in pullResponse.Items.Items)
            {
               yield return item.EprValue;
            }
            endOfSequence = pullResponse.EndOfSequence != null;
            context = pullResponse.EnumerationContext;
         }
      }

      private PullResponse PullNextEPRBatch(EnumerationContextKey context, string resourceUri, int maxElements, IEnumerable<Selector> selectors)
      {
         using (ClientContext<IWSEnumerationContract> ctx =
            new ClientContext<IWSEnumerationContract>(_endpointUri, _binding.MessageVersion.Addressing, _proxyFactory,
               mx =>
                  {
                     mx.Add(new ResourceUriHeader(resourceUri));
                     mx.Add(new SelectorSetHeader(selectors));
                  }))
         {
            FilterMapExtension.Activate(_filterMap);
            EnumerationModeExtension.Activate(EnumerationMode.EnumerateEPR, null);
            return ctx.Channel.Pull(new PullRequest
            {
               EnumerationContext = context,               
               MaxElements = new MaxElements(maxElements)
            });
         }
      }
      
      public EnumerationClient(bool optimize, Uri endpointUri, ChannelFactory<IWSEnumerationContract> proxyFactory)
      {
         _endpointUri = endpointUri;         
         _optimize = optimize;
		 _binding = proxyFactory.Endpoint.Binding;
		_proxyFactory = proxyFactory;
      }

      public void Dispose()
      {
         if (_disposed)
         {
            return;
         }
         try
         {
            _proxyFactory.Close();
         }
         catch (Exception)
         {
            _proxyFactory.Abort();
         }
         _disposed = true;
      }

      private bool _disposed;
      private readonly Uri _endpointUri;      
      private readonly bool _optimize;
      private readonly IChannelFactory<IWSEnumerationContract> _proxyFactory;
      private readonly Binding _binding;
      private readonly FilterMap _filterMap = new FilterMap();
   }
}