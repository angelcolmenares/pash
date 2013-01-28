using System;
using System.Management.Automation;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Disconnect", "WSMan", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141439")]
	public class DisconnectWSManCommand : PSCmdlet, IDisposable
	{
		private string computername;

		[Parameter(Position=0)]
		public string ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
				if (string.IsNullOrEmpty(this.computername) || this.computername.Equals(".", StringComparison.CurrentCultureIgnoreCase))
				{
					this.computername = "localhost";
				}
			}
		}

		public DisconnectWSManCommand()
		{
		}

		protected override void BeginProcessing()
		{
			WSManHelper wSManHelper = new WSManHelper(this);
			if (this.computername == null)
			{
				this.computername = "localhost";
			}
			if (base.SessionState.Path.CurrentProviderLocation("WSMan").Path.StartsWith(string.Concat("WSMan:", (char)92, this.computername), StringComparison.CurrentCultureIgnoreCase))
			{
				wSManHelper.AssertError(wSManHelper.GetResourceMsgFromResourcetext("DisconnectFailure"), false, this.computername);
			}
			if (this.computername.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
			{
				wSManHelper.AssertError(wSManHelper.GetResourceMsgFromResourcetext("LocalHost"), false, this.computername);
			}
			object obj = wSManHelper.RemoveFromDictionary(this.computername);
			if (obj == null)
			{
				wSManHelper.AssertError(wSManHelper.GetResourceMsgFromResourcetext("InvalidComputerName"), false, this.computername);
				return;
			}
			else
			{
				this.Dispose(obj);
				return;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose(object session)
		{
			session = null;
			this.Dispose();
		}
	}
}