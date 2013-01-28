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

namespace Microsoft.WindowsAzure.Management.CloudService.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.Linq;
    using Management.Services;
    using WAPPSCmdlet;
    using Properties;
    using Model;
    using Services;

    /// <summary>
    /// Deletes the specified hosted service from Windows Azure.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureService", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveAzureServiceCommand : DeploymentServiceManagementCmdletBase
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "name of the hosted service")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "name of subscription which has this service")]
        public string Subscription
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm deletion of deployment")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        public RemoveAzureServiceCommand() { }

        public RemoveAzureServiceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        public bool RemoveAzureServiceProcess(string rootName, string inSubscription, string inServiceName)
        {
            string serviceName;
            ServiceSettings settings = GetDefaultSettings(rootName, inServiceName, null, null, null, inSubscription,
                                                            out serviceName);
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception(Resources.InvalidServiceName);
            }

            if (!Force.IsPresent &&
                !ShouldProcess("", string.Format(Resources.RemoveServiceWarning, serviceName),
                                Resources.ShouldProcessCaption))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(settings.Subscription))
            {
                var globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                CurrentSubscription = globalComponents.Subscriptions.Values.First(
                    sub => sub.SubscriptionName == settings.Subscription);
            }

            SafeWriteObjectWithTimestamp(Resources.RemoveServiceStartMessage, serviceName);
            SafeWriteObjectWithTimestamp(Resources.RemoveDeploymentMessage);
            StopAndRemove(rootName, serviceName, CurrentSubscription.SubscriptionName, ArgumentConstants.Slots[Slot.Production]);
            StopAndRemove(rootName, serviceName, CurrentSubscription.SubscriptionName, ArgumentConstants.Slots[Slot.Staging]);
            SafeWriteObjectWithTimestamp(Resources.RemoveServiceMessage);
            RemoveService(serviceName);
            return true;
        }

        private void StopAndRemove(string rootName, string serviceName, string subscription, string slot)
        {
            var deploymentStatusCommand = new GetDeploymentStatus(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
            if (deploymentStatusCommand.DeploymentExists(rootName, serviceName, slot, subscription))
            {
                DeploymentStatusManager setDeployment = new DeploymentStatusManager(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
                setDeployment.SetDeploymentStatusProcess(rootName, DeploymentStatus.Suspended, slot, subscription, serviceName);

                deploymentStatusCommand.WaitForState(DeploymentStatus.Suspended, rootName, serviceName, slot, subscription);

                RemoveAzureDeploymentCommand removeDeployment = new RemoveAzureDeploymentCommand(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
                removeDeployment.RemoveAzureDeploymentProcess(rootName, serviceName, slot, subscription);

                while (deploymentStatusCommand.DeploymentExists(rootName, serviceName, slot, subscription)) ;
            }
        }

        private void RemoveService(string serviceName)
        {
            SafeWriteObjectWithTimestamp(string.Format(Resources.RemoveAzureServiceWaitMessage, serviceName));

            InvokeInOperationContext(() => RetryCall(s => this.Channel.DeleteHostedService(s, serviceName)));
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                if (RemoveAzureServiceProcess(GetServiceRootPath(), Subscription, ServiceName))
                {
                    SafeWriteObjectWithTimestamp(Resources.CompleteMessage);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}