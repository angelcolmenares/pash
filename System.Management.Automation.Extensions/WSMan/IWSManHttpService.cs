using System;
using System.ServiceModel;
using System.Net.Security;

namespace System.Management.Automation.Extensions.Remoting.WSMan
{
	[ServiceContract] //(Name = "WSManService", ProtectionLevel = ProtectionLevel.EncryptAndSign, SessionMode = SessionMode.Required)]
	public interface IWSManHttpService
	{
		/// <summary>
		/// Creates the session.
		/// </summary>
		[OperationContract]
		Guid CreateSession();
		
		/// <summary>
		/// Creates the shell.
		/// </summary>
		[OperationContract]
		void CreateShell(Guid sessionId, Guid shellId,  byte[] openContent);
		
		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name='command'>
		/// Command.
		/// </param>
		/// <param name='powerShellId'>
		/// Power shell identifier.
		/// </param>
		[OperationContract]
		void RunCommand(Guid sessionId, string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments);
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns>
		/// The data.
		/// </returns>
		/// <param name='powerShellCmdId'>
		/// Power shell cmd identifier.
		/// </param>
		[OperationContract]
		byte[] ReceiveData (Guid sessionId, Guid powerShellCmdId);
		
		/// <summary>
		/// Sends the input.
		/// </summary>
		/// <param name='shellId'>
		/// Shell identifier.
		/// </param>
		/// <param name='guid'>
		/// GUID.
		/// </param>
		/// <param name='streamId'>
		/// Stream identifier.
		/// </param>
		/// <param name='content'>
		/// Content.
		/// </param>
		[OperationContract]
		void SendInput (Guid sessionId, Guid shellId, Guid guid, string streamId, byte[] content);
		
		/// <summary>
		/// Connects the shell.
		/// </summary>
		/// <returns>
		/// The shell.
		/// </returns>
		[OperationContract]
		byte[] ConnectShell(Guid sessionId, Guid runspacePoolId, byte[] connectData);
		
		/// <summary>
		/// Closes the shell.
		/// </summary>
		/// <param name='runspacePoolId'>
		/// Runspace pool identifier.
		/// </param>
		[OperationContract]
		void CloseShell (Guid sessionId, Guid runspacePoolId);
		
		/// <summary>
		/// Closes the session.
		/// </summary>
		[OperationContract]
		void CloseSession(Guid sessionId, string reason);
	}
}

