using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SSP")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cred")]
	public class WSManCredSSPCommandBase : PSCmdlet
	{
		internal const string Server = "Server";

		internal const string Client = "Client";

		private string role;

		[Parameter(Mandatory=true, Position=0)]
		[ValidateSet(new string[] { "Client", "Server" })]
		public string Role
		{
			get
			{
				return this.role;
			}
			set
			{
				this.role = value;
			}
		}

		public WSManCredSSPCommandBase()
		{
		}

		internal IWSManSession CreateWSManSession()
		{
			IWSManSession wSManSession;
			IWSManEx wSManClass = (IWSManEx)(new WSManClass());
			try
			{
				IWSManSession wSManSession1 = (IWSManSession)wSManClass.CreateSession(null, 0, null);
				wSManSession = wSManSession1;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				ErrorRecord errorRecord = new ErrorRecord(cOMException, "COMException", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord);
				return null;
			}
			return wSManSession;
		}
	}
}