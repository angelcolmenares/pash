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
    using Model;
    using System.Management.Automation;
    using Properties;
    using WAPPSCmdlet;

    /// <summary>
    /// Stops the deployment of specified slot in the azure service
    /// </summary>
    [Cmdlet(VerbsLifecycle.Stop, "AzureService")]
    public class StopAzureService : DeploymentStatusManager
    {
        /// <summary>
        /// SetDeploymentStatus will handle the execution of this cmdlet
        /// </summary>
        public StopAzureService()
        {
            Status = DeploymentStatus.Suspended;
        }

        public StopAzureService(IServiceManagement channel)
            : base(channel)
        {
            Status = DeploymentStatus.Suspended;
        }

        public override string SetDeploymentStatusProcess(string rootPath, string newStatus, string slot, string subscription, string serviceName)
        {
            SafeWriteObjectWithTimestamp(Resources.StopServiceMessage, serviceName);
            var message = base.SetDeploymentStatusProcess(rootPath, newStatus, slot, subscription, serviceName);
            SafeWriteObjectWithTimestamp(string.IsNullOrEmpty(message) ? Resources.CompleteMessage : message);
            return message;
        }
    }
}
