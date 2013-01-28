using System;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.IO;
using Microsoft.PowerShell;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal class PowwaHost : PSHost, IMessageCreated, IHostSupportsInteractiveSession
	{
		private readonly PowwaHostUserInterface userInterface;

		private readonly CultureInfo currentCulture;

		private readonly CultureInfo currentUICulture;

		private readonly Guid instanceId;

		private readonly Version version;

		private WrappedSerializer errorSerializer;

		private WrappedSerializer outputSerializer;

		private Serialization.DataFormat outputFormat = Serialization.DataFormat.Text;


		private TextWriter standardErrorWriter;
		private TextWriter standardOutputWriter;
		private TextReader standardInputReader;
		private Runspace _runspace;
		private bool wasInitialCommandEncoded;

		public override CultureInfo CurrentCulture
		{
			get
			{
				return this.currentCulture;
			}
		}

		public RunspaceRef RunspaceRef {
			get;set;
		}

		public void SetRunspace (Runspace runspace)
		{
			_runspace = runspace;
			RunspaceRef = new System.Management.Automation.Remoting.RunspaceRef(_runspace);
		}

		public bool IsStandardInputRedirected {
			get { return false; }
		}

		public bool IsStandardOutputRedirected {
			get { return false; }
		}

		public bool IsStandardErrorRedirected {
			get { return false; }
		}

		public bool IsInteractive {
			get { return true; }
		}

		public void ResetProgress ()
		{

		}

		internal Serialization.DataFormat ErrorFormat
		{
			get
			{
				Serialization.DataFormat dataFormat = this.outputFormat;
				if (!this.IsInteractive && this.IsStandardErrorRedirected && this.wasInitialCommandEncoded)
				{
					dataFormat = Serialization.DataFormat.XML;
				}
				return dataFormat;
			}
		}

		internal Serialization.DataFormat OutputFormat
		{
			get
			{
				Serialization.DataFormat dataFormat = this.outputFormat;
				if (!this.IsInteractive && this.IsStandardErrorRedirected && this.wasInitialCommandEncoded)
				{
					dataFormat = Serialization.DataFormat.XML;
				}
				return dataFormat;
			}
		}

		internal WrappedSerializer ErrorSerializer
		{
			get
			{
				TextWriter standardErrorWriter;
				if (this.errorSerializer == null)
				{
					PowwaHost wrappedSerializer = this;
					Serialization.DataFormat errorFormat = this.ErrorFormat;
					string str = "Error";
					if (this.IsStandardErrorRedirected)
					{
						standardErrorWriter = this.StandardErrorWriter;
					}
					else
					{
						standardErrorWriter = this.ConsoleTextWriter;
					}
					wrappedSerializer.errorSerializer = new WrappedSerializer(errorFormat, str, standardErrorWriter);
				}
				return this.errorSerializer;
			}
		}

		internal WrappedSerializer OutputSerializer
		{
			get
			{
				TextWriter standardOutputWriter;
				if (this.outputSerializer == null)
				{
					PowwaHost wrappedSerializer = this;
					Serialization.DataFormat dataFormat = this.outputFormat;
					string str = "Output";
					if (this.IsStandardOutputRedirected)
					{
						standardOutputWriter = this.StandardOutputWriter;
					}
					else
					{
						standardOutputWriter = this.ConsoleTextWriter;
					}
					wrappedSerializer.outputSerializer = new WrappedSerializer(dataFormat, str, standardOutputWriter);
				}
				return this.outputSerializer;
			}
		}

		public TextWriter StandardOutputWriter {
			get { return standardOutputWriter; }
		}

		public TextWriter ConsoleTextWriter {
			get { return standardOutputWriter; }
		}


		internal TextWriter StandardErrorWriter
		{
			get
			{
				if (!this.IsStandardErrorRedirected)
				{
					return null;
				}
				else
				{
					return this.standardErrorWriter;
				}
			}
		}
		
		internal TextReader StandardInReader
		{
			get
			{
				if (!this.IsStandardInputRedirected)
				{
					return null;
				}
				else
				{
					return this.standardInputReader;
				}
			}
		}

		
		private bool InDebugMode
		{
			get
			{
				return false;
			}
		}
		
		internal Serialization.DataFormat InputFormat
		{
			get
			{
				return Serialization.DataFormat.Text;
			}
		}

		public override CultureInfo CurrentUICulture
		{
			get
			{
				return this.currentUICulture;
			}
		}

		public override Guid InstanceId
		{
			get
			{
				return this.instanceId;
			}
		}

		public override string Name
		{
			get
			{
				return "PowerShell Web Access Host";
			}
		}

		public override PSHostUserInterface UI
		{
			get
			{
				return this.userInterface;
			}
		}

		public override Version Version
		{
			get
			{
				return this.version;
			}
		}

		public PowwaHost(ClientInfo clientInfo)
		{
			this.instanceId = new Guid("3BADF81F-2A26-468B-ADCC-F93DC13F8084");
			this.version = new Version(1, 0, 0);
			this.userInterface = new PowwaHostUserInterface(clientInfo);
			this.currentCulture = clientInfo.CurrentCulture;
			this.currentUICulture = clientInfo.CurrentUICulture;
			standardErrorWriter = new WebWriter(this);
			standardOutputWriter = new WebWriter(this);
			standardInputReader = new WebReader(this);
		}

		public static void CheckForSevereException (Exception exception)
		{

		}

		public override void EnterNestedPrompt()
		{
			throw new NotSupportedException();
		}

		public override void ExitNestedPrompt()
		{
			throw new NotSupportedException();
		}

		public override void NotifyBeginApplication()
		{
		}

		public override void NotifyEndApplication()
		{
		}

		private void OnMessageCreated(MessageCreatedEventArgs e)
		{
			EventHandler<MessageCreatedEventArgs> eventHandler = this.MessageCreated;
			if (eventHandler != null)
			{
				eventHandler(this, e);
			}
		}

		public override void SetShouldExit(int exitCode)
		{
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetShouldExit(): Enter");
			try
			{
				this.OnMessageCreated(new MessageCreatedEventArgs(new ExitMessage(exitCode), false));
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetShouldExit(): Exit");
			}
		}

		#region IHostSupportsInteractiveSession implementation

		private Queue<System.Management.Automation.Runspaces.Runspace> _runspaces = new Queue<System.Management.Automation.Runspaces.Runspace>();

		public void PopRunspace ()
		{
			if (_runspaces.Count > 0) {
				SetRunspace (_runspaces.Dequeue ());
			}
		}

		public void PushRunspace (System.Management.Automation.Runspaces.Runspace runspace)
		{
			System.Management.Automation.RemoteRunspace remoteRunspace = runspace as System.Management.Automation.RemoteRunspace;
			if (remoteRunspace != null) {
				remoteRunspace.RunspacePool.RemoteRunspacePoolInternal.SetHost(this);
			}
			_runspaces.Enqueue (this.Runspace);
			SetRunspace (runspace);
		}

		public bool IsRunspacePushed {
			get {
				return _runspaces.Count > 0;
			}
		}

		public System.Management.Automation.Runspaces.Runspace Runspace {
			get {
				return this._runspace;
			}
		}

		#endregion

		public event EventHandler<MessageCreatedEventArgs> MessageCreated;

		internal class WebWriter : TextWriter
		{
			private PowwaHost _host;

			public WebWriter(PowwaHost host)
			{
				_host = host;
			}

			public override void WriteLine ()
			{
				_host.UI.WriteLine ();
			}

			public override void WriteLine (string value)
			{
				_host.UI.WriteLine (value);
			}

			public override void Write (string value)
			{
				_host.UI.Write (value);
			}

			public override System.Text.Encoding Encoding {
				get {
					return System.Text.Encoding.UTF8;
				}
			}
		}

		internal class WebReader : TextReader
		{
			private PowwaHost _host;
			
			public WebReader(PowwaHost host)
			{
				_host = host;
			}

			public override string ReadLine ()
			{
				return _host.UI.ReadLine();
			}
		}
	}
}