using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Internal.Operations;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Microsoft.Management.Infrastructure
{
	public class CimSession : IDisposable
	{
		private readonly SessionHandle _handle;

		private readonly object _disposeThreadSafetyLock;

		private bool _disposed;

		public string ComputerName
		{
			get;
			private set;
		}

		public Guid InstanceId
		{
			get;
			private set;
		}

		private CimSession(SessionHandle handle, string computerName)
		{
			this._disposeThreadSafetyLock = new object();
			this._handle = handle;
			this.ComputerName = computerName;
			this.InstanceId = Guid.NewGuid();
			CimApplication.AddTracking(this);
		}

		internal void AssertNotDisposed()
		{
			lock (this._disposeThreadSafetyLock)
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException(this.ToString());
				}
			}
		}

		public void Close()
		{
			lock (this._disposeThreadSafetyLock)
			{
				if (!this._disposed)
				{
					this._disposed = true;
				}
				else
				{
					return;
				}
			}
			MiResult miResult = this._handle.ReleaseHandleSynchronously();
			CimException.ThrowIfMiResultFailure(miResult);
		}

		public CimAsyncStatus CloseAsync()
		{
			IObservable<object> cimAsyncDelegatedObservable = new CimAsyncDelegatedObservable<object>((IObserver<object> observer) => {
				bool flag;
				lock (this._disposeThreadSafetyLock)
				{
					flag = this._disposed;
					this._disposed = true;
				}
				if (!flag)
				{
					CimSession.CloseAsyncImpersonationWorker closeAsyncImpersonationWorker = new CimSession.CloseAsyncImpersonationWorker(observer);
					MiResult miResult = this._handle.ReleaseHandleAsynchronously(new SessionHandle.OnSessionHandleReleasedDelegate(closeAsyncImpersonationWorker.OnCompleted));
					CimException exceptionIfMiResultFailure = CimException.GetExceptionIfMiResultFailure(miResult, null, null);
					if (exceptionIfMiResultFailure != null)
					{
						observer.OnError(exceptionIfMiResultFailure);
						closeAsyncImpersonationWorker.Dispose();
					}
					return;
				}
				else
				{
					observer.OnCompleted();
					return;
				}
			}
			);
			return new CimAsyncStatus(cimAsyncDelegatedObservable);
		}

		public static CimSession Create(string computerName)
		{
			return CimSession.Create(computerName, null);
		}

		public static CimSession Create(string computerName, CimSessionOptions sessionOptions)
		{
			IPAddress pAddress = null;
			InstanceHandle instanceHandle = null;
			SessionHandle sessionHandle = null;
			CimSession cimSession;
			string protocol;
			DestinationOptionsHandle destinationOptionsHandle;
			string str = computerName;
			if (!string.IsNullOrEmpty(str) && IPAddress.TryParse(str, out pAddress) && pAddress.AddressFamily == AddressFamily.InterNetworkV6 && str[0] != '[')
			{
				str = string.Concat("[", str, "]");
			}
			IDisposable disposable = CimApplication.AssertNoPendingShutdown();
			using (disposable)
			{
				ApplicationHandle handle = CimApplication.Handle;
				if (sessionOptions == null)
				{
					protocol = null;
				}
				else
				{
					protocol = sessionOptions.Protocol;
				}
				string str1 = str;
				if (sessionOptions == null)
				{
					destinationOptionsHandle = null;
				}
				else
				{
					destinationOptionsHandle = sessionOptions.DestinationOptionsHandle ?? sessionOptions.DestinationOptionsHandleOnDemand;
				}
				MiResult miResult = ApplicationMethods.NewSession(handle, protocol, str1, destinationOptionsHandle, out instanceHandle, out sessionHandle);
				if (miResult != MiResult.NOT_FOUND)
				{
					CimException.ThrowIfMiResultFailure(miResult, instanceHandle);
					CimSession cimSession1 = new CimSession(sessionHandle, computerName);
					cimSession = cimSession1;
				}
				else
				{
					throw new CimException(miResult, null, instanceHandle, System.Management.Automation.Strings.UnrecognizedProtocolName);
				}
			}
			return cimSession;
		}

		public static CimAsyncResult<CimSession> CreateAsync(string computerName)
		{
			return CimSession.CreateAsync(computerName, null);
		}

		public static CimAsyncResult<CimSession> CreateAsync(string computerName, CimSessionOptions sessionOptions)
		{
			IObservable<CimSession> cimAsyncDelegatedObservable = new CimAsyncDelegatedObservable<CimSession>((IObserver<CimSession> observer) => {
				CimSession cimSession = null;
				try
				{
					cimSession = CimSession.Create(computerName, sessionOptions);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					observer.OnError(exception);
				}
				observer.OnNext(cimSession);
				observer.OnCompleted();
			}
			);
			return new CimAsyncResult<CimSession>(cimAsyncDelegatedObservable);
		}

		public CimInstance CreateInstance(string namespaceName, CimInstance instance)
		{
			return this.CreateInstance(namespaceName, instance, null);
		}

		public CimInstance CreateInstance(string namespaceName, CimInstance instance, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.CreateInstanceCore(namespaceName, instance, options, asyncCallbacksReceiver));
				return cimSyncInstanceEnumerable.SingleOrDefault<CimInstance>();
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, CimInstance instance)
		{
			return this.CreateInstanceAsync(namespaceName, instance, null);
		}

		public CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, CimInstance instance, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.CreateInstanceCore(namespaceName, instance, options, asyncCallbacksReceiver));
				return new CimAsyncResult<CimInstance>(cimAsyncInstanceObservable);
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		private OperationHandle CreateInstanceCore(string namespaceName, CimInstance instance, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.CreateInstance(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, instance.InstanceHandle, options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public void DeleteInstance(CimInstance instance)
		{
			if (instance != null)
			{
				if (instance.CimSystemProperties.Namespace != null)
				{
					this.DeleteInstance(instance.CimSystemProperties.Namespace, instance);
					return;
				}
				else
				{
					throw new ArgumentNullException("instance", System.Management.Automation.Strings.CimInstanceNamespaceIsNull);
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public void DeleteInstance(string namespaceName, CimInstance instance)
		{
			this.DeleteInstance(namespaceName, instance, null);
		}

		public void DeleteInstance(string namespaceName, CimInstance instance, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.DeleteInstanceCore(namespaceName, instance, options, asyncCallbacksReceiver));
				cimSyncInstanceEnumerable.Count<CimInstance>();
				return;
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncStatus DeleteInstanceAsync(CimInstance instance)
		{
			if (instance != null)
			{
				if (instance.CimSystemProperties.Namespace != null)
				{
					return this.DeleteInstanceAsync(instance.CimSystemProperties.Namespace, instance);
				}
				else
				{
					throw new ArgumentNullException("instance", System.Management.Automation.Strings.CimInstanceNamespaceIsNull);
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncStatus DeleteInstanceAsync(string namespaceName, CimInstance instance)
		{
			return this.DeleteInstanceAsync(namespaceName, instance, null);
		}

		public CimAsyncStatus DeleteInstanceAsync(string namespaceName, CimInstance instance, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.DeleteInstanceCore(namespaceName, instance, options, asyncCallbacksReceiver));
				return new CimAsyncStatus(cimAsyncInstanceObservable);
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		private OperationHandle DeleteInstanceCore(string namespaceName, CimInstance instance, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.DeleteInstance(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, instance.InstanceHandle, options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			lock (this._disposeThreadSafetyLock)
			{
				if (!this._disposed)
				{
					this._disposed = true;
				}
				else
				{
					return;
				}
			}
			if (disposing)
			{
				this._handle.Dispose();
				CimApplication.RemoveTracking(this);
			}
		}

		public IEnumerable<CimInstance> EnumerateAssociatedInstances(string namespaceName, CimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
		{
			return this.EnumerateAssociatedInstances(namespaceName, sourceInstance, associationClassName, resultClassName, sourceRole, resultRole, null);
		}

		public IEnumerable<CimInstance> EnumerateAssociatedInstances(string namespaceName, CimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (sourceInstance != null)
			{
				return new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateAssociatedInstancesCore(namespaceName, sourceInstance, associationClassName, resultClassName, sourceRole, resultRole, options, asyncCallbacksReceiver));
			}
			else
			{
				throw new ArgumentNullException("sourceInstance");
			}
		}

		public CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, CimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
		{
			return this.EnumerateAssociatedInstancesAsync(namespaceName, sourceInstance, associationClassName, resultClassName, sourceRole, resultRole, null);
		}

		public CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, CimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (sourceInstance != null)
			{
				IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateAssociatedInstancesCore(namespaceName, sourceInstance, associationClassName, resultClassName, sourceRole, resultRole, options, asyncCallbacksReceiver));
				return new CimAsyncMultipleResults<CimInstance>(cimAsyncInstanceObservable);
			}
			else
			{
				throw new ArgumentNullException("sourceInstance");
			}
		}

		private OperationHandle EnumerateAssociatedInstancesCore(string namespaceName, CimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.AssociatorInstances(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, sourceInstance.InstanceHandle, associationClassName, resultClassName, sourceRole, resultRole, options.GetKeysOnly(), options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public IEnumerable<CimClass> EnumerateClasses(string namespaceName)
		{
			return this.EnumerateClasses(namespaceName, null, null);
		}

		public IEnumerable<CimClass> EnumerateClasses(string namespaceName, string className)
		{
			return this.EnumerateClasses(namespaceName, className, null);
		}

		public IEnumerable<CimClass> EnumerateClasses(string namespaceName, string className, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			return new CimSyncClassEnumerable(options, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateClassesCore(namespaceName, className, options, asyncCallbacksReceiver));
		}

		public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName)
		{
			return this.EnumerateClassesAsync(namespaceName, null, null);
		}

		public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className)
		{
			return this.EnumerateClassesAsync(namespaceName, className, null);
		}

		public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			IObservable<CimClass> cimAsyncClassObservable = new CimAsyncClassObservable(options, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateClassesCore(namespaceName, className, options, asyncCallbacksReceiver));
			return new CimAsyncMultipleResults<CimClass>(cimAsyncClassObservable);
		}

		private OperationHandle EnumerateClassesCore(string namespaceName, string className, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.EnumerateClasses(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, className, options.GetClassNamesOnly(), options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public IEnumerable<CimInstance> EnumerateInstances(string namespaceName, string className)
		{
			return this.EnumerateInstances(namespaceName, className, null);
		}

		public IEnumerable<CimInstance> EnumerateInstances(string namespaceName, string className, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			return new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateInstancesCore(namespaceName, className, options, asyncCallbacksReceiver));
		}

		public CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className)
		{
			return this.EnumerateInstancesAsync(namespaceName, className, null);
		}

		public CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateInstancesCore(namespaceName, className, options, asyncCallbacksReceiver));
			return new CimAsyncMultipleResults<CimInstance>(cimAsyncInstanceObservable);
		}

		private OperationHandle EnumerateInstancesCore(string namespaceName, string className, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.EnumerateInstances(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, className, options.GetKeysOnly(), options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public IEnumerable<CimInstance> EnumerateReferencingInstances(string namespaceName, CimInstance sourceInstance, string associationClassName, string sourceRole)
		{
			return this.EnumerateReferencingInstances(namespaceName, sourceInstance, associationClassName, sourceRole, null);
		}

		public IEnumerable<CimInstance> EnumerateReferencingInstances(string namespaceName, CimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (sourceInstance != null)
			{
				return new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateReferencingInstancesCore(namespaceName, sourceInstance, associationClassName, sourceRole, options, asyncCallbacksReceiver));
			}
			else
			{
				throw new ArgumentNullException("sourceInstance");
			}
		}

		public CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, CimInstance sourceInstance, string associationClassName, string sourceRole)
		{
			return this.EnumerateReferencingInstancesAsync(namespaceName, sourceInstance, associationClassName, sourceRole, null);
		}

		public CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, CimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (sourceInstance != null)
			{
				IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.EnumerateReferencingInstancesCore(namespaceName, sourceInstance, associationClassName, sourceRole, options, asyncCallbacksReceiver));
				return new CimAsyncMultipleResults<CimInstance>(cimAsyncInstanceObservable);
			}
			else
			{
				throw new ArgumentNullException("sourceInstance");
			}
		}

		private OperationHandle EnumerateReferencingInstancesCore(string namespaceName, CimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.ReferenceInstances(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, sourceInstance.InstanceHandle, associationClassName, sourceRole, options.GetKeysOnly(), options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public CimClass GetClass(string namespaceName, string className)
		{
			return this.GetClass(namespaceName, className, null);
		}

		public CimClass GetClass(string namespaceName, string className, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			IEnumerable<CimClass> cimSyncClassEnumerable = new CimSyncClassEnumerable(options, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.GetClassCore(namespaceName, className, options, asyncCallbacksReceiver));
			return cimSyncClassEnumerable.Single<CimClass>();
		}

		public CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className)
		{
			return this.GetClassAsync(namespaceName, className, null);
		}

		public CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			IObservable<CimClass> cimAsyncClassObservable = new CimAsyncClassObservable(options, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.GetClassCore(namespaceName, className, options, asyncCallbacksReceiver));
			return new CimAsyncResult<CimClass>(cimAsyncClassObservable);
		}

		private OperationHandle GetClassCore(string namespaceName, string className, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.GetClass(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, className, options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public CimInstance GetInstance(string namespaceName, CimInstance instanceId)
		{
			return this.GetInstance(namespaceName, instanceId, null);
		}

		public CimInstance GetInstance(string namespaceName, CimInstance instanceId, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instanceId != null)
			{
				IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.GetInstanceCore(namespaceName, instanceId, options, asyncCallbacksReceiver));
				return cimSyncInstanceEnumerable.Single<CimInstance>();
			}
			else
			{
				throw new ArgumentNullException("instanceId");
			}
		}

		public CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, CimInstance instanceId)
		{
			return this.GetInstanceAsync(namespaceName, instanceId, null);
		}

		public CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, CimInstance instanceId, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instanceId != null)
			{
				IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.GetInstanceCore(namespaceName, instanceId, options, asyncCallbacksReceiver));
				return new CimAsyncResult<CimInstance>(cimAsyncInstanceObservable);
			}
			else
			{
				throw new ArgumentNullException("instanceId");
			}
		}

		private OperationHandle GetInstanceCore(string namespaceName, CimInstance instanceId, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.GetInstance(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, instanceId.InstanceHandle, options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public CimMethodResult InvokeMethod(CimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
		{
			if (instance != null)
			{
				if (instance.CimSystemProperties.Namespace != null)
				{
					return this.InvokeMethod(instance.CimSystemProperties.Namespace, instance, methodName, methodParameters);
				}
				else
				{
					throw new ArgumentNullException("instance", System.Management.Automation.Strings.CimInstanceNamespaceIsNull);
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimMethodResult InvokeMethod(string namespaceName, CimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
		{
			return this.InvokeMethod(namespaceName, instance, methodName, methodParameters, null);
		}

		public CimMethodResult InvokeMethod(string namespaceName, CimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				if (!string.IsNullOrWhiteSpace(methodName))
				{
					IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.InvokeMethodCore(namespaceName, instance.CimSystemProperties.ClassName, instance, methodName, methodParameters, options, asyncCallbacksReceiver));
					CimInstance cimInstance = cimSyncInstanceEnumerable.SingleOrDefault<CimInstance>();
					if (cimInstance == null)
					{
						return null;
					}
					else
					{
						return new CimMethodResult(cimInstance);
					}
				}
				else
				{
					throw new ArgumentNullException("methodName");
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
		{
			return this.InvokeMethod(namespaceName, className, methodName, methodParameters, null);
		}

		public CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (!string.IsNullOrWhiteSpace(methodName))
			{
				IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.InvokeMethodCore(namespaceName, className, null, methodName, methodParameters, options, asyncCallbacksReceiver));
				CimInstance cimInstance = cimSyncInstanceEnumerable.SingleOrDefault<CimInstance>();
				if (cimInstance == null)
				{
					return null;
				}
				else
				{
					return new CimMethodResult(cimInstance);
				}
			}
			else
			{
				throw new ArgumentNullException("methodName");
			}
		}

		public CimAsyncResult<CimMethodResult> InvokeMethodAsync(CimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
		{
			if (instance != null)
			{
				if (instance.CimSystemProperties.Namespace != null)
				{
					return this.InvokeMethodAsync(instance.CimSystemProperties.Namespace, instance, methodName, methodParameters);
				}
				else
				{
					throw new ArgumentNullException("instance", System.Management.Automation.Strings.CimInstanceNamespaceIsNull);
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, CimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
		{
			IObservable<CimMethodResultBase> observable = this.InvokeMethodAsync(namespaceName, instance, methodName, methodParameters, null);
			IObservable<CimMethodResult> convertingObservable = new ConvertingObservable<CimMethodResultBase, CimMethodResult>(observable);
			return new CimAsyncResult<CimMethodResult>(convertingObservable);
		}

		public CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, CimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				if (!string.IsNullOrWhiteSpace(methodName))
				{
					IObservable<CimMethodResultBase> cimAsyncMethodResultObservable = new CimAsyncMethodResultObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.InvokeMethodCore(namespaceName, instance.CimSystemProperties.ClassName, instance, methodName, methodParameters, options, asyncCallbacksReceiver));
					return new CimAsyncMultipleResults<CimMethodResultBase>(cimAsyncMethodResultObservable);
				}
				else
				{
					throw new ArgumentNullException("methodName");
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
		{
			IObservable<CimMethodResultBase> observable = this.InvokeMethodAsync(namespaceName, className, methodName, methodParameters, null);
			IObservable<CimMethodResult> convertingObservable = new ConvertingObservable<CimMethodResultBase, CimMethodResult>(observable);
			return new CimAsyncResult<CimMethodResult>(convertingObservable);
		}

		public CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (!string.IsNullOrWhiteSpace(methodName))
			{
				IObservable<CimMethodResultBase> cimAsyncMethodResultObservable = new CimAsyncMethodResultObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.InvokeMethodCore(namespaceName, className, null, methodName, methodParameters, options, asyncCallbacksReceiver));
				return new CimAsyncMultipleResults<CimMethodResultBase>(cimAsyncMethodResultObservable);
			}
			else
			{
				throw new ArgumentNullException("methodName");
			}
		}

		private OperationHandle InvokeMethodCore(string namespaceName, string className, CimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			InstanceHandle instanceHandle;
			InstanceHandle instanceHandleForMethodInvocation;
			SessionHandle sessionHandle = this._handle;
			MiOperationFlags operationFlags = options.GetOperationFlags();
			OperationOptionsHandle operationOptionsHandle = options.GetOperationOptionsHandle();
			string str = namespaceName;
			string str1 = className;
			string str2 = methodName;
			if (instance != null)
			{
				instanceHandle = instance.InstanceHandle;
			}
			else
			{
				instanceHandle = null;
			}
			if (methodParameters != null)
			{
				instanceHandleForMethodInvocation = methodParameters.InstanceHandleForMethodInvocation;
			}
			else
			{
				instanceHandleForMethodInvocation = null;
			}
			SessionMethods.Invoke(sessionHandle, operationFlags, operationOptionsHandle, str, str1, str2, instanceHandle, instanceHandleForMethodInvocation, options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public CimInstance ModifyInstance(CimInstance instance)
		{
			if (instance != null)
			{
				if (instance.CimSystemProperties.Namespace != null)
				{
					return this.ModifyInstance(instance.CimSystemProperties.Namespace, instance);
				}
				else
				{
					throw new ArgumentNullException("instance", System.Management.Automation.Strings.CimInstanceNamespaceIsNull);
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimInstance ModifyInstance(string namespaceName, CimInstance instance)
		{
			return this.ModifyInstance(namespaceName, instance, null);
		}

		public CimInstance ModifyInstance(string namespaceName, CimInstance instance, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.ModifyInstanceCore(namespaceName, instance, options, asyncCallbacksReceiver));
				return cimSyncInstanceEnumerable.SingleOrDefault<CimInstance>();
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncResult<CimInstance> ModifyInstanceAsync(CimInstance instance)
		{
			if (instance != null)
			{
				if (instance.CimSystemProperties.Namespace != null)
				{
					return this.ModifyInstanceAsync(instance.CimSystemProperties.Namespace, instance);
				}
				else
				{
					throw new ArgumentNullException("instance", System.Management.Automation.Strings.CimInstanceNamespaceIsNull);
				}
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		public CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, CimInstance instance)
		{
			return this.ModifyInstanceAsync(namespaceName, instance, null);
		}

		public CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, CimInstance instance, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (instance != null)
			{
				IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.ModifyInstanceCore(namespaceName, instance, options, asyncCallbacksReceiver));
				return new CimAsyncResult<CimInstance>(cimAsyncInstanceObservable);
			}
			else
			{
				throw new ArgumentNullException("instance");
			}
		}

		private OperationHandle ModifyInstanceCore(string namespaceName, CimInstance instance, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.ModifyInstance(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, instance.InstanceHandle, options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public IEnumerable<CimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression)
		{
			return this.QueryInstances(namespaceName, queryDialect, queryExpression, null);
		}

		public IEnumerable<CimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (!string.IsNullOrWhiteSpace(queryDialect))
			{
				if (!string.IsNullOrWhiteSpace(queryExpression))
				{
					return new CimSyncInstanceEnumerable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.QueryInstancesCore(namespaceName, queryDialect, queryExpression, options, asyncCallbacksReceiver));
				}
				else
				{
					throw new ArgumentNullException("queryExpression");
				}
			}
			else
			{
				throw new ArgumentNullException("queryDialect");
			}
		}

		public CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression)
		{
			return this.QueryInstancesAsync(namespaceName, queryDialect, queryExpression, null);
		}

		public CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options)
		{
			this.AssertNotDisposed();
			if (!string.IsNullOrWhiteSpace(queryDialect))
			{
				if (!string.IsNullOrWhiteSpace(queryExpression))
				{
					IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(options, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.QueryInstancesCore(namespaceName, queryDialect, queryExpression, options, asyncCallbacksReceiver));
					return new CimAsyncMultipleResults<CimInstance>(cimAsyncInstanceObservable);
				}
				else
				{
					throw new ArgumentNullException("queryExpression");
				}
			}
			else
			{
				throw new ArgumentNullException("queryDialect");
			}
		}

		private OperationHandle QueryInstancesCore(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.QueryInstances(this._handle, options.GetOperationFlags(), options.GetOperationOptionsHandle(), namespaceName, queryDialect, queryExpression, options.GetKeysOnly(), options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression)
		{
			return this.Subscribe(namespaceName, queryDialect, queryExpression, null, null);
		}

		public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions)
		{
			return this.Subscribe(namespaceName, queryDialect, queryExpression, operationOptions, null);
		}

		public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options)
		{
			return this.Subscribe(namespaceName, queryDialect, queryExpression, null, options);
		}

		public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options)
		{
			this.AssertNotDisposed();
			if (!string.IsNullOrWhiteSpace(queryDialect))
			{
				if (!string.IsNullOrWhiteSpace(queryExpression))
				{
					return new CimSyncIndicationEnumerable(operationOptions, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.SubscribeCore(namespaceName, queryDialect, queryExpression, operationOptions, options, asyncCallbacksReceiver));
				}
				else
				{
					throw new ArgumentNullException("queryExpression");
				}
			}
			else
			{
				throw new ArgumentNullException("queryDialect");
			}
		}

		public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression)
		{
			return this.SubscribeAsync(namespaceName, queryDialect, queryExpression, null, null);
		}

		public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions)
		{
			return this.SubscribeAsync(namespaceName, queryDialect, queryExpression, operationOptions, null);
		}

		public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options)
		{
			return this.SubscribeAsync(namespaceName, queryDialect, queryExpression, null, options);
		}

		public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options)
		{
			this.AssertNotDisposed();
			if (!string.IsNullOrWhiteSpace(queryDialect))
			{
				if (!string.IsNullOrWhiteSpace(queryExpression))
				{
					IObservable<CimSubscriptionResult> cimAsyncIndicationObservable = new CimAsyncIndicationObservable(operationOptions, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.SubscribeCore(namespaceName, queryDialect, queryExpression, operationOptions, options, asyncCallbacksReceiver));
					return new CimAsyncMultipleResults<CimSubscriptionResult>(cimAsyncIndicationObservable);
				}
				else
				{
					throw new ArgumentNullException("queryExpression");
				}
			}
			else
			{
				throw new ArgumentNullException("queryDialect");
			}
		}

		private OperationHandle SubscribeCore(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.Subscribe(this._handle, operationOptions.GetOperationFlags(), operationOptions.GetOperationOptionsHandle(), namespaceName, queryDialect, queryExpression, options.GetSubscriptionDeliveryOptionsHandle(), operationOptions.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public bool TestConnection()
		{
			CimInstance cimInstance = null;
			CimException cimException = null;
			return this.TestConnection(out cimInstance, out cimException);
		}

		public bool TestConnection(out CimInstance instance, out CimException exception)
		{
			this.AssertNotDisposed();
			bool flag = true;
			instance = null;
			exception = null;
			IEnumerable<CimInstance> cimSyncInstanceEnumerable = new CimSyncInstanceEnumerable(null, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.TestConnectionCore(null, asyncCallbacksReceiver));
			try
			{
				instance = cimSyncInstanceEnumerable.SingleOrDefault<CimInstance>();
			}
			catch (CimException cimException1)
			{
				CimException cimException = cimException1;
				exception = cimException;
				flag = false;
			}
			return flag;
		}

		public CimAsyncResult<CimInstance> TestConnectionAsync()
		{
			this.AssertNotDisposed();
			IObservable<CimInstance> cimAsyncInstanceObservable = new CimAsyncInstanceObservable(null, this.InstanceId, this.ComputerName, (CimAsyncCallbacksReceiverBase asyncCallbacksReceiver) => this.TestConnectionCore(null, asyncCallbacksReceiver));
			return new CimAsyncResult<CimInstance>(cimAsyncInstanceObservable);
		}

		private OperationHandle TestConnectionCore(CimOperationOptions options, CimAsyncCallbacksReceiverBase asyncCallbacksReceiver)
		{
			OperationHandle operationHandle = null;
			SessionMethods.TestConnection(this._handle, options.GetOperationFlags(), options.GetOperationCallbacks(asyncCallbacksReceiver), out operationHandle);
			return operationHandle;
		}

		public override string ToString()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			string cimSessionToString = System.Management.Automation.Strings.CimSessionToString;
			object[] objArray = new object[1];
			object[] objArray1 = objArray;
			int num = 0;
			string computerName = this.ComputerName;
			object obj = computerName;
			if (computerName == null)
			{
				obj = ".";
			}
			objArray1[num] = obj;
			string str = string.Format(invariantCulture, cimSessionToString, objArray);
			return str;
		}

		private class CloseAsyncImpersonationWorker : IDisposable
		{
			private ExecutionContext _executionContext;

			private IObserver<object> _wrappedObserver;

			internal CloseAsyncImpersonationWorker(IObserver<object> wrappedObserver)
			{
				this._executionContext = ExecutionContext.Capture();
				this._wrappedObserver = wrappedObserver;
			}

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected void Dispose(bool disposing)
			{
				if (disposing)
				{
					this._executionContext.Dispose();
				}
			}

			internal void OnCompleted()
			{
				ExecutionContext.Run(this._executionContext, new ContextCallback(this.OnCompletedCore), null);
				this.Dispose();
			}

			private void OnCompletedCore(object state)
			{
				this._wrappedObserver.OnCompleted();
			}
		}
	}
}