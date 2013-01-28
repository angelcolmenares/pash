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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Xml;

    /// <summary>
    /// The Windows Azure SQL Database related part of the external API
    /// </summary>
    public partial interface ISqlDatabaseManagement
    {
        /// <summary>
        /// Enumerates SQL Database servers that are provisioned for a subscription.  
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/sqlservers/servers")]
        IAsyncResult BeginGetServers(string subscriptionId, AsyncCallback callback, object state);

        SqlDatabaseServerList EndGetServers(IAsyncResult asyncResult);

        /// <summary>
        /// Adds a new SQL Database server to a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/sqlservers/servers")]
        IAsyncResult BeginNewServer(string subscriptionId, NewSqlDatabaseServerInput input, AsyncCallback callback, object state);

        XmlElement EndNewServer(IAsyncResult asyncResult);

        /// <summary>
        /// Drops a SQL Database server from a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/sqlservers/servers/{serverName}")]
        IAsyncResult BeginRemoveServer(string subscriptionId, string serverName, AsyncCallback callback, object state);

        void EndRemoveServer(IAsyncResult asyncResult);

        /// <summary>
        /// Sets the administrative password of a SQL Database server for a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/sqlservers/servers/{serverName}?op=ResetPassword", BodyStyle = WebMessageBodyStyle.Bare)]
        IAsyncResult BeginSetPassword(string subscriptionId, string serverName, XmlElement password, AsyncCallback callback, object state);

        void EndSetPassword(IAsyncResult asyncResult);

        /// <summary>
        /// Retrieves a list of all the firewall rules for a SQL Database server that belongs to a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/sqlservers/servers/{serverName}/firewallrules")]
        IAsyncResult BeginGetServerFirewallRules(string subscriptionId, string serverName, AsyncCallback callback, object state);

        SqlDatabaseFirewallRulesList EndGetServerFirewallRules(IAsyncResult asyncResult);

        /// <summary>
        /// Creates a new firewall rule for a SQL Database server that belongs to a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/sqlservers/servers/{serverName}/firewallrules")]
        IAsyncResult BeginNewServerFirewallRule(string subscriptionId, string serverName, SqlDatabaseFirewallRuleInput input, AsyncCallback callback, object state);

        void EndNewServerFirewallRule(IAsyncResult asyncResult);

        /// <summary>
        /// Updates an existing firewall rule for a SQL Database server that belongs to a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/sqlservers/servers/{serverName}/firewallrules/{ruleName}")]
        IAsyncResult BeginUpdateServerFirewallRule(string subscriptionId, string serverName, string ruleName, SqlDatabaseFirewallRuleInput input, AsyncCallback callback, object state);

        void EndUpdateServerFirewallRule(IAsyncResult asyncResult);

        /// <summary>
        /// Deletes a firewall rule from a SQL Database server that belongs to a subscription
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/sqlservers/servers/{serverName}/firewallrules/{ruleName}")]
        IAsyncResult BeginRemoveServerFirewallRule(string subscriptionId, string serverName, string ruleName, AsyncCallback callback, object state);

        void EndRemoveServerFirewallRule(IAsyncResult asyncResult);
    }
}
