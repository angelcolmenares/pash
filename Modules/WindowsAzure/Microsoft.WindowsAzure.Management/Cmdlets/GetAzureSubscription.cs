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
    using System.ServiceModel;
    using System.Collections.Generic;
    using Samples.WindowsAzure.ServiceManagement;
    using Common;
    using Extensions;
    using Model;
    using Properties;

    /// <summary>
    /// Gets details about subscriptions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSubscription", DefaultParameterSetName = "ByName")]
    public class GetAzureSubscriptionCommand : ServiceManagementCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription data file.", ParameterSetName = "ByName")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription data file.", ParameterSetName = "Default")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionDataFile { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves the default subscription.", ParameterSetName = "Default")]
        public SwitchParameter Default { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves the current subscription.", ParameterSetName = "Current")]
        public SwitchParameter Current { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieve extended details about subscription such as quota and usage.")]
        public SwitchParameter ExtendedDetails { get; set; }

        protected virtual void WriteSubscription(SubscriptionData subscriptionData)
        {
            WriteObject(subscriptionData, true);
        }

        private void WriteExtendedSubscription(SubscriptionData subscriptionData, string subscriptionsDataFile)
        {
            SubscriptionData currentSubscription = this.GetCurrentSubscription();
            this.SetCurrentSubscription(subscriptionData.SubscriptionName, subscriptionsDataFile);
            InitChannelCurrentSubscription();
            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    var subprops = RetryCall(s => Channel.GetSubscription(subscriptionData.SubscriptionId));
                    Operation operation = WaitForOperation(CommandRuntime.ToString());
                    var subscriptionDataExtended = new SubscriptionDataExtended(subprops,
                                                                                subscriptionData,
                                                                                CommandRuntime.ToString(),
                                                                                operation);
                    WriteSubscription(subscriptionDataExtended);
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                }
                finally
                {
                    if (currentSubscription != null && currentSubscription.Certificate != null && currentSubscription.SubscriptionId != null)
                    {
                        this.SetCurrentSubscription(currentSubscription.SubscriptionName, subscriptionsDataFile);
                    }
                }
            }
        }

        internal void GetSubscriptionProcess(string parameterSetName, string subscriptionName, string subscriptionsDataFile)
        {
            IDictionary<string, SubscriptionData> subscriptions = new Dictionary<string, SubscriptionData>();
            if (parameterSetName == "ByName")
            {
                if (subscriptionName == null)
                {
                    subscriptions = this.GetSubscriptions(subscriptionsDataFile);
                }
                else
                {
                    SubscriptionData subscriptionData = this.GetSubscription(subscriptionName, subscriptionsDataFile);
                    if (subscriptionData == null)
                    {
                        WriteSubscription(null);
                        return;
                    }

                    subscriptions.Add(subscriptionName, subscriptionData);
                }
            }
            else if (parameterSetName == "Current")
            {
                SubscriptionData subscriptionData = this.GetCurrentSubscription();
                if (subscriptionData == null)
                {
                    WriteError(new ErrorRecord(
                                    new InvalidOperationException(Resources.InvalidSelectedSubscription),
                                    string.Empty,
                                    ErrorCategory.InvalidData,
                                    null));
                }
                else
                {
                    subscriptions.Add(subscriptionData.SubscriptionName, subscriptionData);
                }
            }
            else if (parameterSetName == "Default")
            {
                SubscriptionData subscriptionData = this.GetSubscriptions(subscriptionsDataFile).Values.FirstOrDefault(p => p.IsDefault);
                if (subscriptionData == null)
                {
                    WriteError(new ErrorRecord(
                                new InvalidOperationException(Resources.InvalidDefaultSubscription),
                                string.Empty,
                                ErrorCategory.InvalidData,
                                null));
                }
                else
                {
                    subscriptions.Add(subscriptionData.SubscriptionName, subscriptionData);
                }
            }

            foreach (var subscriptionData in subscriptions.Values)
            {
                if (ExtendedDetails.IsPresent)
                {
                    WriteExtendedSubscription(subscriptionData, subscriptionsDataFile);
                }
                else
                {
                    WriteSubscription(subscriptionData);
                }
            }
        }

        protected override void ProcessRecord()
        {
            SkipChannelInit = true;
            base.ProcessRecord();
            GetSubscriptionProcess(ParameterSetName, SubscriptionName, this.ResolvePath(SubscriptionDataFile));
        }
    }
}