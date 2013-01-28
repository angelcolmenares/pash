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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Model;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using WAPPSCmdlet = Microsoft.WindowsAzure.Management.CloudService.WAPPSCmdlet;

    /// <summary>
    /// Retrieves a list of firewall rule from a Windows Azure SQL Database server in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseServerFirewallRule", ConfirmImpact = ConfirmImpact.None)]
    public class GetAzureSqlDatabaseServerFirewallRule : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAzureSqlDatabaseServerFirewallRule"/> class.
        /// </summary>
        public GetAzureSqlDatabaseServerFirewallRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAzureSqlDatabaseServerFirewallRule"/> class.
        /// </summary>
        /// <param name="channel">Channel used for communication with Azure's service management APIs.</param>
        public GetAzureSqlDatabaseServerFirewallRule(ISqlDatabaseManagement channel)
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

        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "SQL Database server firewall rule name.")]
        [ValidateNotNullOrEmpty]
        public string RuleName
        {
            get;
            set;
        }

        /// <summary>
        /// Retrieves one or more firewall rules on the specified server.
        /// </summary>
        /// <param name="serverName">
        /// The name of the server to retrieve firewall rules for.
        /// </param>
        /// <param name="ruleName">
        /// The specific name of the rule to retrieve, or <c>null</c> to
        /// retrieve all rules on the specified server.
        /// </param>
        /// <returns>A list of firewall rules on the server.</returns>
        internal IEnumerable<SqlDatabaseServerFirewallRuleContext> GetAzureSqlDatabaseServerFirewallRuleProcess(string serverName, string ruleName)
        {
            IEnumerable<SqlDatabaseServerFirewallRuleContext> processResult = null;

            try
            {
                InvokeInOperationContext(() =>
                {
                    SqlDatabaseFirewallRulesList firewallRules = RetryCall(subscription =>
                        Channel.GetServerFirewallRules(subscription, this.ServerName));
                    WAPPSCmdlet.Operation operation = WaitForSqlDatabaseOperation();

                    if (string.IsNullOrEmpty(ruleName))
                    {
                        // Firewall rule name is not specified, return all
                        // firewall rules.
                        processResult = firewallRules.Select(p => new SqlDatabaseServerFirewallRuleContext()
                        {
                            OperationDescription = CommandRuntime.ToString(),
                            OperationId = operation.OperationTrackingId,
                            OperationStatus = operation.Status,
                            ServerName = serverName,
                            RuleName = p.Name,
                            StartIpAddress = p.StartIPAddress,
                            EndIpAddress = p.EndIPAddress
                        });
                    }
                    else
                    {
                        // Firewall rule name is specified, find the one
                        // with the specified rule name and return that.
                        SqlDatabaseFirewallRule firewallRule = firewallRules.FirstOrDefault(p => p.Name == ruleName);
                        if (firewallRule != null)
                        {
                            processResult = new List<SqlDatabaseServerFirewallRuleContext>
                            {
                                new SqlDatabaseServerFirewallRuleContext
                                {
                                    OperationDescription = CommandRuntime.ToString(),
                                    OperationId = operation.OperationTrackingId,
                                    OperationStatus = operation.Status,
                                    ServerName = serverName,
                                    RuleName = firewallRule.Name,
                                    StartIpAddress = firewallRule.StartIPAddress,
                                    EndIpAddress = firewallRule.EndIPAddress
                                }
                            };
                        }
                        else
                        {
                            throw new ItemNotFoundException(string.Format(CultureInfo.InvariantCulture, Resources.GetAzureSqlDatabaseServerFirewallRuleNotFound, ruleName, serverName));
                        }
                    }

                });
            }
            catch (CommunicationException ex)
            {
                this.WriteErrorDetails(ex);
            }

            return processResult;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                var rules = this.GetAzureSqlDatabaseServerFirewallRuleProcess(this.ServerName, this.RuleName);

                if (rules != null)
                {
                    WriteObject(rules, true);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
