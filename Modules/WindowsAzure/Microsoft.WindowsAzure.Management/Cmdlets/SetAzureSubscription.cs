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

namespace Microsoft.WindowsAzure.Management.Cmdlets
{
    using System;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Extensions;
    using Model;
    using Properties;
    using System.IO;
    using Services;
    using Service;

    /// <summary>
    /// Sets an azure subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureSubscription", DefaultParameterSetName = "CommonSettings")]
    public class SetAzureSubscriptionCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "CommonSettings")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "ResetCurrentStorageAccount")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Account subscription ID.", ParameterSetName = "CommonSettings")]
        public string SubscriptionId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Account certificate.", ParameterSetName = "CommonSettings")]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Service endpoint.", ParameterSetName = "CommonSettings")]
        public string ServiceEndpoint { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Current storage account name.", ParameterSetName = "CommonSettings")]
        [ValidateNotNullOrEmpty]
        public string CurrentStorageAccount { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Default subscription name.", ParameterSetName = "DefaultSubscription")]
        public string DefaultSubscription { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Reset the current default subscription setting.", ParameterSetName = "ResetDefaultSubscription")]
        public SwitchParameter NoDefaultSubscription { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription data file.")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionDataFile { get; set; }

        protected virtual void WriteMessage(string message)
        {
            WriteVerbose(message);
        }

        /// <summary>
        /// Sets the default subscription properties based on the cmdlet parameters.
        /// </summary>
        /// <param name="subscription">The subscription where to set the properties.</param>
        /// <param name="subscriptionId">The subscription's identifier.</param>
        /// <param name="certificate">The subscription's certificate.</param>
        /// <param name="serviceEndpoint">The subscription's service endpoint</param>
        /// <param name="currentStorageAccount">The current storage account.</param>
        private void SetCommonSettingsProcess(SubscriptionData subscription, string subscriptionId, X509Certificate2 certificate, string serviceEndpoint, string currentStorageAccount)
        {
            if (subscriptionId != null)
            {
                subscription.SubscriptionId = subscriptionId;
            }

            if (certificate != null)
            {
                subscription.Certificate = certificate;
            }

            if (serviceEndpoint != null)
            {
                subscription.ServiceEndpoint = serviceEndpoint;
            }

            if (currentStorageAccount != null)
            {
                subscription.NullCurrentStorageAccount(); // next time it is retrieved get the right account info.
                subscription.CurrentStorageAccount = currentStorageAccount;
            }
        }

        /// <summary>
        /// Executes the set subscription cmdlet operation.
        /// </summary>
        /// <param name="parameterSetName">The type of set operation to perform.</param>
        /// <param name="subscriptionName">The existing or new subscription name.</param>
        /// <param name="subscriptionId">The subscription identifier for the existing or new subscription.</param>
        /// <param name="certificate">The certificate for the existing or new subscription.</param>
        /// <param name="serviceEndpoint">The service endpoint for the existing or new subscription.</param>
        /// <param name="defaultSubscription">The subscription name for the new subscription.</param>
        /// <param name="currentStorageAccount">The current storage account.</param>
        /// <param name="subscriptionDataFile">The input/output subscription data file to use.</param>
        internal void SetSubscriptionProcess(string parameterSetName, string subscriptionName, string subscriptionId, X509Certificate2 certificate, string serviceEndpoint, string defaultSubscription, string currentStorageAccount, string subscriptionDataFile)
        {
            SubscriptionData currentSubscription = this.GetCurrentSubscription();
            if (currentSubscription != null && 
                !String.IsNullOrEmpty(currentSubscription.ServiceEndpoint) &&
                String.IsNullOrEmpty(serviceEndpoint))
            {
                // If the current subscription already has a service endpoint do not overwrite if not specified 
                serviceEndpoint = currentSubscription.ServiceEndpoint;
            }
            else if (String.IsNullOrEmpty(serviceEndpoint))
            {
                // No current subscription and nothing specified initialize with the default
                serviceEndpoint = ConfigurationConstants.ServiceManagementEndpoint;
            }

            if (parameterSetName == "DefaultSubscription")
            {
                // Set a new default subscription
                this.SetDefaultSubscription(defaultSubscription, subscriptionDataFile);
                this.SetCurrentSubscription(defaultSubscription, subscriptionDataFile);
            }
            else if (parameterSetName == "ResetDefaultSubscription")
            {
                // Reset default subscription
                this.SetDefaultSubscription(null, subscriptionDataFile);
            }
            else
            {
                // Update or create a new subscription
                GlobalComponents globalComponents;
                try
                {
                    globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);
                }
                catch (FileNotFoundException)
                {
                    // assume that import has never been ran and just create it.
                    globalComponents = GlobalComponents.Create(GlobalPathInfo.GlobalSettingsDirectory,
                        subscriptionDataFile,
                        certificate,
                        serviceEndpoint);
                }

                SubscriptionData subscription = globalComponents.Subscriptions.ContainsKey(subscriptionName)
                    ? globalComponents.Subscriptions[subscriptionName]
                    : new SubscriptionData { SubscriptionName = subscriptionName, IsDefault = (globalComponents.Subscriptions.Count == 0) };

                if (parameterSetName == "CommonSettings")
                {
                    SetCommonSettingsProcess(subscription, subscriptionId, certificate, serviceEndpoint, currentStorageAccount);
                }

                // Create or update
                globalComponents.Subscriptions[subscription.SubscriptionName] = subscription;
                globalComponents.SaveSubscriptions(subscriptionDataFile);

                currentSubscription = this.GetCurrentSubscription();
                if (currentSubscription == null || string.Compare(currentSubscription.SubscriptionName, subscription.SubscriptionName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // If the user modifies a subscription using Set-AzureSubscription, but doesn't call Select-AzureSubscription
                    // it is not immediately reflected in the code. This takes into account if they modify the current subscription 
                    // that they shouldn't have to call Select-AzureSubscription once again to have the values updated in session.
                    this.SetCurrentSubscription(subscription.SubscriptionName, subscriptionDataFile);

                    if (currentSubscription != null)
                    {
                        WriteMessage(string.Format(
                            Resources.UpdatedSettings,
                            subscriptionName,
                            currentSubscription.SubscriptionName));
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                SetSubscriptionProcess(
                    ParameterSetName,
                    SubscriptionName,
                    SubscriptionId,
                    Certificate,
                    ServiceEndpoint,
                    DefaultSubscription,
                    CurrentStorageAccount,
                    this.TryResolvePath(SubscriptionDataFile));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.InvalidData, null));
            }
        }
    }
}