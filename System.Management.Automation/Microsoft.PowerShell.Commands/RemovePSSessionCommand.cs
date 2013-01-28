namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Remove", "PSSession", SupportsShouldProcess=true, DefaultParameterSetName="Id", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135250", RemotingCapability=RemotingCapability.OwnedByCommand)]
    public class RemovePSSessionCommand : PSRunspaceCmdlet
    {
        private PSSession[] remoteRunspaceInfos;

        protected override void ProcessRecord()
        {
            ICollection<PSSession> remoteRunspaceInfos = null;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName != null)
            {
                if ((!(parameterSetName == "ComputerName") && !(parameterSetName == "Name")) && (!(parameterSetName == "InstanceId") && !(parameterSetName == "Id")))
                {
                    if (parameterSetName == "Session")
                    {
                        remoteRunspaceInfos = this.remoteRunspaceInfos;
                        goto Label_0076;
                    }
                }
                else
                {
                    remoteRunspaceInfos = base.GetMatchingRunspaces(false, true).Values;
                    goto Label_0076;
                }
            }
            remoteRunspaceInfos = new Collection<PSSession>();
        Label_0076:
            foreach (PSSession session in remoteRunspaceInfos)
            {
                RemoteRunspace targetObject = (RemoteRunspace) session.Runspace;
                if (base.ShouldProcess(targetObject.ConnectionInfo.ComputerName, "Remove"))
                {
                    if (session.Runspace.RunspaceStateInfo.State == RunspaceState.Disconnected)
                    {
                        bool flag;
                        try
                        {
                            session.Runspace.Connect();
                            flag = true;
                        }
                        catch (InvalidRunspaceStateException)
                        {
                            flag = false;
                        }
                        catch (PSRemotingTransportException)
                        {
                            flag = false;
                        }
                        if (!flag)
                        {
                            Exception exception = new RuntimeException(StringUtil.Format(RemotingErrorIdStrings.RemoveRunspaceNotConnected, targetObject.Name));
                            ErrorRecord errorRecord = new ErrorRecord(exception, "RemoveSessionCannotConnectToServer", ErrorCategory.InvalidOperation, targetObject);
                            base.WriteError(errorRecord);
                        }
                    }
                    try
                    {
                        targetObject.Dispose();
                    }
                    catch (PSRemotingTransportException)
                    {
                    }
                    try
                    {
                        base.RunspaceRepository.Remove(session);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Session")]
        public PSSession[] Session
        {
            get
            {
                return this.remoteRunspaceInfos;
            }
            set
            {
                this.remoteRunspaceInfos = value;
            }
        }
    }
}

