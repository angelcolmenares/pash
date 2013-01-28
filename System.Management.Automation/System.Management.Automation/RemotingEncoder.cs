namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class RemotingEncoder
    {
        internal static void AddNoteProperty<T>(PSObject pso, string propertyName, ValueGetterDelegate<T> valueGetter)
        {
            T local = default(T);
            try
            {
                local = valueGetter();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                PSEtwLog.LogAnalyticWarning(PSEventId.Serializer_PropertyGetterFailed, PSOpcode.Exception, PSTask.Serialization, PSKeyword.Serializer | PSKeyword.UseAlwaysAnalytic, new object[] { propertyName, (valueGetter.Target == null) ? string.Empty : valueGetter.Target.GetType().FullName, exception.ToString(), (exception.InnerException == null) ? string.Empty : exception.InnerException.ToString() });
            }
            try
            {
                pso.Properties.Add(new PSNoteProperty(propertyName, local));
            }
            catch (ExtendedTypeSystemException)
            {
                object obj1 = pso.Properties[propertyName].Value;
            }
        }

        internal static PSObject CreateEmptyPSObject()
        {
            return new PSObject { InternalTypeNames = ConsolidatedString.Empty };
        }

        private static PSNoteProperty CreateHostInfoProperty(HostInfo hostInfo)
        {
            return new PSNoteProperty("HostInfo", RemoteHostEncoder.EncodeObject(hostInfo));
        }

        internal static RemoteDataObject GenerateApplicationPrivateData(Guid clientRunspacePoolId, PSPrimitiveDictionary applicationPrivateData)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("ApplicationPrivateData", applicationPrivateData));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.ApplicationPrivateData, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateClientSessionCapability(RemoteSessionCapability capability, Guid runspacePoolId)
        {
            PSObject data = GenerateSessionCapability(capability);
            data.Properties.Add(new PSNoteProperty("TimeZone", RemoteSessionCapability.GetCurrentTimeZoneInByteFormat()));
            return RemoteDataObject.CreateFrom(capability.RemotingDestination, RemotingDataType.SessionCapability, runspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateConnectRunspacePool(Guid clientRunspacePoolId, int minRunspaces, int maxRunspaces)
        {
            PSObject data = CreateEmptyPSObject();
            int num = 0;
            if (minRunspaces != -1)
            {
                data.Properties.Add(new PSNoteProperty("MinRunspaces", minRunspaces));
                num++;
            }
            if (maxRunspaces != -1)
            {
                data.Properties.Add(new PSNoteProperty("MaxRunspaces", maxRunspaces));
                num++;
            }
            if (num > 0)
            {
                return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.ConnectRunspacePool, clientRunspacePoolId, Guid.Empty, data);
            }
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.ConnectRunspacePool, clientRunspacePoolId, Guid.Empty, string.Empty);
        }

        internal static RemoteDataObject GenerateCreatePowerShell(ClientRemotePowerShell shell)
        {
            HostInfo info;
            PowerShell powerShell = shell.PowerShell;
            PSInvocationSettings settings = shell.Settings;
            PSObject data = CreateEmptyPSObject();
            Guid empty = Guid.Empty;
            ApartmentState unknown = ApartmentState.Unknown;
            RunspacePool runspaceConnection = powerShell.GetRunspaceConnection() as RunspacePool;
            empty = runspaceConnection.InstanceId;
            unknown = runspaceConnection.ApartmentState;
            data.Properties.Add(new PSNoteProperty("PowerShell", powerShell.ToPSObjectForRemoting()));
            data.Properties.Add(new PSNoteProperty("NoInput", shell.NoInput));
            if (settings == null)
            {
                info = new HostInfo(null) {
                    UseRunspaceHost = true
                };
                data.Properties.Add(new PSNoteProperty("ApartmentState", unknown));
                data.Properties.Add(new PSNoteProperty("RemoteStreamOptions", RemoteStreamOptions.AddInvocationInfo));
                data.Properties.Add(new PSNoteProperty("AddToHistory", false));
            }
            else
            {
                info = new HostInfo(settings.Host);
                if (settings.Host == null)
                {
                    info.UseRunspaceHost = true;
                }
                data.Properties.Add(new PSNoteProperty("ApartmentState", settings.ApartmentState));
                data.Properties.Add(new PSNoteProperty("RemoteStreamOptions", settings.RemoteStreamOptions));
                data.Properties.Add(new PSNoteProperty("AddToHistory", settings.AddToHistory));
            }
            PSNoteProperty member = CreateHostInfoProperty(info);
            data.Properties.Add(member);
            data.Properties.Add(new PSNoteProperty("IsNested", shell.PowerShell.IsNested));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.CreatePowerShell, empty, shell.InstanceId, data);
        }

        internal static RemoteDataObject GenerateCreateRunspacePool(Guid clientRunspacePoolId, int minRunspaces, int maxRunspaces, RemoteRunspacePoolInternal runspacePool, PSHost host, PSPrimitiveDictionary applicationArguments)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("MinRunspaces", minRunspaces));
            data.Properties.Add(new PSNoteProperty("MaxRunspaces", maxRunspaces));
            data.Properties.Add(new PSNoteProperty("PSThreadOptions", runspacePool.ThreadOptions));
            data.Properties.Add(new PSNoteProperty("ApartmentState", runspacePool.ApartmentState));
            data.Properties.Add(new PSNoteProperty("ApplicationArguments", applicationArguments));
            data.Properties.Add(CreateHostInfoProperty(new HostInfo(host)));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.CreateRunspacePool, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateEncryptedSessionKeyResponse(Guid runspacePoolId, string encryptedSessionKey)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("EncryptedSessionKey", encryptedSessionKey));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.EncryptedSessionKey, runspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateGetAvailableRunspaces(Guid clientRunspacePoolId, long callId)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("ci", callId));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.AvailableRunspaces, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateGetCommandMetadata(ClientRemotePowerShell shell)
        {
            Command command = null;
            foreach (Command command2 in shell.PowerShell.Commands.Commands)
            {
                if (command2.CommandText.Equals("Get-Command", StringComparison.OrdinalIgnoreCase))
                {
                    command = command2;
                    break;
                }
            }
            string[] strArray = null;
            CommandTypes types = CommandTypes.Cmdlet | CommandTypes.Filter | CommandTypes.Function | CommandTypes.Alias;
            string[] strArray2 = null;
            object[] objArray = null;
            foreach (CommandParameter parameter in command.Parameters)
            {
                if (parameter.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    strArray = (string[]) LanguagePrimitives.ConvertTo(parameter.Value, typeof(string[]), CultureInfo.InvariantCulture);
                }
                else if (parameter.Name.Equals("CommandType", StringComparison.OrdinalIgnoreCase))
                {
                    types = (CommandTypes) LanguagePrimitives.ConvertTo(parameter.Value, typeof(CommandTypes), CultureInfo.InvariantCulture);
                }
                else if (parameter.Name.Equals("Module", StringComparison.OrdinalIgnoreCase))
                {
                    strArray2 = (string[]) LanguagePrimitives.ConvertTo(parameter.Value, typeof(string[]), CultureInfo.InvariantCulture);
                }
                else if (parameter.Name.Equals("ArgumentList", StringComparison.OrdinalIgnoreCase))
                {
                    objArray = (object[]) LanguagePrimitives.ConvertTo(parameter.Value, typeof(object[]), CultureInfo.InvariantCulture);
                }
            }
            RunspacePool runspaceConnection = shell.PowerShell.GetRunspaceConnection() as RunspacePool;
            Guid instanceId = runspaceConnection.InstanceId;
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("Name", strArray));
            data.Properties.Add(new PSNoteProperty("CommandType", types));
            data.Properties.Add(new PSNoteProperty("Namespace", strArray2));
            data.Properties.Add(new PSNoteProperty("ArgumentList", objArray));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.GetCommandMetadata, instanceId, shell.InstanceId, data);
        }

        internal static RemoteDataObject GenerateMyPublicKey(Guid runspacePoolId, string publicKey, RemotingDestination destination)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("PublicKey", publicKey));
            return RemoteDataObject.CreateFrom(destination, RemotingDataType.PublicKey, runspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GeneratePowerShellError(object errorRecord, Guid clientRunspacePoolId, Guid clientPowerShellId)
        {
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.PowerShellErrorRecord, clientRunspacePoolId, clientPowerShellId, PSObject.AsPSObject(errorRecord));
        }

        internal static RemoteDataObject GeneratePowerShellInformational(ProgressRecord progressRecord, Guid clientRunspacePoolId, Guid clientPowerShellId)
        {
            if (progressRecord == null)
            {
                throw PSTraceSource.NewArgumentNullException("progressRecord");
            }
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.PowerShellProgress, clientRunspacePoolId, clientPowerShellId, progressRecord.ToPSObjectForRemoting());
        }

        internal static RemoteDataObject GeneratePowerShellInformational(object data, Guid clientRunspacePoolId, Guid clientPowerShellId, RemotingDataType dataType)
        {
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, dataType, clientRunspacePoolId, clientPowerShellId, PSObject.AsPSObject(data));
        }

        internal static RemoteDataObject GeneratePowerShellInput(object data, Guid clientRemoteRunspacePoolId, Guid clientPowerShellId)
        {
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.PowerShellInput, clientRemoteRunspacePoolId, clientPowerShellId, data);
        }

        internal static RemoteDataObject GeneratePowerShellInputEnd(Guid clientRemoteRunspacePoolId, Guid clientPowerShellId)
        {
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.PowerShellInputEnd, clientRemoteRunspacePoolId, clientPowerShellId, null);
        }

        internal static RemoteDataObject GeneratePowerShellOutput(PSObject data, Guid clientPowerShellId, Guid clientRunspacePoolId)
        {
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.PowerShellOutput, clientRunspacePoolId, clientPowerShellId, data);
        }

        internal static RemoteDataObject GeneratePowerShellStateInfo(PSInvocationStateInfo stateInfo, Guid clientPowerShellId, Guid clientRunspacePoolId)
        {
            PSObject data = CreateEmptyPSObject();
            PSNoteProperty member = new PSNoteProperty("PipelineState", (int) stateInfo.State);
            data.Properties.Add(member);
            if (stateInfo.Reason != null)
            {
                string errorId = "RemotePSInvocationStateInfoReason";
                PSNoteProperty property2 = GetExceptionProperty(stateInfo.Reason, errorId, ErrorCategory.NotSpecified);
                data.Properties.Add(property2);
            }
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.PowerShellStateInfo, clientRunspacePoolId, clientPowerShellId, data);
        }

        internal static RemoteDataObject GeneratePSEventArgs(Guid clientRunspacePoolId, PSEventArgs e)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("PSEventArgs.EventIdentifier", e.EventIdentifier));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.SourceIdentifier", e.SourceIdentifier));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.TimeGenerated", e.TimeGenerated));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.Sender", e.Sender));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.SourceArgs", e.SourceArgs));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.MessageData", e.MessageData));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.ComputerName", e.ComputerName));
            data.Properties.Add(new PSNoteProperty("PSEventArgs.RunspaceId", e.RunspaceId));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.PSEventArgs, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GeneratePublicKeyRequest(Guid runspacePoolId)
        {
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.PublicKeyRequest, runspacePoolId, Guid.Empty, string.Empty);
        }

        internal static RemoteDataObject GenerateRunspacePoolInitData(Guid runspacePoolId, int minRunspaces, int maxRunspaces)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("MinRunspaces", minRunspaces));
            data.Properties.Add(new PSNoteProperty("MaxRunspaces", maxRunspaces));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.RunspacePoolInitData, runspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateRunspacePoolOperationResponse(Guid clientRunspacePoolId, object response, long callId)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("SetMinMaxRunspacesResponse", response));
            data.Properties.Add(new PSNoteProperty("ci", callId));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.RunspacePoolOperationResponse, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateRunspacePoolStateInfo(Guid clientRunspacePoolId, RunspacePoolStateInfo stateInfo)
        {
            PSObject data = CreateEmptyPSObject();
            PSNoteProperty member = new PSNoteProperty("RunspaceState", (int) stateInfo.State);
            data.Properties.Add(member);
            if (stateInfo.Reason != null)
            {
                string errorId = "RemoteRunspaceStateInfoReason";
                PSNoteProperty property2 = GetExceptionProperty(stateInfo.Reason, errorId, ErrorCategory.NotSpecified);
                data.Properties.Add(property2);
            }
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Client, RemotingDataType.RunspacePoolStateInfo, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateServerSessionCapability(RemoteSessionCapability capability, Guid runspacePoolId)
        {
            PSObject data = GenerateSessionCapability(capability);
            return RemoteDataObject.CreateFrom(capability.RemotingDestination, RemotingDataType.SessionCapability, runspacePoolId, Guid.Empty, data);
        }

        private static PSObject GenerateSessionCapability(RemoteSessionCapability capability)
        {
            PSObject obj2 = CreateEmptyPSObject();
            obj2.Properties.Add(new PSNoteProperty("protocolversion", capability.ProtocolVersion));
            obj2.Properties.Add(new PSNoteProperty("PSVersion", capability.PSVersion));
            obj2.Properties.Add(new PSNoteProperty("SerializationVersion", capability.SerializationVersion));
            return obj2;
        }

        internal static RemoteDataObject GenerateSetMaxRunspaces(Guid clientRunspacePoolId, int maxRunspaces, long callId)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("MaxRunspaces", maxRunspaces));
            data.Properties.Add(new PSNoteProperty("ci", callId));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.SetMaxRunspaces, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static RemoteDataObject GenerateSetMinRunspaces(Guid clientRunspacePoolId, int minRunspaces, long callId)
        {
            PSObject data = CreateEmptyPSObject();
            data.Properties.Add(new PSNoteProperty("MinRunspaces", minRunspaces));
            data.Properties.Add(new PSNoteProperty("ci", callId));
            return RemoteDataObject.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, RemotingDataType.SetMinRunspaces, clientRunspacePoolId, Guid.Empty, data);
        }

        internal static ErrorRecord GetErrorRecordFromException(Exception exception)
        {
            ErrorRecord record = null;
            IContainsErrorRecord record2 = exception as IContainsErrorRecord;
            if (record2 != null)
            {
                record = new ErrorRecord(record2.ErrorRecord, exception);
            }
            return record;
        }

        private static PSNoteProperty GetExceptionProperty(Exception exception, string errorId, ErrorCategory category)
        {
            ErrorRecord errorRecordFromException = GetErrorRecordFromException(exception);
            if (errorRecordFromException == null)
            {
                errorRecordFromException = new ErrorRecord(exception, errorId, category, null);
            }
            return new PSNoteProperty("ExceptionAsErrorRecord", errorRecordFromException);
        }

        internal static Version GetPSRemotingProtocolVersion(RunspacePool rsPool)
        {
            if ((rsPool != null) && (rsPool.RemoteRunspacePoolInternal != null))
            {
                return rsPool.RemoteRunspacePoolInternal.PSRemotingProtocolVersion;
            }
            return null;
        }

        internal delegate T ValueGetterDelegate<T>();
    }
}

