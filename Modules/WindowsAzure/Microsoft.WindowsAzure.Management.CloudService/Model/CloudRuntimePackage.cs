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
    using System;
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;

    public class CloudRuntimePackage
    {
        public const string VersionKey = "version";
        public const string FileKey = "filepath";
        public const string DefaultKey = "default";
        public const string RuntimeKey = "type";

        public CloudRuntimePackage(XmlNode versionNode, string baseUri)
        {
            this.Version = versionNode.Attributes[CloudRuntimePackage.VersionKey].Value;
            string filePath = versionNode.Attributes[CloudRuntimePackage.FileKey].Value;
            this.PackageUri = GetUri(baseUri, filePath);
            this.Runtime = GetRuntimeType(versionNode.Attributes[CloudRuntimePackage.RuntimeKey].Value);
            XmlAttribute defaultAttribute = versionNode.Attributes[CloudRuntimePackage.DefaultKey];
            this.IsDefault = defaultAttribute != null && bool.Parse(defaultAttribute.Value);
        }

        public Runtime Runtime
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public Uri PackageUri
        {
            get;
            private set;
        }

        public bool IsDefault
        {
            get;
            private set;
        }

        private static Uri GetUri(string baseUri, string filePath)
        {
            UriBuilder baseBuilder = new UriBuilder(baseUri);
            baseBuilder.Path = filePath;
            return baseBuilder.Uri;
        }

        private static Runtime GetRuntimeType(string typeValue)
        {
            Debug.Assert(typeValue != null);
            foreach (Runtime runtime in Enum.GetValues(typeof(Runtime)))
            {
                string comparisonValue = Enum.GetName(typeof(Runtime), runtime);
                if (string.Equals(typeValue, comparisonValue, StringComparison.OrdinalIgnoreCase))
                {
                    return runtime;
                }
            }

            throw new ArgumentException(string.Format(Resources.InvalidRuntimeError, typeValue));
        }
    }
}
