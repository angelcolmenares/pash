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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;

    public static class SqlDatabaseManagementHelper
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing the factory would also dispose the channel we are returning.")]
        public static ISqlDatabaseManagement CreateSqlDatabaseManagementChannel(Binding binding, Uri remoteUri, X509Certificate2 cert, string requestSessionId)
        {
            WebChannelFactory<ISqlDatabaseManagement> factory = new WebChannelFactory<ISqlDatabaseManagement>(binding, remoteUri);
            factory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector(requestSessionId));
            factory.Credentials.ClientCertificate.Certificate = cert;
            return factory.CreateChannel();
        }

        /// <summary>
        /// Generates a client side tracing Id of the format:
        /// [Guid]-[Time in UTC]
        /// </summary>
        /// <returns>A string representation of the client side tracing Id.</returns>
        public static string GenerateClientTracingId()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("u"));
        }

        /// <summary>
        /// Retrieves the exception details contained in the exception and wrap it in a PowerShell <see cref="ErrorRecord"/>.
        /// </summary>
        /// <param name="exception">The exception containing the error details.</param>
        /// <param name="errorRecord">An output parameter for the error record containing the error details.</param>
        /// <param name="requestId">An output parameter for the request Id present in the reponse headers.</param>
        public static void RetrieveExceptionDetails(Exception exception, out ErrorRecord errorRecord, out string requestId)
        {
            errorRecord = null;
            requestId = null;

            // Look for known exceptions through the exceptions and inner exceptions
            Exception innerException = exception;
            while (innerException != null)
            {
                WebException webException = innerException as WebException;
                if ((webException != null) &&
                    (webException.Response != null))
                {
                    HttpWebResponse response = webException.Response as HttpWebResponse;

                    // Extract the request Ids and write them as warnings
                    if (response.Headers != null)
                    {
                        requestId = response.Headers[Constants.RequestIdHeaderName];
                    }

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        responseStream.Seek(0, SeekOrigin.Begin);
                        // Check if it's a service resource error message
                        ServiceResourceError serviceResourceError;
                        if (ServiceResourceError.TryParse(responseStream, out serviceResourceError))
                        {
                            errorRecord = new ErrorRecord(new CommunicationException(serviceResourceError.Message), string.Empty, ErrorCategory.InvalidOperation, null);
                            break;
                        }

                        responseStream.Seek(0, SeekOrigin.Begin);
                        // Check if it's a database management error message
                        SqlDatabaseManagementError databaseManagementError;
                        if (SqlDatabaseManagementError.TryParse(responseStream, out databaseManagementError))
                        {
                            string errorDetails = string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.DatabaseManagementErrorFormat,
                                databaseManagementError.Code,
                                databaseManagementError.Message);

                            errorRecord = new ErrorRecord(new CommunicationException(errorDetails), string.Empty, ErrorCategory.InvalidOperation, null);
                            break;
                        }
                    }

                    // Check if it's a not found message
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        string message = string.Format(CultureInfo.InvariantCulture, Resources.UriDoesNotExist, response.ResponseUri.AbsoluteUri);
                        string errorDetails = string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.DatabaseManagementErrorFormat,
                            response.StatusCode.ToString(),
                            message);

                        errorRecord = new ErrorRecord(new CommunicationException(errorDetails), string.Empty, ErrorCategory.InvalidOperation, null);
                        break;
                    }
                }

                innerException = innerException.InnerException;
            }

            // If it's here, it was an unknown exception, wrap the original exception as is.
            if (errorRecord == null)
            {
                errorRecord = new ErrorRecord(exception, string.Empty, ErrorCategory.NotSpecified, null);
            }
        }
    }
}
