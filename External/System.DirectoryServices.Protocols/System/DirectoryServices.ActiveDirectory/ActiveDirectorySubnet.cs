using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectorySubnet : IDisposable
	{
		private ActiveDirectorySite site;

		private string name;

		internal DirectoryContext context;

		private bool disposed;

		internal bool existing;

		internal DirectoryEntry cachedEntry;

		public string Location
		{
			get
			{
				string item;
				if (!this.disposed)
				{
					try
					{
						if (!this.cachedEntry.Properties.Contains("location"))
						{
							item = null;
						}
						else
						{
							item = (string)this.cachedEntry.Properties["location"][0];
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return item;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.disposed)
				{
					try
					{
						if (value != null)
						{
							this.cachedEntry.Properties["location"].Value = value;
						}
						else
						{
							if (this.cachedEntry.Properties.Contains("location"))
							{
								this.cachedEntry.Properties["location"].Clear();
							}
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public string Name
		{
			get
			{
				if (!this.disposed)
				{
					return this.name;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ActiveDirectorySite Site
		{
			get
			{
				if (!this.disposed)
				{
					return this.site;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.disposed)
				{
					if (value == null || value.existing)
					{
						this.site = value;
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = value;
						throw new InvalidOperationException(Res.GetString("SiteNotCommitted", objArray));
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ActiveDirectorySubnet(DirectoryContext context, string subnetName)
		{
			ActiveDirectorySubnet.ValidateArgument(context, subnetName);
			context = new DirectoryContext(context);
			this.context = context;
			this.name = subnetName;
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
					string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
					string str = string.Concat("CN=Subnets,CN=Sites,", propertyValue);
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str);
					string escapedPath = string.Concat("cn=", this.name);
					escapedPath = Utils.GetEscapedPath(escapedPath);
					this.cachedEntry = directoryEntry.Children.Add(escapedPath, "subnet");
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] name = new object[1];
					name[0] = context.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", name));
				}
			}
		}

		public ActiveDirectorySubnet(DirectoryContext context, string subnetName, string siteName) : this(context, subnetName)
		{
			if (siteName != null)
			{
				if (siteName.Length != 0)
				{
					try
					{
						this.site = ActiveDirectorySite.FindByName(this.context, siteName);
					}
					catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
					{
						object[] objArray = new object[1];
						objArray[0] = siteName;
						throw new ArgumentException(Res.GetString("SiteNotExist", objArray), "siteName");
					}
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
				}
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		internal ActiveDirectorySubnet(DirectoryContext context, string subnetName, string siteName, bool existing)
		{
			this.context = context;
			this.name = subnetName;
			if (siteName != null)
			{
				try
				{
					this.site = ActiveDirectorySite.FindByName(context, siteName);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] objArray = new object[1];
					objArray[0] = siteName;
					throw new ArgumentException(Res.GetString("SiteNotExist", objArray), "siteName");
				}
			}
			this.existing = true;
		}

		public void Delete()
		{
			if (!this.disposed)
			{
				if (this.existing)
				{
					try
					{
						this.cachedEntry.Parent.Children.Remove(this.cachedEntry);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return;
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotDelete"));
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && this.cachedEntry != null)
			{
				this.cachedEntry.Dispose();
			}
			this.disposed = true;
		}

		public static ActiveDirectorySubnet FindByName(DirectoryContext context, string subnetName)
		{
			DirectoryEntry directoryEntry;
			ActiveDirectorySubnet activeDirectorySubnet;
			ActiveDirectorySubnet activeDirectorySubnet1;
			ActiveDirectorySubnet.ValidateArgument(context, subnetName);
			context = new DirectoryContext(context);
			try
			{
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
				string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
				string str = string.Concat("CN=Subnets,CN=Sites,", propertyValue);
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
			}
			catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
			{
				object[] name = new object[1];
				name[0] = context.Name;
				throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", name));
			}
			using (directoryEntry)
			{
				try
				{
					string[] strArrays = new string[1];
					strArrays[0] = "distinguishedName";
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=subnet)(objectCategory=subnet)(name=", Utils.GetEscapedFilterValue(subnetName), "))"), strArrays, SearchScope.OneLevel, false, false);
					SearchResult searchResult = aDSearcher.FindOne();
					if (searchResult != null)
					{
						string str1 = null;
						DirectoryEntry directoryEntry1 = searchResult.GetDirectoryEntry();
						if (directoryEntry1.Properties.Contains("siteObject"))
						{
							NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
							pathname.EscapedMode = 4;
							string item = (string)directoryEntry1.Properties["siteObject"][0];
							pathname.Set(item, 4);
							string str2 = pathname.Retrieve(11);
							str1 = str2.Substring(3);
						}
						if (str1 != null)
						{
							activeDirectorySubnet = new ActiveDirectorySubnet(context, subnetName, str1, true);
						}
						else
						{
							activeDirectorySubnet = new ActiveDirectorySubnet(context, subnetName, null, true);
						}
						activeDirectorySubnet.cachedEntry = directoryEntry1;
						activeDirectorySubnet1 = activeDirectorySubnet;
					}
					else
					{
						Exception exception = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySubnet), subnetName);
						throw exception;
					}
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					if (cOMException2.ErrorCode != -2147016656)
					{
						throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException2);
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySubnet), subnetName);
					}
				}
			}
			return activeDirectorySubnet1;
		}

		public DirectoryEntry GetDirectoryEntry()
		{
			if (!this.disposed)
			{
				if (this.existing)
				{
					return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedEntry.Path);
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotGetObject"));
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public void Save()
		{
			if (!this.disposed)
			{
				try
				{
					if (!this.existing)
					{
						if (this.Site != null)
						{
							this.cachedEntry.Properties["siteObject"].Add(this.site.cachedEntry.Properties["distinguishedName"][0]);
						}
						this.cachedEntry.CommitChanges();
						this.existing = true;
					}
					else
					{
						if (this.site != null)
						{
							this.cachedEntry.Properties["siteObject"].Value = this.site.cachedEntry.Properties["distinguishedName"][0];
						}
						else
						{
							if (this.cachedEntry.Properties.Contains("siteObject"))
							{
								this.cachedEntry.Properties["siteObject"].Clear();
							}
						}
						this.cachedEntry.CommitChanges();
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override string ToString()
		{
			if (!this.disposed)
			{
				return this.Name;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private static void ValidateArgument(DirectoryContext context, string subnetName)
		{
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isServer() || context.isADAMConfigSet())
					{
						if (subnetName != null)
						{
							if (subnetName.Length != 0)
							{
								return;
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "subnetName");
							}
						}
						else
						{
							throw new ArgumentNullException("subnetName");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}
	}
}