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
    using Extensions;

    /// <summary>
    /// Selects a subscription from the previously imported ones.
    /// </summary>
    [Cmdlet(VerbsCommon.Select, "AzureSubscription", DefaultParameterSetName = "Set")]
    public class SelectAzureSubscriptionCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "Set")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Specify to clear the current selection.", ParameterSetName = "Clear")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Clear { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription data file.")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionDataFile { get; set; }

        internal void SelectSubscriptionProcess(string parameterSetName, string subscriptionName, string subscriptionsDataFile)
        {
            switch (parameterSetName)
            {
                case "Set":
                    this.SetCurrentSubscription(
                        subscriptionName,
                        subscriptionsDataFile);
                    break;
                case "Clear":
                    this.ClearCurrentSubscription();
                    break;
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                SelectSubscriptionProcess(
                    ParameterSetName,
                    SubscriptionName,
                    this.ResolvePath(SubscriptionDataFile));
            }
            catch (Exception exception)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
