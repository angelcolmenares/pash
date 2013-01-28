using System;
using System.Management.Automation.Remoting.Server;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Remoting.WSMan
{
	internal class WSManServerSessionTransportManager : AbstractServerSessionTransportManager
	{
		private WSManOutputWriter _writer;
		private Dictionary<Guid, AbstractServerTransportManager> _transportManagers = new Dictionary<Guid, AbstractServerTransportManager>();

		internal WSManServerSessionTransportManager ()
			: base(32768, new PSRemotingCryptoHelperServer())
		{
			_writer = new WSManOutputWriter();
		}

		#region implemented abstract members of AbstractServerTransportManager

		internal override void Close (Exception reasonForClose)
		{

		}

		internal override void ReportExecutionStatusAsRunning ()
		{

		}


		internal byte[] GetBuffer ()
		{
			return _writer.GetBuffer (false);
		}

		internal override void SendDataToClient<T> (RemoteDataObject<T> data, bool flush, bool reportPending)
		{
			base.SendDataToClient (data, flush, reportPending);
			if (data.DataType == RemotingDataType.RemoteHostCallUsingPowerShellHost) {
				_writer.CompleteFeedback ();
			}
		}
		
		protected override void SendDataToClient (byte[] data, bool flush, bool reportAsPending, bool reportAsDataBoundary)
		{
			_writer.SendDataToClient (data, flush, reportAsPending, reportAsDataBoundary);
		}

		#endregion

		#region implemented abstract members of AbstractServerSessionTransportManager

		internal override AbstractServerTransportManager GetCommandTransportManager (Guid powerShellCmdId)
		{
			if (_transportManagers.ContainsKey (powerShellCmdId)) {
				return _transportManagers[powerShellCmdId];
			}
			var manager = new WSManServerTransportManager(powerShellCmdId, _writer);
			manager.MigrateDataReadyEventHandlers (this);
			_transportManagers.Add (powerShellCmdId, manager);
			return manager;
		}

		internal override void ProcessRawDataAsync(byte[] data, string stream)
		{
			AsyncResult result = new AsyncResult(Guid.Empty, null, null);
			AsyncResult feedback = new AsyncResult(Guid.Empty, null, null);
			_writer.QueueProcess(result);
			_writer.QueueFeedback(feedback);
			Action<byte[], string> method = base.ProcessRawDataAsync;
			method.BeginInvoke (data, stream, CompleteProcess, null);
			feedback.EndInvoke ();
		}

		private void CompleteProcess (object state)
		{
			_writer.CompleteProcess();
		}

		public override void CompleteProcessRawData ()
		{
			_writer.EndNextInvoke();
		}

		internal override void RemoveCommandTransportManager (Guid powerShellCmdId)
		{
			if (_transportManagers.ContainsKey (powerShellCmdId)) {
				_transportManagers.Remove (powerShellCmdId);
			}
		}

		#endregion
	}
}

