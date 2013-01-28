using System;
using System.Management.Automation.Remoting.Server;
using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;

namespace System.Management.Automation.Remoting.WSMan
{
	internal class WSManServerTransportManager : AbstractServerTransportManager
	{
		private WSManOutputWriter _writer;
		private Guid _shellId;

		internal WSManServerTransportManager (Guid shellId, WSManOutputWriter writer)
			: base(32778, new PSRemotingCryptoHelperServer())
		{
			_shellId = shellId;
			_writer = writer;
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
			return _writer.GetBuffer (true);
		}

		internal override void SendDataToClient<T> (RemoteDataObject<T> data, bool flush, bool reportPending)
		{
			base.SendDataToClient (data, flush, reportPending);
			if (data.DataType == RemotingDataType.RemoteHostCallUsingPowerShellHost) {
				var  obj = data.Data as PSObject;
				if (obj != null)
				{
					object value = obj.Properties["mi"].Value;
					if (value != null)
					if (value.ToString ().StartsWith ("Prompt") || value.ToString ().StartsWith ("Get"))
					{
						_writer.CompleteFeedback ();
					}
				}
			}
		}
		
		protected override void SendDataToClient (byte[] data, bool flush, bool reportAsPending, bool reportAsDataBoundary)
		{
			_writer.SendDataToClient (data, flush, reportAsPending, reportAsDataBoundary);
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

		#endregion
	}
}

