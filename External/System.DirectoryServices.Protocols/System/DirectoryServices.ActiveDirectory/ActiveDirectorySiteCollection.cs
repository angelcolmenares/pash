using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySiteCollection : CollectionBase
	{
		internal DirectoryEntry de;

		internal bool initialized;

		internal DirectoryContext context;

		public ActiveDirectorySite this[int index]
		{
			get
			{
				return (ActiveDirectorySite)base.InnerList[index];
			}
			set
			{
				ActiveDirectorySite activeDirectorySite = value;
				if (activeDirectorySite != null)
				{
					if (activeDirectorySite.existing)
					{
						if (this.Contains(activeDirectorySite))
						{
							object[] objArray = new object[1];
							objArray[0] = activeDirectorySite;
							throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "value");
						}
						else
						{
							base.List[index] = activeDirectorySite;
							return;
						}
					}
					else
					{
						object[] name = new object[1];
						name[0] = activeDirectorySite.Name;
						throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		internal ActiveDirectorySiteCollection()
		{
		}

		internal ActiveDirectorySiteCollection(ArrayList sites)
		{
			for (int i = 0; i < sites.Count; i++)
			{
				this.Add((ActiveDirectorySite)sites[i]);
			}
		}

		public int Add(ActiveDirectorySite site)
		{
			if (site != null)
			{
				if (site.existing)
				{
					if (this.Contains(site))
					{
						object[] objArray = new object[1];
						objArray[0] = site;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "site");
					}
					else
					{
						return base.List.Add(site);
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = site.Name;
					throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("site");
			}
		}

		public void AddRange(ActiveDirectorySite[] sites)
		{
			if (sites != null)
			{
				for (int i = 0; i < (int)sites.Length; i++)
				{
					this.Add(sites[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("sites");
			}
		}

		public void AddRange(ActiveDirectorySiteCollection sites)
		{
			if (sites != null)
			{
				int count = sites.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(sites[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("sites");
			}
		}

		public bool Contains(ActiveDirectorySite site)
		{
			if (site != null)
			{
				if (site.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySite item = (ActiveDirectorySite)base.InnerList[num];
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
					name[0] = site.Name;
					throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("site");
			}
		}

		public void CopyTo(ActiveDirectorySite[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(ActiveDirectorySite site)
		{
			if (site != null)
			{
				if (site.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySite item = (ActiveDirectorySite)base.InnerList[num];
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
					name[0] = site.Name;
					throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("site");
			}
		}

		public void Insert(int index, ActiveDirectorySite site)
		{
			if (site != null)
			{
				if (site.existing)
				{
					if (this.Contains(site))
					{
						object[] objArray = new object[1];
						objArray[0] = site;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "site");
					}
					else
					{
						base.List.Insert(index, site);
						return;
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = site.Name;
					throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("site");
			}
		}

		protected override void OnClearComplete()
		{
			if (this.initialized)
			{
				try
				{
					if (this.de.Properties.Contains("siteList"))
					{
						this.de.Properties["siteList"].Clear();
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
				ActiveDirectorySite activeDirectorySite = (ActiveDirectorySite)value;
				string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySite.context, activeDirectorySite.cachedEntry, PropertyManager.DistinguishedName);
				try
				{
					this.de.Properties["siteList"].Add(propertyValue);
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
			ActiveDirectorySite activeDirectorySite = (ActiveDirectorySite)value;
			string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySite.context, activeDirectorySite.cachedEntry, PropertyManager.DistinguishedName);
			try
			{
				this.de.Properties["siteList"].Remove(propertyValue);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			ActiveDirectorySite activeDirectorySite = (ActiveDirectorySite)newValue;
			string propertyValue = (string)PropertyManager.GetPropertyValue(activeDirectorySite.context, activeDirectorySite.cachedEntry, PropertyManager.DistinguishedName);
			try
			{
				this.de.Properties["siteList"][index] = propertyValue;
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
				if (value as ActiveDirectorySite != null)
				{
					if (((ActiveDirectorySite)value).existing)
					{
						return;
					}
					else
					{
						object[] name = new object[1];
						name[0] = ((ActiveDirectorySite)value).Name;
						throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
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

		public void Remove(ActiveDirectorySite site)
		{
			if (site != null)
			{
				if (site.existing)
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName);
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySite item = (ActiveDirectorySite)base.InnerList[num];
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
					objArray[0] = site;
					throw new ArgumentException(Res.GetString("NotFoundInCollection", objArray), "site");
				}
				else
				{
					object[] name = new object[1];
					name[0] = site.Name;
					throw new InvalidOperationException(Res.GetString("SiteNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("site");
			}
		}
	}
}