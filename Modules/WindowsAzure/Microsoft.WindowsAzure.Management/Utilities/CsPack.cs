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

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using AzureDeploymentTool.Model;
using AzureDeploymentTool.Properties;
using AzureDeploymentTool.ServiceDefinitionSchema;

namespace AzureDeploymentTool.Utilities
{
    class CsPack
    {
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static void CreatePackage(ServiceDefinition definition, string rootPath, PackageType type, out string standardOutput, out string standardError)
        {
            string arguments;

            arguments = ConstructArgs(definition, rootPath, type);
            Execute(arguments, out standardOutput, out standardError);
        }

        private static string ConstructArgs(ServiceDefinition serviceDefinition, string rootPath, PackageType type)
        {
            string arguments;
            string rolesArg = "";
            string sitesArg = "";

            if (serviceDefinition == null) throw new ArgumentNullException("serviceDefinition", string.Format(Resources.InvalidOrEmptyArgumentMessage, "Service definition"));
            if (string.IsNullOrEmpty(rootPath) || System.IO.File.Exists(rootPath)) throw new ArgumentException(Resources.InvalidRootNameMessage, "rootPath");

            if (serviceDefinition.WebRole != null)
            {
                foreach (WebRole webRole in serviceDefinition.WebRole)
                {
                    rolesArg += string.Format(Resources.RoleArgTemplate, webRole.name, rootPath);

                    foreach (Site site in webRole.Sites.Site)
                    {
                        sitesArg += string.Format(Resources.SitesArgTemplate, webRole.name, site.name, rootPath);
                    }
                }
            }

            if (serviceDefinition.WorkerRole != null)
            {
                foreach (WorkerRole workerRole in serviceDefinition.WorkerRole)
                {
                    rolesArg += string.Format(Resources.RoleArgTemplate, workerRole.name, rootPath);
                }
            }

            arguments = string.Format((type == PackageType.Local) ? Resources.CsPackLocalArg : Resources.CsPackCloudArg, rootPath, rolesArg, sitesArg);
            return arguments;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        private static void Execute(string arguments, out string standardOutput, out string standardError)
        {
            ProcessStartInfo pi = new ProcessStartInfo(
                Path.Combine(General.AzureSDKBinFolder, Resources.CsPackExe),
                arguments);
            ProcessHelper.StartAndWaitForProcess(pi, out standardOutput, out standardError);
        }
    }
}