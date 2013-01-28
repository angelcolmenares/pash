using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	public sealed class X509StoreLocation
	{
		private StoreLocation location;

		public StoreLocation Location
		{
			get
			{
				return this.location;
			}
			set
			{
				this.location = value;
			}
		}

		public string LocationName
		{
			get
			{
				return this.location.ToString();
			}
		}

		public Hashtable StoreNames
		{
			get
			{
				Hashtable hashtables = new Hashtable(StringComparer.OrdinalIgnoreCase);
				List<string> storeNamesAtLocation = Crypt32Helpers.GetStoreNamesAtLocation(this.location);
				foreach (string str in storeNamesAtLocation)
				{
					hashtables.Add(str, true);
				}
				return hashtables;
			}
		}

		public X509StoreLocation(StoreLocation location)
		{
			this.location = StoreLocation.CurrentUser;
			this.Location = location;
		}
	}
}