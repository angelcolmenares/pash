using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class DirectoryEntryManager
	{
		private Hashtable directoryEntries;

		private string bindingPrefix;

		private DirectoryContext context;

		//private NativeComInterfaces.IAdsPathname pathCracker;

		internal DirectoryEntryManager(DirectoryContext context)
		{
			this.directoryEntries = new Hashtable();
			this.context = context;
			//this.pathCracker = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
			//this.pathCracker.EscapedMode = 2;
		}

		internal static DirectoryEntry Bind(string ldapPath, string username, string password, bool useServerBind)
		{
			AuthenticationTypes defaultAuthType = Utils.DefaultAuthType;
			if (DirectoryContext.ServerBindSupported && useServerBind)
			{
				defaultAuthType = defaultAuthType | AuthenticationTypes.ServerBind;
			}
			ldapPath = ldapPath.Replace ("RootDSE", "");
			DirectoryEntry directoryEntry = new DirectoryEntry(ldapPath, username, password, defaultAuthType);

			return directoryEntry;
		}

		internal string ExpandWellKnownDN(WellKnownDN dn)
		{
			string propertyValue = null;
			WellKnownDN wellKnownDN = dn;
			if (wellKnownDN == WellKnownDN.RootDSE)
			{
				propertyValue = "RootDSE";
			}
			else if (wellKnownDN == WellKnownDN.DefaultNamingContext)
			{
				DirectoryEntry cachedDirectoryEntry = this.GetCachedDirectoryEntry("RootDSE");
				propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DefaultNamingContext);
			}
			else if (wellKnownDN == WellKnownDN.SchemaNamingContext)
			{
				DirectoryEntry directoryEntry = this.GetCachedDirectoryEntry("RootDSE");
				propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.SchemaNamingContext);
			}
			else if (wellKnownDN == WellKnownDN.ConfigurationNamingContext)
			{
				DirectoryEntry cachedDirectoryEntry1 = this.GetCachedDirectoryEntry("RootDSE");
				propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry1, PropertyManager.ConfigurationNamingContext);
			}
			else if (wellKnownDN == WellKnownDN.PartitionsContainer)
			{
				propertyValue = string.Concat("CN=Partitions,", this.ExpandWellKnownDN(WellKnownDN.ConfigurationNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.SitesContainer)
			{
				propertyValue = string.Concat("CN=Sites,", this.ExpandWellKnownDN(WellKnownDN.ConfigurationNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.SystemContainer)
			{
				propertyValue = string.Concat("CN=System,", this.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.RidManager)
			{
				propertyValue = string.Concat("CN=RID Manager$,", this.ExpandWellKnownDN(WellKnownDN.SystemContainer));
			}
			else if (wellKnownDN == WellKnownDN.Infrastructure)
			{
				propertyValue = string.Concat("CN=Infrastructure,", this.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.RootDomainNamingContext)
			{
				DirectoryEntry directoryEntry1 = this.GetCachedDirectoryEntry("RootDSE");
				propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry1, PropertyManager.RootDomainNamingContext);
			}
			else
			{
				throw new InvalidEnumArgumentException("dn", (int)dn, typeof(WellKnownDN));
			}
			return propertyValue;
		}

		internal static string ExpandWellKnownDN(DirectoryContext context, WellKnownDN dn)
		{
			string propertyValue = null;
			WellKnownDN wellKnownDN = dn;
			if (wellKnownDN == WellKnownDN.RootDSE)
			{
				propertyValue = "RootDSE";
			}
			else if (wellKnownDN == WellKnownDN.DefaultNamingContext)
			{
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, "RootDSE");
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.DefaultNamingContext);
				}
				finally
				{
					directoryEntry.Dispose();
				}
			}
			else if (wellKnownDN == WellKnownDN.SchemaNamingContext)
			{
				DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(context, "RootDSE");
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry1, PropertyManager.SchemaNamingContext);
				}
				finally
				{
					directoryEntry1.Dispose();
				}
			}
			else if (wellKnownDN == WellKnownDN.ConfigurationNamingContext)
			{
				DirectoryEntry directoryEntry2 = DirectoryEntryManager.GetDirectoryEntry(context, "RootDSE");
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry2, PropertyManager.ConfigurationNamingContext);
				}
				finally
				{
					directoryEntry2.Dispose();
				}
			}
			else if (wellKnownDN == WellKnownDN.PartitionsContainer)
			{
				propertyValue = string.Concat("CN=Partitions,", DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.ConfigurationNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.SitesContainer)
			{
				propertyValue = string.Concat("CN=Sites,", DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.ConfigurationNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.SystemContainer)
			{
				propertyValue = string.Concat("CN=System,", DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.DefaultNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.RidManager)
			{
				propertyValue = string.Concat("CN=RID Manager$,", DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.SystemContainer));
			}
			else if (wellKnownDN == WellKnownDN.Infrastructure)
			{
				propertyValue = string.Concat("CN=Infrastructure,", DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.DefaultNamingContext));
			}
			else if (wellKnownDN == WellKnownDN.RootDomainNamingContext)
			{
				DirectoryEntry directoryEntry3 = DirectoryEntryManager.GetDirectoryEntry(context, "RootDSE");
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry3, PropertyManager.RootDomainNamingContext);
				}
				finally
				{
					directoryEntry3.Dispose();
				}
			}
			else
			{
				throw new InvalidEnumArgumentException("dn", (int)dn, typeof(WellKnownDN));
			}
			return propertyValue;
		}

		internal ICollection GetCachedDirectoryEntries()
		{
			return this.directoryEntries.Values;
		}

		internal DirectoryEntry GetCachedDirectoryEntry(WellKnownDN dn)
		{
			return this.GetCachedDirectoryEntry(this.ExpandWellKnownDN(dn));
		}

		internal DirectoryEntry GetCachedDirectoryEntry(string distinguishedName)
		{
			object obj = distinguishedName;
			if (string.Compare(distinguishedName, "rootdse", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(distinguishedName, "schema", StringComparison.OrdinalIgnoreCase) != 0)
			{
				obj = new DistinguishedName(distinguishedName);
			}
			if (!this.directoryEntries.ContainsKey(obj))
			{
				DirectoryEntry newDirectoryEntry = this.GetNewDirectoryEntry(distinguishedName);
				this.directoryEntries.Add(obj, newDirectoryEntry);
			}
			return (DirectoryEntry)this.directoryEntries[obj];
		}

		internal static DirectoryEntry GetDirectoryEntry(DirectoryContext context, WellKnownDN dn)
		{
			return DirectoryEntryManager.GetDirectoryEntry(context, DirectoryEntryManager.ExpandWellKnownDN(context, dn));
		}

		internal static DirectoryEntry GetDirectoryEntry(DirectoryContext context, string dn)
		{
			string str = string.Concat("LDAP://", context.GetServerName(), "/");
			/* NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
			pathname.EscapedMode = 2;
			pathname.Set(dn, 4);
			string str1 = pathname.Retrieve(7);
			*/
			string str1 = dn;
			return DirectoryEntryManager.Bind(string.Concat(str, str1), context.UserName, context.Password, context.useServerBind());
		}

		internal static DirectoryEntry GetDirectoryEntryInternal(DirectoryContext context, string path)
		{
			return DirectoryEntryManager.Bind(path, context.UserName, context.Password, context.useServerBind());
		}

		private DirectoryEntry GetNewDirectoryEntry(string dn)
		{
			if (this.bindingPrefix == null)
			{
				this.bindingPrefix = string.Concat("LDAP://", this.context.GetServerName(), ":389", "/");
			}
			//this.pathCracker.Set(dn, 4);
			//string str = this.pathCracker.Retrieve(7);
			return DirectoryEntryManager.Bind(string.Concat(this.bindingPrefix, dn), this.context.UserName, this.context.Password, this.context.useServerBind());
		}

		internal void RemoveIfExists(string distinguishedName)
		{
			object obj = distinguishedName;
			if (string.Compare(distinguishedName, "rootdse", StringComparison.OrdinalIgnoreCase) != 0)
			{
				obj = new DistinguishedName(distinguishedName);
			}
			if (this.directoryEntries.ContainsKey(obj))
			{
				DirectoryEntry item = (DirectoryEntry)this.directoryEntries[obj];
				if (item != null)
				{
					this.directoryEntries.Remove(obj);
					item.Dispose();
				}
			}
		}
	}
}