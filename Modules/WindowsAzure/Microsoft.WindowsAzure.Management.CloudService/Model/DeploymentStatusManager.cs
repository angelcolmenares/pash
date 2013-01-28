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

namespace Microsoft.WindowsAzure.Management.CloudService.Model
{
    using System.Linq;
    using System;
    using System.Management.Automation;
    using Services;
    using Management.Services;
    using WAPPSCmdlet;
    using Properties;

    /// <summary>
    /// Change deployment status to running or suspended.
    /// </summary>
    public class DeploymentStatusManager : DeploymentServiceManagementCmdletBase
    {
        public DeploymentStatusManager() { }

        public DeploymentStatusManager(IServiceManagement channel)
        {
            Channel = channel;
        }

        public string Status
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription name")]
        public string Subscription
        {
            get;
            set;
        }

        public virtual string SetDeploymentStatusProcess(string rootPath, string newStatus, string slot, string subscription, string serviceName)
        {
            if (!string.IsNullOrEmpty(subscription))
            {
                var globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                CurrentSubscription = globalComponents.Subscriptions.Values.First(
                    sub => sub.SubscriptionName == subscription);
            }

            string result = CheckDeployment(newStatus, serviceName, slot);
            if (string.IsNullOrEmpty(result))
            {
                SetDeployment(newStatus, serviceName, slot);
                var deploymentStatusCommand = new GetDeploymentStatus(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
                deploymentStatusCommand.WaitForState(newStatus, rootPath, serviceName, slot, CurrentSubscription.SubscriptionName);
            }

            return result;
        }

        private string CheckDeployment(string status, string serviceName, string slot)
        {
            string result = string.Empty;

            try
            {
                var deployment = RetryCall(s => Channel.GetDeploymentBySlot(s, serviceName, slot));

                // Check to see if the service is in transitioning state
                //
                if (deployment.Status != DeploymentStatus.Running && deployment.Status != DeploymentStatus.Suspended)
                {
                    result = string.Format(Resources.ServiceIsInTransitionState, slot, serviceName, deployment.Status);
                }
                // Check to see if user trying to stop an already stopped service or 
                // starting an already starting service
                //
                else if (deployment.Status == DeploymentStatus.Running && status == DeploymentStatus.Running ||
                    deployment.Status == DeploymentStatus.Suspended && status == DeploymentStatus.Suspended)
                {
                    result = string.Format(Resources.DeploymentAlreadyInState, slot, serviceName, status);
                }
            }
            catch
            {
                // If we reach here that means the service or slot doesn't exist
                //
                result = string.Format(Resources.ServiceSlotDoesNotExist, serviceName, slot);
            }

            return result;
        }

        private void SetDeployment(string status, string serviceName, string slot)
        {
            var updateDeploymentStatus = new UpdateDeploymentStatusInput
            {
                Status = status
            };

            InvokeInOperationContext(() => RetryCall(s => Channel.UpdateDeploymentStatusBySlot(
                s,
                serviceName,
                slot,
                updateDeploymentStatus)));
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                string serviceName;
                var rootPath = GetServiceRootPath();
                ServiceSettings settings = GetDefaultSettings(rootPath, ServiceName, Slot, null, null, Subscription, out serviceName);
                SetDeploymentStatusProcess(rootPath, Status, settings.Slot, settings.Subscription, serviceName);
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}