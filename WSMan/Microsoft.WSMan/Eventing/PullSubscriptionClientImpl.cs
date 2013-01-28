using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Eventing
{
   public class PullSubscriptionClientImpl<T> : IPullSubscriptionClient<T>
   {
      public IEnumerable<T> PullOnce()
      {
         PullResponse pullResponse = PullNextBatch(_context, 100, new Selector[] {});
         _context = pullResponse.EnumerationContext;
         if (pullResponse.Items == null)
         {
            return new T[] {};
         }
         return pullResponse.Items.Items.Select(x => x.ObjectValue).Cast<T>();         
      }      

      public IEnumerable<T> Pull()
      {
         bool endOfSequence = false;
         while (!endOfSequence)
         {
            PullResponse pullResponse = PullNextBatch(_context, 100, new Selector[] { });
            if (pullResponse.Items != null)
            {
               foreach (EnumerationItem item in pullResponse.Items.Items)
               {
                  yield return (T) item.ObjectValue;
               }
            }
            endOfSequence = pullResponse.EndOfSequence != null;
            _context = pullResponse.EnumerationContext;
         }
      }

      private PullResponse PullNextBatch(EnumerationContextKey context, int maxElements, IEnumerable<Selector> selectors)
      {
         using (ClientContext<IWSEventingPullDeliveryContract> ctx =
            new ClientContext<IWSEventingPullDeliveryContract>(_endpointUri, _binding.MessageVersion.Addressing, _enumerationProxyFactory,
               mx =>
                  {
                     mx.Add(new ResourceUriHeader(_resourceUri));
                     mx.Add(new SelectorSetHeader(selectors));
                  }))
         {
            FilterMapExtension.Activate(_filterMap);
            EnumerationModeExtension.Activate(EnumerationMode.EnumerateObjectAndEPR, typeof(T));
            try
            {
               return ctx.Channel.Pull(new PullRequest
               {
                  MaxTime = new MaxTime(TimeSpan.FromSeconds(10)),
                  EnumerationContext = context,
                  MaxElements = new MaxElements(maxElements)
               });
            }
            catch (FaultException ex)
            {
               if (ex.IsA(Faults.TimedOut))
               {
                  return new PullResponse {EnumerationContext = context};
               }
               throw;
            }            
         }
      }

      private void Unsubscribe()
      {
         using (ClientContext<IWSEventingContract> ctx =
            new ClientContext<IWSEventingContract>(_endpointUri, _binding.MessageVersion.Addressing, _eventingProxyFactory, 
               mx =>
                  {
                     mx.Add(new ResourceUriHeader(_resourceUri));
                     mx.Add(new IdentifierHeader(_context.Text));
                  }))
         {
            ctx.Channel.Unsubscribe(new UnsubscribeRequest());
         }
      }

      public void Dispose()
      {
         if (_disposed)
         {
            return;
         }
         try
         {
            Unsubscribe();
            _enumerationProxyFactory.Close();
         }
         catch (Exception)
         {
            _enumerationProxyFactory.Abort();
         }
         _disposed = true;
      }

      public PullSubscriptionClientImpl(Uri endpointUri, Binding binding, FilterMap filterMap, 
         EnumerationContextKey context, string resourceUri,
         //IChannelFactory<IWSEnumerationContract> enumerationProxyFactory, 
         IChannelFactory<IWSEventingContract> eventingProxyFactory)
      {
         _endpointUri = endpointUri;
         _resourceUri = resourceUri;
         _eventingProxyFactory = eventingProxyFactory;
         _context = context;
         _filterMap = filterMap;
         _binding = binding;
         _enumerationProxyFactory = new ChannelFactory<IWSEventingPullDeliveryContract>(binding);
      }

      private bool _disposed;
      private readonly Uri _endpointUri;
      private readonly string _resourceUri;
      private readonly IChannelFactory<IWSEventingPullDeliveryContract> _enumerationProxyFactory;
      private readonly IChannelFactory<IWSEventingContract> _eventingProxyFactory;
      private readonly FilterMap _filterMap = new FilterMap();
      private readonly Binding _binding;
      private EnumerationContextKey _context;
   }
}