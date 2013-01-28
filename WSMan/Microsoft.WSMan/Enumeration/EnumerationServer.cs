using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationServer : IWSEnumerationContract, IFilterMapProvider
   {           
      public EnumerateResponse Enumerate(EnumerateRequest request)
      {         
         EnumerationContextKey contextKey = EnumerationContextKey.Unique();         
         EnumerationContext context = new EnumerationContext(contextKey.Text, request.Filter, SelectorSetHeader.GetCurrent());
         if (RequestTotalItemsCountEstimate.IsPresent)
         {
            HandleCountEnumerate(contextKey.Text, request.Filter);
            return new EnumerateResponse
            {
               EnumerationContext = contextKey
            };
         }
         if (request.OptimizeEnumeration != null)
         {
            return HandleOptimizedEnumerate(contextKey, request, context);
         }         

         IEnumerator<object> enumerator = GetHandler(request.Filter).Enumerate(context).GetEnumerator();
         _activeEnumerations[contextKey] = new EnumerationState(enumerator, request.EnumerationMode);
         return new EnumerateResponse
                   {
                      EnumerationContext = contextKey,
                      Expires = request.Expires
                   };
      }
      private void HandleCountEnumerate(string context, Filter filter)
      {
         int count = GetHandler(filter).EstimateRemainingItemsCount(new EnumerationContext(context, filter, SelectorSetHeader.GetCurrent()));
         OperationContextProxy.Current.AddHeader(new TotalItemsCountEstimate(count));         
      }

      private EnumerateResponse HandleOptimizedEnumerate(EnumerationContextKey contextKey, EnumerateRequest request, EnumerationContext context)
      {
         int maxElements = request.MaxElements != null 
                              ? request.MaxElements.Value 
                              : 1;

         if (request.EnumerationMode == EnumerationMode.EnumerateEPR)
         {
            IEnumerator<object> enumerator = GetHandler(request.Filter).Enumerate(context).GetEnumerator();

            bool endOfSequence;
            EnumerationItemList items = new EnumerationItemList(PullItems(maxElements, request.EnumerationMode,enumerator, out endOfSequence));
            if (!endOfSequence)
            {
               _activeEnumerations[contextKey] = new EnumerationState(enumerator, request.EnumerationMode);
            }
            return new EnumerateResponse
                      {
                         Items = items,
                         EndOfSequence = endOfSequence ? new EndOfSequence() : null,
                         EnumerationContext = endOfSequence ? null : contextKey
                      };
         }
         throw new NotSupportedException();
      }

      public PullResponse Pull(PullRequest request)
      {
         EnumerationState holder;
         if (!_activeEnumerations.TryGetValue(request.EnumerationContext, out holder))
         {
            throw Faults.InvalidEnumerationContext.Create();            
         }
         
         int maxElements = request.MaxElements != null
                              ? request.MaxElements.Value
                              : 1;

         bool endOfSequence;
         EnumerationItemList items = new EnumerationItemList(PullItems(maxElements, holder.Mode, holder.Enumerator, out endOfSequence));
         if (endOfSequence)
         {
            _activeEnumerations.Remove(request.EnumerationContext);
         }
         return new PullResponse
                   {
                      Items = items,
                      EndOfSequence = endOfSequence ? new EndOfSequence() : null,
                      EnumerationContext = endOfSequence ? null : request.EnumerationContext
                   };
      }

      private static IEnumerable<EnumerationItem> PullItems(int maximum, EnumerationMode mode, IEnumerator<object> enumerator, out bool endOfSequence)
      {
         int i = 0;
         List<EnumerationItem> result = new List<EnumerationItem>();
         bool moveNext = false;       
         while (i < maximum && (moveNext = enumerator.MoveNext()))
         {            
            if (mode == EnumerationMode.EnumerateEPR)
            {
               if (i == 0)
               {
                  EnumerationModeExtension.Activate(EnumerationMode.EnumerateEPR, null);
               }
               result.Add(new EnumerationItem((EndpointAddress)enumerator.Current));
            }
            else
            {
               if (i == 0)
               {
                  EnumerationModeExtension.Activate(EnumerationMode.EnumerateObjectAndEPR,
                                                    enumerator.Current.GetType());
               }
               result.Add(new EnumerationItem(
                             new EndpointAddress("http://tempuri.org"),
                             enumerator.Current));
            }                        
            i++;
         }
         endOfSequence = !moveNext || i < maximum;
         return result;
      }

      private IEnumerationRequestHandler GetHandler(Filter filter)
      {
         //TODO: Add fault if not found
         ResourceUriHeader resourceUriHeader = OperationContextProxy.Current.FindHeader<ResourceUriHeader>();            

         string dialect = (filter != null && filter.Dialect != null) 
            ? filter.Dialect 
            : FilterMap.DefaultDialect;

         return _handlerMap[new HandlerMapKey(resourceUriHeader.ResourceUri, dialect)];
      }

      public EnumerationServer Bind(Uri resourceUri, string dialect, Type filterType, IEnumerationRequestHandler handler)
      {         
         _filterMap.Bind(dialect, filterType);
         _handlerMap[new HandlerMapKey(resourceUri.ToString(), dialect)] = handler;
         return this;
      }      

      public FilterMap ProvideFilterMap()
      {
         return _filterMap;
      }      

      private readonly Dictionary<EnumerationContextKey, EnumerationState> _activeEnumerations = new Dictionary<EnumerationContextKey, EnumerationState>();
      private readonly Dictionary<HandlerMapKey, IEnumerationRequestHandler> _handlerMap = new Dictionary<HandlerMapKey, IEnumerationRequestHandler>();
      private readonly FilterMap _filterMap = new FilterMap();
   }
}