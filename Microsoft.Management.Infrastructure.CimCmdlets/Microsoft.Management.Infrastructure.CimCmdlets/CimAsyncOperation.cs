using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal abstract class CimAsyncOperation : IDisposable
	{
		internal const string ComputerNameArgument = "ComputerName";

		internal const string CimSessionArgument = "CimSession";

		private long _disposed;

		private readonly object myLock;

		private uint operationCount;

		private ManualResetEventSlim moreActionEvent;

		private ConcurrentQueue<CimBaseAction> actionQueue;

		private readonly object cimSessionProxyCacheLock;

		private List<CimSessionProxy> cimSessionProxyCache;

		protected ManualResetEventSlim ackedEvent;

		protected bool Disposed
		{
			get
			{
				return Interlocked.Read(ref this._disposed) == (long)1;
			}
		}

		public CimAsyncOperation()
		{
			this.myLock = new object();
			this.cimSessionProxyCacheLock = new object();
			this.moreActionEvent = new ManualResetEventSlim(false);
			this.actionQueue = new ConcurrentQueue<CimBaseAction>();
			this._disposed = (long)0;
			this.operationCount = 0;
		}

		protected void AddCimSessionProxy(CimSessionProxy sessionproxy)
		{
			lock (this.cimSessionProxyCacheLock)
			{
				if (this.cimSessionProxyCache == null)
				{
					this.cimSessionProxyCache = new List<CimSessionProxy>();
				}
				if (!this.cimSessionProxyCache.Contains(sessionproxy))
				{
					this.cimSessionProxyCache.Add(sessionproxy);
				}
			}
		}

		private void Cleanup()
		{
			CimBaseAction cimBaseAction = null;
			List<CimSessionProxy> cimSessionProxies;
			DebugHelper.WriteLogEx();
			this.moreActionEvent.Set();
			while (this.GetActionAndRemove(out cimBaseAction))
			{
				object[] objArray = new object[1];
				objArray[0] = cimBaseAction;
				DebugHelper.WriteLog("Action {0}", 2, objArray);
				if (cimBaseAction as CimSyncAction == null)
				{
					continue;
				}
				(cimBaseAction as CimSyncAction).OnComplete();
			}
			if (this.cimSessionProxyCache != null)
			{
				lock (this.cimSessionProxyCache)
				{
					cimSessionProxies = new List<CimSessionProxy>(this.cimSessionProxyCache);
					this.cimSessionProxyCache.Clear();
				}
				foreach (CimSessionProxy cimSessionProxy in cimSessionProxies)
				{
					DebugHelper.WriteLog("Dispose proxy ", 2);
					cimSessionProxy.Dispose();
				}
			}
			this.moreActionEvent.Dispose();
			if (this.ackedEvent != null)
			{
				this.ackedEvent.Dispose();
			}
			DebugHelper.WriteLog("Cleanup complete.", 2);
		}

		protected CimSessionProxy CreateCimSessionProxy(CimSessionProxy originalProxy)
		{
			CimSessionProxy cimSessionProxy = new CimSessionProxy(originalProxy);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxy);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateCimSessionProxy(CimSessionProxy originalProxy, bool passThru)
		{
			CimSessionProxy cimSessionProxySetCimInstance = new CimSessionProxySetCimInstance(originalProxy, passThru);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxySetCimInstance);
			return cimSessionProxySetCimInstance;
		}

		protected CimSessionProxy CreateCimSessionProxy(CimSession session)
		{
			CimSessionProxy cimSessionProxy = new CimSessionProxy(session);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxy);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateCimSessionProxy(CimSession session, bool passThru)
		{
			CimSessionProxy cimSessionProxySetCimInstance = new CimSessionProxySetCimInstance(session, passThru);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxySetCimInstance);
			return cimSessionProxySetCimInstance;
		}

		protected CimSessionProxy CreateCimSessionProxy(string computerName)
		{
			CimSessionProxy cimSessionProxy = new CimSessionProxy(computerName);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxy);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateCimSessionProxy(string computerName, CimSessionOptions options)
		{
			CimSessionProxy cimSessionProxy = new CimSessionProxy(computerName, options);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxy);
			return cimSessionProxy;
		}


		protected CimSessionProxy CreateCimSessionProxy(string computerName, CimInstance cimInstance)
		{
			CimSessionProxy cimSessionProxy = new CimSessionProxy(computerName, cimInstance);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxy);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateCimSessionProxy(string computerName, CimInstance cimInstance, bool passThru)
		{
			CimSessionProxy cimSessionProxySetCimInstance = new CimSessionProxySetCimInstance(computerName, cimInstance, passThru);
			this.SubscribeEventAndAddProxytoCache(cimSessionProxySetCimInstance);
			return cimSessionProxySetCimInstance;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Interlocked.CompareExchange(ref this._disposed, (long)1, (long)0) == (long)0 && disposing)
			{
				this.Cleanup();
			}
		}

		protected bool GetActionAndRemove(out CimBaseAction action)
		{
			return this.actionQueue.TryDequeue(out action);
		}

		protected object GetBaseObject(object value)
		{
			PSObject pSObject = value as PSObject;
			if (pSObject != null)
			{
				object baseObject = pSObject.BaseObject;
				object[] objArray = baseObject as object[];
				if (objArray != null)
				{
					object[] baseObject1 = new object[(int)objArray.Length];
					for (int i = 0; i < (int)objArray.Length; i++)
					{
						baseObject1[i] = this.GetBaseObject(objArray[i]);
					}
					return baseObject1;
				}
				else
				{
					return baseObject;
				}
			}
			else
			{
				return value;
			}
		}

		protected object GetReferenceOrReferenceArrayObject(object value, ref CimType referenceType)
		{
			PSReference pSReference = value as PSReference;
			if (pSReference == null)
			{
				object[] objArray = value as object[];
				if (objArray != null)
				{
					if (objArray[0] as PSReference != null)
					{
						CimInstance[] cimInstanceArray = new CimInstance[(int)objArray.Length];
						int num = 0;
						while (num < (int)objArray.Length)
						{
							PSReference pSReference1 = objArray[num] as PSReference;
							if (pSReference1 != null)
							{
								object baseObject = this.GetBaseObject(pSReference1.Value);
								cimInstanceArray[num] = baseObject as CimInstance;
								if (cimInstanceArray[num] != null)
								{
									num++;
								}
								else
								{
									return null;
								}
							}
							else
							{
								return null;
							}
						}
						referenceType = CimType.ReferenceArray;
						return cimInstanceArray;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
			else
			{
				object obj = this.GetBaseObject(pSReference.Value);
				CimInstance cimInstance = obj as CimInstance;
				if (cimInstance != null)
				{
					referenceType = CimType.Reference;
					return cimInstance;
				}
				else
				{
					return null;
				}
			}
		}

		protected bool IsActive()
		{
			bool flag;
			object[] disposed = new object[2];
			disposed[0] = this.Disposed;
			disposed[1] = this.operationCount;
			DebugHelper.WriteLogEx("Disposed {0}, Operation Count {1}", 2, disposed);
			if (this.Disposed)
			{
				flag = false;
			}
			else
			{
				flag = this.operationCount > 0;
			}
			bool flag1 = flag;
			return flag1;
		}

		protected void NewCmdletActionHandler(object cimSession, CmdletActionEventArgs actionArgs)
		{
			object[] action = new object[2];
			action[0] = this._disposed;
			action[1] = actionArgs.Action;
			DebugHelper.WriteLogEx("Disposed {0}, action type = {1}", 0, action);
			if (!this.Disposed)
			{
				bool isEmpty = this.actionQueue.IsEmpty;
				this.actionQueue.Enqueue(actionArgs.Action);
				if (isEmpty)
				{
					this.moreActionEvent.Set();
				}
				return;
			}
			else
			{
				if (actionArgs.Action as CimSyncAction != null)
				{
					(actionArgs.Action as CimSyncAction).OnComplete();
				}
				return;
			}
		}

		protected void OperationCreatedHandler(object cimSession, OperationEventArgs actionArgs)
		{
			DebugHelper.WriteLogEx();
			lock (this.myLock)
			{
				CimAsyncOperation cimAsyncOperation = this;
				cimAsyncOperation.operationCount = cimAsyncOperation.operationCount + 1;
			}
		}

		protected void OperationDeletedHandler(object cimSession, OperationEventArgs actionArgs)
		{
			DebugHelper.WriteLogEx();
			lock (this.myLock)
			{
				CimAsyncOperation cimAsyncOperation = this;
				cimAsyncOperation.operationCount = cimAsyncOperation.operationCount - 1;
				if (this.operationCount == 0)
				{
					this.moreActionEvent.Set();
				}
			}
		}

		public void ProcessActions(CmdletOperationBase cmdletOperation)
		{
			CimBaseAction cimBaseAction = null;
			if (!this.actionQueue.IsEmpty)
			{
				while (this.GetActionAndRemove(out cimBaseAction))
				{
					cimBaseAction.Execute(cmdletOperation);
					if (!this.Disposed)
					{
						continue;
					}
					return;
				}
			}
		}

		public void ProcessRemainActions(CmdletOperationBase cmdletOperation)
		{
			DebugHelper.WriteLogEx();
			while (true)
			{
				this.ProcessActions(cmdletOperation);
				if (this.IsActive())
				{
					try
					{
						this.moreActionEvent.Wait();
						this.moreActionEvent.Reset();
					}
					catch (ObjectDisposedException objectDisposedException1)
					{
						ObjectDisposedException objectDisposedException = objectDisposedException1;
						object[] objArray = new object[1];
						objArray[0] = objectDisposedException;
						DebugHelper.WriteLogEx("moreActionEvent was disposed: {0}.", 2, objArray);
						break;
					}
				}
				else
				{
					DebugHelper.WriteLogEx("Either disposed or all operations completed.", 2);
					break;
				}
			}
			this.ProcessActions(cmdletOperation);
		}

		protected void SubscribeEventAndAddProxytoCache(CimSessionProxy proxy)
		{
			this.AddCimSessionProxy(proxy);
			this.SubscribeToCimSessionProxyEvent(proxy);
		}

		protected PSCredential GetOnTheFlyCredentials (Cmdlet cmdlet)
		{
			PSCredential credential = null;
			try 
			{
				credential = cmdlet.Context.EngineIntrinsics.Host.UI.PromptForCredential ("CIM Credentials","Enter credentials:", System.Security.Principal.WindowsIdentity.GetCurrent().Name, "");
			}
			catch(Exception ex)
			{
				
			}
			return credential;
		}

		protected virtual void SubscribeToCimSessionProxyEvent(CimSessionProxy proxy)
		{
			DebugHelper.WriteLogEx();
			proxy.OnNewCmdletAction += new CimSessionProxy.NewCmdletActionHandler(this.NewCmdletActionHandler);
			proxy.OnOperationCreated += new CimSessionProxy.OperationEventHandler(this.OperationCreatedHandler);
			proxy.OnOperationDeleted += new CimSessionProxy.OperationEventHandler(this.OperationDeletedHandler);
		}
	}
}