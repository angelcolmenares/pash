using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;
using System.Threading;

namespace System.DirectoryServices.AccountManagement
{
	internal class SDSCache
	{
		private static SDSCache domainCache;

		private static SDSCache localMachineCache;

		private Hashtable table;

		private object tableLock;

		private bool isSAM;

		public static SDSCache Domain
		{
			get
			{
				return SDSCache.domainCache;
			}
		}

		public static SDSCache LocalMachine
		{
			get
			{
				return SDSCache.localMachineCache;
			}
		}

		static SDSCache()
		{
			SDSCache.domainCache = new SDSCache(false);
			SDSCache.localMachineCache = new SDSCache(true);
		}

		private SDSCache(bool isSAM)
		{
			this.table = new Hashtable();
			this.tableLock = new object();
			this.isSAM = isSAM;
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		public PrincipalContext GetContext(string name, NetCred credentials, ContextOptions contextOptions)
		{
			string nT4UserName;
			PrincipalContext target;
			PrincipalContext principalContext;
			ContextType contextType;
			string userName;
			string password;
			Hashtable hashtables;
			Hashtable hashtables1;
			string domainName = name;
			bool flag = false;
			if (credentials == null || credentials.UserName == null)
			{
				nT4UserName = Utils.GetNT4UserName();
			}
			else
			{
				if (credentials.Domain == null)
				{
					nT4UserName = credentials.UserName;
				}
				else
				{
					nT4UserName = string.Concat(credentials.Domain, "\\", credentials.UserName);
				}
				flag = true;
			}
			if (!this.isSAM)
			{
				int num = 0x40000110;
				UnsafeNativeMethods.DomainControllerInfo dcName = Utils.GetDcName(null, domainName, null, num);
				domainName = dcName.DomainName;
			}
			ManualResetEvent manualResetEvent = null;
			while (true)
			{
				Hashtable hashtables2 = null;
				if (manualResetEvent != null)
				{
					manualResetEvent.WaitOne();
				}
				manualResetEvent = null;
				lock (this.tableLock)
				{
					SDSCache.CredHolder item = (SDSCache.CredHolder)this.table[domainName];
					if (item != null)
					{
						if (flag)
						{
							hashtables1 = item.explicitCreds;
						}
						else
						{
							hashtables1 = item.defaultCreds;
						}
						hashtables2 = hashtables1;
						object obj = hashtables2[nT4UserName];
						if (obj as SDSCache.Placeholder == null)
						{
							WeakReference weakReference = obj as WeakReference;
							if (weakReference != null)
							{
								target = (PrincipalContext)weakReference.Target;
								if (target == null || target.Disposed)
								{
									hashtables2.Remove(nT4UserName);
								}
								else
								{
									principalContext = target;
									break;
								}
							}
						}
						else
						{
							manualResetEvent = ((SDSCache.Placeholder)obj).contextReadyEvent;
							continue;
						}
					}
					if (item == null)
					{
						item = new SDSCache.CredHolder();
						this.table[domainName] = item;
						if (flag)
						{
							hashtables = item.explicitCreds;
						}
						else
						{
							hashtables = item.defaultCreds;
						}
						hashtables2 = hashtables;
					}
					hashtables2[nT4UserName] = new SDSCache.Placeholder();
					if (this.isSAM)
					{
						contextType = ContextType.Machine;
					}
					else
					{
						contextType = ContextType.Domain;
					}
					string str = domainName;
					object obj1 = null;
					ContextOptions contextOption = contextOptions;
					if (credentials != null)
					{
						userName = credentials.UserName;
					}
					else
					{
						userName = null;
					}
					if (credentials != null)
					{
						password = credentials.Password;
					}
					else
					{
						password = null;
					}
					target = new PrincipalContext(contextType, str, obj1, contextOption, userName, password);
					lock (this.tableLock)
					{
						SDSCache.Placeholder placeholder = (SDSCache.Placeholder)hashtables2[nT4UserName];
						hashtables2[nT4UserName] = new WeakReference(target);
						placeholder.contextReadyEvent.Set();
					}
					return target;
				}
			}
			return principalContext;
		}

		private class CredHolder
		{
			public Hashtable explicitCreds;

			public Hashtable defaultCreds;

			public CredHolder()
			{
				this.explicitCreds = new Hashtable();
				this.defaultCreds = new Hashtable();
			}
		}

		private class Placeholder
		{
			public ManualResetEvent contextReadyEvent;

			public Placeholder()
			{
				this.contextReadyEvent = new ManualResetEvent(false);
			}
		}
	}
}