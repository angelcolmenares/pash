using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class SessionMethods
	{
		private SessionMethods()
		{

		}

		internal static unsafe void AssociatorInstances(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, InstanceHandle sourceInstance, string assocClass, string resultClass, string sourceRole, string resultRole, bool keysOnly, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			operationHandle = null;
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(sourceInstance);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(operationOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(assocClass);
							IntPtr intPtr1 = hGlobalUni1;
							try
							{
								IntPtr hGlobalUni2 = Marshal.StringToHGlobalUni(resultClass);
								IntPtr intPtr2 = hGlobalUni2;
								try
								{
									IntPtr hGlobalUni3 = Marshal.StringToHGlobalUni(sourceRole);
									IntPtr intPtr3 = hGlobalUni3;
									try
									{
										IntPtr hGlobalUni4 = Marshal.StringToHGlobalUni(resultRole);
										IntPtr intPtr4 = hGlobalUni4;
										try
										{
											_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
											void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
											if (flag)
											{
												_MIOperationCallbacksPointer = &_MIOperationCallback;
											}
											else
											{
												_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
											}
											_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
											IntPtr intPtr5 = dangerousHandleAccessor1.DangerousGetHandle();
											<Module>.?A0xf16864c4.MI_Session_AssociatorInstances((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)intPtr5, (void*)hGlobalUni1, (void*)hGlobalUni2, (void*)hGlobalUni3, (void*)hGlobalUni4, keysOnly, _MIOperationCallbacksPointer1, _MIOperationPointer);
											OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
											operationHandle = operationHandle;
											operationHandle.SetOperationCallback((long)_MIOperationCallback);
										}
										finally
										{
											if (intPtr4 != IntPtr.Zero)
											{
												Marshal.FreeHGlobal(intPtr4);
											}
										}
									}
									finally
									{
										if (intPtr3 != IntPtr.Zero)
										{
											Marshal.FreeHGlobal(intPtr3);
										}
									}
								}
								finally
								{
									if (intPtr2 != IntPtr.Zero)
									{
										Marshal.FreeHGlobal(intPtr2);
									}
								}
							}
							finally
							{
								if (intPtr1 != IntPtr.Zero)
								{
									Marshal.FreeHGlobal(intPtr1);
								}
							}
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void CreateInstance(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, InstanceHandle instanceHandle, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			IntPtr sessionPtr = sessionHandle.DangerousGetHandle ();
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle (IntPtr.Zero, true);
			operationHandle.SetOperationCallback (IntPtr.Zero.ToPointer ());
			var currentContext = new OperationCallbackProcessingContext (operationCallbacks.ManagedOperationContext);

			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(instanceHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(operationOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							IntPtr intPtr1 = dangerousHandleAccessor1.DangerousGetHandle();
							<Module>.?A0xf16864c4.MI_Session_CreateInstance((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)intPtr1, _MIOperationCallbacksPointer1, _MIOperationPointer);
							OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
							operationHandle = operationHandle;
							operationHandle.SetOperationCallback((long)_MIOperationCallback);
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void DeleteInstance(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, InstanceHandle instanceHandle, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			operationHandle = null;
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(instanceHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(operationOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							IntPtr intPtr1 = dangerousHandleAccessor1.DangerousGetHandle();
							<Module>.?A0xf16864c4.MI_Session_DeleteInstance((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)intPtr1, _MIOperationCallbacksPointer1, _MIOperationPointer);
							OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
							operationHandle = operationHandle;
							operationHandle.SetOperationCallback((long)_MIOperationCallback);
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void EnumerateClasses (SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, string className, bool classNamesOnly, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			/* TODO: Use ClassName if classNamesOnly */
			IntPtr sessionPtr = sessionHandle.DangerousGetHandle ();
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle (IntPtr.Zero, true);
			operationHandle.SetOperationCallback (IntPtr.Zero.ToPointer ());
			var currentContext = new OperationCallbackProcessingContext (operationCallbacks.ManagedOperationContext);
			int i = 0;
			var instances = CimNativeApi.QueryClasses (options, namespaceName, CimNativeApi.WQLNamespace, "SELECT * FROM Meta_Class" + (string.IsNullOrEmpty (className) ? "" : " WHERE ClassName LIKE '" + className.Replace ("*", "%") + "'"));
			int count = instances.Count ();
			if (count >= 0) {
				foreach (var obj in instances) {
					obj.SessionHandle = sessionPtr;
					IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimClass> (obj);
					ClassHandle resultHandle = new ClassHandle (instancePtr, true);
					operationCallbacks.ClassCallback.Invoke (currentContext, operationHandle, resultHandle, i < count - 1, MiResult.OK, null, null);
					i++;
				}
			} else {
				ClassHandle resultHandle = new ClassHandle (IntPtr.Zero, true);
				operationCallbacks.ClassCallback.Invoke (currentContext, operationHandle, resultHandle, false, MiResult.OK, null, null);

			}
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationOptionsHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
					&_MIOperationCallback;
					0;
					bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
					IntPtr intPtr = hGlobalUni;
					try
					{
						IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(className);
						IntPtr intPtr1 = hGlobalUni1;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							<Module>.?A0xf16864c4.MI_Session_EnumerateClasses((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)hGlobalUni1, classNamesOnly, _MIOperationCallbacksPointer1, _MIOperationPointer);
							IntPtr intPtr2 = (IntPtr)_MIOperationPointer;
							operationHandle = new OperationHandle(intPtr2, true);
						}
						finally
						{
							if (intPtr1 != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr1);
							}
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void EnumerateInstances (SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, string className, bool keysOnly, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			IntPtr sessionPtr = sessionHandle.DangerousGetHandle ();
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle (IntPtr.Zero, true);
			operationHandle.SetOperationCallback (IntPtr.Zero.ToPointer ());
			int i = 0;
			var instances = CimNativeApi.QueryInstances (options, namespaceName, CimNativeApi.WQLNamespace, "SELECT * FROM " + className, keysOnly);
			var currentContext = new OperationCallbackProcessingContext (operationCallbacks.ManagedOperationContext);
			int count = instances.Count ();
			if (count > 0) {
				foreach (var obj in instances) {
					obj.SessionHandle = sessionPtr;
					IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance> (obj);
					InstanceHandle resultHandle = new InstanceHandle (instancePtr, true);
					operationCallbacks.InstanceResultCallback.Invoke (currentContext, operationHandle, resultHandle, i < count - 1, MiResult.OK, null, null);
					i++;
				}
			} else {
				InstanceHandle resultHandle = new InstanceHandle (IntPtr.Zero, true);
				operationCallbacks.InstanceResultCallback.Invoke (currentContext, operationHandle, resultHandle, false, MiResult.NOT_FOUND, null, null);
			}
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationOptionsHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
					&_MIOperationCallback;
					0;
					bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
					IntPtr intPtr = hGlobalUni;
					try
					{
						IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(className);
						IntPtr intPtr1 = hGlobalUni1;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							<Module>.?A0xf16864c4.MI_Session_EnumerateInstances((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)hGlobalUni1, keysOnly, _MIOperationCallbacksPointer1, _MIOperationPointer);
							OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
							operationHandle = operationHandle;
							operationHandle.SetOperationCallback((long)_MIOperationCallback);
						}
						finally
						{
							if (intPtr1 != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr1);
							}
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void GetClass(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, string className, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			IntPtr sessionPtr = sessionHandle.DangerousGetHandle ();
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle(IntPtr.Zero, true);
			operationHandle.SetOperationCallback(IntPtr.Zero.ToPointer ());
			var queryDialect = CimNativeApi.WQLNamespace;
			var queryExpression = "SELECT * FROM Meta_Class WHERE ClassName = '" + className + "'";
			var instances = CimNativeApi.QueryClasses (options, namespaceName, queryDialect, queryExpression);
			var currentContext = new OperationCallbackProcessingContext (operationCallbacks.ManagedOperationContext);
			int count = instances.Count ();
			int i = 0;
			foreach (var obj in instances) {
				obj.SessionHandle = sessionPtr;
				IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimClass> (obj);
				ClassHandle resultHandle = new ClassHandle (instancePtr, true);
				operationCallbacks.ClassCallback.Invoke (currentContext, operationHandle, resultHandle, i < count - 1, MiResult.OK, null, null);
				i++;
			}

			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationOptionsHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
					&_MIOperationCallback;
					0;
					bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
					IntPtr intPtr = hGlobalUni;
					try
					{
						IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(className);
						IntPtr intPtr1 = hGlobalUni1;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							<Module>.?A0xf16864c4.MI_Session_GetClass((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)hGlobalUni1, _MIOperationCallbacksPointer1, _MIOperationPointer);
							IntPtr intPtr2 = (IntPtr)_MIOperationPointer;
							operationHandle = new OperationHandle(intPtr2, true);
						}
						finally
						{
							if (intPtr1 != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr1);
							}
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void GetInstance(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, InstanceHandle instanceHandle, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			IntPtr sessionPtr = sessionHandle.DangerousGetHandle ();
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle(IntPtr.Zero, true);
			operationHandle.SetOperationCallback(IntPtr.Zero.ToPointer ());
			NativeCimInstance obj = new NativeCimInstance();
			obj.Namespace = namespaceName;
			obj.ServerName = "localhost";
			obj.SessionHandle = sessionPtr;
			var properties = new NativeCimProperties();
			//properties.Keys = new string[] { "PSShowComputerName", "Name", "Status", "PSComputerName" };
			//properties.Values = new object[] { true, "test1", "Started", "localhost" };
			obj.Properties = NativeCimPropertiesHelper.Serialize (properties);
			IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance>(obj);
			InstanceHandle resultHandle = new InstanceHandle(instancePtr, true);
			operationCallbacks.InstanceResultCallback.Invoke (new OperationCallbackProcessingContext(operationCallbacks.ManagedOperationContext), operationHandle, resultHandle, false, MiResult.OK, null, null);

			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(instanceHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(operationOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							IntPtr intPtr1 = dangerousHandleAccessor1.DangerousGetHandle();
							<Module>.?A0xf16864c4.MI_Session_GetInstance((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)intPtr1, _MIOperationCallbacksPointer1, _MIOperationPointer);
							OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
							operationHandle = operationHandle;
							operationHandle.SetOperationCallback((long)_MIOperationCallback);
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void Invoke(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, string className, string methodName, InstanceHandle instanceHandleForTargetOfInvocation, InstanceHandle instanceHandleForMethodParameters, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle(IntPtr.Zero, true);
			operationHandle.SetOperationCallback(IntPtr.Zero.ToPointer ());
			NativeCimInstance instance = instanceHandleForTargetOfInvocation == null ? new NativeCimInstance() : CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(instanceHandleForTargetOfInvocation.DangerousGetHandle ());
			NativeCimInstance inSignature = instanceHandleForMethodParameters == null ? new NativeCimInstance() :  CimNativeApi.MarshalledObject.FromPointer<NativeCimInstance>(instanceHandleForMethodParameters.DangerousGetHandle ());
			NativeCimInstance result = CimNativeApi.InvokeMethod(options, namespaceName, className, methodName, instance, inSignature);
			IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance>(result);
			InstanceHandle resultHandle = new InstanceHandle(instancePtr, true);


			operationCallbacks.InstanceResultCallback.Invoke (new OperationCallbackProcessingContext(operationCallbacks.ManagedOperationContext), operationHandle, resultHandle, false, MiResult.OK, null, null);


			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			DangerousHandleAccessor dangerousHandleAccessor3 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationOptionsHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
					&_MIOperationCallback;
					0;
					bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
					IntPtr intPtr = hGlobalUni;
					try
					{
						IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(className);
						IntPtr intPtr1 = hGlobalUni1;
						try
						{
							IntPtr hGlobalUni2 = Marshal.StringToHGlobalUni(methodName);
							IntPtr intPtr2 = hGlobalUni2;
							try
							{
								_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
								DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(instanceHandleForTargetOfInvocation);
								try
								{
									dangerousHandleAccessor2 = dangerousHandleAccessor2;
									_MI_Instance* _MIInstancePointer = (_MI_Instance*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
									DangerousHandleAccessor dangerousHandleAccessor3 = new DangerousHandleAccessor(instanceHandleForMethodParameters);
									try
									{
										dangerousHandleAccessor3 = dangerousHandleAccessor3;
										_MI_Instance* _MIInstancePointer1 = (_MI_Instance*)((void*)dangerousHandleAccessor3.DangerousGetHandle());
										void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
										if (flag)
										{
											_MIOperationCallbacksPointer = &_MIOperationCallback;
										}
										else
										{
											_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
										}
										_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
										<Module>.?A0xf16864c4.MI_Session_Invoke((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)hGlobalUni1, (void*)hGlobalUni2, _MIInstancePointer, _MIInstancePointer1, _MIOperationCallbacksPointer1, _MIOperationPointer);
										OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
										operationHandle = operationHandle;
										operationHandle.SetOperationCallback((long)_MIOperationCallback);
									}
									dangerousHandleAccessor3.Dispose();
								}
								dangerousHandleAccessor2.Dispose();
							}
							finally
							{
								if (intPtr2 != IntPtr.Zero)
								{
									Marshal.FreeHGlobal(intPtr2);
								}
							}
						}
						finally
						{
							if (intPtr1 != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr1);
							}
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void ModifyInstance(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, InstanceHandle instanceHandle, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			operationHandle = null;
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(instanceHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(operationOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
							void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
							if (flag)
							{
								_MIOperationCallbacksPointer = &_MIOperationCallback;
							}
							else
							{
								_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
							}
							_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
							IntPtr intPtr1 = dangerousHandleAccessor1.DangerousGetHandle();
							<Module>.?A0xf16864c4.MI_Session_ModifyInstance((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)intPtr1, _MIOperationCallbacksPointer1, _MIOperationPointer);
							OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
							operationHandle = operationHandle;
							operationHandle.SetOperationCallback((long)_MIOperationCallback);
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="keysOnly")]
		internal static unsafe void QueryInstances (SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, string queryDialect, string queryExpression, bool keysOnly, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			IntPtr sessionPtr = sessionHandle.DangerousGetHandle ();
			NativeDestinationOptions options = CimNativeApi.GetDestinationOptions (sessionHandle);
			operationHandle = new OperationHandle (IntPtr.Zero, true);
			operationHandle.SetOperationCallback (IntPtr.Zero.ToPointer ());
			int i = 0;
			var instances = CimNativeApi.QueryInstances (options, namespaceName, queryDialect, queryExpression, keysOnly);
			var currentContext = new OperationCallbackProcessingContext (operationCallbacks.ManagedOperationContext);
			int count = instances.Count ();
			if (count > 0) {
				foreach (var obj in instances) {
					obj.ClassName = null;
					obj.SessionHandle = sessionPtr;
					IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimInstance> (obj);
					InstanceHandle resultHandle = new InstanceHandle (instancePtr, true);
					operationCallbacks.InstanceResultCallback.Invoke (currentContext, operationHandle, resultHandle, i < count - 1, MiResult.OK, null, null);
					i++;
				}
			} else {
				InstanceHandle resultHandle = new InstanceHandle (IntPtr.Zero, true);
				operationCallbacks.InstanceResultCallback.Invoke (currentContext, operationHandle, resultHandle, false, MiResult.NOT_FOUND, null, null);

			}
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationOptionsHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
					&_MIOperationCallback;
					0;
					bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
					IntPtr intPtr = hGlobalUni;
					try
					{
						IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(queryDialect);
						IntPtr intPtr1 = hGlobalUni1;
						try
						{
							IntPtr hGlobalUni2 = Marshal.StringToHGlobalUni(queryExpression);
							IntPtr intPtr2 = hGlobalUni2;
							try
							{
								_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
								void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
								if (flag)
								{
									_MIOperationCallbacksPointer = &_MIOperationCallback;
								}
								else
								{
									_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
								}
								_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
								<Module>.?A0xf16864c4.MI_Session_QueryInstances((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)hGlobalUni1, (void*)hGlobalUni2, _MIOperationCallbacksPointer1, _MIOperationPointer);
								OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
								operationHandle = operationHandle;
								operationHandle.SetOperationCallback((long)_MIOperationCallback);
							}
							finally
							{
								if (intPtr2 != IntPtr.Zero)
								{
									Marshal.FreeHGlobal(intPtr2);
								}
							}
						}
						finally
						{
							if (intPtr1 != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr1);
							}
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void ReferenceInstances(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, InstanceHandle sourceInstance, string associationClassName, string sourceRole, bool keysOnly, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			operationHandle = null;
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(sourceInstance);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(operationOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(associationClassName);
							IntPtr intPtr1 = hGlobalUni1;
							try
							{
								IntPtr hGlobalUni2 = Marshal.StringToHGlobalUni(sourceRole);
								IntPtr intPtr2 = hGlobalUni2;
								try
								{
									_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
									void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
									if (flag)
									{
										_MIOperationCallbacksPointer = &_MIOperationCallback;
									}
									else
									{
										_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
									}
									_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
									IntPtr intPtr3 = dangerousHandleAccessor1.DangerousGetHandle();
									<Module>.?A0xf16864c4.MI_Session_ReferenceInstances((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)intPtr3, (void*)hGlobalUni1, (void*)hGlobalUni2, keysOnly, _MIOperationCallbacksPointer1, _MIOperationPointer);
									OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
									operationHandle = operationHandle;
									operationHandle.SetOperationCallback((long)_MIOperationCallback);
								}
								finally
								{
									if (intPtr2 != IntPtr.Zero)
									{
										Marshal.FreeHGlobal(intPtr2);
									}
								}
							}
							finally
							{
								if (intPtr1 != IntPtr.Zero)
								{
									Marshal.FreeHGlobal(intPtr1);
								}
							}
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void Subscribe(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationOptionsHandle operationOptionsHandle, string namespaceName, string queryDialect, string queryExpression, SubscriptionDeliveryOptionsHandle subscriptionDeliveryOptionsHandle, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			operationHandle = null;
			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			DangerousHandleAccessor dangerousHandleAccessor2 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationOptionsHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
					_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
					DangerousHandleAccessor dangerousHandleAccessor2 = new DangerousHandleAccessor(subscriptionDeliveryOptionsHandle);
					try
					{
						dangerousHandleAccessor2 = dangerousHandleAccessor2;
						_MI_SubscriptionDeliveryOptions* _MISubscriptionDeliveryOptionsPointer = (_MI_SubscriptionDeliveryOptions*)((void*)dangerousHandleAccessor2.DangerousGetHandle());
						&_MIOperationCallback;
						0;
						bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(namespaceName);
						IntPtr intPtr = hGlobalUni;
						try
						{
							IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(queryDialect);
							IntPtr intPtr1 = hGlobalUni1;
							try
							{
								IntPtr hGlobalUni2 = Marshal.StringToHGlobalUni(queryExpression);
								IntPtr intPtr2 = hGlobalUni2;
								try
								{
									_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
									void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
									if (flag)
									{
										_MIOperationCallbacksPointer = &_MIOperationCallback;
									}
									else
									{
										_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
									}
									_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
									<Module>.?A0xf16864c4.MI_Session_Subscribe((_MI_Session*)voidPointer, operationFlags, _MIOperationOptionsPointer, (void*)hGlobalUni, (void*)hGlobalUni1, (void*)hGlobalUni2, _MISubscriptionDeliveryOptionsPointer, _MIOperationCallbacksPointer1, _MIOperationPointer);
									OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
									operationHandle = operationHandle;
									operationHandle.SetOperationCallback((long)_MIOperationCallback);
								}
								finally
								{
									if (intPtr2 != IntPtr.Zero)
									{
										Marshal.FreeHGlobal(intPtr2);
									}
								}
							}
							finally
							{
								if (intPtr1 != IntPtr.Zero)
								{
									Marshal.FreeHGlobal(intPtr1);
								}
							}
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
					dangerousHandleAccessor2.Dispose();
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}

		internal static unsafe void TestConnection(SessionHandle sessionHandle, MiOperationFlags operationFlags, OperationCallbacks operationCallbacks, out OperationHandle operationHandle)
		{
			operationHandle = new OperationHandle(IntPtr.Zero, true);
			operationHandle.SetOperationCallback(IntPtr.Zero.ToPointer ());
			NativeCimSession session = CimNativeApi.MarshalledObject.FromPointer<NativeCimSession>(sessionHandle.DangerousGetHandle ());
			IntPtr instancePtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimSession>(session);
			InstanceHandle testInstance = new InstanceHandle(instancePtr, false);
			var context = new OperationCallbackProcessingContext(operationCallbacks.ManagedOperationContext);
			operationCallbacks.InstanceResultCallback.Invoke (context, operationHandle, testInstance, false, MiResult.OK, null, null);

			/*
			_MI_OperationCallbacks _MIOperationCallback;
			_MI_OperationCallbacks* _MIOperationCallbacksPointer;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			operationHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(sessionHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				&_MIOperationCallback;
				0;
				bool flag = operationCallbacks.SetMiOperationCallbacks(ref _MIOperationCallback);
				_MI_Operation* _MIOperationPointer = (_MI_Operation*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
				void* voidPointer = (void*)dangerousHandleAccessor.DangerousGetHandle();
				if (flag)
				{
					_MIOperationCallbacksPointer = &_MIOperationCallback;
				}
				else
				{
					_MIOperationCallbacksPointer = (_MI_OperationCallbacks*)((long)0);
				}
				_MI_OperationCallbacks* _MIOperationCallbacksPointer1 = _MIOperationCallbacksPointer;
				<Module>.?A0xf16864c4.MI_Session_TestConnection((_MI_Session*)voidPointer, operationFlags, _MIOperationCallbacksPointer1, _MIOperationPointer);
				OperationHandle operationHandle = new OperationHandle((IntPtr)_MIOperationPointer, true);
				operationHandle = operationHandle;
				DangerousHandleAccessor dangerousHandleAccessor1 = new DangerousHandleAccessor(operationHandle);
				try
				{
					dangerousHandleAccessor1 = dangerousHandleAccessor1;
				}
				dangerousHandleAccessor1.Dispose();
			}
			dangerousHandleAccessor.Dispose();
			*/
		}
	}
}