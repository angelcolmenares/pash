using System;
using System.Globalization;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class ClientInfo
	{
		public string Agent
		{
			get;
			private set;
		}

		public CultureInfo CurrentCulture
		{
			get;
			private set;
		}

		public CultureInfo CurrentUICulture
		{
			get;
			private set;
		}

		public ClientInfo(string agent, CultureInfo currentCulture, CultureInfo currentUICulture)
		{
			if (agent != null)
			{
				if (currentCulture != null)
				{
					if (currentUICulture != null)
					{
						this.Agent = agent;
						this.CurrentCulture = currentCulture;
						this.CurrentUICulture = currentUICulture;
						return;
					}
					else
					{
						throw new ArgumentNullException("currentUICulture");
					}
				}
				else
				{
					throw new ArgumentNullException("currentCulture");
				}
			}
			else
			{
				throw new ArgumentNullException("agent");
			}
		}
	}
}