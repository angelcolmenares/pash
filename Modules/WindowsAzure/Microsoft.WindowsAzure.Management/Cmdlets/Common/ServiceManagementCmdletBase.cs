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

namespace Microsoft.WindowsAzure.Management.Cmdlets.Common
{
    using System;
    using Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Service;

    public class ServiceManagementCmdletBase : CmdletBase<IServiceManagement>
    {
        protected override IServiceManagement CreateChannel()
        {
            if (ServiceBinding == null)
            {
                ServiceBinding = ConfigurationConstants.WebHttpBinding();
            }

            ServiceEndpoint = string.IsNullOrEmpty(CurrentSubscription.ServiceEndpoint) 
                ? ConfigurationConstants.ServiceManagementEndpoint 
                : CurrentSubscription.ServiceEndpoint;

            return ServiceManagementHelper.CreateServiceManagementChannel(ServiceBinding, new Uri(ServiceEndpoint), CurrentSubscription.Certificate);
        }
    }
}
