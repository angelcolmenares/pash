using System;

namespace System.Management.Automation.Remoting.WSMan
{
	public interface IWSManService
	{
		/// <summary>
		/// Initialize this instance.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Creates the session.
		/// </summary>
		Guid CreateSession(string connection, string username, string password, int authMechanism, int protocolVersion);

		/// <summary>
		/// Creates the shell.
		/// </summary>
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
		void RunCommand(Guid sessionId, string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments);

		/// <summary>
		/// Completes the command.
		/// </summary>
		/// <param name='sessionId'>
		/// Session identifier.
		/// </param>
		void CompleteCommand(Guid sessionId);

		/// <summary>
		/// 
		/// </summary>
		/// <returns>
		/// The data.
		/// </returns>
		/// <param name='powerShellCmdId'>
		/// Power shell cmd identifier.
		/// </param>
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
		void SendInput (Guid sessionId, Guid shellId, Guid guid, string streamId, byte[] content);

		/// <summary>
		/// Connects the shell.
		/// </summary>
		/// <returns>
		/// The shell.
		/// </returns>
		byte[] ConnectShell(Guid sessionId, Guid runspacePoolId, byte[] connectData);

		/// <summary>
		/// Closes the shell.
		/// </summary>
		/// <param name='runspacePoolId'>
		/// Runspace pool identifier.
		/// </param>
		void CloseShell (Guid sessionId, Guid runspacePoolId);

		/// <summary>
		/// Closes the session.
		/// </summary>
		void CloseSession(Guid sessionId, string reason);
	}
}

