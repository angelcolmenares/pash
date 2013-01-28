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

namespace Microsoft.WindowsAzure.Management.CloudService.Services
{
    using System.Diagnostics;
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;
    using Model;
    using Utilities;
    using WAPPSCmdlet;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;

    public abstract class CloudCmdlet<T> : CmdletBase<T>
        where T : class
    {
        private bool hasOutput = false;
        private IMessageWriter writer;

        protected CloudCmdlet()
        {
        }

        protected CloudCmdlet(IMessageWriter writer)
            : this()
        {
            this.writer = writer;
        }

        public int MaxStringContentLength
        {
            get;
            set;
        }

        protected string GetServiceRootPath() { return PathUtility.FindServiceRootDirectory(CurrentPath()); }

        protected string CurrentPath()
        {
            // SessionState is only available within Powershell so default to
            // the CurrentDirectory when being run from tests.
            return (SessionState != null) ?
                SessionState.Path.CurrentLocation.Path :
                Environment.CurrentDirectory;
        }

        private void SafeWriteObjectInternal(object sendToPipeline)
        {
            if (CommandRuntime != null)
            {
                WriteObject(sendToPipeline);
            }
            else
            {
                Trace.WriteLine(sendToPipeline);
            }
        }

        private void WriteLineIfFirstOutput()
        {
            if (!hasOutput)
            {
                hasOutput = true;
                SafeWriteObjectInternal(Environment.NewLine);
            }
        }

        protected void SafeWriteObject(string message, params object[] args)
        {
            object sendToPipeline = message;
            WriteLineIfFirstOutput();
            if (args.Length > 0)
            {
                sendToPipeline = string.Format(message, args);
            }
            SafeWriteObjectInternal(sendToPipeline);

            if (writer != null)
            {
                writer.Write(sendToPipeline.ToString());
            }
        }

        protected void SafeWriteObjectWithTimestamp(string message, params object[] args)
        {
            SafeWriteObject(string.Format("{0:T} - {1}", DateTime.Now, string.Format(message, args)));
        }

        /// <summary>
        /// Wrap the base Cmdlet's SafeWriteProgress call so that it will not
        /// throw a NotSupportedException when called without a CommandRuntime
        /// (i.e., when not called from within Powershell).
        /// </summary>
        /// <param name="progress">The progress to write.</param>
        protected void SafeWriteProgress(ProgressRecord progress)
        {
            WriteLineIfFirstOutput();

            if (CommandRuntime != null)
            {
                WriteProgress(progress);
            }
            else
            {
                Trace.WriteLine(string.Format("{0}% Complete", progress.PercentComplete));
            }
        }

        /// <summary>
        /// Wrap the base Cmdlet's WriteError call so that it will not throw
        /// a NotSupportedException when called without a CommandRuntime (i.e.,
        /// when not called from within Powershell).
        /// </summary>
        /// <param name="errorRecord">The error to write.</param>
        protected void SafeWriteError(ErrorRecord errorRecord)
        {
            Debug.Assert(errorRecord != null, "errorRecord cannot be null.");

            // If the exception is an Azure Service Management error, pull the
            // Azure message out to the front instead of the generic response.
            errorRecord = AzureServiceManagementException.WrapExistingError(errorRecord);

            if (CommandRuntime != null)
            {
                WriteError(errorRecord);
            }
            else
            {
                Trace.WriteLine(errorRecord);
            }
        }

        /// <summary>
        /// Write an error message for a given exception.
        /// </summary>
        /// <param name="ex">The exception resulting from the error.</param>
        protected void SafeWriteError(Exception ex)
        {
            Debug.Assert(ex != null, "ex cannot be null or empty.");
            SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
        }

        protected override void ProcessRecord()
        {
            Validate.ValidateInternetConnection();
            base.ProcessRecord();
        }

        protected ServiceSettings GetDefaultSettings(string rootPath, string inServiceName, string slot, string location, string storageName, string subscription, out string serviceName)
        {
            ServiceSettings serviceSettings;

            if (string.IsNullOrEmpty(rootPath))
            {
                serviceSettings = ServiceSettings.LoadDefault(null, slot, location, subscription, storageName, inServiceName, null, out serviceName);
            }
            else
            {
                serviceSettings = ServiceSettings.LoadDefault(new AzureService(rootPath, null).Paths.Settings,
                slot, location, subscription, storageName, inServiceName, new AzureService(rootPath, null).ServiceName, out serviceName);
            }

            return serviceSettings;
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected void InvokeInOperationContext(Action action)
        {
            IContextChannel contextChannel = Channel as IContextChannel;
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }
    }
}