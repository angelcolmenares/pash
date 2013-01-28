namespace System.Management.Automation.Remoting.Server
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;

    internal class OutOfProcessServerSessionTransportManager : AbstractServerSessionTransportManager
    {
        private Dictionary<Guid, OutOfProcessServerTransportManager> cmdTransportManagers;
        private OutOfProcessTextWriter stdOutWriter;
        private object syncObject;

        internal OutOfProcessServerSessionTransportManager(OutOfProcessTextWriter outWriter) : base(0x8000, new PSRemotingCryptoHelperServer())
        {
            this.syncObject = new object();
            this.stdOutWriter = outWriter;
            this.cmdTransportManagers = new Dictionary<Guid, OutOfProcessServerTransportManager>();
        }

        internal override void Close(Exception reasonForClose)
        {
            base.RaiseClosingEvent();
        }

        internal void CreateCommandTransportManager(Guid powerShellCmdId)
        {
            OutOfProcessServerTransportManager manager = new OutOfProcessServerTransportManager(this.stdOutWriter, powerShellCmdId, base.TypeTable, base.Fragmentor.FragmentSize, base.CryptoHelper);
            manager.MigrateDataReadyEventHandlers(this);
            lock (this.syncObject)
            {
                this.cmdTransportManagers.Add(powerShellCmdId, manager);
            }
            this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateCommandAckPacket(powerShellCmdId));
        }

        internal override AbstractServerTransportManager GetCommandTransportManager(Guid powerShellCmdId)
        {
            lock (this.syncObject)
            {
                OutOfProcessServerTransportManager manager = null;
                this.cmdTransportManagers.TryGetValue(powerShellCmdId, out manager);
                return manager;
            }
        }

        internal override void Prepare()
        {
            throw new NotSupportedException();
        }

        internal override void ProcessRawData(byte[] data, string stream)
        {
            base.ProcessRawData(data, stream);
            this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataAckPacket(Guid.Empty));
        }

        internal override void RemoveCommandTransportManager(Guid powerShellCmdId)
        {
            lock (this.syncObject)
            {
                if (this.cmdTransportManagers.ContainsKey(powerShellCmdId))
                {
                    this.cmdTransportManagers.Remove(powerShellCmdId);
                }
            }
        }

        internal override void ReportExecutionStatusAsRunning()
        {
        }

        protected override void SendDataToClient(byte[] data, bool flush, bool reportAsPending, bool reportAsDataBoundary)
        {
            this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, DataPriorityType.Default, Guid.Empty));
        }
    }
}

