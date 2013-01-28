using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectoryInterSiteTransport : IDisposable
	{
		private DirectoryContext context;

		private DirectoryEntry cachedEntry;

		private ActiveDirectoryTransportType transport;

		private bool disposed;

		private bool linkRetrieved;

		private bool bridgeRetrieved;

		private ReadOnlySiteLinkCollection siteLinkCollection;

		private ReadOnlySiteLinkBridgeCollection bridgeCollection;

		public bool BridgeAllSiteLinks
		{
			get
			{
				if (!this.disposed)
				{
					int item = 0;
					try
					{
						if (this.cachedEntry.Properties.Contains("options"))
						{
							item = (int)this.cachedEntry.Properties["options"][0];
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if ((item & 2) == 0)
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
						if (this.cachedEntry.Properties.Contains("options"))
						{
							item = (int)this.cachedEntry.Properties["options"][0];
						}
						if (!value)
						{
							item = item | 2;
						}
						else
						{
							item = item & -3;
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

		public bool IgnoreReplicationSchedule
		{
			get
			{
				if (!this.disposed)
				{
					int item = 0;
					try
					{
						if (this.cachedEntry.Properties.Contains("options"))
						{
							item = (int)this.cachedEntry.Properties["options"][0];
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if ((item & 1) == 0)
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
						if (this.cachedEntry.Properties.Contains("options"))
						{
							item = (int)this.cachedEntry.Properties["options"][0];
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

		public ReadOnlySiteLinkBridgeCollection SiteLinkBridges
		{
			get
			{
				if (!this.disposed)
				{
					if (!this.bridgeRetrieved)
					{
						this.bridgeCollection.Clear();
						string[] strArrays = new string[1];
						strArrays[0] = "cn";
						ADSearcher aDSearcher = new ADSearcher(this.cachedEntry, "(&(objectClass=siteLinkBridge)(objectCategory=SiteLinkBridge))", strArrays, SearchScope.OneLevel);
						SearchResultCollection searchResultCollections = null;
						try
						{
							searchResultCollections = aDSearcher.FindAll();
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						try
						{
							foreach (SearchResult searchResult in searchResultCollections)
							{
								DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
								string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
								ActiveDirectorySiteLinkBridge activeDirectorySiteLinkBridge = new ActiveDirectorySiteLinkBridge(this.context, searchResultPropertyValue, this.transport, true);
								activeDirectorySiteLinkBridge.cachedEntry = directoryEntry;
								this.bridgeCollection.Add(activeDirectorySiteLinkBridge);
							}
						}
						finally
						{
							searchResultCollections.Dispose();
						}
						this.bridgeRetrieved = true;
					}
					return this.bridgeCollection;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ReadOnlySiteLinkCollection SiteLinks
		{
			get
			{
				if (!this.disposed)
				{
					if (!this.linkRetrieved)
					{
						this.siteLinkCollection.Clear();
						string[] strArrays = new string[1];
						strArrays[0] = "cn";
						ADSearcher aDSearcher = new ADSearcher(this.cachedEntry, "(&(objectClass=siteLink)(objectCategory=SiteLink))", strArrays, SearchScope.OneLevel);
						SearchResultCollection searchResultCollections = null;
						try
						{
							searchResultCollections = aDSearcher.FindAll();
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						try
						{
							foreach (SearchResult searchResult in searchResultCollections)
							{
								DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
								string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
								ActiveDirectorySiteLink activeDirectorySiteLink = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue, this.transport, true, directoryEntry);
								this.siteLinkCollection.Add(activeDirectorySiteLink);
							}
						}
						finally
						{
							searchResultCollections.Dispose();
						}
						this.linkRetrieved = true;
					}
					return this.siteLinkCollection;
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

		internal ActiveDirectoryInterSiteTransport(DirectoryContext context, ActiveDirectoryTransportType transport, DirectoryEntry entry)
		{
			this.siteLinkCollection = new ReadOnlySiteLinkCollection();
			this.bridgeCollection = new ReadOnlySiteLinkBridgeCollection();
			this.context = context;
			this.transport = transport;
			this.cachedEntry = entry;
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

		public static ActiveDirectoryInterSiteTransport FindByTransportType(DirectoryContext context, ActiveDirectoryTransportType transport)
		{
			DirectoryEntry directoryEntry;
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isServer() || context.isADAMConfigSet())
					{
						if (transport < ActiveDirectoryTransportType.Rpc || transport > ActiveDirectoryTransportType.Smtp)
						{
							throw new InvalidEnumArgumentException("value", (int)transport, typeof(ActiveDirectoryTransportType));
						}
						else
						{
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
								string[] strArrays = new string[1];
								strArrays[0] = "options";
								directoryEntry.RefreshCache(strArrays);
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
									DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
									if (!Utils.CheckCapability(directoryEntry1, Capability.ActiveDirectoryApplicationMode) || transport != ActiveDirectoryTransportType.Smtp)
									{
										object[] objArray = new object[1];
										objArray[0] = transport.ToString();
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("TransportNotFound", objArray), typeof(ActiveDirectoryInterSiteTransport), transport.ToString());
									}
									else
									{
										throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
									}
								}
							}
							return new ActiveDirectoryInterSiteTransport(context, transport, directoryEntry);
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

		public DirectoryEntry GetDirectoryEntry()
		{
			if (!this.disposed)
			{
				return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedEntry.Path);
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
					this.cachedEntry.CommitChanges();
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
				return this.transport.ToString();
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}
	}
}