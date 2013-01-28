using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class RangeRetriever : CollectionBase, IEnumerable, IEnumerator, IDisposable
	{
		private bool disposed;

		private bool disposeDirEntry;

		private bool cacheValues;

		private DirectoryEntry de;

		private string propertyName;

		private bool endReached;

		private int lowRange;

		private int currentIndex;

		private bool cacheFilled;

		private object currentResult;

		private IEnumerator currentEnumerator;

		public bool CacheValues
		{
			set
			{
				this.cacheValues = value;
			}
		}

		public object Current
		{
			get
			{
				return this.currentResult;
			}
		}

		public RangeRetriever(DirectoryEntry de, string propertyName, bool disposeDirEntry)
		{
			this.de = de;
			this.propertyName = propertyName;
			this.disposeDirEntry = disposeDirEntry;
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}

		private IEnumerator GetNextChunk()
		{
			IEnumerator enumerator;
			object[] objArray = new object[2];
			objArray[0] = this.propertyName;
			objArray[1] = this.lowRange;
			string str = string.Format(CultureInfo.InvariantCulture, "{0};range={1}-*", objArray);
			try
			{
				string[] strArrays = new string[2];
				strArrays[0] = str;
				strArrays[1] = this.propertyName;
				this.de.RefreshCache(strArrays);
				goto Label0;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				if (cOMException.ErrorCode != -2147016672)
				{
					throw;
				}
				else
				{
					enumerator = null;
				}
			}
			return enumerator;
		Label0:
			PropertyValueCollection item = this.de.Properties[this.propertyName];
			if (item == null || item.Count == 0)
			{
				return null;
			}
			else
			{
				this.lowRange = this.lowRange + item.Count;
				return item.GetEnumerator();
			}
		}

		public bool MoveNext()
		{
			bool flag;
			bool flag1;
			if (!this.endReached)
			{
				if (this.currentIndex >= base.InnerList.Count)
				{
					if (!this.cacheFilled)
					{
						if (!this.endReached && this.currentEnumerator == null)
						{
							this.currentEnumerator = this.GetNextChunk();
							if (this.currentEnumerator == null)
							{
								this.endReached = true;
							}
						}
						if (!this.endReached)
						{
							do
							{
								flag = false;
								flag1 = this.currentEnumerator.MoveNext();
								if (!flag1)
								{
									this.currentEnumerator = this.GetNextChunk();
									if (this.currentEnumerator != null)
									{
										flag = true;
									}
									else
									{
										this.endReached = true;
										this.cacheFilled = this.cacheValues;
									}
								}
								else
								{
									this.currentResult = this.currentEnumerator.Current;
								}
							}
							while (flag);
							if (flag1)
							{
								if (this.cacheValues)
								{
									base.InnerList.Add(this.currentResult);
								}
								RangeRetriever rangeRetrievers = this;
								rangeRetrievers.currentIndex = rangeRetrievers.currentIndex + 1;
							}
							return flag1;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					this.currentResult = base.InnerList[this.currentIndex];
					RangeRetriever rangeRetrievers1 = this;
					rangeRetrievers1.currentIndex = rangeRetrievers1.currentIndex + 1;
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public void Reset()
		{
			this.endReached = false;
			this.lowRange = 0;
			this.currentResult = null;
			this.currentIndex = 0;
		}

		void System.IDisposable.Dispose()
		{
			if (!this.disposed && this.disposeDirEntry)
			{
				this.de.Dispose();
			}
			this.disposed = true;
		}
	}
}