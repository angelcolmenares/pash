using System;
using System.Collections.Generic;

namespace System.DirectoryServices
{
	public static class NativeEntryFactory
	{
		public static IDictionary<string, object> GetProperties (DirectoryEntry entry)
		{
			var ret = new Dictionary<string, object> ();

			return ret;
		}

		public static IDictionary<string, object> GetProperties (string host, int port, string dn, string username, string password, string[] properties)
		{
			var ret = new Dictionary<string, object> ();
			try {
				var cn = new Novell.Directory.Ldap.LdapConnection ();
				cn.Connect (host, port);
				if (string.IsNullOrEmpty (dn)) dn = "";
				cn.Bind (username, password, Novell.Directory.Ldap.AuthenticationTypes.Secure | Novell.Directory.Ldap.AuthenticationTypes.ServerBind | Novell.Directory.Ldap.AuthenticationTypes.Signing | Novell.Directory.Ldap.AuthenticationTypes.FastBind);
				var results = cn.Search (dn, Novell.Directory.Ldap.LdapConnection.SCOPE_BASE, null, properties, false);
				var item = results.next ();
				var props = item.getAttributeSet ();
				foreach(Novell.Directory.Ldap.LdapAttribute att in props) {
					object val = null;
					if (att.Name.Equals ("objectsid", StringComparison.OrdinalIgnoreCase)) {
						val = Array.ConvertAll (att.ByteValue, (a) => (byte)a);
					}
					else if (att.Name.Equals ("objectGUID", StringComparison.OrdinalIgnoreCase)) {
						var guidBytes = Array.ConvertAll (att.ByteValue, (a) => (byte)a);
						val = new Guid?(new Guid(guidBytes));
					}
					else if (att.Name.Equals ("objectClass", StringComparison.OrdinalIgnoreCase)) {
						val = att.StringValueArray[att.StringValueArray.GetUpperBound (0)];
					}
					else if (att.Name.Equals ("msDS-LogonTimeSyncInterval", StringComparison.OrdinalIgnoreCase)) {
						var guidBytes = Array.ConvertAll (att.ByteValue, (a) => (byte)a);
						val = Convert.ToInt64 (guidBytes[0]);
					}
					else {
						if (att.size () == 1) {
							val = att.StringValue;
						} else {
							val = att.StringValueArray;
						}
					}
					ret.Add (att.Name, val);
				}
			} finally {

			}
			return ret;
		}
	}
}

