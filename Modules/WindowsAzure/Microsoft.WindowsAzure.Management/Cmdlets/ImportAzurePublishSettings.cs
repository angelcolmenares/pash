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
    using System.Linq;
    using Extensions;
    using Properties;
    using Services;

    /// <summary>
    /// Imports publish profiles.
    /// </summary>
    [Cmdlet(VerbsData.Import, "AzurePublishSettingsFile")]
    public class ImportAzurePublishSettingsCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Path to the publish settings file.")]
        [ValidateNotNullOrEmpty]
        public string PublishSettingsFile { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Path to the subscription data output file.")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionDataFile { get; set; }

        internal void ImportSubscriptionProcess(string publishSettingsFile, string subscriptionsDataFile)
        {
            GlobalComponents globalComponents = GlobalComponents.CreateFromPublishSettings(
                GlobalPathInfo.GlobalSettingsDirectory,
                subscriptionsDataFile,
                publishSettingsFile);

            // Set a current and default subscription if possible
            if (globalComponents.Subscriptions != null && globalComponents.Subscriptions.Count > 0)
            {
                var currentDefaultSubscription =
                    globalComponents.Subscriptions.Values.FirstOrDefault(subscription =>
                                                                                subscription.IsDefault) ??
                    globalComponents.Subscriptions.Values.First();

                // Set the default subscription as current
                currentDefaultSubscription.IsDefault = true;

                this.SetCurrentSubscription(currentDefaultSubscription);
                
                // Save subscriptions file to make sure publish settings subscriptions get merged
                // into the subscriptions data file and the default subscription is updated.
                globalComponents.SaveSubscriptions(subscriptionsDataFile);

                this.SafeWriteObject(string.Format(
                    Resources.DefaultAndCurrentSubscription,
                    currentDefaultSubscription.SubscriptionName));
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ImportSubscriptionProcess(
                    this.ResolvePath(PublishSettingsFile),
                    this.TryResolvePath(SubscriptionDataFile));
            }
            catch (Exception exception)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}