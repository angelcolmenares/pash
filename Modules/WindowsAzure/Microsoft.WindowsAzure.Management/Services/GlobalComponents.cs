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
    using System.Web.Script.Serialization;
    using Model;
    using Properties;
    using Utilities;
    using XmlSchema;

    public class GlobalComponents
    {
        public GlobalPathInfo GlobalPaths { get; private set; }
        public PublishData PublishSettings { get; private set; }
        public X509Certificate2 Certificate { get; private set; }
        public SubscriptionsManager SubscriptionManager { get; private set; }
        public ServiceConfiguration ServiceConfiguration { get; private set; }

        public IDictionary<string, SubscriptionData> Subscriptions
        {
            get { return SubscriptionManager.Subscriptions; }
        }

        public static GlobalComponents CreateFromPublishSettings(string azurePath, string subscriptionsDataFile, string publishSettingsFile)
        {
            Validate.ValidateNullArgument(azurePath, string.Format(Resources.InvalidNullArgument, "azurePath"));

            var globalComponents = new GlobalComponents(azurePath, subscriptionsDataFile);
            globalComponents.NewFromPublishSettings(globalComponents.GlobalPaths.SubscriptionsDataFile, publishSettingsFile);
            globalComponents.Save();

            return globalComponents;
        }

        public static GlobalComponents Create(string azurePath, string subscriptionsDataFile, X509Certificate2 certificate, string serviceEndpoint)
        {
            Validate.ValidateNullArgument(azurePath, string.Format(Resources.InvalidNullArgument, "azurePath"));
            Validate.ValidateNullArgument(certificate, string.Format(Resources.InvalidCertificateSingle, "certificate"));
            Validate.ValidateNullArgument(serviceEndpoint, string.Format(Resources.InvalidEndpoint, "serviceEndpoint"));

            var globalComponents = new GlobalComponents(azurePath, subscriptionsDataFile);
            globalComponents.New(globalComponents.GlobalPaths.SubscriptionsDataFile, certificate, serviceEndpoint);
            globalComponents.Save();

            return globalComponents;
        }

        public static GlobalComponents Load(string azurePath)
        {
            return Load(azurePath, null);
        }

        public static GlobalComponents Load(string azurePath, string subscriptionsDataFile)
        {
            Validate.ValidateNullArgument(azurePath, string.Format(Resources.InvalidNullArgument, "azurePath"));
            Validate.ValidateStringIsNullOrEmpty(azurePath, Resources.AzureDirectoryName);

            var globalComponents = new GlobalComponents(azurePath, subscriptionsDataFile);
            globalComponents.LoadCurrent();

            return globalComponents;
        }

        protected GlobalComponents(string azurePath)
            : this(azurePath, null)
        {
        }

        protected GlobalComponents(string azurePath, string subscriptionsDataFile)
        {
            GlobalPaths = new GlobalPathInfo(azurePath, subscriptionsDataFile);
        }

        private void NewFromPublishSettings(string subscriptionsDataFile, string publishSettingsPath)
        {
            Validate.ValidateStringIsNullOrEmpty(GlobalPaths.AzureDirectory, Resources.AzureDirectoryName);
            Validate.ValidateFileFull(publishSettingsPath, Resources.PublishSettings);
            Validate.ValidateFileExtention(publishSettingsPath, Resources.PublishSettingsFileExtention);

            PublishSettings = General.DeserializeXmlFile<PublishData>(publishSettingsPath, string.Format(Resources.InvalidPublishSettingsSchema, publishSettingsPath));
            Certificate = new X509Certificate2(Convert.FromBase64String(PublishSettings.Items[0].ManagementCertificate), string.Empty);
            PublishSettings.Items[0].ManagementCertificate = Certificate.Thumbprint;
            SubscriptionManager = SubscriptionsManager.Import(subscriptionsDataFile, PublishSettings, Certificate);
            ServiceConfiguration = new ServiceConfiguration
            {
                endpoint = PublishSettings.Items.First().Url,
                subscription = PublishSettings.Items.First().Subscription.First().Id,
                subscriptionName = PublishSettings.Items.First().Subscription.First().Name
            };
        }

        private void New(string subscriptionsDataFile, X509Certificate2 certificate, string serviceEndpoint)
        {
            Validate.ValidateStringIsNullOrEmpty(GlobalPaths.AzureDirectory, Resources.AzureDirectoryName);

            Certificate = certificate;
            SubscriptionManager = SubscriptionsManager.Import(subscriptionsDataFile, null, certificate);
            ServiceConfiguration = new ServiceConfiguration { endpoint = serviceEndpoint };
            PublishSettings = new PublishData();

            var publishDataProfile = new PublishDataPublishProfile
            {
                ManagementCertificate = certificate.Thumbprint,
                Url = ServiceConfiguration.endpoint
            };

            if (SubscriptionManager.Subscriptions != null &&
                SubscriptionManager.Subscriptions.Count > 0)
            {
                var subscription = SubscriptionManager.Subscriptions.Values.First();

                ServiceConfiguration.subscription = subscription.SubscriptionId;
                ServiceConfiguration.subscriptionName = subscription.SubscriptionName;
                publishDataProfile.Subscription = new [] {
                    new PublishDataPublishProfileSubscription
                    {
                        Id = subscription.SubscriptionId,
                        Name = subscription.SubscriptionName
                    }
                };
            }

            PublishSettings.Items = new [] { publishDataProfile };
        }

        internal void LoadCurrent()
        {
            Validate.ValidateDirectoryExists(GlobalPaths.AzureDirectory, Resources.GlobalComponents_Load_PublishSettingsNotFound);
            Validate.ValidateFileExists(GlobalPaths.PublishSettingsFile, string.Format(Resources.PathDoesNotExistForElement, Resources.PublishSettingsFileName, GlobalPaths.PublishSettingsFile));

            PublishSettings = General.DeserializeXmlFile<PublishData>(GlobalPaths.PublishSettingsFile);
            Certificate = General.GetCertificateFromStore(PublishSettings.Items.First().ManagementCertificate);
            SubscriptionManager = SubscriptionsManager.Import(GlobalPaths.SubscriptionsDataFile);
            ServiceConfiguration = new JavaScriptSerializer().Deserialize<ServiceConfiguration>(File.ReadAllText(GlobalPaths.ServiceConfigurationFile));
            var defaultSubscription = SubscriptionManager.Subscriptions.Values.FirstOrDefault(subscription => 
                subscription.SubscriptionId == ServiceConfiguration.subscription &&
                (string.IsNullOrEmpty(ServiceConfiguration.subscriptionName) || subscription.SubscriptionName == ServiceConfiguration.subscriptionName));
            if (defaultSubscription != null)
            {
                defaultSubscription.IsDefault = true;
            }
        }

        internal void Save()
        {
            // Create new Azure directory if doesn't exist
            //
            Directory.CreateDirectory(GlobalPaths.AzureDirectory);

            // Save *.publishsettings
            //
            General.SerializeXmlFile(PublishSettings, GlobalPaths.PublishSettingsFile);

            // Save certificate in the store
            //
            General.AddCertificateToStore(Certificate);

            // TODO: Save certificate in the file system

            // Save service configuration
            //
            File.WriteAllText(GlobalPaths.ServiceConfigurationFile, new JavaScriptSerializer().Serialize(ServiceConfiguration));

            // Save subscriptions
            //
            SubscriptionManager.SaveSubscriptions(GlobalPaths.SubscriptionsDataFile);
        }

        internal void SaveSubscriptions()
        {
            SaveSubscriptions(null);
        }

        internal void SaveSubscriptions(string subscriptionDataFile)
        {
            if (subscriptionDataFile == null)
            {
                subscriptionDataFile = GlobalPaths.SubscriptionsDataFile;
            }

            SubscriptionManager.SaveSubscriptions(subscriptionDataFile);

            var defaultSubscription = SubscriptionManager.Subscriptions.Values.FirstOrDefault(s => s.IsDefault);
            if (defaultSubscription != null)
            {
                ServiceConfiguration.subscription = defaultSubscription.SubscriptionId;
                ServiceConfiguration.subscriptionName = defaultSubscription.SubscriptionName;
                ServiceConfiguration.endpoint = defaultSubscription.ServiceEndpoint;
                File.WriteAllText(GlobalPaths.ServiceConfigurationFile, new JavaScriptSerializer().Serialize(ServiceConfiguration));
            }
        }

        public string GetSubscriptionId(string subscriptionName)
        {
            foreach (var subscription in Subscriptions.Values)
            {
                if (subscription.SubscriptionName.Equals(subscriptionName))
                {
                    Validate.IsGuid(subscription.SubscriptionId);
                    return subscription.SubscriptionId;
                }
            }

            throw new ArgumentException(string.Format(Resources.SubscriptionIdNotFoundMessage, subscriptionName, GlobalPaths.PublishSettingsFile));
        }

        internal void DeleteGlobalComponents()
        {
            General.RemoveCertificateFromStore(Certificate);
            File.Delete(GlobalPaths.PublishSettingsFile);
            File.Delete(GlobalPaths.SubscriptionsDataFile);
            File.Delete(GlobalPaths.ServiceConfigurationFile);
            Directory.Delete(GlobalPaths.AzureDirectory, true);
        }
    }
}