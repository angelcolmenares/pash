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
    using System.IO;
    using System.Management.Automation;
    using Model;
    using Properties;
    using Services;

    /// <summary>
    /// Create scaffolding for a new hosted service. Generates a basic folder structure, 
    /// default cscfg file which wires up node/iisnode at startup in Azure as well as startup.js. 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureServiceProject")]
    public class NewAzureServiceProjectCommand : DeploymentServiceManagementCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the node.js project")]
        public string ServiceName { get; set; }

        internal string NewAzureServiceProcess(string parentDirectory, string serviceName)
        {
            string message;
            AzureService newService;

            // Create scaffolding structure
            //
            newService = new AzureService(parentDirectory, serviceName, null);
            
            message = string.Format(Resources.NewServiceCreatedMessage, newService.Paths.RootPath);

            return message;
        }

        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = true;
                base.ProcessRecord();
                
                // Create new service
                //
                string result = NewAzureServiceProcess(CurrentPath(), ServiceName);
                // Set current directory to the root of the new service
                //
                SessionState.Path.SetLocation(Path.Combine(CurrentPath(), ServiceName));
                SafeWriteObject(result);
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}