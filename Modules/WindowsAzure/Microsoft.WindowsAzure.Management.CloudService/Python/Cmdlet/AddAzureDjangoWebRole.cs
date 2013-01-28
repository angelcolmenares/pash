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

namespace Microsoft.WindowsAzure.Management.CloudService.Python.Cmdlet
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.Win32;
    using Microsoft.WindowsAzure.Management.CloudService.Utilities;
    using Model;
    using Properties;

    /// <summary>
    /// Create scaffolding for a new Python Django web role, change cscfg file and csdef to include the added web role
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureDjangoWebRole")]
    public class AddAzureDjangoWebRoleCommand : AddRole
    {
        const string PythonCorePath = "SOFTWARE\\Python\\PythonCore";
        const string SupportedPythonVersion = "2.7";
        const string InstallPathSubKey = "InstallPath";
        const string PythonInterpreterExe = "python.exe";
        const string DjangoStartProjectCommand = "-m django.bin.django-admin startproject {0}";

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        internal string AddAzureDjangoWebRoleProcess(string webRoleName, int instances, string rootPath)
        {
            string result;
            AzureService service = new AzureService(rootPath, null);
            RoleInfo webRole = service.AddDjangoWebRole(webRoleName, instances);

            // let Django create it's scaffolding
            var interpPath = FindPythonInterpreterPath();
            if (interpPath != null)
            {
                string stdOut, stdErr;
                Environment.CurrentDirectory = Path.Combine(rootPath, webRole.Name);

                ProcessHelper.StartAndWaitForProcess(
                    new ProcessStartInfo(
                        Path.Combine(interpPath, PythonInterpreterExe),
                        String.Format(DjangoStartProjectCommand, webRole.Name)
                    ),
                    out stdOut,
                    out stdErr
                );

                if (!string.IsNullOrEmpty(stdErr))
                {
                    SafeWriteObject(String.Format(Resources.UnableToCreateDjangoApp, stdErr));
                    SafeWriteObject(Resources.UnableToCreateDjangoAppFix);
                }
            }
            else
            {
                SafeWriteObject(Resources.MissingPythonPreReq);
            }

            try
            {
                service.ChangeRolePermissions(webRole);
            }
            catch (UnauthorizedAccessException)
            {
                SafeWriteObject(Resources.AddRoleMessageInsufficientPermissions);
                SafeWriteObject(Environment.NewLine);
            }

            result = string.Format(Resources.AddRoleMessageCreatePython, rootPath, webRole.Name);
            return result;
        }

        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = true;
                base.ProcessRecord();
                string result = AddAzureDjangoWebRoleProcess(Name, Instances, base.GetServiceRootPath());
                SafeWriteObject(result);
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        internal static string FindPythonInterpreterPath()
        {
            foreach (var baseKey in new[] { Registry.LocalMachine, Registry.CurrentUser })
            {
                using (var python = baseKey.OpenSubKey(PythonCorePath))
                {
                    if (python != null)
                    {
                        foreach (var key in python.GetSubKeyNames())
                        {
                            if (key == SupportedPythonVersion)
                            {
                                var value = python.OpenSubKey(key + "\\" + InstallPathSubKey);
                                if (value != null)
                                {
                                    return value.GetValue("") as string;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}