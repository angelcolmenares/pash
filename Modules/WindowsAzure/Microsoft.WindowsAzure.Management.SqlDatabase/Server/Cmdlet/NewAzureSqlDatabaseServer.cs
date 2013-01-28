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
    using System.Management.Automation;
    using System.ServiceModel;
    using System.Xml;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Model;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using WAPPSCmdlet = Microsoft.WindowsAzure.Management.CloudService.WAPPSCmdlet;

    /// <summary>
    /// Creates a new Windows Azure SQL Database server in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSqlDatabaseServer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    public class NewAzureSqlDatabaseServer : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="NewAzureSqlDatabaseServer"/> class.
        /// </summary>
        public NewAzureSqlDatabaseServer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="NewAzureSqlDatabaseServer"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureSqlDatabaseServer(ISqlDatabaseManagement channel)
        {
            this.Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Administrator login name for the new SQL Database server.")]
        [ValidateNotNullOrEmpty]
        public string AdministratorLogin
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Administrator login password for the new SQL Database server.")]
        [ValidateNotNullOrEmpty]
        public string AdministratorLoginPassword
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Location in which to create the new SQL Database server.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm on the creation of the server")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new server in the current subscription.
        /// </summary>
        /// <param name="adminLogin">
        /// The administrator login name for the new server.
        /// </param>
        /// <param name="adminLoginPassword">
        /// The administrator login password for the new server.
        /// </param>
        /// <param name="location">
        /// The location in which to create the new server.
        /// </param>
        /// <returns>The context to the newly created server.</returns>
        internal SqlDatabaseServerContext NewAzureSqlDatabaseServerProcess(string adminLogin, string adminLoginPassword, string location)
        {
            // Do nothing if force is not specified and user cancelled the operation
            if (!Force.IsPresent &&
                !ShouldProcess(
                    Resources.NewAzureSqlDatabaseServerDescription,
                    Resources.NewAzureSqlDatabaseServerWarning,
                    Resources.ShouldProcessCaption))
            {
                return null;
            }

            SqlDatabaseServerContext operationContext = null;
            try
            {
                InvokeInOperationContext(() =>
                {
                    XmlElement serverName = RetryCall(subscription =>
                        Channel.NewServer(subscription, adminLogin, adminLoginPassword, location));
                    WAPPSCmdlet.Operation operation = WaitForSqlDatabaseOperation();

                    operationContext = new SqlDatabaseServerContext()
                    {
                        ServerName = serverName.InnerText,
                        Location = location,
                        AdministratorLogin = adminLogin,
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
                SqlDatabaseServerContext context = this.NewAzureSqlDatabaseServerProcess(this.AdministratorLogin, this.AdministratorLoginPassword, this.Location);

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
