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

namespace Microsoft.WindowsAzure.Management.Extensions
{
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Diagnostics;
    using System.Data.Services.Client;
    using System.Xml.Linq;
    using Model;

    public static class CmdletExtensions
    {
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void WriteVerboseOutputForObject(this PSCmdlet powerShellCmdlet, object obj)
        {
            bool verbose = powerShellCmdlet.MyInvocation.BoundParameters.ContainsKey("Verbose") && ((SwitchParameter)powerShellCmdlet.MyInvocation.BoundParameters["Verbose"]).ToBool();
            if (verbose == false)
            {
                return;
            }

            string deserializedobj;
            var serializer = new DataContractSerializer(obj.GetType());

            using (var backing = new StringWriter())
            {
                using (var writer = new XmlTextWriter(backing))
                {
                    writer.Formatting = Formatting.Indented;

                    serializer.WriteObject(writer, obj);
                    deserializedobj = backing.ToString();
                }
            }

            deserializedobj = deserializedobj.Replace("/d2p1:", string.Empty);
            deserializedobj = deserializedobj.Replace("d2p1:", string.Empty);
            powerShellCmdlet.WriteVerbose(powerShellCmdlet.CommandRuntime.ToString());
            powerShellCmdlet.WriteVerbose(deserializedobj);
        }

        public static void SafeWriteObject(this PSCmdlet psCmdlet, object sendToPipeline)
        {
            psCmdlet.SafeWriteObject(sendToPipeline, false);
        }

        public static void SafeWriteObject(this PSCmdlet psCmdlet, object sendToPipeline, bool enumerateCollection)
        {
            try
            {
                psCmdlet.WriteObject(sendToPipeline, enumerateCollection);
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        public static void SafeWriteWarning(this PSCmdlet psCmdlet, string text)
        {
            try
            {
                psCmdlet.WriteWarning(text);
            }
            catch (Exception)
            {
                Trace.WriteLine(text);
            }
        }

        public static string TryResolvePath(this PSCmdlet psCmdlet, string path)
        {
            try
            {
                return psCmdlet.ResolvePath(path);
            }
            catch
            {
                return path;
            }
        }

        public static string ResolvePath(this PSCmdlet psCmdlet, string path)
        {
            if (path == null)
            {
                return null;
            }

            if (psCmdlet.SessionState == null)
            {
                return path;
            }

            var result = psCmdlet.SessionState.Path.GetResolvedPSPathFromPSPath(path);
            string fullPath = string.Empty;

            if (result != null && result.Count > 0)
            {
                fullPath = result[0].Path;
            }

            return fullPath;
        }

        public static Exception ProcessExceptionDetails(this PSCmdlet cmdlet, Exception exception)
        {
            if ((exception is DataServiceQueryException) && (exception.InnerException != null))
            {
                var dscException = FindDataServiceClientException(exception.InnerException);

                if (dscException == null)
                {
                    return new InnerDataServiceException(exception.InnerException.Message);
                }

                var message = dscException.Message;
                try
                {
                    XNamespace ns = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
                    XDocument doc = XDocument.Parse(message);
                    if (doc.Root != null)
                    {
                        return new InnerDataServiceException(doc.Root.Element(ns + "message").Value);
                    }
                }
                catch
                {
                    return new InnerDataServiceException(message);
                }
            }

            return exception;
        }

        private static Exception FindDataServiceClientException(Exception ex)
        {
            if (ex is DataServiceClientException)
            {
                return ex;
            }

            return ex.InnerException != null ? FindDataServiceClientException(ex.InnerException) : null;
        }
    }
}
