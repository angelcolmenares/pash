namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class RemotingDecoder
    {
        private static T ConvertPropertyValueTo<T>(string propertyName, object propertyValue)
        {
            if (propertyName == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyName");
            }
            if (typeof(T).IsEnum)
            {
                if (propertyValue is string)
                {
                    try
                    {
                        string str = (string) propertyValue;
                        return (T) Enum.Parse(typeof(T), str, true);
                    }
                    catch (ArgumentException)
                    {
                        throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastPropertyToExpectedType, new object[] { propertyName, typeof(T).FullName, propertyValue.GetType().FullName });
                    }
                }
                try
                {
                    Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                    return (T) LanguagePrimitives.ConvertTo(propertyValue, underlyingType, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException)
                {
                    throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastPropertyToExpectedType, new object[] { propertyName, typeof(T).FullName, propertyValue.GetType().FullName });
                }
            }
            if (typeof(T).Equals(typeof(PSObject)))
            {
                if (propertyValue == null)
                {
                    return default(T);
                }
                return (T) Convert.ChangeType(PSObject.AsPSObject(propertyValue), typeof(T));
            }
            if (propertyValue == null)
            {
                if (typeof(T).IsValueType && (!typeof(T).IsGenericType || !typeof(T).GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
                {
                    throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastPropertyToExpectedType, new object[] { propertyName, typeof(T).FullName, (propertyValue != null) ? propertyValue.GetType().FullName : "null" });
                }
                return default(T);
            }
            if (propertyValue is T)
            {
                return (T) propertyValue;
            }
            if (propertyValue is PSObject)
            {
                PSObject obj3 = (PSObject) propertyValue;
                return ConvertPropertyValueTo<T>(propertyName, obj3.BaseObject);
            }
            if ((propertyValue is Hashtable) && typeof(T).Equals(typeof(PSPrimitiveDictionary)))
            {
                try
                {
                    return (T) Convert.ChangeType(new PSPrimitiveDictionary((Hashtable) propertyValue), typeof(T));
                }
                catch (ArgumentException)
                {
                    throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastPropertyToExpectedType, new object[] { propertyName, typeof(T).FullName, (propertyValue != null) ? propertyValue.GetType().FullName : "null" });
                }
            }
            throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastPropertyToExpectedType, new object[] { propertyName, typeof(T).FullName, propertyValue.GetType().FullName });
        }

        internal static IEnumerable<KeyValuePair<KeyType, ValueType>> EnumerateHashtableProperty<KeyType, ValueType>(PSObject psObject, string propertyName)
        {
            if (psObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("psObject");
            }
            if (propertyName == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyName");
            }
            Hashtable propertyValue = GetPropertyValue<Hashtable>(psObject, propertyName);
            if (propertyValue != null)
            {
                IDictionaryEnumerator enumerator = propertyValue.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                    KeyType key = ConvertPropertyValueTo<KeyType>(propertyName, current.Key);
                    ValueType iteratorVariable3 = ConvertPropertyValueTo<ValueType>(propertyName, current.Value);
                    yield return new KeyValuePair<KeyType, ValueType>(key, iteratorVariable3);
                }
            }
        }

        internal static IEnumerable<T> EnumerateListProperty<T>(PSObject psObject, string propertyName)
        {
            if (psObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("psObject");
            }
            if (propertyName == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyName");
            }
            IEnumerable propertyValue = GetPropertyValue<IEnumerable>(psObject, propertyName);
            if (propertyValue != null)
            {
                IEnumerator enumerator = propertyValue.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                    yield return ConvertPropertyValueTo<T>(propertyName, current);
                }
            }
        }

        internal static bool GetAddToHistory(object data)
        {
            PSObject psObject = PSObject.AsPSObject(data);
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            return GetPropertyValue<bool>(psObject, "AddToHistory");
        }

        internal static ApartmentState GetApartmentState(object data)
        {
            return GetPropertyValue<ApartmentState>(PSObject.AsPSObject(data), "ApartmentState");
        }

        internal static PSPrimitiveDictionary GetApplicationArguments(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<PSPrimitiveDictionary>(dataAsPSObject, "ApplicationArguments");
        }

        internal static PSPrimitiveDictionary GetApplicationPrivateData(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<PSPrimitiveDictionary>(dataAsPSObject, "ApplicationPrivateData");
        }

        internal static PowerShell GetCommandDiscoveryPipeline(object data)
        {
            string[] strArray;
            string[] strArray2;
            object[] objArray;
            PSObject psObject = PSObject.AsPSObject(data);
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            CommandTypes propertyValue = GetPropertyValue<CommandTypes>(psObject, "CommandType");
            if (GetPropertyValue<PSObject>(psObject, "Name") != null)
            {
                strArray = new List<string>(EnumerateListProperty<string>(psObject, "Name")).ToArray();
            }
            else
            {
                strArray = new string[] { "*" };
            }
            if (GetPropertyValue<PSObject>(psObject, "Namespace") != null)
            {
                strArray2 = new List<string>(EnumerateListProperty<string>(psObject, "Namespace")).ToArray();
            }
            else
            {
                strArray2 = new string[] { "" };
            }
            if (GetPropertyValue<PSObject>(psObject, "ArgumentList") != null)
            {
                objArray = new List<object>(EnumerateListProperty<object>(psObject, "ArgumentList")).ToArray();
            }
            else
            {
                objArray = null;
            }
            PowerShell shell = PowerShell.Create();
            shell.AddCommand("Get-Command");
            shell.AddParameter("Name", strArray);
            shell.AddParameter("CommandType", propertyValue);
            shell.AddParameter("Module", strArray2);
            shell.AddParameter("ArgumentList", objArray);
            return shell;
        }

        internal static string GetEncryptedSessionKey(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<string>(dataAsPSObject, "EncryptedSessionKey");
        }

        internal static Exception GetExceptionFromSerializedErrorRecord(object serializedErrorRecord)
        {
            ErrorRecord record = ErrorRecord.FromPSObjectForRemoting(PSObject.AsPSObject(serializedErrorRecord));
            if (record == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.DecodingErrorForErrorRecord);
            }
            return record.Exception;
        }

        private static Exception GetExceptionFromStateInfoObject(PSObject stateInfo)
        {
            PSPropertyInfo info = stateInfo.Properties["ExceptionAsErrorRecord"];
            if ((info != null) && (info.Value != null))
            {
                return GetExceptionFromSerializedErrorRecord(info.Value);
            }
            return null;
        }

        internal static HostInfo GetHostInfo(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return (RemoteHostEncoder.DecodeObject(GetPropertyValue<PSObject>(dataAsPSObject, "HostInfo"), typeof(HostInfo)) as HostInfo);
        }

        internal static bool GetIsNested(object data)
        {
            PSObject psObject = PSObject.AsPSObject(data);
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            return GetPropertyValue<bool>(psObject, "IsNested");
        }

        internal static int GetMaxRunspaces(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<int>(dataAsPSObject, "MaxRunspaces");
        }

        internal static int GetMinRunspaces(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<int>(dataAsPSObject, "MinRunspaces");
        }

        internal static bool GetNoInput(object data)
        {
            PSObject psObject = PSObject.AsPSObject(data);
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            return GetPropertyValue<bool>(psObject, "NoInput");
        }

        internal static PowerShell GetPowerShell(object data)
        {
            PSObject psObject = PSObject.AsPSObject(data);
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            return PowerShell.FromPSObjectForRemoting(GetPropertyValue<PSObject>(psObject, "PowerShell"));
        }

		internal static RemoteRunspace GetRemoteRunspace(object data)
		{
			PSObject psObject = PSObject.AsPSObject(data);
			if (psObject == null)
			{
				throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
			}
			return RemoteRunspace.FromPSObjectForRemoting(GetPropertyValue<PSObject>(psObject, "RemoteRunspace"));
		}

        internal static DebugRecord GetPowerShellDebug(object data)
        {
            if (data == null)
            {
                throw PSTraceSource.NewArgumentNullException("data");
            }
            return new DebugRecord((PSObject) data);
        }

        internal static ErrorRecord GetPowerShellError(object data)
        {
            if (data == null)
            {
                throw PSTraceSource.NewArgumentNullException("data");
            }
            PSObject serializedErrorRecord = data as PSObject;
            return ErrorRecord.FromPSObjectForRemoting(serializedErrorRecord);
        }

        internal static object GetPowerShellOutput(object data)
        {
            return data;
        }

        internal static ProgressRecord GetPowerShellProgress(object data)
        {
            PSObject progressAsPSObject = PSObject.AsPSObject(data);
            if (progressAsPSObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            return ProgressRecord.FromPSObjectForRemoting(progressAsPSObject);
        }

        internal static PSInvocationStateInfo GetPowerShellStateInfo(object data)
        {
            PSObject psObject = data as PSObject;
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.DecodingErrorForPowerShellStateInfo);
            }
            PSInvocationState propertyValue = GetPropertyValue<PSInvocationState>(psObject, "PipelineState");
            return new PSInvocationStateInfo(propertyValue, GetExceptionFromStateInfoObject(psObject));
        }

        internal static VerboseRecord GetPowerShellVerbose(object data)
        {
            if (data == null)
            {
                throw PSTraceSource.NewArgumentNullException("data");
            }
            return new VerboseRecord((PSObject) data);
        }

        internal static WarningRecord GetPowerShellWarning(object data)
        {
            if (data == null)
            {
                throw PSTraceSource.NewArgumentNullException("data");
            }
            return new WarningRecord((PSObject) data);
        }

        private static PSPropertyInfo GetProperty(PSObject psObject, string propertyName)
        {
            if (psObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("psObject");
            }
            if (propertyName == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyName");
            }
            PSPropertyInfo info = psObject.Properties[propertyName];
            if (info == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.MissingProperty, new object[] { propertyName });
            }
            return info;
        }

        internal static T GetPropertyValue<T>(PSObject psObject, string propertyName)
        {
            if (psObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("psObject");
            }
            if (propertyName == null)
            {
                throw PSTraceSource.NewArgumentNullException("propertyName");
            }
            object propertyValue = GetProperty(psObject, propertyName).Value;
            return ConvertPropertyValueTo<T>(propertyName, propertyValue);
        }

        internal static PSEventArgs GetPSEventArgs(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            int propertyValue = GetPropertyValue<int>(dataAsPSObject, "PSEventArgs.EventIdentifier");
            string sourceIdentifier = GetPropertyValue<string>(dataAsPSObject, "PSEventArgs.SourceIdentifier");
            object sender = GetPropertyValue<object>(dataAsPSObject, "PSEventArgs.Sender");
            object obj3 = GetPropertyValue<object>(dataAsPSObject, "PSEventArgs.MessageData");
            string computerName = GetPropertyValue<string>(dataAsPSObject, "PSEventArgs.ComputerName");
            Guid runspaceId = GetPropertyValue<Guid>(dataAsPSObject, "PSEventArgs.RunspaceId");
            ArrayList list = new ArrayList();
            foreach (object obj4 in EnumerateListProperty<object>(dataAsPSObject, "PSEventArgs.SourceArgs"))
            {
                list.Add(obj4);
            }
            return new PSEventArgs(computerName, runspaceId, propertyValue, sourceIdentifier, sender, list.ToArray(), (obj3 == null) ? null : PSObject.AsPSObject(obj3)) { TimeGenerated = GetPropertyValue<DateTime>(dataAsPSObject, "PSEventArgs.TimeGenerated") };
        }

        internal static string GetPublicKey(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<string>(dataAsPSObject, "PublicKey");
        }

        internal static RemoteStreamOptions GetRemoteStreamOptions(object data)
        {
            return GetPropertyValue<RemoteStreamOptions>(PSObject.AsPSObject(data), "RemoteStreamOptions");
        }

        internal static RunspacePoolInitInfo GetRunspacePoolInitInfo(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return new RunspacePoolInitInfo(GetPropertyValue<int>(dataAsPSObject, "MinRunspaces"), GetPropertyValue<int>(dataAsPSObject, "MaxRunspaces"));
        }

        internal static RunspacePoolStateInfo GetRunspacePoolStateInfo(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            RunspacePoolState propertyValue = GetPropertyValue<RunspacePoolState>(dataAsPSObject, "RunspaceState");
            return new RunspacePoolStateInfo(propertyValue, GetExceptionFromStateInfoObject(dataAsPSObject));
        }

        internal static RemoteSessionCapability GetSessionCapability(object data)
        {
            PSObject psObject = data as PSObject;
            if (psObject == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, new object[] { data.GetType().FullName });
            }
            Version propertyValue = GetPropertyValue<Version>(psObject, "protocolversion");
            Version psVersion = GetPropertyValue<Version>(psObject, "PSVersion");
            Version serVersion = GetPropertyValue<Version>(psObject, "SerializationVersion");
            RemoteSessionCapability capability = new RemoteSessionCapability(RemotingDestination.InvalidDestination, propertyValue, psVersion, serVersion);
            if (psObject.Properties["TimeZone"] != null)
            {
                byte[] buffer = GetPropertyValue<byte[]>(psObject, "TimeZone");
                capability.TimeZone = RemoteSessionCapability.ConvertFromByteToTimeZone(buffer);
            }
            return capability;
        }

        internal static PSThreadOptions GetThreadOptions(PSObject dataAsPSObject)
        {
            if (dataAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("dataAsPSObject");
            }
            return GetPropertyValue<PSThreadOptions>(dataAsPSObject, "PSThreadOptions");
        }

        internal static bool ServerSupportsBatchInvocation(Runspace runspace)
        {
            return (((runspace != null) && (runspace.RunspaceStateInfo.State != RunspaceState.BeforeOpen)) && (runspace.GetRemoteProtocolVersion() >= RemotingConstants.ProtocolVersionWin8RTM));
        }

        
    }
}

