using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectorySiteLink : IDisposable
	{
		internal DirectoryContext context;

		private string name;

		private ActiveDirectoryTransportType transport;

		private bool disposed;

		internal bool existing;

		internal DirectoryEntry cachedEntry;

		private TimeSpan systemDefaultInterval;

		private ActiveDirectorySiteCollection sites;

		private bool siteRetrieved;

		private const int systemDefaultCost = 0;

		private const int appDefaultCost = 100;

		private const int appDefaultInterval = 180;

		public int Cost
		{
			get
			{
				int item;
				if (!this.disposed)
				{
					try
					{
						if (!this.cachedEntry.Properties.Contains("cost"))
						{
							return 0;
						}
						else
						{
							item = (int)this.cachedEntry.Properties["cost"][0];
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
					if (value >= 0)
					{
						try
						{
							this.cachedEntry.Properties["cost"].Value = value;
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
						throw new ArgumentException("value");
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public bool DataCompressionEnabled
		{
			get
			{
				if (!this.disposed)
				{
					int item = 0;
					PropertyValueCollection propertyValueCollection = null;
					try
					{
						propertyValueCollection = this.cachedEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (propertyValueCollection.Count != 0)
					{
						item = (int)propertyValueCollection[0];
					}
					if ((item & 4) != 0)
					{
						return false;
					}
					else
					{
						return true;
					}
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
					int item = 0;
					try
					{
						PropertyValueCollection propertyValueCollection = this.cachedEntry.Properties["options"];
						if (propertyValueCollection.Count != 0)
						{
							item = (int)propertyValueCollection[0];
						}
						if (value)
						{
							item = item & -5;
						}
						else
						{
							item = item | 4;
						}
						this.cachedEntry.Properties["options"].Value = item;
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

		public ActiveDirectorySchedule InterSiteReplicationSchedule
		{
			get
			{
				if (!this.disposed)
				{
					ActiveDirectorySchedule activeDirectorySchedule = null;
					try
					{
						if (this.cachedEntry.Properties.Contains("schedule"))
						{
							byte[] item = (byte[])this.cachedEntry.Properties["schedule"][0];
							activeDirectorySchedule = new ActiveDirectorySchedule();
							activeDirectorySchedule.SetUnmanagedSchedule(item);
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return activeDirectorySchedule;
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
							this.cachedEntry.Properties["schedule"].Value = value.GetUnmanagedSchedule();
						}
						else
						{
							if (this.cachedEntry.Properties.Contains("schedule"))
							{
								this.cachedEntry.Properties["schedule"].Clear();
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

		public bool NotificationEnabled
		{
			get
			{
				if (!this.disposed)
				{
					int item = 0;
					PropertyValueCollection propertyValueCollection = null;
					try
					{
						propertyValueCollection = this.cachedEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (propertyValueCollection.Count != 0)
					{
						item = (int)propertyValueCollection[0];
					}
					if ((item & 1) != 0)
					{
						return true;
					}
					else
					{
						return false;
					}
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
					int item = 0;
					try
					{
						PropertyValueCollection propertyValueCollection = this.cachedEntry.Properties["options"];
						if (propertyValueCollection.Count != 0)
						{
							item = (int)propertyValueCollection[0];
						}
						if (!value)
						{
							item = item & -2;
						}
						else
						{
							item = item | 1;
						}
						this.cachedEntry.Properties["options"].Value = item;
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

		public bool ReciprocalReplicationEnabled
		{
			get
			{
				if (!this.disposed)
				{
					int item = 0;
					PropertyValueCollection propertyValueCollection = null;
					try
					{
						propertyValueCollection = this.cachedEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (propertyValueCollection.Count != 0)
					{
						item = (int)propertyValueCollection[0];
					}
					if ((item & 2) != 0)
					{
						return true;
					}
					else
					{
						return false;
					}
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
					int item = 0;
					try
					{
						PropertyValueCollection propertyValueCollection = this.cachedEntry.Properties["options"];
						if (propertyValueCollection.Count != 0)
						{
							item = (int)propertyValueCollection[0];
						}
						if (!value)
						{
							item = item & -3;
						}
						else
						{
							item = item | 2;
						}
						this.cachedEntry.Properties["options"].Value = item;
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

		public TimeSpan ReplicationInterval
		{
			get
			{
				TimeSpan timeSpan;
				if (!this.disposed)
				{
					try
					{
						if (!this.cachedEntry.Properties.Contains("replInterval"))
						{
							return this.systemDefaultInterval;
						}
						else
						{
							int item = (int)this.cachedEntry.Properties["replInterval"][0];
							timeSpan = new TimeSpan(0, item, 0);
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return timeSpan;
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
					if (value >= TimeSpan.Zero)
					{
						double totalMinutes = value.TotalMinutes;
						if (totalMinutes <= 2147483647)
						{
							int num = (int)totalMinutes;
							if ((double)num >= totalMinutes)
							{
								try
								{
									this.cachedEntry.Properties["replInterval"].Value = num;
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
								throw new ArgumentException(Res.GetString("ReplicationIntervalInMinutes"), "value");
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("ReplicationIntervalExceedMax"), "value");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ActiveDirectorySiteCollection Sites
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.siteRetrieved)
					{
						this.sites.initialized = false;
						this.sites.Clear();
						this.GetSites();
						this.siteRetrieved = true;
					}
					this.sites.initialized = true;
					this.sites.de = this.cachedEntry;
					this.sites.context = this.context;
					return this.sites;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ActiveDirectoryTransportType TransportType
		{
			get
			{
				if (!this.disposed)
				{
					return this.transport;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName) : this(context, siteLinkName, 0, null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport) : this(context, siteLinkName, transport, null)
		{
		}

		public ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport, ActiveDirectorySchedule schedule)
		{
			DirectoryEntry directoryEntry;
			string str;
			this.systemDefaultInterval = new TimeSpan(0, 15, 0);
			this.sites = new ActiveDirectorySiteCollection();
			ActiveDirectorySiteLink.ValidateArgument(context, siteLinkName, transport);
			context = new DirectoryContext(context);
			this.context = context;
			this.name = siteLinkName;
			this.transport = transport;
			try
			{
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
				string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
				if (transport != ActiveDirectoryTransportType.Rpc)
				{
					str = string.Concat("CN=SMTP,CN=Inter-Site Transports,CN=Sites,", propertyValue);
				}
				else
				{
					str = string.Concat("CN=IP,CN=Inter-Site Transports,CN=Sites,", propertyValue);
				}
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
			try
			{
				try
				{
					string escapedPath = string.Concat("cn=", this.name);
					escapedPath = Utils.GetEscapedPath(escapedPath);
					this.cachedEntry = directoryEntry.Children.Add(escapedPath, "siteLink");
					this.cachedEntry.Properties["cost"].Value = 100;
					this.cachedEntry.Properties["replInterval"].Value = 180;
					if (schedule != null)
					{
						this.cachedEntry.Properties["schedule"].Value = schedule.GetUnmanagedSchedule();
					}
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					if (cOMException2.ErrorCode == -2147016656)
					{
						DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
						if (Utils.CheckCapability(directoryEntry1, Capability.ActiveDirectoryApplicationMode) && transport == ActiveDirectoryTransportType.Smtp)
						{
							throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
						}
					}
					throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException2);
				}
			}
			finally
			{
				directoryEntry.Dispose();
			}
		}

		internal ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport, bool existing, DirectoryEntry entry)
		{
			this.systemDefaultInterval = new TimeSpan(0, 15, 0);
			this.sites = new ActiveDirectorySiteCollection();
			this.context = context;
			this.name = siteLinkName;
			this.transport = transport;
			this.existing = existing;
			this.cachedEntry = entry;
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

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static ActiveDirectorySiteLink FindByName(DirectoryContext context, string siteLinkName)
		{
			return ActiveDirectorySiteLink.FindByName(context, siteLinkName, ActiveDirectoryTransportType.Rpc);
		}

		public static ActiveDirectorySiteLink FindByName(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport)
		{
			DirectoryEntry directoryEntry;
			ActiveDirectorySiteLink activeDirectorySiteLink;
			ActiveDirectorySiteLink.ValidateArgument(context, siteLinkName, transport);
			context = new DirectoryContext(context);
			try
			{
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
				string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
				string str = string.Concat("CN=Inter-Site Transports,CN=Sites,", propertyValue);
				if (transport != ActiveDirectoryTransportType.Rpc)
				{
					str = string.Concat("CN=SMTP,", str);
				}
				else
				{
					str = string.Concat("CN=IP,", str);
				}
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
			try
			{
				try
				{
					string[] strArrays = new string[1];
					strArrays[0] = "distinguishedName";
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=siteLink)(objectCategory=SiteLink)(name=", Utils.GetEscapedFilterValue(siteLinkName), "))"), strArrays, SearchScope.OneLevel, false, false);
					SearchResult searchResult = aDSearcher.FindOne();
					if (searchResult != null)
					{
						DirectoryEntry directoryEntry1 = searchResult.GetDirectoryEntry();
						ActiveDirectorySiteLink activeDirectorySiteLink1 = new ActiveDirectorySiteLink(context, siteLinkName, transport, true, directoryEntry1);
						activeDirectorySiteLink = activeDirectorySiteLink1;
					}
					else
					{
						Exception exception = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLink), siteLinkName);
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
						DirectoryEntry directoryEntry2 = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
						if (!Utils.CheckCapability(directoryEntry2, Capability.ActiveDirectoryApplicationMode) || transport != ActiveDirectoryTransportType.Smtp)
						{
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLink), siteLinkName);
						}
						else
						{
							throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
						}
					}
				}
			}
			finally
			{
				directoryEntry.Dispose();
			}
			return activeDirectorySiteLink;
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

		private void GetSites()
		{
			NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
			ArrayList arrayLists = new ArrayList();
			pathname.EscapedMode = 4;
			string str = "siteList";
			arrayLists.Add(str);
			Hashtable valuesWithRangeRetrieval = Utils.GetValuesWithRangeRetrieval(this.cachedEntry, "(objectClass=*)", arrayLists, SearchScope.Base);
			ArrayList item = (ArrayList)valuesWithRangeRetrieval[str.ToLower(CultureInfo.InvariantCulture)];
			if (item != null)
			{
				for (int i = 0; i < item.Count; i++)
				{
					string item1 = (string)item[i];
					pathname.Set(item1, 4);
					string str1 = pathname.Retrieve(11);
					str1 = str1.Substring(3);
					ActiveDirectorySite activeDirectorySite = new ActiveDirectorySite(this.context, str1, true);
					this.sites.Add(activeDirectorySite);
				}
				return;
			}
			else
			{
				return;
			}
		}

		public void Save()
		{
			if (!this.disposed)
			{
				try
				{
					this.cachedEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				if (!this.existing)
				{
					this.existing = true;
					return;
				}
				else
				{
					this.siteRetrieved = false;
					return;
				}
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
				return this.name;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private static void ValidateArgument(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport)
		{
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isServer() || context.isADAMConfigSet())
					{
						if (siteLinkName != null)
						{
							if (siteLinkName.Length != 0)
							{
								if (transport < ActiveDirectoryTransportType.Rpc || transport > ActiveDirectoryTransportType.Smtp)
								{
									throw new InvalidEnumArgumentException("value", (int)transport, typeof(ActiveDirectoryTransportType));
								}
								else
								{
									return;
								}
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteLinkName");
							}
						}
						else
						{
							throw new ArgumentNullException("siteLinkName");
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