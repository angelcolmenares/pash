using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class PrincipalSearcher : IDisposable
	{
		[SecuritySafeCritical]
		private PrincipalContext ctx;

		private bool disposed;

		private Principal qbeFilter;

		private int pageSize;

		private object underlyingSearcher;

		public PrincipalContext Context
		{
			[SecuritySafeCritical]
			get
			{
				this.CheckDisposed();
				return this.ctx;
			}
		}

		internal int PageSize
		{
			get
			{
				return this.pageSize;
			}
		}

		public Principal QueryFilter
		{
			[SecurityCritical]
			get
			{
				this.CheckDisposed();
				return this.qbeFilter;
			}
			[SecurityCritical]
			set
			{
				if (value != null)
				{
					this.CheckDisposed();
					if (value == null || value.unpersisted)
					{
						this.qbeFilter = value;
						this.ctx = this.qbeFilter.Context;
						return;
					}
					else
					{
						throw new ArgumentException(StringResources.PrincipalSearcherPersistedPrincipal);
					}
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "queryFilter";
					throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullArgument, objArray));
				}
			}
		}

		internal object UnderlyingSearcher
		{
			get
			{
				return this.underlyingSearcher;
			}
			set
			{
				this.underlyingSearcher = value;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[SecurityCritical]
		public PrincipalSearcher()
		{
			this.SetDefaultPageSizeForContext();
		}

		[SecurityCritical]
		public PrincipalSearcher(Principal queryFilter)
		{
			if (queryFilter != null)
			{
				this.ctx = queryFilter.Context;
				this.QueryFilter = queryFilter;
				this.SetDefaultPageSizeForContext();
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "queryFilter";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullArgument, objArray));
			}
		}

		private void CheckDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().ToString());
			}
		}

		[SecurityCritical]
		public virtual void Dispose()
		{
			if (!this.disposed)
			{
				if (this.UnderlyingSearcher != null && this.UnderlyingSearcher as IDisposable != null)
				{
					((IDisposable)this.UnderlyingSearcher).Dispose();
				}
				this.disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		[SecurityCritical]
		public PrincipalSearchResult<Principal> FindAll()
		{
			this.CheckDisposed();
			return this.FindAll(false);
		}

		[SecuritySafeCritical]
		private PrincipalSearchResult<Principal> FindAll(bool returnOne)
		{
			int num;
			if (this.qbeFilter != null)
			{
				if (this.qbeFilter.unpersisted)
				{
					if (!this.HasReferentialPropertiesSet())
					{
						StoreCtx queryCtx = this.ctx.QueryCtx;
						PrincipalSearcher principalSearcher = this;
						if (returnOne)
						{
							num = 1;
						}
						else
						{
							num = -1;
						}
						ResultSet resultSet = queryCtx.Query(principalSearcher, num);
						PrincipalSearchResult<Principal> principals = new PrincipalSearchResult<Principal>(resultSet);
						return principals;
					}
					else
					{
						throw new InvalidOperationException(StringResources.PrincipalSearcherNonReferentialProps);
					}
				}
				else
				{
					throw new InvalidOperationException(StringResources.PrincipalSearcherPersistedPrincipal);
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalSearcherMustSetFilter);
			}
		}

		[SecurityCritical]
		public Principal FindOne()
		{
			Principal current;
			this.CheckDisposed();
			PrincipalSearchResult<Principal> principals = this.FindAll(true);
			using (principals)
			{
				FindResultEnumerator<Principal> enumerator = (FindResultEnumerator<Principal>)principals.GetEnumerator();
				if (!enumerator.MoveNext())
				{
					current = null;
				}
				else
				{
					current = enumerator.Current;
				}
			}
			return current;
		}

		[SecurityCritical]
		public object GetUnderlyingSearcher()
		{
			this.CheckDisposed();
			if (this.qbeFilter != null)
			{
				if (this.qbeFilter.unpersisted)
				{
					if (!this.HasReferentialPropertiesSet())
					{
						StoreCtx queryCtx = this.ctx.QueryCtx;
						if (queryCtx.SupportsSearchNatively)
						{
							this.underlyingSearcher = queryCtx.PushFilterToNativeSearcher(this);
							return this.underlyingSearcher;
						}
						else
						{
							throw new InvalidOperationException(StringResources.PrincipalSearcherNoUnderlying);
						}
					}
					else
					{
						throw new InvalidOperationException(StringResources.PrincipalSearcherNonReferentialProps);
					}
				}
				else
				{
					throw new InvalidOperationException(StringResources.PrincipalSearcherPersistedPrincipal);
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalSearcherMustSetFilter);
			}
		}

		[SecurityCritical]
		public Type GetUnderlyingSearcherType()
		{
			this.CheckDisposed();
			if (this.qbeFilter != null)
			{
				StoreCtx queryCtx = this.ctx.QueryCtx;
				if (queryCtx.SupportsSearchNatively)
				{
					return queryCtx.SearcherNativeType();
				}
				else
				{
					throw new InvalidOperationException(StringResources.PrincipalSearcherNoUnderlying);
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalSearcherMustSetFilter);
			}
		}

		[SecuritySafeCritical]
		private bool HasReferentialPropertiesSet()
		{
			bool flag;
			if (this.qbeFilter != null)
			{
				Type type = this.qbeFilter.GetType();
				ArrayList item = (ArrayList)ReferentialProperties.Properties[type];
				if (item != null)
				{
					IEnumerator enumerator = item.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string current = (string)enumerator.Current;
							if (!this.qbeFilter.GetChangeStatusForProperty(current))
							{
								continue;
							}
							flag = true;
							return flag;
						}
						return false;
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
					return flag;
				}
				return false;
			}
			else
			{
				return false;
			}
		}

		[SecurityCritical]
		private void SetDefaultPageSizeForContext()
		{
			this.pageSize = 0;
			if (this.qbeFilter != null && this.ctx.QueryCtx as ADStoreCtx != null)
			{
				this.pageSize = 0x100;
			}
		}
	}
}