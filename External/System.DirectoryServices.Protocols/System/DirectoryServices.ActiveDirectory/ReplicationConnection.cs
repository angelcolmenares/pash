using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ReplicationConnection : IDisposable
	{
		internal DirectoryContext context;

		internal DirectoryEntry cachedDirectoryEntry;

		internal bool existingConnection;

		private bool disposed;

		private bool checkADAM;

		private bool isADAMServer;

		private int options;

		private string connectionName;

		private string sourceServerName;

		private string destinationServerName;

		private ActiveDirectoryTransportType transport;

		private const string ADAMGuid = "1.2.840.113556.1.4.1851";

		public NotificationStatus ChangeNotificationStatus
		{
			get
			{
				if (!this.disposed)
				{
					PropertyValueCollection item = null;
					try
					{
						item = this.cachedDirectoryEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Count != 0)
					{
						this.options = (int)item[0];
					}
					else
					{
						this.options = 0;
					}
					int num = this.options & 4;
					int num1 = this.options & 8;
					if (num != 4 || num1 != 0)
					{
						if (num != 4 || num1 != 8)
						{
							return NotificationStatus.IntraSiteOnly;
						}
						else
						{
							return NotificationStatus.NotificationAlways;
						}
					}
					else
					{
						return NotificationStatus.NoNotification;
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
					if (value < NotificationStatus.NoNotification || value > NotificationStatus.NotificationAlways)
					{
						throw new InvalidEnumArgumentException("value", (int)value, typeof(NotificationStatus));
					}
					else
					{
						try
						{
							PropertyValueCollection item = this.cachedDirectoryEntry.Properties["options"];
							if (item.Count != 0)
							{
								this.options = (int)item[0];
							}
							else
							{
								this.options = 0;
							}
							if (value != NotificationStatus.IntraSiteOnly)
							{
								if (value != NotificationStatus.NoNotification)
								{
									ReplicationConnection replicationConnection = this;
									replicationConnection.options = replicationConnection.options | 4;
									ReplicationConnection replicationConnection1 = this;
									replicationConnection1.options = replicationConnection1.options | 8;
								}
								else
								{
									ReplicationConnection replicationConnection2 = this;
									replicationConnection2.options = replicationConnection2.options | 4;
									ReplicationConnection replicationConnection3 = this;
									replicationConnection3.options = replicationConnection3.options & -9;
								}
							}
							else
							{
								ReplicationConnection replicationConnection4 = this;
								replicationConnection4.options = replicationConnection4.options & -5;
								ReplicationConnection replicationConnection5 = this;
								replicationConnection5.options = replicationConnection5.options & -9;
							}
							this.cachedDirectoryEntry.Properties["options"].Value = this.options;
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						return;
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
					PropertyValueCollection item = null;
					try
					{
						item = this.cachedDirectoryEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Count != 0)
					{
						this.options = (int)item[0];
					}
					else
					{
						this.options = 0;
					}
					if ((this.options & 16) != 0)
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
					try
					{
						PropertyValueCollection item = this.cachedDirectoryEntry.Properties["options"];
						if (item.Count != 0)
						{
							this.options = (int)item[0];
						}
						else
						{
							this.options = 0;
						}
						if (value)
						{
							ReplicationConnection replicationConnection = this;
							replicationConnection.options = replicationConnection.options & -17;
						}
						else
						{
							ReplicationConnection replicationConnection1 = this;
							replicationConnection1.options = replicationConnection1.options | 16;
						}
						this.cachedDirectoryEntry.Properties["options"].Value = this.options;
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

		public string DestinationServer
		{
			get
			{
				if (!this.disposed)
				{
					if (this.destinationServerName == null)
					{
						DirectoryEntry parent = null;
						DirectoryEntry directoryEntry = null;
						try
						{
							parent = this.cachedDirectoryEntry.Parent;
							directoryEntry = parent.Parent;
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.DnsHostName);
						if (!this.IsADAM)
						{
							this.destinationServerName = propertyValue;
						}
						else
						{
							int num = (int)PropertyManager.GetPropertyValue(this.context, parent, PropertyManager.MsDSPortLDAP);
							if (num == 0x185)
							{
								this.destinationServerName = propertyValue;
							}
							else
							{
								this.destinationServerName = string.Concat(propertyValue, ":", num);
							}
						}
					}
					return this.destinationServerName;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public bool Enabled
		{
			get
			{
				bool item;
				if (!this.disposed)
				{
					try
					{
						if (!this.cachedDirectoryEntry.Properties.Contains("enabledConnection"))
						{
							item = false;
						}
						else
						{
							item = (bool)this.cachedDirectoryEntry.Properties["enabledConnection"][0];
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
						this.cachedDirectoryEntry.Properties["enabledConnection"].Value = value;
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

		public bool GeneratedByKcc
		{
			get
			{
				if (!this.disposed)
				{
					PropertyValueCollection item = null;
					try
					{
						item = this.cachedDirectoryEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Count != 0)
					{
						this.options = (int)item[0];
					}
					else
					{
						this.options = 0;
					}
					if ((this.options & 1) != 0)
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
					try
					{
						PropertyValueCollection item = this.cachedDirectoryEntry.Properties["options"];
						if (item.Count != 0)
						{
							this.options = (int)item[0];
						}
						else
						{
							this.options = 0;
						}
						if (!value)
						{
							ReplicationConnection replicationConnection = this;
							replicationConnection.options = replicationConnection.options & -2;
						}
						else
						{
							ReplicationConnection replicationConnection1 = this;
							replicationConnection1.options = replicationConnection1.options | 1;
						}
						this.cachedDirectoryEntry.Properties["options"].Value = this.options;
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

		private bool IsADAM
		{
			get
			{
				if (!this.checkADAM)
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					PropertyValueCollection item = null;
					try
					{
						item = directoryEntry.Properties["supportedCapabilities"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Contains("1.2.840.113556.1.4.1851"))
					{
						this.isADAMServer = true;
					}
				}
				return this.isADAMServer;
			}
		}

		public string Name
		{
			get
			{
				if (!this.disposed)
				{
					return this.connectionName;
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
					PropertyValueCollection item = null;
					try
					{
						item = this.cachedDirectoryEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Count != 0)
					{
						this.options = (int)item[0];
					}
					else
					{
						this.options = 0;
					}
					if ((this.options & 2) != 0)
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
					try
					{
						PropertyValueCollection item = this.cachedDirectoryEntry.Properties["options"];
						if (item.Count != 0)
						{
							this.options = (int)item[0];
						}
						else
						{
							this.options = 0;
						}
						if (!value)
						{
							ReplicationConnection replicationConnection = this;
							replicationConnection.options = replicationConnection.options & -3;
						}
						else
						{
							ReplicationConnection replicationConnection1 = this;
							replicationConnection1.options = replicationConnection1.options | 2;
						}
						this.cachedDirectoryEntry.Properties["options"].Value = this.options;
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

		public ActiveDirectorySchedule ReplicationSchedule
		{
			get
			{
				if (!this.disposed)
				{
					ActiveDirectorySchedule activeDirectorySchedule = null;
					bool flag = false;
					try
					{
						flag = this.cachedDirectoryEntry.Properties.Contains("schedule");
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (flag)
					{
						byte[] item = (byte[])this.cachedDirectoryEntry.Properties["schedule"][0];
						activeDirectorySchedule = new ActiveDirectorySchedule();
						activeDirectorySchedule.SetUnmanagedSchedule(item);
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
							this.cachedDirectoryEntry.Properties["schedule"].Value = value.GetUnmanagedSchedule();
						}
						else
						{
							if (this.cachedDirectoryEntry.Properties.Contains("schedule"))
							{
								this.cachedDirectoryEntry.Properties["schedule"].Clear();
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

		public bool ReplicationScheduleOwnedByUser
		{
			get
			{
				if (!this.disposed)
				{
					PropertyValueCollection item = null;
					try
					{
						item = this.cachedDirectoryEntry.Properties["options"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Count != 0)
					{
						this.options = (int)item[0];
					}
					else
					{
						this.options = 0;
					}
					if ((this.options & 32) != 0)
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
					try
					{
						PropertyValueCollection item = this.cachedDirectoryEntry.Properties["options"];
						if (item.Count != 0)
						{
							this.options = (int)item[0];
						}
						else
						{
							this.options = 0;
						}
						if (!value)
						{
							ReplicationConnection replicationConnection = this;
							replicationConnection.options = replicationConnection.options & -33;
						}
						else
						{
							ReplicationConnection replicationConnection1 = this;
							replicationConnection1.options = replicationConnection1.options | 32;
						}
						this.cachedDirectoryEntry.Properties["options"].Value = this.options;
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

		public ReplicationSpan ReplicationSpan
		{
			get
			{
				if (!this.disposed)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, this.cachedDirectoryEntry, PropertyManager.FromServer);
					string value = Utils.GetDNComponents(propertyValue)[3].Value;
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					string str = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ServerName);
					string value1 = Utils.GetDNComponents(str)[2].Value;
					if (Utils.Compare(value, value1) != 0)
					{
						return ReplicationSpan.InterSite;
					}
					else
					{
						return ReplicationSpan.IntraSite;
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public string SourceServer
		{
			get
			{
				if (!this.disposed)
				{
					if (this.sourceServerName == null)
					{
						string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, this.cachedDirectoryEntry, PropertyManager.FromServer);
						DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, propertyValue);
						if (!this.IsADAM)
						{
							this.sourceServerName = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry.Parent, PropertyManager.DnsHostName);
						}
						else
						{
							int num = (int)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.MsDSPortLDAP);
							string str = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry.Parent, PropertyManager.DnsHostName);
							if (num != 0x185)
							{
								this.sourceServerName = string.Concat(str, ":", num);
							}
						}
					}
					return this.sourceServerName;
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
					if (!this.existingConnection)
					{
						return this.transport;
					}
					else
					{
						PropertyValueCollection item = null;
						try
						{
							item = this.cachedDirectoryEntry.Properties["transportType"];
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						if (item.Count != 0)
						{
							return Utils.GetTransportTypeFromDN((string)item[0]);
						}
						else
						{
							return ActiveDirectoryTransportType.Rpc;
						}
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer) : this(context, name, sourceServer, null, 0)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectorySchedule schedule) : this(context, name, sourceServer, schedule, 0)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectoryTransportType transport) : this(context, name, sourceServer, null, transport)
		{
		}

		public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectorySchedule schedule, ActiveDirectoryTransportType transport)
		{
			ReplicationConnection.ValidateArgument(context, name);
			if (sourceServer != null)
			{
				if (transport < ActiveDirectoryTransportType.Rpc || transport > ActiveDirectoryTransportType.Smtp)
				{
					throw new InvalidEnumArgumentException("value", (int)transport, typeof(ActiveDirectoryTransportType));
				}
				else
				{
					context = new DirectoryContext(context);
					this.ValidateTargetAndSourceServer(context, sourceServer);
					this.context = context;
					this.connectionName = name;
					this.transport = transport;
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
					try
					{
						try
						{
							string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ServerName);
							string str = string.Concat("CN=NTDS Settings,", propertyValue);
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str);
							string escapedPath = string.Concat("cn=", this.connectionName);
							escapedPath = Utils.GetEscapedPath(escapedPath);
							this.cachedDirectoryEntry = directoryEntry.Children.Add(escapedPath, "nTDSConnection");
							DirectoryContext directoryContext = sourceServer.Context;
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(directoryContext, WellKnownDN.RootDSE);
							string propertyValue1 = (string)PropertyManager.GetPropertyValue(directoryContext, directoryEntry, PropertyManager.ServerName);
							propertyValue1 = string.Concat("CN=NTDS Settings,", propertyValue1);
							this.cachedDirectoryEntry.Properties["fromServer"].Add(propertyValue1);
							if (schedule != null)
							{
								this.cachedDirectoryEntry.Properties["schedule"].Value = schedule.GetUnmanagedSchedule();
							}
							string dNFromTransportType = Utils.GetDNFromTransportType(this.TransportType, context);
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dNFromTransportType);
							try
							{
								//TODO: REVIEW: URGENT!!: directoryEntry.Bind(true);
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (cOMException.ErrorCode == -2147016656)
								{
									DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
									if (Utils.CheckCapability(directoryEntry1, Capability.ActiveDirectoryApplicationMode) && transport == ActiveDirectoryTransportType.Smtp)
									{
										throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
									}
								}
								throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
							}
							this.cachedDirectoryEntry.Properties["transportType"].Add(dNFromTransportType);
							this.cachedDirectoryEntry.Properties["enabledConnection"].Value = false;
							this.cachedDirectoryEntry.Properties["options"].Value = 0;
						}
						catch (COMException cOMException3)
						{
							COMException cOMException2 = cOMException3;
							throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException2);
						}
					}
					finally
					{
						directoryEntry.Close();
					}
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("sourceServer");
			}
		}

		internal ReplicationConnection(DirectoryContext context, DirectoryEntry connectionEntry, string name)
		{
			this.context = context;
			this.cachedDirectoryEntry = connectionEntry;
			this.connectionName = name;
			this.existingConnection = true;
		}

		public void Delete()
		{
			if (!this.disposed)
			{
				if (this.existingConnection)
				{
					try
					{
						this.cachedDirectoryEntry.Parent.Children.Remove(this.cachedDirectoryEntry);
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
			if (!this.disposed)
			{
				if (disposing && this.cachedDirectoryEntry != null)
				{
					this.cachedDirectoryEntry.Dispose();
				}
				this.disposed = true;
			}
		}

		~ReplicationConnection()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public static ReplicationConnection FindByName(DirectoryContext context, string name)
		{
			ReplicationConnection replicationConnection;
			ReplicationConnection.ValidateArgument(context, name);
			context = new DirectoryContext(context);
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
			try
			{
				string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ServerName);
				string str = string.Concat("CN=NTDS Settings,", propertyValue);
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str);
				string[] strArrays = new string[1];
				strArrays[0] = "distinguishedName";
				ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=nTDSConnection)(objectCategory=NTDSConnection)(name=", Utils.GetEscapedFilterValue(name), "))"), strArrays, SearchScope.OneLevel, false, false);
				SearchResult searchResult = null;
				try
				{
					searchResult = aDSearcher.FindOne();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					if (cOMException.ErrorCode != -2147016656)
					{
						throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ReplicationConnection), name);
					}
				}
				if (searchResult != null)
				{
					DirectoryEntry directoryEntry1 = searchResult.GetDirectoryEntry();
					replicationConnection = new ReplicationConnection(context, directoryEntry1, name);
				}
				else
				{
					Exception activeDirectoryObjectNotFoundException = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ReplicationConnection), name);
					throw activeDirectoryObjectNotFoundException;
				}
			}
			finally
			{
				directoryEntry.Dispose();
			}
			return replicationConnection;
		}

		public DirectoryEntry GetDirectoryEntry()
		{
			if (!this.disposed)
			{
				if (this.existingConnection)
				{
					return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedDirectoryEntry.Path);
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
					this.cachedDirectoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				if (!this.existingConnection)
				{
					this.existingConnection = true;
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

		private static void ValidateArgument(DirectoryContext context, string name)
		{
			if (context != null)
			{
				if (context.Name == null || !context.isServer())
				{
					throw new ArgumentException(Res.GetString("DirectoryContextNeedHost"));
				}
				else
				{
					if (name != null)
					{
						if (name.Length != 0)
						{
							return;
						}
						else
						{
							throw new ArgumentException(Res.GetString("EmptyStringParameter"), "name");
						}
					}
					else
					{
						throw new ArgumentNullException("name");
					}
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private void ValidateTargetAndSourceServer(DirectoryContext context, DirectoryServer sourceServer)
		{
			bool flag = false;
			DirectoryEntry directoryEntry = null;
			DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
			try
			{
				try
				{
					if (!Utils.CheckCapability(directoryEntry1, Capability.ActiveDirectory))
					{
						if (!Utils.CheckCapability(directoryEntry1, Capability.ActiveDirectoryApplicationMode))
						{
							throw new ArgumentException(Res.GetString("DirectoryContextNeedHost"), "context");
						}
					}
					else
					{
						flag = true;
					}
					if (!flag || sourceServer as DomainController != null)
					{
						if (flag || sourceServer as DomainController == null)
						{
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(sourceServer.Context, WellKnownDN.RootDSE);
							if (!flag)
							{
								string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry1, PropertyManager.ConfigurationNamingContext);
								string str = (string)PropertyManager.GetPropertyValue(sourceServer.Context, directoryEntry, PropertyManager.ConfigurationNamingContext);
								if (Utils.Compare(propertyValue, str) != 0)
								{
									throw new ArgumentException(Res.GetString("ConnectionSourcServerSameConfigSet"), "sourceServer");
								}
							}
							else
							{
								string propertyValue1 = (string)PropertyManager.GetPropertyValue(context, directoryEntry1, PropertyManager.RootDomainNamingContext);
								string str1 = (string)PropertyManager.GetPropertyValue(sourceServer.Context, directoryEntry, PropertyManager.RootDomainNamingContext);
								if (Utils.Compare(propertyValue1, str1) != 0)
								{
									throw new ArgumentException(Res.GetString("ConnectionSourcServerSameForest"), "sourceServer");
								}
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("ConnectionSourcServerShouldBeADAM"), "sourceServer");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("ConnectionSourcServerShouldBeDC"), "sourceServer");
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
			}
			finally
			{
				if (directoryEntry1 != null)
				{
					directoryEntry1.Close();
				}
				if (directoryEntry != null)
				{
					directoryEntry.Close();
				}
			}
		}
	}
}