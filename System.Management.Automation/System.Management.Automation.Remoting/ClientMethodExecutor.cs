namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces.Internal;

    internal class ClientMethodExecutor
    {
        private PSHost _clientHost;
        private Guid _clientPowerShellId;
        private Guid _clientRunspacePoolId;
        private System.Management.Automation.Remoting.RemoteHostCall _remoteHostCall;
        private BaseClientTransportManager _transportManager;

        private ClientMethodExecutor(BaseClientTransportManager transportManager, PSHost clientHost, Guid clientRunspacePoolId, Guid clientPowerShellId, System.Management.Automation.Remoting.RemoteHostCall remoteHostCall)
        {
            this._transportManager = transportManager;
            this._remoteHostCall = remoteHostCall;
            this._clientHost = clientHost;
            this._clientRunspacePoolId = clientRunspacePoolId;
            this._clientPowerShellId = clientPowerShellId;
        }

        internal static void Dispatch(BaseClientTransportManager transportManager, PSHost clientHost, PSDataCollectionStream<ErrorRecord> errorStream, ObjectStream methodExecutorStream, bool isMethodExecutorStreamEnabled, RemoteRunspacePoolInternal runspacePool, Guid clientPowerShellId, System.Management.Automation.Remoting.RemoteHostCall remoteHostCall)
        {
            ClientMethodExecutor executor = new ClientMethodExecutor(transportManager, clientHost, runspacePool.InstanceId, clientPowerShellId, remoteHostCall);
            if (clientPowerShellId == Guid.Empty)
            {
                executor.Execute(errorStream);
            }
            else
            {
                bool flag = false;
                if (clientHost != null)
                {
                    PSObject privateData = clientHost.PrivateData;
                    if (privateData != null)
                    {
                        PSNoteProperty property = privateData.Properties["AllowSetShouldExitFromRemote"] as PSNoteProperty;
                        flag = ((property != null) && (property.Value is bool)) ? ((bool) property.Value) : false;
                    }
                }
                if ((remoteHostCall.IsSetShouldExit && isMethodExecutorStreamEnabled) && !flag)
                {
                    runspacePool.Close();
                }
                else if (isMethodExecutorStreamEnabled)
                {
                    methodExecutorStream.Write(executor);
                }
                else
                {
                    executor.Execute(errorStream);
                }
            }
        }

        internal void Execute(Action<ErrorRecord> writeErrorAction)
        {
            if (this._remoteHostCall.IsVoidMethod)
            {
                this.ExecuteVoid(writeErrorAction);
            }
            else
            {
                RemotingDataType dataType = (this._clientPowerShellId == Guid.Empty) ? RemotingDataType.RemoteRunspaceHostResponseData : RemotingDataType.RemotePowerShellHostResponseData;
                RemoteDataObject<PSObject> data = RemoteDataObject<PSObject>.CreateFrom(RemotingDestination.InvalidDestination | RemotingDestination.Server, dataType, this._clientRunspacePoolId, this._clientPowerShellId, this._remoteHostCall.ExecuteNonVoidMethod(this._clientHost).Encode());
                this._transportManager.DataToBeSentCollection.Add<PSObject>(data, DataPriorityType.PromptResponse);
            }
        }

        internal void Execute(Cmdlet cmdlet)
        {
            this.Execute(new Action<ErrorRecord>(cmdlet.WriteError));
        }

        internal void Execute(PSDataCollectionStream<ErrorRecord> errorStream)
        {
            Action<ErrorRecord> action2 = null;
            Action<ErrorRecord> action3 = null;
            Action<ErrorRecord> writeErrorAction = null;
            if ((errorStream == null) || this.IsRunspacePushed(this._clientHost))
            {
                if (action2 == null)
                {
                    action2 = delegate (ErrorRecord errorRecord) {
                        try
                        {
                            if (this._clientHost.UI != null)
                            {
                                this._clientHost.UI.WriteErrorLine(errorRecord.ToString());
                            }
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                        }
                    };
                }
                writeErrorAction = action2;
            }
            else
            {
                if (action3 == null)
                {
                    action3 = errorRecord => errorStream.Write(errorRecord);
                }
                writeErrorAction = action3;
            }
            this.Execute(writeErrorAction);
        }

        internal void ExecuteVoid(Action<ErrorRecord> writeErrorAction)
        {
            try
            {
                this._remoteHostCall.ExecuteVoidMethod(this._clientHost);
            }
            catch (Exception innerException)
            {
                CommandProcessorBase.CheckForSevereException(innerException);
                if (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                ErrorRecord record = new ErrorRecord(innerException, PSRemotingErrorId.RemoteHostCallFailed.ToString(), ErrorCategory.InvalidArgument, this._remoteHostCall.MethodName);
                writeErrorAction(record);
            }
        }

        private bool IsRunspacePushed(PSHost host)
        {
            IHostSupportsInteractiveSession session = host as IHostSupportsInteractiveSession;
            if (session == null)
            {
                return false;
            }
            return session.IsRunspacePushed;
        }

        internal System.Management.Automation.Remoting.RemoteHostCall RemoteHostCall
        {
            get
            {
                return this._remoteHostCall;
            }
        }
    }
}

