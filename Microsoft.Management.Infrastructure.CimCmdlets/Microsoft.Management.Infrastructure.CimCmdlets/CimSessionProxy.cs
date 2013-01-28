using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionProxy : IDisposable
	{
		private static long gOperationCounter;

		private readonly static object temporarySessionCacheLock;

		private static Dictionary<CimSession, uint> temporarySessionCache;

		private long operationID;

		private CimSession session;

		private CimInstance targetCimInstance;

		private bool isTemporaryCimSession;

		private CimOperationOptions options;

		private readonly object stateLock;

		private IObservable<object> operation;

		private string operationName;

		private Hashtable operationParameters;

		private IDisposable _cancelOperation;

		private int _cancelOperationDisposed;

		private ProtocolType protocol;

		private XOperationContextBase contextObject;

		private InvocationContext invocationContextObject;

		private IObjectPreProcess objectPreprocess;

		private bool isDefaultSession;

		private int _disposed;

		private IDisposable CancelOperation
		{
			get
			{
				return this._cancelOperation;
			}
			set
			{
				DebugHelper.WriteLogEx();
				this._cancelOperation = value;
				Interlocked.Exchange(ref this._cancelOperationDisposed, 0);
			}
		}

		internal CimSession CimSession
		{
			get
			{
				return this.session;
			}
		}

		private bool Completed
		{
			get
			{
				return this.operation == null;
			}
		}

		internal XOperationContextBase ContextObject
		{
			get
			{
				return this.contextObject;
			}
			set
			{
				this.contextObject = value;
			}
		}

		public bool EnableMethodResultStreaming
		{
			get
			{
				return this.options.EnableMethodResultStreaming;
			}
			set
			{
				object[] objArray = new object[1];
				objArray[0] = value;
				DebugHelper.WriteLogEx("EnableMethodResultStreaming {0}", 0, objArray);
				this.options.EnableMethodResultStreaming = value;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return this._disposed == 1;
			}
		}

		internal bool IsTemporaryCimSession
		{
			get
			{
				return this.isTemporaryCimSession;
			}
		}

		public SwitchParameter KeyOnly
		{
			set
			{
				this.options.KeysOnly = value.IsPresent;
			}
		}

		internal IObjectPreProcess ObjectPreProcess
		{
			get
			{
				return this.objectPreprocess;
			}
			set
			{
				this.objectPreprocess = value;
			}
		}

		internal CimOperationOptions OperationOptions
		{
			get
			{
				return this.options;
			}
		}

		public uint OperationTimeout
		{
			get
			{
				TimeSpan timeout = this.options.Timeout;
				return (uint)timeout.TotalSeconds;
			}
			set
			{
				object[] objArray = new object[1];
				objArray[0] = value;
				DebugHelper.WriteLogEx("OperationTimeout {0},", 0, objArray);
				this.options.Timeout = TimeSpan.FromSeconds((double)((float)value));
			}
		}

		internal ProtocolType Protocol
		{
			get
			{
				return this.protocol;
			}
		}

		public Uri ResourceUri
		{
			get
			{
				return this.options.ResourceUri;
			}
			set
			{
				object[] objArray = new object[1];
				objArray[0] = value;
				DebugHelper.WriteLogEx("ResourceUri {0},", 0, objArray);
				this.options.ResourceUri = value;
			}
		}

		public SwitchParameter Shallow
		{
			set
			{
				if (!value.IsPresent)
				{
					this.options.Flags = CimOperationFlags.None;
					return;
				}
				else
				{
					this.options.Flags = CimOperationFlags.PolymorphismShallow;
					return;
				}
			}
		}

		internal CimInstance TargetCimInstance
		{
			get
			{
				return this.targetCimInstance;
			}
		}

		static CimSessionProxy()
		{
			CimSessionProxy.gOperationCounter = (long)0;
			CimSessionProxy.temporarySessionCacheLock = new object();
			CimSessionProxy.temporarySessionCache = new Dictionary<CimSession, uint>();
		}

		public CimSessionProxy(CimSessionProxy proxy)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			object[] protocol = new object[1];
			protocol[0] = proxy.Protocol;
			DebugHelper.WriteLogEx("protocol = {0}", 1, protocol);
			this.CreateSetSession(null, proxy.CimSession, null, proxy.OperationOptions, proxy.IsTemporaryCimSession);
			this.protocol = proxy.Protocol;
			this.OperationTimeout = proxy.OperationTimeout;
			this.isDefaultSession = proxy.isDefaultSession;
		}

		public CimSessionProxy(string computerName)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			this.CreateSetSession(computerName, null, null, null, false);
			this.isDefaultSession = computerName == ConstValue.NullComputerName;
		}

		public CimSessionProxy(string computerName, CimSessionOptions sessionOptions)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			this.CreateSetSession(computerName, null, sessionOptions, null, false);
			this.isDefaultSession = computerName == ConstValue.NullComputerName;
		}

		public CimSessionProxy(string computerName, CimInstance cimInstance)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			object[] cimSessionInstanceId = new object[3];
			cimSessionInstanceId[0] = computerName;
			cimSessionInstanceId[1] = cimInstance.GetCimSessionInstanceId();
			cimSessionInstanceId[2] = cimInstance.GetCimSessionComputerName();
			DebugHelper.WriteLogEx("ComptuerName {0}; cimInstance.CimSessionInstanceID = {1}; cimInstance.CimSessionComputerName = {2}.", 0, cimSessionInstanceId);
			if (computerName == ConstValue.NullComputerName)
			{
				CimSessionState cimSessionState = CimSessionBase.GetCimSessionState();
				if (cimSessionState != null)
				{
					CimSession cimSession = cimSessionState.QuerySession(cimInstance.GetCimSessionInstanceId());
					if (cimSession != null)
					{
						object[] objArray = new object[1];
						objArray[0] = cimInstance.GetCimSessionInstanceId();
						DebugHelper.WriteLogEx("Found the session from cache with InstanceID={0}.", 0, objArray);
						this.CreateSetSession(null, cimSession, null, null, false);
						return;
					}
				}
				string cimSessionComputerName = cimInstance.GetCimSessionComputerName();
				this.CreateSetSession(cimSessionComputerName, null, null, null, false);
				this.isDefaultSession = cimSessionComputerName == ConstValue.NullComputerName;
				object[] objArray1 = new object[1];
				objArray1[0] = cimSessionComputerName;
				DebugHelper.WriteLogEx("Create a temp session with computerName = {0}.", 0, objArray1);
				return;
			}
			else
			{
				this.CreateSetSession(computerName, null, null, null, false);
				return;
			}
		}

		public CimSessionProxy(string computerName, CimSessionOptions sessionOptions, CimOperationOptions operOptions)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			this.CreateSetSession(computerName, null, sessionOptions, operOptions, false);
			this.isDefaultSession = computerName == ConstValue.NullComputerName;
		}

		public CimSessionProxy(string computerName, CimOperationOptions operOptions)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			this.CreateSetSession(computerName, null, null, operOptions, false);
			this.isDefaultSession = computerName == ConstValue.NullComputerName;
		}

		public CimSessionProxy(CimSession session)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			this.CreateSetSession(null, session, null, null, false);
		}

		public CimSessionProxy(CimSession session, CimOperationOptions operOptions)
		{
			this.stateLock = new object();
			this.operationParameters = new Hashtable();
			this.CreateSetSession(null, session, null, operOptions, false);
		}

		internal static void AddCimSessionToTemporaryCache(CimSession session)
		{
			if (session != null)
			{
				lock (CimSessionProxy.temporarySessionCacheLock)
				{
					if (!CimSessionProxy.temporarySessionCache.ContainsKey(session))
					{
						CimSessionProxy.temporarySessionCache.Add(session, 1);
						object[] item = new object[1];
						item[0] = CimSessionProxy.temporarySessionCache[session];
						DebugHelper.WriteLogEx("Add cimsession to cache. Ref count {0}", 1, item);
					}
					else
					{
						Dictionary<CimSession, uint> cimSessions = CimSessionProxy.temporarySessionCache;
						Dictionary<CimSession, uint> cimSessions1 = cimSessions;
						CimSession cimSession = session;
						CimSession cimSession1 = cimSession;
						cimSessions[cimSession] = cimSessions1[cimSession1] + 1;
						object[] objArray = new object[1];
						objArray[0] = CimSessionProxy.temporarySessionCache[session];
						DebugHelper.WriteLogEx("Increase cimsession ref count {0}", 1, objArray);
					}
				}
			}
		}

		private void AddOperation(IObservable<object> operation)
		{
			DebugHelper.WriteLogEx();
			lock (this.stateLock)
			{
				this.operation = operation;
			}
		}

		private static void AddShowComputerNameMarker(object o)
		{
			if (o != null)
			{
				PSObject pSObject = PSObject.AsPSObject(o);
				if (pSObject.BaseObject as CimInstance != null)
				{
					PSNoteProperty pSNoteProperty = new PSNoteProperty(ConstValue.ShowComputerNameNoteProperty, (object)(true));
					pSObject.Members.Add(pSNoteProperty);
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void AssertSession()
		{
			if (this.IsDisposed || this.session == null)
			{
				object[] isDisposed = new object[2];
				isDisposed[0] = this.IsDisposed;
				isDisposed[1] = this.session;
				DebugHelper.WriteLogEx("Invalid CimSessionProxy object, disposed? {0}; session object {1}", 1, isDisposed);
				throw new ObjectDisposedException(this.ToString());
			}
			else
			{
				return;
			}
		}

		private void CheckAvailability()
		{
			DebugHelper.WriteLogEx();
			this.AssertSession();
			lock (this.stateLock)
			{
				if (!this.Completed)
				{
					throw new InvalidOperationException(Strings.OperationInProgress);
				}
			}
			object[] keysOnly = new object[1];
			keysOnly[0] = this.options.KeysOnly;
			DebugHelper.WriteLog("KeyOnly {0},", 1, keysOnly);
		}

		protected void ConsumeCimClassAsync(IObservable<CimClass> asyncResult, CimResultContext cimResultContext)
		{
			CimResultObserver<CimClass> cimResultObserver = new CimResultObserver<CimClass>(this.session, asyncResult, cimResultContext);
			cimResultObserver.OnNewResult += new CimResultObserver<CimClass>.ResultEventHandler(this.ResultEventHandler);
			this.operationID = Interlocked.Increment(ref CimSessionProxy.gOperationCounter);
			this.AddOperation(asyncResult);
			this.CancelOperation = asyncResult.Subscribe(cimResultObserver);
			this.FireOperationCreatedEvent(this.CancelOperation, asyncResult);
		}

		protected void ConsumeCimInstanceAsync(IObservable<CimInstance> asyncResult, CimResultContext cimResultContext)
		{
			this.ConsumeCimInstanceAsync(asyncResult, false, cimResultContext);
		}

		protected void ConsumeCimInstanceAsync(IObservable<CimInstance> asyncResult, bool ignoreResultObjects, CimResultContext cimResultContext)
		{
			CimResultObserver<CimInstance> cimResultObserver;
			if (!ignoreResultObjects)
			{
				cimResultObserver = new CimResultObserver<CimInstance>(this.session, asyncResult, cimResultContext);
			}
			else
			{
				cimResultObserver = new IgnoreResultObserver(this.session, asyncResult);
			}
			cimResultObserver.OnNewResult += new CimResultObserver<CimInstance>.ResultEventHandler(this.ResultEventHandler);
			this.operationID = Interlocked.Increment(ref CimSessionProxy.gOperationCounter);
			this.AddOperation(asyncResult);
			this.CancelOperation = asyncResult.Subscribe(cimResultObserver);
			this.FireOperationCreatedEvent(this.CancelOperation, asyncResult);
		}

		protected void ConsumeCimInvokeMethodResultAsync(IObservable<CimMethodResultBase> asyncResult, string className, string methodName, CimResultContext cimResultContext)
		{
			CimMethodResultObserver cimMethodResultObserver = new CimMethodResultObserver(this.session, asyncResult, cimResultContext);
			cimMethodResultObserver.ClassName = className;
			cimMethodResultObserver.MethodName = methodName;
			CimMethodResultObserver cimMethodResultObserver1 = cimMethodResultObserver;
			cimMethodResultObserver1.OnNewResult += new CimResultObserver<CimMethodResultBase>.ResultEventHandler(this.ResultEventHandler);
			this.operationID = Interlocked.Increment(ref CimSessionProxy.gOperationCounter);
			this.AddOperation(asyncResult);
			this.CancelOperation = asyncResult.Subscribe(cimMethodResultObserver1);
			this.FireOperationCreatedEvent(this.CancelOperation, asyncResult);
		}

		protected void ConsumeCimSubscriptionResultAsync(IObservable<CimSubscriptionResult> asyncResult, CimResultContext cimResultContext)
		{
			CimSubscriptionResultObserver cimSubscriptionResultObserver = new CimSubscriptionResultObserver(this.session, asyncResult, cimResultContext);
			cimSubscriptionResultObserver.OnNewResult += new CimResultObserver<CimSubscriptionResult>.ResultEventHandler(this.ResultEventHandler);
			this.operationID = Interlocked.Increment(ref CimSessionProxy.gOperationCounter);
			this.AddOperation(asyncResult);
			this.CancelOperation = asyncResult.Subscribe(cimSubscriptionResultObserver);
			this.FireOperationCreatedEvent(this.CancelOperation, asyncResult);
		}

		protected void ConsumeObjectAsync(IObservable<object> asyncResult, CimResultContext cimResultContext)
		{
			CimResultObserver<object> cimResultObserver = new CimResultObserver<object>(this.session, asyncResult, cimResultContext);
			cimResultObserver.OnNewResult += new CimResultObserver<object>.ResultEventHandler(this.ResultEventHandler);
			this.operationID = Interlocked.Increment(ref CimSessionProxy.gOperationCounter);
			this.AddOperation(asyncResult);
			this.CancelOperation = asyncResult.Subscribe(cimResultObserver);
			DebugHelper.WriteLog("FireOperationCreatedEvent");
			this.FireOperationCreatedEvent(this.CancelOperation, asyncResult);
		}

		private CimSession CreateCimSessionByComputerName(string computerName)
		{
			object[] objArray = new object[1];
			objArray[0] = computerName;
			DebugHelper.WriteLogEx("ComputerName {0}", 0, objArray);
			CimSessionOptions cimSessionOption = CimSessionProxy.CreateCimSessionOption(computerName, 0, null);
			if (cimSessionOption as DComSessionOptions == null)
			{
				DebugHelper.WriteLog("Create wsman cimSession");
				return CimSession.Create(computerName, cimSessionOption);
			}
			else
			{
				DebugHelper.WriteLog("Create dcom cimSession");
				this.protocol = ProtocolType.Dcom;
				return CimSession.Create(ConstValue.NullComputerName, cimSessionOption);
			}
		}

		internal static CimSessionOptions CreateCimSessionOption(string computerName, uint timeout, CimCredential credential)
		{
			CimSessionOptions wSManSessionOption;
			DebugHelper.WriteLogEx();
			if (!ConstValue.IsDefaultComputerName(computerName))
			{
				object[] objArray = new object[1];
				objArray[0] = computerName;
				DebugHelper.WriteLog("<<<<<<<<<< Use protocol WSMAN {0}", 1, objArray);
				wSManSessionOption = new WSManSessionOptions();
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = computerName;
				DebugHelper.WriteLog("<<<<<<<<<< Use protocol DCOM  {0}", 1, objArray1);
				wSManSessionOption = new DComSessionOptions();
			}
			if (timeout != 0)
			{
				wSManSessionOption.Timeout = TimeSpan.FromSeconds((double)((float)timeout));
			}
			if (credential != null)
			{
				wSManSessionOption.AddDestinationCredentials(credential);
			}
			object[] objArray2 = new object[1];
			objArray2[0] = wSManSessionOption;
			DebugHelper.WriteLogEx("returned option :{0}.", 1, objArray2);
			return wSManSessionOption;
		}

		public void CreateInstanceAsync(string namespaceName, CimInstance instance)
		{
			object[] enableMethodResultStreaming = new object[1];
			enableMethodResultStreaming[0] = this.options.EnableMethodResultStreaming;
			DebugHelper.WriteLogEx("EnableMethodResultStreaming = {0}", 0, enableMethodResultStreaming);
			this.CheckAvailability();
			this.targetCimInstance = instance;
			this.operationName = Strings.CimOperationNameCreateInstance;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("instance", instance);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncResult<CimInstance> cimAsyncResult = (CimAsyncResult<CimInstance>)this.session.CreateInstanceAsync(namespaceName, instance, this.options);
			this.ConsumeCimInstanceAsync(cimAsyncResult, new CimResultContext(instance));
		}

		private void CreateSetSession(string computerName, CimSession cimSession, CimSessionOptions sessionOptions, CimOperationOptions operOptions, bool temporaryCimSession)
		{
			string nullComputerName;
			object[] objArray = new object[4];
			objArray[0] = computerName;
			objArray[1] = cimSession;
			objArray[2] = sessionOptions;
			objArray[3] = operOptions;
			DebugHelper.WriteLogEx("computername {0}; cimsession {1}; sessionOptions {2}; operationOptions {3}.", 0, objArray);
			lock (this.stateLock)
			{
				this.CancelOperation = null;
				this.operation = null;
			}
			this.InitOption(operOptions);
			this.protocol = ProtocolType.Wsman;
			this.isTemporaryCimSession = temporaryCimSession;

			if (cimSession == null)
			{
				if (sessionOptions == null)
				{
					this.session = this.CreateCimSessionByComputerName(computerName);
				}
				else
				{
					if (sessionOptions as DComSessionOptions == null)
					{
						this.session = CimSession.Create(computerName, sessionOptions);
					}
					else
					{
						if (ConstValue.IsDefaultComputerName(computerName))
						{
							nullComputerName = ConstValue.NullComputerName;
						}
						else
						{
							nullComputerName = computerName;
						}
						string str = nullComputerName;
						this.session = CimSession.Create(str, sessionOptions);
						this.protocol = ProtocolType.Dcom;
					}
				}
				this.isTemporaryCimSession = true;
			}
			else
			{
				this.session = cimSession;
				CimSessionState cimSessionState = CimSessionBase.GetCimSessionState();
				if (cimSessionState != null)
				{
					CimSessionWrapper cimSessionWrapper = cimSessionState.QuerySession(cimSession);
					if (cimSessionWrapper != null)
					{
						this.protocol = cimSessionWrapper.GetProtocolType();
					}
				}
			}
			if (this.isTemporaryCimSession)
			{
				CimSessionProxy.AddCimSessionToTemporaryCache(this.session);
			}
			this.invocationContextObject = new InvocationContext(this);
			object[] objArray1 = new object[2];
			objArray1[0] = this.protocol;
			objArray1[1] = this.isTemporaryCimSession;
			DebugHelper.WriteLog("Protocol {0}, Is temporary session ? {1}", 1, objArray1);
		}

		public void DeleteInstanceAsync(string namespaceName, CimInstance instance)
		{
			object[] className = new object[2];
			className[0] = namespaceName;
			className[1] = instance.CimSystemProperties.ClassName;
			DebugHelper.WriteLogEx("namespace = {0}; classname = {1};", 0, className);
			this.CheckAvailability();
			this.targetCimInstance = instance;
			this.operationName = Strings.CimOperationNameDeleteInstance;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("instance", instance);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			var cimAsyncStatu = this.session.DeleteInstanceAsync(namespaceName, instance, this.options);
			this.ConsumeObjectAsync(cimAsyncStatu, new CimResultContext(instance));
		}

		public CimSession Detach()
		{
			DebugHelper.WriteLogEx();
			CimSessionProxy.RemoveCimSessionFromTemporaryCache(this.session, false);
			CimSession cimSession = this.session;
			this.session = null;
			this.isTemporaryCimSession = false;
			return cimSession;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			object[] isDisposed = new object[1];
			isDisposed[0] = this.IsDisposed;
			DebugHelper.WriteLogEx("Disposed = {0}", 0, isDisposed);
			if (Interlocked.CompareExchange(ref this._disposed, 1, 0) == 0 && disposing)
			{
				this.DisposeCancelOperation();
				if (this.options != null)
				{
					this.options.Dispose();
					this.options = null;
				}
				this.DisposeTemporaryCimSession();
			}
		}

		private void DisposeCancelOperation()
		{
			object[] objArray = new object[1];
			objArray[0] = this._cancelOperationDisposed;
			DebugHelper.WriteLogEx("CancelOperation Disposed = {0}", 0, objArray);
			if (Interlocked.CompareExchange(ref this._cancelOperationDisposed, 1, 0) == 0 && this._cancelOperation != null)
			{
				DebugHelper.WriteLog("CimSessionProxy::Dispose async operation.", 4);
				this._cancelOperation.Dispose();
				this._cancelOperation = null;
			}
		}

		private void DisposeTemporaryCimSession()
		{
			if (this.isTemporaryCimSession && this.session != null)
			{
				CimSessionProxy.RemoveCimSessionFromTemporaryCache(this.session);
				this.isTemporaryCimSession = false;
				this.session = null;
			}
		}

		private void EnablePSSemantics()
		{
			DebugHelper.WriteLogEx();
			this.options.WriteErrorMode = CimCallbackMode.Inquire;
			this.options.WriteError = this.WriteError;
			this.options.WriteMessage = this.WriteMessage;
			this.options.WriteProgress = this.WriteProgress;
		}

		public void EnumerateAssociatedInstancesAsync(string namespaceName, CimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
		{
			object[] className = new object[2];
			className[0] = sourceInstance.CimSystemProperties.ClassName;
			className[1] = associationClassName;
			DebugHelper.WriteLogEx("Instance class {0}, association class {1}", 0, className);
			this.CheckAvailability();
			this.targetCimInstance = sourceInstance;
			this.operationName = Strings.CimOperationNameEnumerateAssociatedInstances;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("sourceInstance", sourceInstance);
			this.operationParameters.Add("associationClassName", associationClassName);
			this.operationParameters.Add("resultClassName", resultClassName);
			this.operationParameters.Add("sourceRole", sourceRole);
			this.operationParameters.Add("resultRole", resultRole);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimInstance> cimAsyncMultipleResult = this.session.EnumerateAssociatedInstancesAsync(namespaceName, sourceInstance, associationClassName, resultClassName, sourceRole, resultRole, this.options);
			this.ConsumeCimInstanceAsync(cimAsyncMultipleResult, new CimResultContext(sourceInstance));
		}

		public void EnumerateClassesAsync(string namespaceName)
		{
			object[] objArray = new object[1];
			objArray[0] = namespaceName;
			DebugHelper.WriteLogEx("namespace {0}", 0, objArray);
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameEnumerateClasses;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimClass> cimAsyncMultipleResult = this.session.EnumerateClassesAsync(namespaceName, null, this.options);
			this.ConsumeCimClassAsync(cimAsyncMultipleResult, null);
		}

		public void EnumerateClassesAsync(string namespaceName, string className)
		{
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameEnumerateClasses;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("className", className);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimClass> cimAsyncMultipleResult = this.session.EnumerateClassesAsync(namespaceName, className, this.options);
			object[] objArray = new object[2];
			objArray[0] = namespaceName;
			objArray[1] = className;
			string str = string.Format(CultureInfo.CurrentUICulture, "{0}:{1}", objArray);
			this.ConsumeCimClassAsync(cimAsyncMultipleResult, new CimResultContext(str));
		}

		public void EnumerateInstancesAsync(string namespaceName, string className)
		{
			object[] keysOnly = new object[1];
			keysOnly[0] = this.options.KeysOnly;
			DebugHelper.WriteLogEx("KeyOnly {0}", 0, keysOnly);
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameEnumerateInstances;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("className", className);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimInstance> cimAsyncMultipleResult = this.session.EnumerateInstancesAsync(namespaceName, className, this.options);
			object[] objArray = new object[2];
			objArray[0] = namespaceName;
			objArray[1] = className;
			string str = string.Format(CultureInfo.CurrentUICulture, "{0}:{1}", objArray);
			this.ConsumeCimInstanceAsync(cimAsyncMultipleResult, new CimResultContext(str));
		}

		public void EnumerateReferencingInstancesAsync(string namespaceName, CimInstance sourceInstance, string associationClassName, string sourceRole)
		{
			this.CheckAvailability();
		}

		protected void FireNewActionEvent(CimBaseAction action)
		{
			DebugHelper.WriteLogEx();
			CmdletActionEventArgs cmdletActionEventArg = new CmdletActionEventArgs(action);
			if (this.PreNewActionEvent(cmdletActionEventArg))
			{
				CimSessionProxy.NewCmdletActionHandler newCmdletActionHandler = this.OnNewCmdletAction;
				if (newCmdletActionHandler == null)
				{
					DebugHelper.WriteLog("Ignore action since OnNewCmdletAction is null.", 5);
				}
				else
				{
					newCmdletActionHandler(this.session, cmdletActionEventArg);
				}
				this.PostNewActionEvent(cmdletActionEventArg);
				return;
			}
			else
			{
				return;
			}
		}

		private void FireOperationCreatedEvent(IDisposable cancelOperation, IObservable<object> operation)
		{
			DebugHelper.WriteLogEx();
			OperationEventArgs operationEventArg = new OperationEventArgs(cancelOperation, operation, false);
			CimSessionProxy.OperationEventHandler operationEventHandler = this.OnOperationCreated;
			if (operationEventHandler != null)
			{
				operationEventHandler(this.session, operationEventArg);
			}
			this.PostOperationCreateEvent(operationEventArg);
		}

		private void FireOperationDeletedEvent(IObservable<object> operation, bool success)
		{
			DebugHelper.WriteLogEx();
			this.WriteOperationCompleteMessage(this.operationName);
			OperationEventArgs operationEventArg = new OperationEventArgs(null, operation, success);
			this.PreOperationDeleteEvent(operationEventArg);
			CimSessionProxy.OperationEventHandler operationEventHandler = this.OnOperationDeleted;
			if (operationEventHandler != null)
			{
				operationEventHandler(this.session, operationEventArg);
			}
			this.PostOperationDeleteEvent(operationEventArg);
			this.RemoveOperation(operation);
			this.operationName = null;
		}

		public void GetClassAsync(string namespaceName, string className)
		{
			object[] objArray = new object[2];
			objArray[0] = namespaceName;
			objArray[1] = className;
			DebugHelper.WriteLogEx("namespace = {0}, className = {1}", 0, objArray);
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameGetClass;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("className", className);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncResult<CimClass> classAsync = this.session.GetClassAsync(namespaceName, className, this.options);
			object[] objArray1 = new object[2];
			objArray1[0] = namespaceName;
			objArray1[1] = className;
			string str = string.Format(CultureInfo.CurrentUICulture, "{0}:{1}", objArray1);
			this.ConsumeCimClassAsync(classAsync, new CimResultContext(str));
		}

		public void GetInstanceAsync(string namespaceName, CimInstance instance)
		{
			object[] className = new object[3];
			className[0] = namespaceName;
			className[1] = instance.CimSystemProperties.ClassName;
			className[2] = this.options.KeysOnly;
			DebugHelper.WriteLogEx("namespace = {0}; classname = {1}; keyonly = {2}", 0, className);
			this.CheckAvailability();
			this.targetCimInstance = instance;
			this.operationName = Strings.CimOperationNameGetInstance;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("instance", instance);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			var instanceAsync = this.session.GetInstanceAsync(namespaceName, instance, this.options);
			this.ConsumeCimInstanceAsync(instanceAsync, new CimResultContext(instance));
		}

		private void InitOption(CimOperationOptions operOptions)
		{
			DebugHelper.WriteLogEx();
			if (operOptions == null)
			{
				if (this.options == null)
				{
					this.options = new CimOperationOptions();
				}
			}
			else
			{
				this.options = new CimOperationOptions(operOptions);
			}
			this.EnableMethodResultStreaming = true;
			this.EnablePSSemantics();
		}

		public void InvokeMethodAsync(string namespaceName, CimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
		{
			object[] enableMethodResultStreaming = new object[1];
			enableMethodResultStreaming[0] = this.options.EnableMethodResultStreaming;
			DebugHelper.WriteLogEx("EnableMethodResultStreaming = {0}", 0, enableMethodResultStreaming);
			this.CheckAvailability();
			this.targetCimInstance = instance;
			this.operationName = Strings.CimOperationNameInvokeMethod;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("instance", instance);
			this.operationParameters.Add("methodName", methodName);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimMethodResultBase> cimAsyncMultipleResult = this.session.InvokeMethodAsync(namespaceName, instance, methodName, methodParameters, this.options);
			this.ConsumeCimInvokeMethodResultAsync(cimAsyncMultipleResult, instance.CimSystemProperties.ClassName, methodName, new CimResultContext(instance));
		}

		public void InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
		{
			object[] enableMethodResultStreaming = new object[1];
			enableMethodResultStreaming[0] = this.options.EnableMethodResultStreaming;
			DebugHelper.WriteLogEx("EnableMethodResultStreaming = {0}", 0, enableMethodResultStreaming);
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameInvokeMethod;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("className", className);
			this.operationParameters.Add("methodName", methodName);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimMethodResultBase> cimAsyncMultipleResult = this.session.InvokeMethodAsync(namespaceName, className, methodName, methodParameters, this.options);
			object[] objArray = new object[2];
			objArray[0] = namespaceName;
			objArray[1] = className;
			string str = string.Format(CultureInfo.CurrentUICulture, "{0}:{1}", objArray);
			this.ConsumeCimInvokeMethodResultAsync(cimAsyncMultipleResult, className, methodName, new CimResultContext(str));
		}

		public void ModifyInstanceAsync(string namespaceName, CimInstance instance)
		{
			object[] className = new object[2];
			className[0] = namespaceName;
			className[1] = instance.CimSystemProperties.ClassName;
			DebugHelper.WriteLogEx("namespace = {0}; classname = {1}", 0, className);
			this.CheckAvailability();
			this.targetCimInstance = instance;
			this.operationName = Strings.CimOperationNameModifyInstance;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("instance", instance);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			var cimAsyncResult = this.session.ModifyInstanceAsync(namespaceName, instance, this.options);
			this.ConsumeObjectAsync(cimAsyncResult, new CimResultContext(instance));
		}

		protected virtual void PostNewActionEvent(CmdletActionEventArgs args)
		{
		}

		protected virtual void PostOperationCreateEvent(OperationEventArgs args)
		{
		}

		protected virtual void PostOperationDeleteEvent(OperationEventArgs args)
		{
		}

		protected virtual bool PreNewActionEvent(CmdletActionEventArgs args)
		{
			return true;
		}

		protected virtual void PreOperationDeleteEvent(OperationEventArgs args)
		{
		}

		public CimResponseType PromptUser(string message, CimPromptType prompt)
		{
			CimResponseType response;
			object[] objArray = new object[2];
			objArray[0] = message;
			objArray[1] = prompt;
			DebugHelper.WriteLogEx("message:{0} prompt:{1}", 0, objArray);
			try
			{
				CimPromptUser cimPromptUser = new CimPromptUser(message, prompt);
				this.FireNewActionEvent(cimPromptUser);
				response = cimPromptUser.GetResponse();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray1 = new object[1];
				objArray1[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray1);
				response = CimResponseType.NoToAll;
			}
			return response;
		}

		public void QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression)
		{
			object[] keysOnly = new object[1];
			keysOnly[0] = this.options.KeysOnly;
			DebugHelper.WriteLogEx("KeyOnly = {0}", 0, keysOnly);
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameQueryInstances;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("queryDialect", queryDialect);
			this.operationParameters.Add("queryExpression", queryExpression);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimAsyncMultipleResults<CimInstance> cimAsyncMultipleResult = this.session.QueryInstancesAsync(namespaceName, queryDialect, queryExpression, this.options);
			this.ConsumeCimInstanceAsync(cimAsyncMultipleResult, null);
		}

		private static void RemoveCimSessionFromTemporaryCache(CimSession session, bool dispose)
		{
			if (session != null)
			{
				bool flag = false;
				lock (CimSessionProxy.temporarySessionCacheLock)
				{
					if (CimSessionProxy.temporarySessionCache.ContainsKey(session))
					{
						Dictionary<CimSession, uint> item = CimSessionProxy.temporarySessionCache;
						Dictionary<CimSession, uint> cimSessions = item;
						CimSession cimSession = session;
						CimSession cimSession1 = cimSession;
						item[cimSession] = cimSessions[cimSession1] - 1;
						object[] objArray = new object[1];
						objArray[0] = CimSessionProxy.temporarySessionCache[session];
						DebugHelper.WriteLogEx("Decrease cimsession ref count {0}", 1, objArray);
						if (CimSessionProxy.temporarySessionCache[session] == 0)
						{
							CimSessionProxy.temporarySessionCache.Remove(session);
						}
					}
				}
				if (flag && dispose)
				{
					DebugHelper.WriteLogEx("Dispose cimsession ", 1);
					session.Dispose();
				}
			}
		}

		internal static void RemoveCimSessionFromTemporaryCache(CimSession session)
		{
			CimSessionProxy.RemoveCimSessionFromTemporaryCache(session, true);
		}

		private void RemoveOperation(IObservable<object> operation)
		{
			DebugHelper.WriteLogEx();
			lock (this.stateLock)
			{
				this.DisposeCancelOperation();
				if (this.operation != null)
				{
					this.operation = null;
				}
				if (this.session != null && this.ContextObject == null)
				{
					DebugHelper.WriteLog("Dispose this proxy object @ RemoveOperation");
					this.Dispose();
				}
			}
		}

		internal void ResultEventHandler(object observer, AsyncResultEventArgsBase resultArgs)
		{
			DebugHelper.WriteLogEx();
			AsyncResultType asyncResultType = resultArgs.resultType;
			switch (asyncResultType)
			{
				case AsyncResultType.Result:
				{
					AsyncResultObjectEventArgs asyncResultObjectEventArg = resultArgs as AsyncResultObjectEventArgs;
					object[] objArray = new object[1];
					objArray[0] = asyncResultObjectEventArg.resultObject;
					DebugHelper.WriteLog("ResultEventHandler::Result {0}", 4, objArray);
					object obj = asyncResultObjectEventArg.resultObject;
					if (!this.isDefaultSession)
					{
						CimSessionProxy.AddShowComputerNameMarker(obj);
					}
					if (this.ObjectPreProcess != null)
					{
						obj = this.ObjectPreProcess.Process(obj);
					}
					CimWriteResultObject cimWriteResultObject = new CimWriteResultObject(obj, this.ContextObject);
					this.FireNewActionEvent(cimWriteResultObject);
					return;
				}
				case AsyncResultType.Exception:
				{
					AsyncResultErrorEventArgs asyncResultErrorEventArg = resultArgs as AsyncResultErrorEventArgs;
					object[] objArray1 = new object[1];
					objArray1[0] = asyncResultErrorEventArg.error;
					DebugHelper.WriteLog("ResultEventHandler::Exception {0}", 4, objArray1);
					using (CimWriteError cimWriteError = new CimWriteError(asyncResultErrorEventArg.error, this.invocationContextObject, asyncResultErrorEventArg.context))
					{
						this.FireNewActionEvent(cimWriteError);
					}
					this.FireOperationDeletedEvent(asyncResultErrorEventArg.observable, false);
					return;
				}
				case AsyncResultType.Completion:
				{
					DebugHelper.WriteLog("ResultEventHandler::Completion", 4);
					AsyncResultCompleteEventArgs asyncResultCompleteEventArg = resultArgs as AsyncResultCompleteEventArgs;
					this.FireOperationDeletedEvent(asyncResultCompleteEventArg.observable, true);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		public void SubscribeAsync(string namespaceName, string queryDialect, string queryExpression)
		{
			object[] objArray = new object[2];
			objArray[0] = queryDialect;
			objArray[1] = queryExpression;
			DebugHelper.WriteLogEx("QueryDialect = '{0}'; queryExpression = '{1}'", 0, objArray);
			this.CheckAvailability();
			this.targetCimInstance = null;
			this.operationName = Strings.CimOperationNameSubscribeIndication;
			this.operationParameters.Clear();
			this.operationParameters.Add("namespaceName", namespaceName);
			this.operationParameters.Add("queryDialect", queryDialect);
			this.operationParameters.Add("queryExpression", queryExpression);
			this.WriteOperationStartMessage(this.operationName, this.operationParameters);
			CimOperationOptions flags = this.options;
			flags.Flags = flags.Flags | CimOperationFlags.ReportOperationStarted;
			CimAsyncMultipleResults<CimSubscriptionResult> cimAsyncMultipleResult = this.session.SubscribeAsync(namespaceName, queryDialect, queryExpression, this.options);
			this.ConsumeCimSubscriptionResultAsync(cimAsyncMultipleResult, null);
		}

		public void TestConnectionAsync()
		{
			DebugHelper.WriteLogEx("Start test connection", 0);
			this.CheckAvailability();
			this.targetCimInstance = null;
			CimAsyncResult<CimInstance> cimAsyncResult = this.session.TestConnectionAsync();
			this.ConsumeCimInstanceAsync(cimAsyncResult, true, null);
		}

		public CimResponseType WriteError(CimInstance instance)
		{
			CimResponseType response;
			object[] objArray = new object[1];
			objArray[0] = instance;
			DebugHelper.WriteLogEx("Error:{0}", 0, objArray);
			try
			{
				CimWriteError cimWriteError = new CimWriteError(instance, this.invocationContextObject);
				this.FireNewActionEvent(cimWriteError);
				response = cimWriteError.GetResponse();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray1 = new object[1];
				objArray1[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray1);
				response = CimResponseType.NoToAll;
			}
			return response;
		}

		internal void WriteMessage(uint channel, string message)
		{
			object[] objArray = new object[2];
			objArray[0] = channel;
			objArray[1] = message;
			DebugHelper.WriteLogEx("Channel = {0} message = {1}", 0, objArray);
			try
			{
				CimWriteMessage cimWriteMessage = new CimWriteMessage(channel, message);
				this.FireNewActionEvent(cimWriteMessage);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray1 = new object[1];
				objArray1[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray1);
			}
		}

		internal void WriteOperationCompleteMessage(string operation)
		{
			DebugHelper.WriteLogEx();
			object[] objArray = new object[1];
			objArray[0] = operation;
			string str = string.Format(CultureInfo.CurrentUICulture, Strings.CimOperationCompleted, objArray);
			this.WriteMessage(1, str);
		}

		internal void WriteOperationStartMessage(string operation, Hashtable parameterList)
		{
			object str;
			DebugHelper.WriteLogEx();
			StringBuilder stringBuilder = new StringBuilder();
			if (parameterList != null)
			{
				foreach (string key in parameterList.Keys)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(",");
					}
					object[] item = new object[2];
					item[0] = key;
					item[1] = parameterList[key];
					stringBuilder.Append(string.Format(CultureInfo.CurrentUICulture, "'{0}' = {1}", item));
				}
			}
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			string cimOperationStart = Strings.CimOperationStart;
			object[] objArray = new object[2];
			objArray[0] = operation;
			object[] objArray1 = objArray;
			int num = 1;
			if (stringBuilder.Length == 0)
			{
				str = "null";
			}
			else
			{
				str = stringBuilder.ToString();
			}
			objArray1[num] = str;
			string str1 = string.Format(currentUICulture, cimOperationStart, objArray);
			this.WriteMessage(1, str1);
		}

		public void WriteProgress(string activity, string currentOperation, string statusDescription, int percentageCompleted, int secondsRemaining)
		{
			object[] objArray = new object[4];
			objArray[0] = activity;
			objArray[1] = currentOperation;
			objArray[2] = percentageCompleted;
			objArray[3] = secondsRemaining;
			DebugHelper.WriteLogEx("activity:{0}; currentOperation:{1}; percentageCompleted:{2}; secondsRemaining:{3}", 0, objArray);
			try
			{
				CimWriteProgress cimWriteProgress = new CimWriteProgress(activity, (int)this.operationID, currentOperation, statusDescription, percentageCompleted, secondsRemaining);
				this.FireNewActionEvent(cimWriteProgress);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray1 = new object[1];
				objArray1[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray1);
			}
		}

		public event CimSessionProxy.NewCmdletActionHandler OnNewCmdletAction;
		public event CimSessionProxy.OperationEventHandler OnOperationCreated;
		public event CimSessionProxy.OperationEventHandler OnOperationDeleted;
		public delegate void NewCmdletActionHandler(object cimSession, CmdletActionEventArgs actionArgs);

		public delegate void OperationEventHandler(object cimSession, OperationEventArgs actionArgs);
	}
}