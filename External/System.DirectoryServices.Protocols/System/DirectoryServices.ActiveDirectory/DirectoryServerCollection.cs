using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class DirectoryServerCollection : CollectionBase
	{
		internal string siteDN;

		internal string transportDN;

		internal DirectoryContext context;

		internal bool initialized;

		internal Hashtable changeList;

		private ArrayList copyList;

		private DirectoryEntry crossRefEntry;

		private bool isADAM;

		private bool isForNC;

		public DirectoryServer this[int index]
		{
			get
			{
				return (DirectoryServer)base.InnerList[index];
			}
			set
			{
				DirectoryServer directoryServer = value;
				if (directoryServer != null)
				{
					if (this.Contains(directoryServer))
					{
						object[] objArray = new object[1];
						objArray[0] = directoryServer;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "value");
					}
					else
					{
						base.List[index] = directoryServer;
						return;
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		internal DirectoryServerCollection(DirectoryContext context, string siteDN, string transportName)
		{
			this.copyList = new ArrayList();
			Hashtable hashtables = new Hashtable();
			this.changeList = Hashtable.Synchronized(hashtables);
			this.context = context;
			this.siteDN = siteDN;
			this.transportDN = transportName;
		}

		internal DirectoryServerCollection(DirectoryContext context, DirectoryEntry crossRefEntry, bool isADAM, ReadOnlyDirectoryServerCollection servers)
		{
			this.copyList = new ArrayList();
			this.context = context;
			this.crossRefEntry = crossRefEntry;
			this.isADAM = isADAM;
			this.isForNC = true;
			foreach (DirectoryServer server in servers)
			{
				base.InnerList.Add(server);
			}
		}

		public int Add(DirectoryServer server)
		{
			string siteObjectName;
			if (server != null)
			{
				if (!this.isForNC)
				{
					if (server as DomainController != null)
					{
						siteObjectName = ((DomainController)server).SiteObjectName;
					}
					else
					{
						siteObjectName = ((AdamInstance)server).SiteObjectName;
					}
					string str = siteObjectName;
					if (Utils.Compare(this.siteDN, str) == 0)
					{
						if (this.Contains(server))
						{
							object[] objArray = new object[1];
							objArray[0] = server;
							throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "server");
						}
						else
						{
							return base.List.Add(server);
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NotWithinSite"));
					}
				}
				else
				{
					if (!this.isADAM)
					{
						if (server as DomainController != null)
						{
							if (((DomainController)server).NumericOSVersion < 5.2)
							{
								throw new ArgumentException(Res.GetString("ServerShouldBeW2K3"), "server");
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("ServerShouldBeDC"), "server");
						}
					}
					if (this.Contains(server))
					{
						object[] objArray1 = new object[1];
						objArray1[0] = server;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray1), "server");
					}
					else
					{
						return base.List.Add(server);
					}
				}
			}
			else
			{
				throw new ArgumentNullException("server");
			}
		}

		public void AddRange(DirectoryServer[] servers)
		{
			if (servers != null)
			{
				DirectoryServer[] directoryServerArray = servers;
				int num = 0;
				while (num < (int)directoryServerArray.Length)
				{
					DirectoryServer directoryServer = directoryServerArray[num];
					if (directoryServer != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException("servers");
					}
				}
				for (int i = 0; i < (int)servers.Length; i++)
				{
					this.Add(servers[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("servers");
			}
		}

		public bool Contains(DirectoryServer server)
		{
			if (server != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DirectoryServer item = (DirectoryServer)base.InnerList[num];
					if (Utils.Compare(item.Name, server.Name) != 0)
					{
						num++;
					}
					else
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				throw new ArgumentNullException("server");
			}
		}

		public void CopyTo(DirectoryServer[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		internal string[] GetMultiValuedProperty()
		{
			string ntdsaObjectName;
			ArrayList arrayLists = new ArrayList();
			for (int i = 0; i < base.InnerList.Count; i++)
			{
				DirectoryServer item = (DirectoryServer)base.InnerList[i];
				if (item as DomainController != null)
				{
					ntdsaObjectName = ((DomainController)item).NtdsaObjectName;
				}
				else
				{
					ntdsaObjectName = ((AdamInstance)item).NtdsaObjectName;
				}
				string str = ntdsaObjectName;
				arrayLists.Add(str);
			}
			return (string[])arrayLists.ToArray(typeof(string));
		}

		public int IndexOf(DirectoryServer server)
		{
			if (server != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DirectoryServer item = (DirectoryServer)base.InnerList[num];
					if (Utils.Compare(item.Name, server.Name) != 0)
					{
						num++;
					}
					else
					{
						return num;
					}
				}
				return -1;
			}
			else
			{
				throw new ArgumentNullException("server");
			}
		}

		public void Insert(int index, DirectoryServer server)
		{
			string siteObjectName;
			if (server != null)
			{
				if (!this.isForNC)
				{
					if (server as DomainController != null)
					{
						siteObjectName = ((DomainController)server).SiteObjectName;
					}
					else
					{
						siteObjectName = ((AdamInstance)server).SiteObjectName;
					}
					string str = siteObjectName;
					if (Utils.Compare(this.siteDN, str) == 0)
					{
						if (this.Contains(server))
						{
							object[] objArray = new object[1];
							objArray[0] = server;
							throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray));
						}
						else
						{
							base.List.Insert(index, server);
							return;
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NotWithinSite"), "server");
					}
				}
				else
				{
					if (!this.isADAM)
					{
						if (server as DomainController != null)
						{
							if (((DomainController)server).NumericOSVersion < 5.2)
							{
								throw new ArgumentException(Res.GetString("ServerShouldBeW2K3"), "server");
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("ServerShouldBeDC"), "server");
						}
					}
					if (this.Contains(server))
					{
						object[] objArray1 = new object[1];
						objArray1[0] = server;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray1), "server");
					}
					else
					{
						base.List.Insert(index, server);
						return;
					}
				}
			}
			else
			{
				throw new ArgumentNullException("server");
			}
		}

		protected override void OnClear()
		{
			if (this.initialized && !this.isForNC)
			{
				this.copyList.Clear();
				foreach (object list in base.List)
				{
					this.copyList.Add(list);
				}
			}
		}

		protected override void OnClearComplete()
		{
			if (!this.isForNC)
			{
				if (this.initialized)
				{
					for (int i = 0; i < this.copyList.Count; i++)
					{
						this.OnRemoveComplete(i, this.copyList[i]);
					}
				}
			}
			else
			{
				if (this.crossRefEntry != null)
				{
					try
					{
						if (this.crossRefEntry.Properties.Contains(PropertyManager.MsDSNCReplicaLocations))
						{
							this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Clear();
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
				}
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			string serverObjectName;
			string ntdsaObjectName;
			if (!this.isForNC)
			{
				if (this.initialized)
				{
					DirectoryServer directoryServer = (DirectoryServer)value;
					string name = directoryServer.Name;
					if (directoryServer as DomainController != null)
					{
						serverObjectName = ((DomainController)directoryServer).ServerObjectName;
					}
					else
					{
						serverObjectName = ((AdamInstance)directoryServer).ServerObjectName;
					}
					string str = serverObjectName;
					try
					{
						if (!this.changeList.Contains(name))
						{
							DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
							directoryEntry.Properties["bridgeheadTransportList"].Value = this.transportDN;
							this.changeList.Add(name, directoryEntry);
						}
						else
						{
							((DirectoryEntry)this.changeList[name]).Properties["bridgeheadTransportList"].Value = this.transportDN;
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
				}
			}
			else
			{
				if (this.crossRefEntry != null)
				{
					try
					{
						DirectoryServer directoryServer1 = (DirectoryServer)value;
						if (directoryServer1 as DomainController != null)
						{
							ntdsaObjectName = ((DomainController)directoryServer1).NtdsaObjectName;
						}
						else
						{
							ntdsaObjectName = ((AdamInstance)directoryServer1).NtdsaObjectName;
						}
						string str1 = ntdsaObjectName;
						this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Add(str1);
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
					}
				}
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			string serverObjectName;
			string ntdsaObjectName;
			if (!this.isForNC)
			{
				DirectoryServer directoryServer = (DirectoryServer)value;
				string name = directoryServer.Name;
				if (directoryServer as DomainController != null)
				{
					serverObjectName = ((DomainController)directoryServer).ServerObjectName;
				}
				else
				{
					serverObjectName = ((AdamInstance)directoryServer).ServerObjectName;
				}
				string str = serverObjectName;
				try
				{
					if (!this.changeList.Contains(name))
					{
						DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
						directoryEntry.Properties["bridgeheadTransportList"].Clear();
						this.changeList.Add(name, directoryEntry);
					}
					else
					{
						((DirectoryEntry)this.changeList[name]).Properties["bridgeheadTransportList"].Clear();
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			else
			{
				try
				{
					if (this.crossRefEntry != null)
					{
						if (value as DomainController != null)
						{
							ntdsaObjectName = ((DomainController)value).NtdsaObjectName;
						}
						else
						{
							ntdsaObjectName = ((AdamInstance)value).NtdsaObjectName;
						}
						string str1 = ntdsaObjectName;
						this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].Remove(str1);
					}
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
				}
			}
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			this.OnRemoveComplete(index, oldValue);
			this.OnInsertComplete(index, newValue);
		}

		protected override void OnValidate(object value)
		{
			if (value != null)
			{
				if (!this.isForNC)
				{
					if (value as DirectoryServer == null)
					{
						throw new ArgumentException("value");
					}
				}
				else
				{
					if (!this.isADAM)
					{
						if (value as DomainController == null)
						{
							throw new ArgumentException(Res.GetString("ServerShouldBeDC"), "value");
						}
					}
					else
					{
						if (value as AdamInstance == null)
						{
							throw new ArgumentException(Res.GetString("ServerShouldBeAI"), "value");
						}
					}
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Remove(DirectoryServer server)
		{
			if (server != null)
			{
				int num = 0;
				while (num < base.InnerList.Count)
				{
					DirectoryServer item = (DirectoryServer)base.InnerList[num];
					if (Utils.Compare(item.Name, server.Name) != 0)
					{
						num++;
					}
					else
					{
						base.List.Remove(item);
						return;
					}
				}
				object[] objArray = new object[1];
				objArray[0] = server;
				throw new ArgumentException(Res.GetString("NotFoundInCollection", objArray), "server");
			}
			else
			{
				throw new ArgumentNullException("server");
			}
		}
	}
}