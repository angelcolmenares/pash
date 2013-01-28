namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Runspaces.Internal;
    using System.Text;
    using System.Threading;

    internal abstract class BaseClientCommandTransportManager : BaseClientTransportManager, IDisposable
    {
        protected StringBuilder cmdText;
        protected Guid powershellInstanceId;
        protected SerializedDataStream serializedPipeline;
        internal bool startInDisconnectedMode;

        internal event EventHandler<EventArgs> SignalCompleted;

        protected BaseClientCommandTransportManager(ClientRemotePowerShell shell, PSRemotingCryptoHelper cryptoHelper, BaseClientSessionTransportManager sessnTM) : base(sessnTM.RunspacePoolInstanceId, cryptoHelper)
        {
            RemoteDataObject obj2;
            base.Fragmentor.FragmentSize = sessnTM.Fragmentor.FragmentSize;
            base.Fragmentor.TypeTable = sessnTM.Fragmentor.TypeTable;
            base.dataToBeSent.Fragmentor = base.Fragmentor;
            this.powershellInstanceId = shell.PowerShell.InstanceId;
            this.cmdText = new StringBuilder();
            foreach (Command command in shell.PowerShell.Commands.Commands)
            {
                this.cmdText.Append(command.CommandText);
                this.cmdText.Append(" | ");
            }
            this.cmdText.Remove(this.cmdText.Length - 3, 3);
            if (shell.PowerShell.IsGetCommandMetadataSpecialPipeline)
            {
                obj2 = RemotingEncoder.GenerateGetCommandMetadata(shell);
            }
            else
            {
                obj2 = RemotingEncoder.GenerateCreatePowerShell(shell);
            }
            this.serializedPipeline = new SerializedDataStream(base.Fragmentor.FragmentSize);
            base.Fragmentor.Fragment<object>(obj2, this.serializedPipeline);
        }

        internal override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                this.serializedPipeline.Dispose();
            }
        }

        internal void RaiseSignalCompleted()
        {
            this.SignalCompleted.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal virtual void ReconnectAsync()
        {
            throw new NotImplementedException();
        }

        internal virtual void SendStopSignal()
        {
            throw new NotImplementedException();
        }

        protected Guid PowershellInstanceId
        {
            get
            {
                return this.powershellInstanceId;
            }
        }
    }
}

