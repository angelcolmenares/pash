using System;
using System.Web.UI;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class Session : Page
	{
		public Session()
		{
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			base.ViewStateUserKey = this.Session.SessionID;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
		}
	}
}