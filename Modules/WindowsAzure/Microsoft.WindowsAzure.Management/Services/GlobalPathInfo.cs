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

namespace Microsoft.WindowsAzure.Management.Services
{
    using System;
    using System.IO;
    using Properties;

    public class GlobalPathInfo
    {
        public string PublishSettingsFile { get; private set; }
        public string SubscriptionsDataFile { get; private set; }
        public string ServiceConfigurationFile { get; private set; }

        public static string AzureAppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Resources.AzureDirectoryName);

        /// <summary>
        /// Path to the global settings directory used by GlobalComponents.
        /// </summary>
        private static string _globalSettingsDirectory;

        /// <summary>
        /// Gets a path to the global settings directory, which defaults to
        /// AzureSdkAppDir.  This can be set internally for the purpose of
        /// testing.
        /// </summary>
        public static string GlobalSettingsDirectory
        {
            get { return _globalSettingsDirectory ?? AzureAppDir; }
            internal set { _globalSettingsDirectory = value; }
        }
	
        public string AzureDirectory { get; private set; }

        public GlobalPathInfo(string rootPath)
            : this(rootPath, null)
        {
        }

        public GlobalPathInfo(string rootPath, string subscriptionsDataFile)
        {
            PublishSettingsFile = Path.Combine(rootPath, Resources.PublishSettingsFileName);
            SubscriptionsDataFile = subscriptionsDataFile ?? Path.Combine(rootPath, Resources.SubscriptionDataFileName);
            ServiceConfigurationFile = Path.Combine(rootPath, Resources.ConfigurationFileName);
            AzureDirectory = rootPath;
        }
    }
}