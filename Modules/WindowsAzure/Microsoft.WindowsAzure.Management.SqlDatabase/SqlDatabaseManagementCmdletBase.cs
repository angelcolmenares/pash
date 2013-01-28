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

namespace Microsoft.WindowsAzure.Management.SqlDatabase
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.Management.CloudService.Services;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using WAPPSCmdlet = Microsoft.WindowsAzure.Management.CloudService.WAPPSCmdlet;

    /// <summary>
    /// The base class for all Windows Azure Sql Database Management Cmdlets
    /// </summary>
    public abstract class SqlDatabaseManagementCmdletBase : CloudCmdlet<ISqlDatabaseManagement>
    {
        /// <summary>
        /// Stores the session Id for all the request made in this session.
        /// </summary>
        internal static string clientSessionId;

        static SqlDatabaseManagementCmdletBase()
        {
            clientSessionId = SqlDatabaseManagementHelper.GenerateClientTracingId();
        }

        /// <summary>
        /// Stores the per request session Id for all request made in this cmdlet call.
        /// </summary>
        private string clientRequestId;

        internal SqlDatabaseManagementCmdletBase()
        {
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();
        }

        /// <summary>
        /// Gets or sets a value indicating whether CreateChannel should share
        /// the command's current Channel when asking for a new one.  This is
        /// only used for testing.
        /// </summary>
        internal bool ShareChannel { get; set; }

        protected override ISqlDatabaseManagement CreateChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return Channel;
            }

            if (this.ServiceBinding == null)
            {
                this.ServiceBinding = ConfigurationConstants.WebHttpBinding(this.MaxStringContentLength);
            }

            if (string.IsNullOrEmpty(CurrentSubscription.ServiceEndpoint))
            {
                this.ServiceEndpoint = ConfigurationConstants.ServiceManagementEndpoint;
            }
            else
            {
                this.ServiceEndpoint = CurrentSubscription.ServiceEndpoint;
            }

            return SqlDatabaseManagementHelper.CreateSqlDatabaseManagementChannel(this.ServiceBinding, new Uri(this.ServiceEndpoint), CurrentSubscription.Certificate, this.clientRequestId);
        }

        // Windows Azure SQL Database doesn't support async calls
        protected static WAPPSCmdlet.Operation WaitForSqlDatabaseOperation()
        {
            string operationId = RetrieveOperationId();
            WAPPSCmdlet.Operation operation = new WAPPSCmdlet.Operation();
            operation.OperationTrackingId = operationId;
            operation.Status = "Success";
            return operation;
        }

        protected override void WriteErrorDetails(CommunicationException exception)
        {
            string requestId;
            ErrorRecord errorRecord;
            SqlDatabaseManagementHelper.RetrieveExceptionDetails(exception, out errorRecord, out requestId);

            // Write the request Id as a warning
            if (requestId != null)
            {
                // requestId was availiable from the server response, write that as warning to the console
                WriteWarning(string.Format(CultureInfo.InvariantCulture, Resources.ExceptionRequestId, requestId));
            }
            else
            {
                // requestId was not availiable from the server response, write the client Ids that was sent
                WriteWarning(string.Format(CultureInfo.InvariantCulture, Resources.ExceptionClientSessionId, SqlDatabaseManagementCmdletBase.clientSessionId));
                WriteWarning(string.Format(CultureInfo.InvariantCulture, Resources.ExceptionClientRequestId, this.clientRequestId));
            }

            // Write the actual errorRecord containing the exception details
            WriteError(errorRecord);
        }
    }
}