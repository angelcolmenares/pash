using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
	internal class WSManOutputWriter
	{
		private byte[] buffer = new byte[0];
		private bool _written;
		private Queue<AsyncResult> asyncCmds = new Queue<AsyncResult>();
		private Queue<AsyncResult> feedbackCmds = new Queue<AsyncResult>();

		public WSManOutputWriter ()
		{

		}

		internal void QueueFeedback (AsyncResult result)
		{
			feedbackCmds.Enqueue (result);
		}

		internal void QueueProcess (AsyncResult result)
		{
			asyncCmds.Enqueue (result);
		}

		internal byte[] GetBuffer (bool commandCall)
		{
			_written = true;
			_packets = 0;
			return buffer;
		}

		public void CompleteProcess ()
		{
			CompleteFeedback ();
			while (asyncCmds.Count > 0) {
				var obj = asyncCmds.Dequeue ();
				obj.SetAsCompleted (null);	
			}
		}

		public void CompleteFeedback ()
		{
			while (feedbackCmds.Count > 0) {
				var obj = feedbackCmds.Dequeue ();
				obj.SetAsCompleted (null);	
			}
		}

		private int _packets;

		public void EndNextInvoke ()
		{
			if (asyncCmds.Count > 0) {
				var result = asyncCmds.Dequeue ();
				result.EndInvoke ();
			}
		}

		internal void SendDataToClient (byte[] data, bool flush, bool reportAsPending, bool reportAsDataBoundary)
		{
			if (_written) {
				buffer = new byte[0];
				_written = false;
			}
			int index = buffer.Length;
			Array.Resize<byte> (ref buffer, buffer.Length + data.Length);
			Array.Copy (data, 0, buffer, index, data.Length);
			_packets++;
		}
	}
}

