using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySubnetCollection : CollectionBase
	{
		internal Hashtable changeList;

		internal bool initialized;

		private string siteDN;

		private DirectoryContext context;

		private ArrayList copyList;

		public ActiveDirectorySubnet this[int index]
		{
			get
			{
				return (ActiveDirectorySubnet)base.InnerList[index];
			}
			set
			{
				ActiveDirectorySubnet activeDirectorySubnet = value;
				if (activeDirectorySubnet != null)
				{
					if (activeDirectorySubnet.existing)
					{
						if (this.Contains(activeDirectorySubnet))
						{
							object[] objArray = new object[1];
							objArray[0] = activeDirectorySubnet;
							throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "value");
						}
						else
						{
							base.List[index] = activeDirectorySubnet;
							return;
						}
					}
					else
					{
						object[] name = new object[1];
						name[0] = activeDirectorySubnet.Name;
						throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		internal ActiveDirectorySubnetCollection(DirectoryContext context, string siteDN)
		{
			this.copyList = new ArrayList();
			this.context = context;
			this.siteDN = siteDN;
			Hashtable hashtables = new Hashtable();
			this.changeList = Hashtable.Synchronized(hashtables);
		}

		public int Add(ActiveDirectorySubnet subnet)
		{
			if (subnet != null)
			{
				if (subnet.existing)
				{
					if (this.Contains(subnet))
					{
						object[] objArray = new object[1];
						objArray[0] = subnet;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "subnet");
					}
					else
					{
						return base.List.Add(subnet);
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = subnet.Name;
					throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("subnet");
			}
		}

		public void AddRange(ActiveDirectorySubnet[] subnets)
		{
			if (subnets != null)
			{
				ActiveDirectorySubnet[] activeDirectorySubnetArray = subnets;
				int num = 0;
				while (num < (int)activeDirectorySubnetArray.Length)
				{
					ActiveDirectorySubnet activeDirectorySubnet = activeDirectorySubnetArray[num];
					if (activeDirectorySubnet != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException("subnets");
					}
				}
				for (int i = 0; i < (int)subnets.Length; i++)
				{
					this.Add(subnets[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("subnets");
			}
		}

		public void AddRange(ActiveDirectorySubnetCollection subnets)
		{
			if (subnets != null)
			{
				int count = subnets.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(subnets[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("subnets");
			}
		}

		public bool Contains(ActiveDirectorySubnet subnet)
		{
			if (subnet != null)
			{
				if (subnet.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySubnet item = (ActiveDirectorySubnet)base.InnerList[num];
						string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedEntry, PropertyManager.DistinguishedName);
						if (Utils.Compare(str, propertyValue) != 0)
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
					object[] name = new object[1];
					name[0] = subnet.Name;
					throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("subnet");
			}
		}

		public void CopyTo(ActiveDirectorySubnet[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(ActiveDirectorySubnet subnet)
		{
			if (subnet != null)
			{
				if (subnet.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySubnet item = (ActiveDirectorySubnet)base.InnerList[num];
						string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedEntry, PropertyManager.DistinguishedName);
						if (Utils.Compare(str, propertyValue) != 0)
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
					object[] name = new object[1];
					name[0] = subnet.Name;
					throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("subnet");
			}
		}

		public void Insert(int index, ActiveDirectorySubnet subnet)
		{
			if (subnet != null)
			{
				if (subnet.existing)
				{
					if (this.Contains(subnet))
					{
						object[] objArray = new object[1];
						objArray[0] = subnet;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "subnet");
					}
					else
					{
						base.List.Insert(index, subnet);
						return;
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = subnet.Name;
					throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("subnet");
			}
		}

		private string MakePath(string subnetDN)
		{
			string rdnFromDN = Utils.GetRdnFromDN(subnetDN);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < rdnFromDN.Length; i++)
			{
				if (rdnFromDN[i] == '/')
				{
					stringBuilder.Append('\\');
				}
				stringBuilder.Append(rdnFromDN[i]);
			}
			return string.Concat(stringBuilder.ToString(), ",", subnetDN.Substring(rdnFromDN.Length + 1));
		}

		protected override void OnClear()
		{
			if (this.initialized)
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
			if (this.initialized)
			{
				for (int i = 0; i < this.copyList.Count; i++)
				{
					this.OnRemoveComplete(i, this.copyList[i]);
				}
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			if (this.initialized)
			{
				ActiveDirectorySubnet activeDirectorySubnet = (ActiveDirectorySubnet)value;
				string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySubnet.context, activeDirectorySubnet.cachedEntry, PropertyManager.DistinguishedName);
				try
				{
					if (!this.changeList.Contains(propertyValue))
					{
						DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.MakePath(propertyValue));
						directoryEntry.Properties["siteObject"].Value = this.siteDN;
						this.changeList.Add(propertyValue, directoryEntry);
					}
					else
					{
						((DirectoryEntry)this.changeList[propertyValue]).Properties["siteObject"].Value = this.siteDN;
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			ActiveDirectorySubnet activeDirectorySubnet = (ActiveDirectorySubnet)value;
			string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySubnet.context, activeDirectorySubnet.cachedEntry, PropertyManager.DistinguishedName);
			try
			{
				if (!this.changeList.Contains(propertyValue))
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.MakePath(propertyValue));
					directoryEntry.Properties["siteObject"].Clear();
					this.changeList.Add(propertyValue, directoryEntry);
				}
				else
				{
					((DirectoryEntry)this.changeList[propertyValue]).Properties["siteObject"].Clear();
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
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
				if (value as ActiveDirectorySubnet != null)
				{
					if (((ActiveDirectorySubnet)value).existing)
					{
						return;
					}
					else
					{
						object[] name = new object[1];
						name[0] = ((ActiveDirectorySubnet)value).Name;
						throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
					}
				}
				else
				{
					throw new ArgumentException("value");
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Remove(ActiveDirectorySubnet subnet)
		{
			if (subnet != null)
			{
				if (subnet.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(subnet.context, subnet.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySubnet item = (ActiveDirectorySubnet)base.InnerList[num];
						string str = (string)PropertyManager.GetPropertyValue(item.context, item.cachedEntry, PropertyManager.DistinguishedName);
						if (Utils.Compare(str, propertyValue) != 0)
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
					objArray[0] = subnet;
					throw new ArgumentException(Res.GetString("NotFoundInCollection", objArray), "subnet");
				}
				else
				{
					object[] name = new object[1];
					name[0] = subnet.Name;
					throw new InvalidOperationException(Res.GetString("SubnetNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("subnet");
			}
		}
	}
}