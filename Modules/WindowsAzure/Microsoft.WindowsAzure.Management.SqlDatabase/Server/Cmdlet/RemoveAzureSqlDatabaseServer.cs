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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Server.Cmdlet
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Model;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using WAPPSCmdlet = Microsoft.WindowsAzure.Management.CloudService.WAPPSCmdlet;

    /// <summary>
    /// Removes an existing Windows Azure SQL Database server in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureSqlDatabaseServer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveAzureSqlDatabaseServer : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="RemoveAzureSqlDatabaseServer"/> class.
        /// </summary>
        public RemoveAzureSqlDatabaseServer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="RemoveAzureSqlDatabaseServer"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public RemoveAzureSqlDatabaseServer(ISqlDatabaseManagement channel)
        {
            this.Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "SQL Database server name.")]
        [ValidateNotNullOrEmpty]
        public string ServerName
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm on the deletion of the server")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Removes an existing server in the current subscription.
        /// </summary>
        /// <param name="serverName">
        /// The name of the server to remove.
        /// </param>
        /// <returns>The context to this operation.</returns>
        internal SqlDatabaseServerOperationContext RemoveAzureSqlDatabaseServerProcess(string serverName)
        {
            // Do nothing if force is not specified and user cancelled the operation
            if (!Force.IsPresent &&
                !ShouldProcess(
                    string.Format(CultureInfo.InvariantCulture, Resources.RemoveAzureSqlDatabaseServerDescription, serverName),
                    string.Format(CultureInfo.InvariantCulture, Resources.RemoveAzureSqlDatabaseServerWarning, serverName),
                    Resources.ShouldProcessCaption))
            {
                return null;
            }

            SqlDatabaseServerOperationContext operationContext = null;
            try
            {
                InvokeInOperationContext(() =>
                {
                    RetryCall(subscription =>
                        Channel.RemoveServer(subscription, serverName));
                    WAPPSCmdlet.Operation operation = WaitForSqlDatabaseOperation();

                    operationContext = new SqlDatabaseServerOperationContext()
                    {
                        ServerName = serverName,
                        OperationStatus = operation.Status,
                        OperationDescription = CommandRuntime.ToString(),
                        OperationId = operation.OperationTrackingId
                    };
                });
            }
            catch (CommunicationException ex)
            {
                this.WriteErrorDetails(ex);
            }

            return operationContext;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                this.RemoveAzureSqlDatabaseServerProcess(this.ServerName);
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
