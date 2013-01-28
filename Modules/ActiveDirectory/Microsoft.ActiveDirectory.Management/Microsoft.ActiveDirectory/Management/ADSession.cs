using System;
using System.Threading;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSession
	{
		private ADSessionInfo _info;

		private ADSessionHandle _handle;

		private ADStoreAccess _storeAccess;

		private ADRootDSE _rootDSE;

		private int _referenceCount;

		private bool _deleted;

		private object _handleLock;

		private static ADSessionCache _sessionCache;

		private static string _debugCategory;

		internal int ReferenceCount
		{
			get
			{
				return this._referenceCount;
			}
		}

		internal ADRootDSE RootDSE
		{
			get
			{
				return this._rootDSE;
			}
			set
			{
				this._rootDSE = value;
			}
		}

		public ADSessionInfo SessionInfo
		{
			get
			{
				return this._info.Copy();
			}
		}

		static ADSession()
		{
			ADSession._sessionCache = ADSessionCache.GetObject();
			ADSession._debugCategory = "ADSessionCache";
		}

		protected ADSession(ADSessionInfo info)
		{
			this._handleLock = new object();
			this._info = info.Copy();
			this._storeAccess = ADServiceStoreAccessFactory.GetObject();
		}

		private ADSession()
		{
			this._handleLock = new object();
		}

		private int AddRef()
		{
			Interlocked.Increment(ref this._referenceCount);
			DebugLogger.LogInfo(ADSession._debugCategory, string.Concat("AddRef: new count = ", this._referenceCount));
			return this._referenceCount;
		}


		public static ADSession ConstructSession(ADSessionInfo info)
		{
			ADSession aDSession = null;
			if (info == null)
			{
				info = new ADSessionInfo();
			}
			lock (ADSession._sessionCache)
			{
				if (!ADSession._sessionCache.GetEntry(info, out aDSession))
				{
					aDSession = new ADSession(info);
					ADSession._sessionCache.AddEntry(aDSession._info, aDSession);
				}
				aDSession.AddRef();
			}
			aDSession.Create();
			return aDSession;
		}

		private bool Create()
		{
			lock (this._handleLock)
			{
				if (this._handle == null && !this._deleted)
				{
					IADSession aDSession = this._storeAccess as IADSession;
					this._handle = aDSession.Create(this._info);
				}
			}
			if (this._handle != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public virtual void Delete()
		{
			DebugLogger.LogInfo(ADSession._debugCategory, string.Concat("Delete: ref count = ", this._referenceCount));
			if (0 >= this.Release())
			{
				lock (ADSession._sessionCache)
				{
					if (0 < this.ReferenceCount)
					{
						return;
					}
					else
					{
						ADSession._sessionCache.DeleteEntry(this._info);
					}
				}
				DebugLogger.LogInfo(ADSession._debugCategory, "Delete: ref count is 0, destroying object");
				IADSession aDSession = this._storeAccess as IADSession;
				if (aDSession != null)
				{
					lock (this._handleLock)
					{
						if (this._handle != null)
						{
							aDSession.Delete(this._handle);
							this._handle = null;
							this._deleted = true;
						}
					}
				}
			}
		}

		internal IADAccountManagement GetAccountManagementInterface()
		{
			if (!this._deleted)
			{
				return this._storeAccess as IADAccountManagement;
			}
			else
			{
				return null;
			}
		}

		internal ADSessionHandle GetSessionHandle()
		{
			if (!this._deleted)
			{
				return this._handle;
			}
			else
			{
				return null;
			}
		}

		internal IADSyncOperations GetSyncOperationsInterface()
		{
			if (!this._deleted)
			{
				return this._storeAccess as IADSyncOperations;
			}
			else
			{
				return null;
			}
		}

		internal IADTopologyManagement GetTopologyManagementInterface()
		{
			if (!this._deleted)
			{
				return this._storeAccess as IADTopologyManagement;
			}
			else
			{
				return null;
			}
		}

		internal bool MatchConnectionOptions(ADSessionInfo info)
		{
			if (info.ServerNameOnly != null)
			{
				IADSession aDSession = this._storeAccess as IADSession;
				string option = (string)aDSession.GetOption(this._handle, 48);
				if (info.ServerNameOnly.Equals(option, StringComparison.OrdinalIgnoreCase) && info.EffectivePortNumber == this.SessionInfo.EffectivePortNumber)
				{
					return ADSessionInfo.MatchConnectionState(this._info, info, true);
				}
			}
			return ADSessionInfo.MatchConnectionState(this._info, info, false);
		}

		private int Release()
		{
			Interlocked.Decrement(ref this._referenceCount);
			DebugLogger.LogInfo(ADSession._debugCategory, string.Concat("Release: Releasing object new count = ", this._referenceCount));
			return this._referenceCount;
		}
	}
}