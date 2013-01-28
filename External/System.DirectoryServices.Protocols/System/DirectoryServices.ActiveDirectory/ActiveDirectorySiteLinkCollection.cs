using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySiteLinkCollection : CollectionBase
	{
		internal DirectoryEntry de;

		internal bool initialized;

		internal DirectoryContext context;

		public ActiveDirectorySiteLink this[int index]
		{
			get
			{
				return (ActiveDirectorySiteLink)base.InnerList[index];
			}
			set
			{
				ActiveDirectorySiteLink activeDirectorySiteLink = value;
				if (activeDirectorySiteLink != null)
				{
					if (activeDirectorySiteLink.existing)
					{
						if (this.Contains(activeDirectorySiteLink))
						{
							object[] objArray = new object[1];
							objArray[0] = activeDirectorySiteLink;
							throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "value");
						}
						else
						{
							base.List[index] = activeDirectorySiteLink;
							return;
						}
					}
					else
					{
						object[] name = new object[1];
						name[0] = activeDirectorySiteLink.Name;
						throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		internal ActiveDirectorySiteLinkCollection()
		{
		}

		public int Add(ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				if (link.existing)
				{
					if (this.Contains(link))
					{
						object[] objArray = new object[1];
						objArray[0] = link;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "link");
					}
					else
					{
						return base.List.Add(link);
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = link.Name;
					throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("link");
			}
		}

		public void AddRange(ActiveDirectorySiteLink[] links)
		{
			if (links != null)
			{
				for (int i = 0; i < (int)links.Length; i++)
				{
					this.Add(links[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("links");
			}
		}

		public void AddRange(ActiveDirectorySiteLinkCollection links)
		{
			if (links != null)
			{
				int count = links.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(links[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("links");
			}
		}

		public bool Contains(ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				if (link.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySiteLink item = (ActiveDirectorySiteLink)base.InnerList[num];
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
					name[0] = link.Name;
					throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("link");
			}
		}

		public void CopyTo(ActiveDirectorySiteLink[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				if (link.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySiteLink item = (ActiveDirectorySiteLink)base.InnerList[num];
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
					name[0] = link.Name;
					throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("link");
			}
		}

		public void Insert(int index, ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				if (link.existing)
				{
					if (this.Contains(link))
					{
						object[] objArray = new object[1];
						objArray[0] = link;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "link");
					}
					else
					{
						base.List.Insert(index, link);
						return;
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = link.Name;
					throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		protected override void OnClearComplete()
		{
			if (this.initialized)
			{
				try
				{
					if (this.de.Properties.Contains("siteLinkList"))
					{
						this.de.Properties["siteLinkList"].Clear();
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			if (this.initialized)
			{
				ActiveDirectorySiteLink activeDirectorySiteLink = (ActiveDirectorySiteLink)value;
				string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySiteLink.context, activeDirectorySiteLink.cachedEntry, PropertyManager.DistinguishedName);
				try
				{
					this.de.Properties["siteLinkList"].Add(propertyValue);
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
			ActiveDirectorySiteLink activeDirectorySiteLink = (ActiveDirectorySiteLink)value;
			string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySiteLink.context, activeDirectorySiteLink.cachedEntry, PropertyManager.DistinguishedName);
			try
			{
				this.de.Properties["siteLinkList"].Remove(propertyValue);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			ActiveDirectorySiteLink activeDirectorySiteLink = (ActiveDirectorySiteLink)newValue;
			string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySiteLink.context, activeDirectorySiteLink.cachedEntry, PropertyManager.DistinguishedName);
			try
			{
				this.de.Properties["siteLinkList"][index] = propertyValue;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
		}

		protected override void OnValidate(object value)
		{
			if (value != null)
			{
				if (value as ActiveDirectorySiteLink != null)
				{
					if (((ActiveDirectorySiteLink)value).existing)
					{
						return;
					}
					else
					{
						object[] name = new object[1];
						name[0] = ((ActiveDirectorySiteLink)value).Name;
						throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
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

		public void Remove(ActiveDirectorySiteLink link)
		{
			if (link != null)
			{
				if (link.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(link.context, link.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySiteLink item = (ActiveDirectorySiteLink)base.InnerList[num];
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
					objArray[0] = link;
					throw new ArgumentException(Res.GetString("NotFoundInCollection", objArray), "link");
				}
				else
				{
					object[] name = new object[1];
					name[0] = link.Name;
					throw new InvalidOperationException(Res.GetString("SiteLinkNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("link");
			}
		}
	}
}