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
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using AzureTools;
    using Model;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using Services;

    /// <summary>
    /// Enable Remote Desktop by adding appropriate imports and settings to
    /// ServiceDefinition.csdef and ServiceConfiguration.*.cscfg
    /// </summary>
    [Cmdlet(VerbsLifecycle.Disable, "AzureServiceProjectRemoteDesktop")]
    public class DisableAzureServiceProjectRemoteDesktopCommand : DeploymentServiceManagementCmdletBase
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void ProcessRecord()
        {
            try
            {
                AzureTool.Validate();
                base.ProcessRecord();
                DisableRemoteDesktop();
            }
            catch (Exception ex)
            {
                SafeWriteError(ex);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void DisableRemoteDesktop()
        {
            AzureService service = new AzureService(GetServiceRootPath(), null);
            WebRole[] webRoles = service.Components.Definition.WebRole ?? new WebRole[0];
            WorkerRole[] workerRoles = service.Components.Definition.WorkerRole ?? new WorkerRole[0];

            string forwarderName = GetForwarderName(webRoles, workerRoles);
            if (forwarderName != null)
            {
                UpdateServiceConfigurations(service, forwarderName);
                service.Components.Save(service.Paths);
            }
        }

        private static string GetForwarderName(WebRole[] webRoles, WorkerRole[] workerRoles)
        {
            string forwarderName = null;
            WorkerRole workerForwarder = workerRoles.FirstOrDefault(r => r.Imports != null && 
                r.Imports.Any(i => i.moduleName == "RemoteForwarder"));
            if (workerForwarder != null)
            {
                // a worker role has the forwarder
                forwarderName = workerForwarder.name;
            }
            else
            {
                WebRole webForwarder = webRoles.FirstOrDefault(r => r.Imports != null && 
                    r.Imports.Any(i => i.moduleName == "RemoteForwarder"));
                if (webForwarder != null)
                {
                    // a web role has the forwarder
                    forwarderName = webForwarder.name;
                }
            }

            return forwarderName;
        }

        private void UpdateServiceConfigurations(AzureService service, string forwarderName)
        {
            foreach (ServiceConfiguration config in new[] { service.Components.LocalConfig, service.Components.CloudConfig })
            {
                foreach (ServiceConfigurationSchema.RoleSettings role in config.Role)
                {
                    if (role.ConfigurationSettings != null)
                    {
                        ServiceConfigurationSchema.ConfigurationSetting setting = role.ConfigurationSettings.
                            FirstOrDefault<ServiceConfigurationSchema.ConfigurationSetting>(t => t.name.Equals(
                                "Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled"));
                        if (setting != null)
                        {
                            setting.value = "false";
                        }

                        if (role.name == forwarderName)
                        {
                            ServiceConfigurationSchema.ConfigurationSetting forwarderSetting = role.ConfigurationSettings.
                                FirstOrDefault<ServiceConfigurationSchema.ConfigurationSetting>(t => t.name.Equals(
                                    "Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled"));
                            if (forwarderSetting != null)
                            {
                                forwarderSetting.value = "false";
                            }
                        }
                    }
                }
            }
        }
    }
}
