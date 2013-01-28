namespace System.Management.Automation.Remoting.Server
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    internal class OutOfProcessServerTransportManager : AbstractServerTransportManager
    {
        private bool isDataAckSendPending;
        private Guid powershellInstanceId;
        private OutOfProcessTextWriter stdOutWriter;

        internal OutOfProcessServerTransportManager(OutOfProcessTextWriter stdOutWriter, Guid powershellInstanceId, TypeTable typeTableToUse, int fragmentSize, PSRemotingCryptoHelper cryptoHelper) : base(fragmentSize, cryptoHelper)
        {
            this.stdOutWriter = stdOutWriter;
            this.powershellInstanceId = powershellInstanceId;
            base.TypeTable = typeTableToUse;
        }

        internal override void Close(Exception reasonForClose)
        {
            base.RaiseClosingEvent();
        }

        internal override void Prepare()
        {
            if (this.isDataAckSendPending)
            {
                this.isDataAckSendPending = false;
                base.Prepare();
                this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataAckPacket(this.powershellInstanceId));
            }
        }

        internal override void ProcessRawData(byte[] data, string stream)
        {
            this.isDataAckSendPending = true;
            base.ProcessRawData(data, stream);
            if (this.isDataAckSendPending)
            {
                this.isDataAckSendPending = false;
                this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataAckPacket(this.powershellInstanceId));
            }
        }

        internal override void ReportExecutionStatusAsRunning()
        {
        }

        protected override void SendDataToClient(byte[] data, bool flush, bool reportAsPending, bool reportAsDataBoundary)
        {
            this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, DataPriorityType.Default, this.powershellInstanceId));
        }
    }
}

