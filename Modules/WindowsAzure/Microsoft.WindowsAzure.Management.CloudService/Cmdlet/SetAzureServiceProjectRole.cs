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
    using Model;
    using Services;

    /// <summary>
    /// Configure the number of instances or installed runtimes for a web/worker role. Updates the cscfg with the number of instances
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceProjectRole")]
    public class SetAzureServiceProjectRoleCommand : DeploymentServiceManagementCmdletBase
    {
        /// <summary>
        /// The role name to edit
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string RoleName { get; set; }

        /// <summary>
        /// The number of instances for the role - parameter set for instances contains role name and instances only
        /// </summary>
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Instances", ValueFromPipelineByPropertyName = true)]
        public int Instances { get; set; }

        /// <summary>
        /// Runtime identifier for the runtime to add. The Runtime parameter set takes rolename, runtime, and version
        /// </summary>
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Runtime", ValueFromPipelineByPropertyName = true)]
        public string Runtime { get; set; }

        /// <summary>
        /// The version of the runtime to install
        /// </summary>
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "Runtime", ValueFromPipelineByPropertyName = true)]
        public string Version { get; set; }

        /// <summary>
        /// The code to run if setting azure instances
        /// </summary>
        /// <param name="roleName">The name of the role to update</param>
        /// <param name="instances">The new number of instances for the role</param>
        /// <param name="rootPath">The root path to the service containing the role</param>
        public void SetAzureInstancesProcess(string roleName, int instances, string rootPath)
        {
            AzureService service = new AzureService(rootPath, null);
            service.SetRoleInstances(service.Paths, roleName, instances);
        }

        /// <summary>
        /// The function to run if setting the runtime for a role
        /// </summary>
        /// <param name="roleName">The name of the role to modify</param>
        /// <param name="runtimeType">The type f role runtiem to configure</param>
        /// <param name="runtimeVersion">The version of the runtime</param>
        /// <param name="rootPath">The path to the service containing the role</param>
        /// <param name="manifest">The manifest containing available runtimes, defaults to the cloud manifest
        /// mainly used a s a test hook</param>
        public void SetAzureRuntimesProcess(string roleName, string runtimeType, string runtimeVersion, string rootPath, string manifest = null)
        {
            AzureService service = new AzureService(rootPath, null);
            service.AddRoleRuntime(service.Paths, roleName, runtimeType, runtimeVersion, manifest);
        }

        /// <summary>
        /// Do pipeline processing
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = true;
                base.ProcessRecord();
                if (string.Equals(this.ParameterSetName, "Instances", StringComparison.OrdinalIgnoreCase))
                {
                    this.SetAzureInstancesProcess(RoleName, Instances, base.GetServiceRootPath());
                }
                else
                {
                    this.SetAzureRuntimesProcess(RoleName, Runtime, Version, base.GetServiceRootPath());
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}