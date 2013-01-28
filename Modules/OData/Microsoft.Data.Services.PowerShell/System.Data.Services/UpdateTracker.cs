namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Reflection;

    internal class UpdateTracker
    {
        private Dictionary<ResourceSetWrapper, Dictionary<object, UpdateOperations>> items;
        private readonly IDataService service;

        private UpdateTracker(IDataService service)
        {
            this.service = service;
            this.items = new Dictionary<ResourceSetWrapper, Dictionary<object, UpdateOperations>>(ReferenceEqualityComparer<ResourceSetWrapper>.Instance);
        }

        [Conditional("DEBUG")]
        private static void AssertActionValues(object target, ResourceSetWrapper container)
        {
        }

        internal static UpdateTracker CreateUpdateTracker(IDataService service)
        {
            return new UpdateTracker(service);
        }

        internal static void FireNotification(IDataService service, object target, ResourceSetWrapper container, UpdateOperations action)
        {
            MethodInfo[] changeInterceptors = container.ChangeInterceptors;
            if (changeInterceptors != null)
            {
                object[] parameters = new object[] { target, action };
                for (int i = 0; i < changeInterceptors.Length; i++)
                {
                    try
                    {
                        changeInterceptors[i].Invoke(service.Instance, parameters);
                    }
                    catch (TargetInvocationException exception)
                    {
                        ErrorHandler.HandleTargetInvocationException(exception);
                        throw;
                    }
                }
            }
        }

        internal void FireNotifications()
        {
            object[] parameters = new object[2];
            foreach (KeyValuePair<ResourceSetWrapper, Dictionary<object, UpdateOperations>> pair in this.items)
            {
                MethodInfo[] changeInterceptors = pair.Key.ChangeInterceptors;
                foreach (KeyValuePair<object, UpdateOperations> pair2 in pair.Value)
                {
                    parameters[0] = this.service.Updatable.ResolveResource(pair2.Key);
                    parameters[1] = pair2.Value;
                    for (int i = 0; i < changeInterceptors.Length; i++)
                    {
                        try
                        {
                            changeInterceptors[i].Invoke(this.service.Instance, parameters);
                        }
                        catch (TargetInvocationException exception)
                        {
                            ErrorHandler.HandleTargetInvocationException(exception);
                            throw;
                        }
                    }
                }
                pair.Value.Clear();
            }
            this.items = null;
        }

        internal void TrackAction(object target, ResourceSetWrapper container, UpdateOperations action)
        {
            if (container.ChangeInterceptors != null)
            {
                Dictionary<object, UpdateOperations> dictionary;
                UpdateOperations operations;
                if (!this.items.TryGetValue(container, out dictionary))
                {
                    if (this.service.Provider.IsV1Provider)
                    {
                        dictionary = new Dictionary<object, UpdateOperations>(EqualityComparer<object>.Default);
                    }
                    else
                    {
                        dictionary = new Dictionary<object, UpdateOperations>(ReferenceEqualityComparer<object>.Instance);
                    }
                    this.items.Add(container, dictionary);
                }
                if (dictionary.TryGetValue(target, out operations))
                {
                    if ((action | operations) != operations)
                    {
                        dictionary[target] = action | operations;
                    }
                }
                else
                {
                    dictionary.Add(target, action);
                }
            }
        }
    }
}

