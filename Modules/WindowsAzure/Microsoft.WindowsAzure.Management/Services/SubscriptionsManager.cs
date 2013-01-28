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

namespace Microsoft.WindowsAzure.Management.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Model;
    using Properties;
    using Utilities;
    using XmlSchema;

    public class SubscriptionsManager : ISubscriptionsManager
    {
        public IDictionary<string, SubscriptionData> Subscriptions { get; set; }

        public SubscriptionsManager()
        {
            Subscriptions = new Dictionary<string, SubscriptionData>(StringComparer.OrdinalIgnoreCase);
        }

        public static SubscriptionsManager Import(string subscriptionsDataFile)
        {
            return Import(subscriptionsDataFile, null, null);
        }

        public static SubscriptionsManager Import(string subscriptionsDataFile, PublishData publishSettings)
        {
            var certificate = new X509Certificate2(Convert.FromBase64String(publishSettings.Items[0].ManagementCertificate), string.Empty);
            publishSettings.Items[0].ManagementCertificate = certificate.Thumbprint;
            return Import(subscriptionsDataFile, publishSettings, certificate);
        }

        public static SubscriptionsManager Import(string subscriptionsDataFile, PublishData publishSettings, X509Certificate2 certificate)
        {
            var subscriptionsManager = new SubscriptionsManager();
            if (File.Exists(subscriptionsDataFile))
            {
                subscriptionsManager.ImportSubscriptionsFile(subscriptionsDataFile, certificate);
            }

            if (publishSettings != null)
            {
                foreach (var subscription in publishSettings.Items.Single().Subscription)
                {
                    var subscriptionData = subscriptionsManager.Subscriptions.ContainsKey(subscription.Name)
                        ? subscriptionsManager.Subscriptions[subscription.Name]
                        : new SubscriptionData { SubscriptionName = subscription.Name };

                    subscriptionData.SubscriptionId = subscription.Id;
                    subscriptionData.Certificate = certificate;
                    subscriptionData.ServiceEndpoint = publishSettings.Items.Single().Url;

                    subscriptionsManager.Subscriptions[subscriptionData.SubscriptionName] = subscriptionData;
                }
            }

            return subscriptionsManager;
        }

        public void SaveSubscriptions(string subscriptionsDataFile)
        {
            if (subscriptionsDataFile == null)
            {
                throw new ArgumentNullException("subscriptionsDataFile");
            }

            var subscriptionsXml = new Subscriptions
            {
                Subscription = Subscriptions.Values.Select(subscription =>
                    {
                        var subscriptionsSubscription = new SubscriptionsSubscription
                        {
                            CurrentStorageAccount = subscription.CurrentStorageAccount,
                            name = subscription.SubscriptionName,
                            ServiceEndpoint = subscription.ServiceEndpoint,
                            SQLAzureServiceEndpoint = subscription.SqlAzureServiceEndpoint,
                            SubscriptionId = subscription.SubscriptionId
                        };

                        if (subscription.Certificate != null)
                        {
                            subscriptionsSubscription.Thumbprint = subscription.Certificate.Thumbprint;
                        }

                        return subscriptionsSubscription;
                    }
                ).ToArray()
            };

            var parentDirectory = Directory.GetParent(subscriptionsDataFile);
            if (!parentDirectory.Exists)
            {
                parentDirectory.Create();
            }

            General.SerializeXmlFile(subscriptionsXml, subscriptionsDataFile);
        }

        public void ImportSubscriptionsFile(string subscriptionsDataFile, X509Certificate2 publishSettingsCertificate)
        {
            var subscriptions = General.DeserializeXmlFile<Subscriptions>(subscriptionsDataFile, string.Format(Resources.InvalidSubscriptionsDataSchema, subscriptionsDataFile));
            if (subscriptions != null && subscriptions.Subscription != null)
            {
                Subscriptions = subscriptions.Subscription
                    .Select(subscription =>
                    {
                        X509Certificate2 certificate = null;
                        if (subscription.Thumbprint != null)
                        {
                            if (publishSettingsCertificate != null && subscription.Thumbprint == publishSettingsCertificate.Thumbprint)
                            {
                                certificate = publishSettingsCertificate;
                            }
                            else
                            {
                                certificate = General.GetCertificateFromStore(subscription.Thumbprint);   
                            }
                        }

                        var subscriptionData = new SubscriptionData
                        {
                            SubscriptionName = subscription.name,
                            SubscriptionId = subscription.SubscriptionId,
                            Certificate = certificate,
                            ServiceEndpoint = subscription.ServiceEndpoint,
                            SqlAzureServiceEndpoint = subscription.SQLAzureServiceEndpoint,
                            CurrentStorageAccount = subscription.CurrentStorageAccount
                        };

                        return subscriptionData;
                    })
                    .ToDictionary(subscriptionData => subscriptionData.SubscriptionName, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}