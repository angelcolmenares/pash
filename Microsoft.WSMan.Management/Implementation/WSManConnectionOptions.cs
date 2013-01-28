using System;

namespace Microsoft.WSMan.Management
{
	public class WSManConnectionOptions : IWSManConnectionOptions
	{
		private string password;

		public WSManConnectionOptions ()
		{

		}

		#region IWSManConnectionOptions implementation

		public string Password {
			set { password = value; }
		}

		public string UserName {
			get;
			set;
		}

		#endregion
	}
}

