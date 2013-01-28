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

namespace Microsoft.WindowsAzure.Management.Model
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Samples.WindowsAzure.ServiceManagement;
    using System.ServiceModel;
    using Utilities;

    public class SubscriptionData
    {
        private CloudStorageAccount _currentStorageAccount;

        public string SubscriptionName { get; set; }

        public string SubscriptionId { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public string ServiceEndpoint { get; set; }

        public string SqlAzureServiceEndpoint { get; set; }

        public string CurrentStorageAccount { get; set; }

        public bool IsDefault { get; set; }

        public CloudStorageAccount GetCurrentStorageAccount(IServiceManagement channel)
        {
            if (String.IsNullOrEmpty(CurrentStorageAccount))
            {
                return null;
            }

            if (_currentStorageAccount != null)
            {
                return _currentStorageAccount;
            }

            CloudStorageAccount currentStorage = null;
            using (new OperationContextScope((IContextChannel)channel))
            {
                var storageService = channel.GetStorageService(SubscriptionId, CurrentStorageAccount);
                var storageServiceKeys = channel.GetStorageKeys(SubscriptionId, CurrentStorageAccount);
                if (storageService != null && storageServiceKeys != null)
                {
                    string connectionString = General.BuildConnectionString("https", storageService.ServiceName, storageServiceKeys.StorageServiceKeys.Primary, storageService.StorageServiceProperties.Endpoints[0].Replace("http://", "https://"), storageService.StorageServiceProperties.Endpoints[2].Replace("http://", "https://"), storageService.StorageServiceProperties.Endpoints[1].Replace("http://", "https://"));
                    currentStorage = CloudStorageAccount.Parse(connectionString);
                }
            }

            _currentStorageAccount = currentStorage;
            return currentStorage;
        }

        public void NullCurrentStorageAccount()
        {
            _currentStorageAccount = null;
        }
    }
}