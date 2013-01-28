namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class QueryRunspaces
    {
        private static readonly object s_SyncObject = new object();
        private static TypeTable s_TypeTable;
        private bool stopProcessing = false;

        internal QueryRunspaces()
        {
        }

        internal static string ExtractMessage(Exception e, out int errorCode)
        {
            errorCode = 0;
            if ((e == null) || (e.Message == null))
            {
                return string.Empty;
            }
            string str = null;
            try
            {
                XmlReaderSettings settings = InternalDeserializer.XmlReaderSettingsForUntrustedXmlDocument.Clone();
                settings.MaxCharactersInDocument = 0x1000L;
                settings.MaxCharactersFromEntities = 0x400L;
                settings.DtdProcessing = DtdProcessing.Prohibit;
                using (XmlReader reader = XmlReader.Create(new StringReader(e.Message), settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.LocalName.Equals("Message", StringComparison.OrdinalIgnoreCase))
                            {
                                str = reader.ReadElementString();
                            }
                            else if (reader.LocalName.Equals("WSManFault", StringComparison.OrdinalIgnoreCase))
                            {
                                string attribute = reader.GetAttribute("Code");
                                if (attribute != null)
                                {
                                    try
                                    {
                                        long num = Convert.ToInt64(attribute, NumberFormatInfo.InvariantInfo);
                                        errorCode = (int) num;
                                        continue;
                                    }
                                    catch (FormatException)
                                    {
                                        continue;
                                    }
                                    catch (OverflowException)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException)
            {
            }
            if (str == null)
            {
                return e.Message;
            }
            return str;
        }

        internal Collection<PSSession> GetDisconnectedSessions(Collection<WSManConnectionInfo> connectionInfos, PSHost host, ObjectStream stream, RunspaceRepository runspaceRepository, int throttleLimit, SessionFilterState filterState, Guid[] matchIds, string[] matchNames, string configurationName)
        {
            Collection<PSSession> collection = new Collection<PSSession>();
            foreach (WSManConnectionInfo info in connectionInfos)
            {
                Runspace[] runspaceArray = null;
                try
                {
                    runspaceArray = Runspace.GetRunspaces(info, host, BuiltInTypesTable);
                }
                catch (RuntimeException exception)
                {
                    if (!(exception.InnerException is InvalidOperationException))
                    {
                        throw;
                    }
                    if ((stream.ObjectWriter != null) && stream.ObjectWriter.IsOpen)
                    {
                        int num;
                        string message = StringUtil.Format(RemotingErrorIdStrings.QueryForRunspacesFailed, info.ComputerName, ExtractMessage(exception.InnerException, out num));
                        string fQEIDFromTransportError = WSManTransportManagerUtils.GetFQEIDFromTransportError(num, "RemotePSSessionQueryFailed");
                        Exception exception2 = new RuntimeException(message, exception.InnerException);
                        ErrorRecord errorRecord = new ErrorRecord(exception2, fQEIDFromTransportError, ErrorCategory.InvalidOperation, info);
                        stream.ObjectWriter.Write(errorRecord);
                        /*
                        stream.ObjectWriter.Write(delegate (Cmdlet cmdlet) {
                            cmdlet.WriteError(errorRecord);
                        });
                        */
                    }
                }
                if (this.stopProcessing)
                {
                    break;
                }
                if (runspaceArray != null)
                {
                    string str3 = null;
                    if (!string.IsNullOrEmpty(configurationName))
                    {
                        str3 = (configurationName.IndexOf("http://schemas.microsoft.com/powershell/", StringComparison.OrdinalIgnoreCase) != -1) ? configurationName : ("http://schemas.microsoft.com/powershell/" + configurationName);
                    }
                    foreach (Runspace runspace in runspaceArray)
                    {
                        if (str3 != null)
                        {
                            WSManConnectionInfo connectionInfo = runspace.ConnectionInfo as WSManConnectionInfo;
                            if ((connectionInfo != null) && !str3.Equals(connectionInfo.ShellUri, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                        }
                        PSSession item = null;
                        if (runspaceRepository != null)
                        {
                            item = runspaceRepository.GetItem(runspace.InstanceId);
                        }
                        if ((item != null) && UseExistingRunspace(item.Runspace, runspace))
                        {
                            if (this.TestRunspaceState(item.Runspace, filterState))
                            {
                                collection.Add(item);
                            }
                        }
                        else if (this.TestRunspaceState(runspace, filterState))
                        {
                            collection.Add(new PSSession(runspace as RemoteRunspace));
                        }
                    }
                }
            }
            if ((matchIds != null) && (collection.Count > 0))
            {
                Collection<PSSession> collection2 = new Collection<PSSession>();
                foreach (Guid guid in matchIds)
                {
                    bool flag = false;
                    foreach (PSSession session2 in collection)
                    {
                        if (this.stopProcessing)
                        {
                            break;
                        }
                        if (session2.Runspace.InstanceId.Equals(guid))
                        {
                            flag = true;
                            collection2.Add(session2);
                            break;
                        }
                    }
                    if ((!flag && (stream.ObjectWriter != null)) && stream.ObjectWriter.IsOpen)
                    {
                        Exception exception3 = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.SessionIdMatchFailed, guid));
                        ErrorRecord errorRecord = new ErrorRecord(exception3, "PSSessionIdMatchFail", ErrorCategory.InvalidOperation, guid);
                        stream.ObjectWriter.Write(errorRecord);
                        /*
                        stream.ObjectWriter.Write(delegate (Cmdlet cmdlet) {
                            cmdlet.WriteError(errorRecord);
                        });
                        */
                    }
                }
                return collection2;
            }
            if ((matchNames == null) || (collection.Count <= 0))
            {
                return collection;
            }
            Collection<PSSession> collection3 = new Collection<PSSession>();
            foreach (string str5 in matchNames)
            {
                WildcardPattern pattern = new WildcardPattern(str5, WildcardOptions.IgnoreCase);
                bool flag2 = false;
                foreach (PSSession session3 in collection)
                {
                    if (this.stopProcessing)
                    {
                        break;
                    }
                    if (pattern.IsMatch(((RemoteRunspace) session3.Runspace).RunspacePool.RemoteRunspacePoolInternal.Name))
                    {
                        flag2 = true;
                        collection3.Add(session3);
                    }
                }
                if ((!flag2 && (stream.ObjectWriter != null)) && stream.ObjectWriter.IsOpen)
                {
                    Exception exception4 = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.SessionNameMatchFailed, str5));
                    ErrorRecord errorRecord = new ErrorRecord(exception4, "PSSessionNameMatchFail", ErrorCategory.InvalidOperation, str5);
                    stream.ObjectWriter.Write(errorRecord);
                    /*
                    stream.ObjectWriter.Write(delegate (Cmdlet cmdlet) {
                        cmdlet.WriteError(errorRecord);
                    });
                    */
                }
            }
            return collection3;
        }

        internal void StopAllOperations()
        {
            this.stopProcessing = true;
        }

        private bool TestRunspaceState(Runspace runspace, SessionFilterState filterState)
        {
            switch (filterState)
            {
                case SessionFilterState.All:
                    return true;

                case SessionFilterState.Opened:
                    return (runspace.RunspaceStateInfo.State == RunspaceState.Opened);

                case SessionFilterState.Disconnected:
                    return (runspace.RunspaceStateInfo.State == RunspaceState.Disconnected);

                case SessionFilterState.Closed:
                    return (runspace.RunspaceStateInfo.State == RunspaceState.Closed);

                case SessionFilterState.Broken:
                    return (runspace.RunspaceStateInfo.State == RunspaceState.Broken);
            }
            return false;
        }

        private static bool UseExistingRunspace(Runspace existingRunspace, Runspace queriedrunspace)
        {
            if (existingRunspace.RunspaceStateInfo.State == RunspaceState.Broken)
            {
                return false;
            }
            if ((existingRunspace.RunspaceStateInfo.State == RunspaceState.Disconnected) && (queriedrunspace.RunspaceAvailability == RunspaceAvailability.Busy))
            {
                return false;
            }
            return true;
        }

        internal static TypeTable BuiltInTypesTable
        {
            get
            {
                if (s_TypeTable == null)
                {
                    lock (s_SyncObject)
                    {
                        if (s_TypeTable == null)
                        {
                            s_TypeTable = TypeTable.LoadDefaultTypeFiles();
                        }
                    }
                }
                return s_TypeTable;
            }
        }
    }
}

