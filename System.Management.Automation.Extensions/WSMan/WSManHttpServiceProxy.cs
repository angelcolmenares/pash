using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.Management.Automation.Extensions.Remoting.WSMan
{
	public class WSManHttpServiceProxy : ClientBase<IWSManHttpService>
	{
		public WSManHttpServiceProxy (Binding binding, EndpointAddress address)
			: base(binding, address)
		{

		}

		public Guid SessionId
		{
			get;set;
		}

		#region IWSManHttpService implementation

		public Guid CreateSession ()
		{
			return this.Channel.CreateSession ();
		}

		public void CreateShell (Guid shellId, byte[] openContent)
		{
			this.Channel.CreateShell (SessionId, shellId, openContent);

		}

		public void RunCommand (string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments)
		{
			this.Channel.RunCommand (SessionId, command, runspacePoolId, powerShellCmdId, arguments);

		}

		public byte[] ReceiveData (Guid powerShellCmdId)
		{
			return this.Channel.ReceiveData (SessionId, powerShellCmdId);
		}

		public void SendInput (Guid shellId, Guid guid, string streamId, byte[] content)
		{
			this.Channel.SendInput (SessionId, shellId, guid, streamId, content);

		}

		public byte[] ConnectShell (Guid runspacePoolId, byte[] connectData)
		{
			return this.Channel.ConnectShell (SessionId, runspacePoolId, connectData);
		}

		public void CloseShell (Guid runspacePoolId)
		{
			this.Channel.CloseShell (SessionId, runspacePoolId);
		}

		public void CloseSession (string reason)
		{
			this.Channel.CloseSession (SessionId, reason);
		}

		#endregion
	}
}

