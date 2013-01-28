// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.IO;

namespace Microsoft.WindowsAzure.Management.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Globalization;
    using Model;
    using Properties;
    using Utilities;
    using Services;

    public static class CmdletSubscriptionExtensions
    {
        public static ISessionManager SessionManager = new PsSessionManager();

        public static SubscriptionData GetCurrentSubscription(this PSCmdlet cmdlet)
        {
            // Check if there is a current subscription already set
            var currentSubscription = SessionManager.GetVariable(cmdlet, Constants.CurrentSubscriptionEnvironmentVariable) as SubscriptionData;
            if (currentSubscription == null)
            {
                try
                {
                    // Check if there is a default subscription available
                    GlobalComponents globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                    currentSubscription =
                        globalComponents.Subscriptions.Values.FirstOrDefault(subscription => subscription.IsDefault);

                    if (currentSubscription != null)
                    {
                        // Set the default subscription to be the new current subscription
                        SessionManager.SetVariable(cmdlet, Constants.CurrentSubscriptionEnvironmentVariable,
                                                   currentSubscription);
                    }
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }

            return currentSubscription;
        }

        public static void SetCurrentSubscription(this PSCmdlet cmdlet, string subscriptionName, string subscriptionDataFile)
        {
            if (subscriptionName == null)
            {
                throw new ArgumentNullException("subscriptionName", Resources.InvalidSubscriptionName);
            }

            var globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);

            SubscriptionData subscriptionData;
            if (!globalComponents.Subscriptions.TryGetValue(subscriptionName, out subscriptionData))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.InvalidSubscription,
                    subscriptionName));
            }

            SetCurrentSubscription(cmdlet, subscriptionData);
        }

        public static void SetCurrentSubscription(this PSCmdlet cmdlet, SubscriptionData subscription)
        {
            SessionManager.SetVariable(cmdlet, Constants.CurrentSubscriptionEnvironmentVariable, subscription);
        }

        public static void ClearCurrentSubscription(this PSCmdlet cmdlet)
        {
            SessionManager.ClearVariable(cmdlet, Constants.CurrentSubscriptionEnvironmentVariable);
        }

        public static SubscriptionData GetSubscription(this PSCmdlet cmdlet, string subscriptionName, string subscriptionDataFile)
        {
            if (subscriptionName == null)
            {
                throw new ArgumentNullException("subscriptionName", Resources.InvalidSubscriptionName);
            }

            IDictionary<string, SubscriptionData> subscriptions = GetSubscriptions(cmdlet, subscriptionDataFile);

            SubscriptionData subscriptionData;

            if (subscriptions.TryGetValue(subscriptionName, out subscriptionData))
            {
                return subscriptionData;
            }

            return null;
        }

        public static IDictionary<string, SubscriptionData> GetSubscriptions(this PSCmdlet cmdlet, string subscriptionDataFile)
        {
            return GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile).Subscriptions;
        }

        public static void SetDefaultSubscription(this PSCmdlet cmdlet, string subscriptionName, string subscriptionDataFile)
        {
            GlobalComponents globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);
            if (globalComponents.Subscriptions.Count > 1)
            {
                var defaultSubscription = globalComponents.Subscriptions.Values.FirstOrDefault(subscription => subscription.IsDefault);
                if (defaultSubscription != null)
                {
                    defaultSubscription.IsDefault = false;
                }

                if (subscriptionName != null)
                {
                    defaultSubscription = globalComponents.Subscriptions.Values.First(subscription => subscription.SubscriptionName == subscriptionName);
                    defaultSubscription.IsDefault = true;
                }
                else
                {
                    defaultSubscription = globalComponents.Subscriptions.Values.First();
                    defaultSubscription.IsDefault = true;
                }

                globalComponents.SaveSubscriptions();
            }
        }

        public static void UpdateSubscriptions(this PSCmdlet cmdlet, IDictionary<string, SubscriptionData> subscriptionsData, string subscriptionDataFile)
        {
            GlobalComponents globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);
            globalComponents.SubscriptionManager.Subscriptions = subscriptionsData;
            globalComponents.SaveSubscriptions();
        }
    }
}