using System;
using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation
{
	/// <summary>
	/// Remote runspace proxy.
	/// </summary>
	internal class RemoteRunspaceProxy : RemoteRunspace
	{
		private RemoteRunspace _realRunspace;

		internal RemoteRunspaceProxy( System.Management.Automation.Runspaces.RunspacePool runspacePool)
			: base(runspacePool)
		{

		}

		public override void Close ()
		{
			base.Close ();
		}

		public override void CloseAsync ()
		{
			base.CloseAsync ();
		}

		public override void Connect ()
		{
			base.Connect ();
		}

		public override void ConnectAsync ()
		{
			base.ConnectAsync ();
		}

		public override System.Management.Automation.Runspaces.Pipeline CreateDisconnectedPipeline ()
		{
			return base.CreateDisconnectedPipeline ();
		}

		public override PowerShell CreateDisconnectedPowerShell ()
		{
			return base.CreateDisconnectedPowerShell ();
		}

		public override System.Management.Automation.Runspaces.Pipeline CreateNestedPipeline ()
		{
			return base.CreateNestedPipeline ();
		}

		public override System.Management.Automation.Runspaces.Pipeline CreateNestedPipeline (string command, bool addToHistory)
		{
			return base.CreateNestedPipeline (command, addToHistory);
		}

		public override System.Management.Automation.Runspaces.Pipeline CreatePipeline ()
		{
			return base.CreatePipeline ();
		}

		public override System.Management.Automation.Runspaces.Pipeline CreatePipeline (string command)
		{
			return base.CreatePipeline (command);
		}

		public override void ResetRunspaceState ()
		{
			base.ResetRunspaceState ();
		}

		public RemoteRunspace RealRunspace {
			get { return _realRunspace; }
			set { _realRunspace = value; }
		}
	}
}

