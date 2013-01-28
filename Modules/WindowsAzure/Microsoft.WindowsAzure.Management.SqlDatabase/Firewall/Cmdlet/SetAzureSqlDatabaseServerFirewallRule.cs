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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Firewall.Cmdlet
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
    /// Update an existing firewall rule for a Windows Azure SQL Database server in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureSqlDatabaseServerFirewallRule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    public class SetAzureSqlDatabaseServerFirewallRule : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="SetAzureSqlDatabaseServerFirewallRule"/> class.
        /// </summary>
        public SetAzureSqlDatabaseServerFirewallRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="SetAzureSqlDatabaseServerFirewallRule"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public SetAzureSqlDatabaseServerFirewallRule(ISqlDatabaseManagement channel)
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

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "SQL Database server firewall rule name.")]
        [ValidateNotNullOrEmpty]
        public string RuleName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Start of the IP Range.", ParameterSetName = "IpRange")]
        [ValidateNotNullOrEmpty]
        public string StartIpAddress
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "End of the IP Range.", ParameterSetName = "IpRange")]
        [ValidateNotNullOrEmpty]
        public string EndIpAddress
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm on the creation of the firewall rule")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Updates a firewall rule on the specified server.
        /// </summary>
        /// <param name="serverName">
        /// The name of the server containing the firewall rule.
        /// </param>
        /// <param name="ruleName">
        /// The name of the firewall rule to update.
        /// </param>
        /// <param name="startIpAddress">
        /// The starting IP address for the firewall rule.
        /// </param>
        /// <param name="endIpAddress">
        /// The ending IP address for the firewall rule.
        /// </param>
        /// <returns>The updated firewall rule.</returns>
        internal SqlDatabaseServerFirewallRuleContext SetAzureSqlDatabaseServerFirewallRuleProcess(string serverName, string ruleName, string startIpAddress, string endIpAddress)
        {
            // Do nothing if force is not specified and user cancelled the operation
            if (!Force.IsPresent &&
                !ShouldProcess(
                    string.Format(CultureInfo.InvariantCulture, Resources.SetAzureSqlDatabaseServerFirewallRuleDescription, ruleName, serverName), 
                    string.Format(CultureInfo.InvariantCulture, Resources.SetAzureSqlDatabaseServerFirewallRuleWarning, ruleName, serverName),
                    Resources.ShouldProcessCaption))
            {
                return null;
            }

            SqlDatabaseServerFirewallRuleContext operationContext = null;
            try
            {
                InvokeInOperationContext(() =>
                {
                    RetryCall(subscription =>
                        Channel.UpdateServerFirewallRule(subscription, serverName, ruleName, startIpAddress, endIpAddress));
                    WAPPSCmdlet.Operation operation = WaitForSqlDatabaseOperation();

                    operationContext = new SqlDatabaseServerFirewallRuleContext()
                    {
                        OperationDescription = CommandRuntime.ToString(),
                        OperationStatus = operation.Status,
                        OperationId = operation.OperationTrackingId,
                        ServerName = serverName,
                        RuleName = ruleName,
                        StartIpAddress = startIpAddress,
                        EndIpAddress = endIpAddress
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
                SqlDatabaseServerOperationContext context = this.SetAzureSqlDatabaseServerFirewallRuleProcess(this.ServerName, this.RuleName, this.StartIpAddress, this.EndIpAddress);

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.WriteError, null));
            }
        }
    }
}
