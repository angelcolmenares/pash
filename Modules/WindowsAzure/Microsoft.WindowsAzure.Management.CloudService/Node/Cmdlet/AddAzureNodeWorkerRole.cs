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

namespace Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;

    /// <summary>
    /// Create scaffolding for a new node worker role, change cscfg file and csdef to include the added worker role
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureNodeWorkerRole")]
    public class AddAzureNodeWorkerRoleCommand : AddRole
    {
        internal string AddAzureNodeWorkerRoleProcess(string workerRoleName, int instances, string rootPath)
        {
            string result;
            AzureService service = new AzureService(rootPath, null);
            RoleInfo workerRole = service.AddWorkerRole(Resources.NodeScaffolding, workerRoleName, instances);
            try
            {
                service.ChangeRolePermissions(workerRole);
            }
            catch (UnauthorizedAccessException)
            {
                SafeWriteObject(Resources.AddRoleMessageInsufficientPermissions);
                SafeWriteObject(Environment.NewLine);
            }

            result = string.Format(Resources.AddRoleMessageCreate, rootPath, workerRole.Name);
            return result;
        }

        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = true;
                base.ProcessRecord();
                string result = AddAzureNodeWorkerRoleProcess(Name, Instances, base.GetServiceRootPath());
                SafeWriteObject(result);
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}