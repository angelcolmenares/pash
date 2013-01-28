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

    /// <summary>
    /// Configure the default location for deploying. Stores the new location in settings.json
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceProject")]
    public class SetAzureServiceProjectCommand : SetSettings
    {
        [Parameter(Mandatory = false)]
        public string Location { get; set; }

        [Parameter(Mandatory = false)]
        public string Slot { set; get; }

        [Parameter(Mandatory = false)]
        public string Storage { get; set; }

        [Parameter(Mandatory = false)]
        public string Subscription { get; set; }

        public void SetAzureServiceProjectProcess(string newLocation, string newSlot, string newStorage, string newSubscription, string settingsPath)
        {
            ServiceSettings settings = ServiceSettings.Load(settingsPath);
            if (newLocation != null)
            {
                settings.Location = newLocation;
            }

            if (newSlot != null)
            {
                settings.Slot = newSlot;
            }

            if (newStorage != null)
            {
                settings.StorageAccountName = newStorage;
            }

            if (newSubscription != null)
            {
                settings.Subscription = newSubscription;
            }

            if (newLocation != null || newSlot != null || newStorage != null || newSubscription != null)
            {
                settings.Save(settingsPath);
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = true;
                base.ProcessRecord();
                this.SetAzureServiceProjectProcess(
                    Location,
                    Slot,
                    Storage,
                    Subscription,
                    base.GetServiceSettingsPath(false));
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}