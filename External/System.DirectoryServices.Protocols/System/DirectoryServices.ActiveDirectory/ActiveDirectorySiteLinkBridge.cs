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
	public class ActiveDirectorySiteLinkBridge : IDisposable
	{
		internal DirectoryContext context;

		private string name;

		private ActiveDirectoryTransportType transport;

		private bool disposed;

		private bool existing;

		internal DirectoryEntry cachedEntry;

		private ActiveDirectorySiteLinkCollection links;

		private bool linksRetrieved;

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

		public ActiveDirectorySiteLinkCollection SiteLinks
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.linksRetrieved)
					{
						this.links.initialized = false;
						this.links.Clear();
						this.GetLinks();
						this.linksRetrieved = true;
					}
					this.links.initialized = true;
					this.links.de = this.cachedEntry;
					this.links.context = this.context;
					return this.links;
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
		public ActiveDirectorySiteLinkBridge(DirectoryContext context, string bridgeName) : this(context, bridgeName, 0)
		{
		}

		public ActiveDirectorySiteLinkBridge(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport)
		{
			DirectoryEntry directoryEntry;
			string str;
			this.links = new ActiveDirectorySiteLinkCollection();
			ActiveDirectorySiteLinkBridge.ValidateArgument(context, bridgeName, transport);
			context = new DirectoryContext(context);
			this.context = context;
			this.name = bridgeName;
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
					this.cachedEntry = directoryEntry.Children.Add(escapedPath, "siteLinkBridge");
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

		internal ActiveDirectorySiteLinkBridge(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport, bool existing)
		{
			this.links = new ActiveDirectorySiteLinkCollection();
			this.context = context;
			this.name = bridgeName;
			this.transport = transport;
			this.existing = existing;
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
		public static ActiveDirectorySiteLinkBridge FindByName(DirectoryContext context, string bridgeName)
		{
			return ActiveDirectorySiteLinkBridge.FindByName(context, bridgeName, ActiveDirectoryTransportType.Rpc);
		}

		public static ActiveDirectorySiteLinkBridge FindByName(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport)
		{
			DirectoryEntry directoryEntry;
			ActiveDirectorySiteLinkBridge activeDirectorySiteLinkBridge;
			ActiveDirectorySiteLinkBridge.ValidateArgument(context, bridgeName, transport);
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
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=siteLinkBridge)(objectCategory=SiteLinkBridge)(name=", Utils.GetEscapedFilterValue(bridgeName), "))"), strArrays, SearchScope.OneLevel, false, false);
					SearchResult searchResult = aDSearcher.FindOne();
					if (searchResult != null)
					{
						DirectoryEntry directoryEntry1 = searchResult.GetDirectoryEntry();
						ActiveDirectorySiteLinkBridge activeDirectorySiteLinkBridge1 = new ActiveDirectorySiteLinkBridge(context, bridgeName, transport, true);
						activeDirectorySiteLinkBridge1.cachedEntry = directoryEntry1;
						activeDirectorySiteLinkBridge = activeDirectorySiteLinkBridge1;
					}
					else
					{
						Exception exception = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLinkBridge), bridgeName);
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
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLinkBridge), bridgeName);
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
			return activeDirectorySiteLinkBridge;
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

		private void GetLinks()
		{
			ArrayList arrayLists = new ArrayList();
			NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
			pathname.EscapedMode = 4;
			string str = "siteLinkList";
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
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, item1);
					ActiveDirectorySiteLink activeDirectorySiteLink = new ActiveDirectorySiteLink(this.context, str1, this.transport, true, directoryEntry);
					this.links.Add(activeDirectorySiteLink);
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
					this.linksRetrieved = false;
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

		private static void ValidateArgument(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport)
		{
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isServer() || context.isADAMConfigSet())
					{
						if (bridgeName != null)
						{
							if (bridgeName.Length != 0)
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
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "bridgeName");
							}
						}
						else
						{
							throw new ArgumentNullException("bridgeName");
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