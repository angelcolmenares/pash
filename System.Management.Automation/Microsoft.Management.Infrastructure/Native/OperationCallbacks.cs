namespace Microsoft.Management.Infrastructure.Native
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class OperationCallbacks
    {
        private ClassCallbackDelegate _ClassCallback;
        private IndicationResultCallbackDelegate _IndicationResultCallback;
        private InstanceResultCallbackDelegate _InstanceResultCallback;
        private InternalErrorCallbackDelegate _InternalErrorCallback;
        private object _ManagedOperationContext;
        private PromptUserCallbackDelegate _PromptUserCallback;
        private StreamedParameterCallbackDelegate _StreamedParameterCallback;
        private WriteErrorCallbackDelegate _WriteErrorCallback;
        private WriteMessageCallbackDelegate _WriteMessageCallback;
        private WriteProgressCallbackDelegate _WriteProgressCallback;
        private static Action<Action, Func<Exception, bool>, Action<Exception>> userFilteredExceptionHandler;

        static OperationCallbacks()
        {
            AssemblyName name = new AssemblyName("Microsoft.Management.Infrastructure.UserFilteredExceptionHandling");
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            Type[] types = new Type[] { typeof(DebuggableAttribute.DebuggingModes) };
            object[] constructorArgs = new object[] { DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints };
            builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(DebuggableAttribute).GetConstructor(types), constructorArgs));
            PropertyInfo[] namedProperties = new PropertyInfo[] { typeof(RuntimeCompatibilityAttribute).GetProperty("WrapNonExceptionThrows") };
            object[] propertyValues = new object[] { true };
            builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(RuntimeCompatibilityAttribute).GetConstructor(new Type[0]), new object[0], namedProperties, propertyValues));
            TypeBuilder builder2 = builder.DefineDynamicModule(name.Name).DefineType("UserFilteredExceptionHandling", TypeAttributes.Public);
            Type[] parameterTypes = new Type[] { typeof(Action), typeof(Func<Exception, bool>), typeof(Action<Exception>) };
            ILGenerator iLGenerator = builder2.DefineMethod("InvokeWithUserFilteredExceptionHandler", MethodAttributes.Static | MethodAttributes.Public, null, parameterTypes).GetILGenerator();
            iLGenerator.DeclareLocal(typeof(Exception));
            iLGenerator.BeginExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.EmitCall(OpCodes.Callvirt, typeof(Action).GetMethod("Invoke"), null);
            iLGenerator.BeginExceptFilterBlock();
            iLGenerator.Emit(OpCodes.Castclass, typeof(Exception));
            iLGenerator.Emit(OpCodes.Stloc_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.EmitCall(OpCodes.Callvirt, typeof(Func<Exception, bool>).GetMethod("Invoke"), null);
            iLGenerator.BeginCatchBlock(null);
            iLGenerator.Emit(OpCodes.Castclass, typeof(Exception));
            iLGenerator.Emit(OpCodes.Stloc_0);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.EmitCall(OpCodes.Callvirt, typeof(Action<Exception>).GetMethod("Invoke"), null);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ret);
            MethodInfo method = builder2.CreateType().GetMethod("InvokeWithUserFilteredExceptionHandler");
            userFilteredExceptionHandler = (Action<Action, Func<Exception, bool>, Action<Exception>>) Delegate.CreateDelegate(typeof(Action<Action, Func<Exception, bool>, Action<Exception>>), method);
        }
        
        /*
        internal static unsafe void ClassAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, _MI_Class pmiClass, byte moreResults, _MI_Result resultCode, ushort errorString, object pmiErrorDetails, _MI_Result modopt(CallConvCdecl) *(_MI_Operation*) resultAcknowledgement)
        {
            new ExceptionSafeClassCallback(pmiOperation, callbackContext, pmiClass, moreResults, resultCode, errorString, pmiErrorDetails, resultAcknowledgement).InvokeUserCallbackAndCatchInternalErrors();
        }

        internal static unsafe void IndicationAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, object pmiInstance, ushort bookmark, ushort machineID, byte moreResults, _MI_Result resultCode, ushort errorString, object pmiErrorDetails, _MI_Result modopt(CallConvCdecl) *(_MI_Operation*) resultAcknowledgement)
        {
            new ExceptionSafeIndicationCallback(pmiOperation, callbackContext, pmiInstance, bookmark, machineID, moreResults, resultCode, errorString, pmiErrorDetails, resultAcknowledgement).InvokeUserCallbackAndCatchInternalErrors();
        }

        internal static unsafe void InstanceResultAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, object pmiInstance, byte moreResults, _MI_Result resultCode, ushort errorString, object pmiErrorDetails, _MI_Result modopt(CallConvCdecl) *(_MI_Operation*) resultAcknowledgement)
        {
            new ExceptionSafeInstanceResultCallback(pmiOperation, callbackContext, pmiInstance, moreResults, resultCode, errorString, pmiErrorDetails, resultAcknowledgement).InvokeUserCallbackAndCatchInternalErrors();
        }
        */

        internal static void InvokeWithUserFilteredExceptionHandler(Action tryBody, Func<Exception, bool> userFilter, Action<Exception> catchBody)
        {
            userFilteredExceptionHandler(tryBody, userFilter, catchBody);
        }

        /*
        internal static unsafe void PromptUserAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, ushort wszMessage, _MI_PromptType promptType, _MI_Result modopt(CallConvCdecl) *(_MI_Operation*, _MI_OperationCallback_ResponseType) promptUserResult)
        {
            new ExceptionSafePromptUserCallback(pmiOperation, callbackContext, wszMessage, promptType, promptUserResult).InvokeUserCallbackAndCatchInternalErrors();
        }
        
        internal static unsafe void ReleaseOperationCallbacksProxy(OperationCallbacksProxy* pOperationCallbacksProxy)
        {
            long num = 0L;
            do
            {
                long num2 = num * 8L;
                void* voidPtr3 = *((void**) (num2 + pOperationCallbacksProxy));
                if (voidPtr3 != null)
                {
                    GCHandle.FromIntPtr((IntPtr) voidPtr3).Free();
                }
                void* voidPtr2 = *((void**) ((pOperationCallbacksProxy + num2) + 0x48L));
                if (voidPtr2 != null)
                {
                    GCHandle.FromIntPtr((IntPtr) voidPtr2).Free();
                }
                num += 1L;
            }
            while (num < 9L);
            void* voidPtr = *((void**) (pOperationCallbacksProxy + 0xd8L));
            if (voidPtr != null)
            {
                GCHandle.FromIntPtr((IntPtr) voidPtr).Free();
            }
            meminit(pOperationCallbacksProxy, 0, 0xe0L);
            free((void*) pOperationCallbacksProxy);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        internal unsafe bool SetMiOperationCallbacks(_MI_OperationCallbacks* pmiOperationCallbacks)
        {
            long num2 = (long) stackalloc byte[__CxxQueryExceptionSize()];
            OperationCallbacksProxy* pOperationCallbacksProxy = Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core(0xe0L);
            try
            {
                bool flag = false;
                if (this._InstanceResultCallback != null)
                {
                    InstanceResultCallbackDelegate externalCallback = this._InstanceResultCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, externalCallback, new InstanceResultAppDomainProxyDelegate(OperationCallbacks.InstanceResultAppDomainProxy), (OperationCallbackId) 0);
                    pmiOperationCallbacks[40L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeInstanceCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEBUobject@@EW4_MI_Result@@PEBG2P6A?AW47@0@Z@Z;
                    flag = true;
                }
                if (this._StreamedParameterCallback != null)
                {
                    StreamedParameterCallbackDelegate delegate8 = this._StreamedParameterCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate8, new StreamedParameterResultAppDomainProxyDelegate(OperationCallbacks.StreamedParameterResultAppDomainProxy), (OperationCallbackId) 1);
                    pmiOperationCallbacks[0x40L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeStreamedParameterResultCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEBGW4_MI_Type@@PEBT_MI_Value@@P6A?AW4_MI_Result@@0@Z@Z;
                    flag = true;
                }
                if (this._WriteMessageCallback != null)
                {
                    WriteMessageCallbackDelegate delegate7 = this._WriteMessageCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate7, new WriteMessageAppDomainProxyDelegate(OperationCallbacks.WriteMessageAppDomainProxy), (OperationCallbackId) 2);
                    pmiOperationCallbacks[0x18L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeWriteMessageCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXIPEBG@Z;
                    flag = true;
                }
                if (this._WriteProgressCallback != null)
                {
                    WriteProgressCallbackDelegate delegate6 = this._WriteProgressCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate6, new WriteProgressAppDomainProxyDelegate(OperationCallbacks.WriteProgressAppDomainProxy), (OperationCallbackId) 3);
                    pmiOperationCallbacks[0x20L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeWriteProgressCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEBG22II@Z;
                    flag = true;
                }
                if (this._WriteErrorCallback != null)
                {
                    WriteErrorCallbackDelegate delegate5 = this._WriteErrorCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate5, new WriteErrorAppDomainProxyDelegate(OperationCallbacks.WriteErrorAppDomainProxy), (OperationCallbackId) 4);
                    pmiOperationCallbacks[0x10L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeWriteErrorCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEAUobject@@P6A?AW4_MI_Result@@0W4_MI_OperationCallback_ResponseType@@@Z@Z;
                    flag = true;
                }
                if (this._PromptUserCallback != null)
                {
                    PromptUserCallbackDelegate delegate4 = this._PromptUserCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate4, new PromptUserAppDomainProxyDelegate(OperationCallbacks.PromptUserAppDomainProxy), (OperationCallbackId) 5);
                    pmiOperationCallbacks[8L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativePromptUserCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEBGW4_MI_PromptType@@P6A?AW4_MI_Result@@0W4_MI_OperationCallback_ResponseType@@@Z@Z;
                    flag = true;
                }
                if (this._IndicationResultCallback != null)
                {
                    IndicationResultCallbackDelegate delegate3 = this._IndicationResultCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate3, new IndicationAppDomainProxyDelegate(OperationCallbacks.IndicationAppDomainProxy), (OperationCallbackId) 6);
                    pmiOperationCallbacks[0x30L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeIndicationCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEBUobject@@PEBG3EW4_MI_Result@@32P6A?AW47@0@Z@Z;
                    flag = true;
                }
                if (this._ClassCallback != null)
                {
                    ClassCallbackDelegate delegate2 = this._ClassCallback;
                    StoreCallbackDelegate(pOperationCallbacksProxy, delegate2, new ClassAppDomainProxyDelegate(OperationCallbacks.ClassAppDomainProxy), (OperationCallbackId) 7);
                    pmiOperationCallbacks[0x38L] = (_MI_OperationCallbacks) __unep@?OperationCallbacks_NativeClassCallback@Native@Infrastructure@Management@Microsoft@@$$FYAXPEAU_MI_Operation@@PEAXPEBU_MI_Class@@EW4_MI_Result@@PEBGPEBUobject@@P6A?AW47@0@Z@Z;
                    flag = true;
                }
                if (flag)
                {
                    if (this._InternalErrorCallback != null)
                    {
                        GCHandle handle2 = GCHandle.Alloc(this._InternalErrorCallback);
                        GCHandle handle4 = handle2;
                        IntPtr ptr2 = GCHandle.ToIntPtr(handle2);
                        pOperationCallbacksProxy[0x40L] = (OperationCallbacksProxy) ((void*) ptr2);
                    }
                    object obj2 = this._ManagedOperationContext;
                    if (obj2 != null)
                    {
                        GCHandle handle = GCHandle.Alloc(obj2);
                        GCHandle handle3 = handle;
                        IntPtr ptr = GCHandle.ToIntPtr(handle);
                        pOperationCallbacksProxy[0xd8L] = (OperationCallbacksProxy) ((void*) ptr);
                    }
                }
                *((long*) pmiOperationCallbacks) = pOperationCallbacksProxy;
                byte num3 = 1;
                return (bool) num3;
            }
            catch when (?)
            {
                uint num = 0;
                __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num2);
                try
                {
                    try
                    {
                        ReleaseOperationCallbacksProxy(pOperationCallbacksProxy);
                        free((void*) pOperationCallbacksProxy);
                        _CxxThrowException(null, null);
                    }
                    catch when (?)
                    {
                    }
                    if (num != 0)
                    {
                        throw;
                    }
                }
                finally
                {
                    __CxxUnregisterExceptionObject((void*) num2, (int) num);
                }
            }
            return false;
        }

        private static unsafe void StoreCallbackDelegate(OperationCallbacksProxy* pOperationCallbacksProxy, Delegate externalCallback, Delegate appDomainProxyCallback, OperationCallbackId callbackId)
        {
            IntPtr ptr3 = GCHandle.ToIntPtr(GCHandle.Alloc(externalCallback));
            long num = (long) callbackId;
            (num * 8L)[(int) pOperationCallbacksProxy] = (long) ((void*) ptr3);
            IntPtr ptr2 = GCHandle.ToIntPtr(GCHandle.Alloc(appDomainProxyCallback));
            ((num + 9L) * 8L)[(int) pOperationCallbacksProxy] = (long) ((void*) ptr2);
            IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(appDomainProxyCallback);
            ((num + 0x12L) * 8L)[(int) pOperationCallbacksProxy] = (long) ((void*) functionPointerForDelegate);
        }

        internal static unsafe void StreamedParameterResultAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, ushort wszParameterName, _MI_Type miType, _MI_Value pmiParameterValue, _MI_Result modopt(CallConvCdecl) *(_MI_Operation*) resultAcknowledgement)
        {
            new ExceptionSafeStreamedParameterResultCallback(pmiOperation, callbackContext, wszParameterName, miType, pmiParameterValue, resultAcknowledgement).InvokeUserCallbackAndCatchInternalErrors();
        }

        internal static unsafe void WriteErrorAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, object* pmiInstance, _MI_Result modopt(CallConvCdecl) *(_MI_Operation*, _MI_OperationCallback_ResponseType) writeErrorResult)
        {
            new ExceptionSafeWriteErrorCallback(pmiOperation, callbackContext, pmiInstance, writeErrorResult).InvokeUserCallbackAndCatchInternalErrors();
        }

        internal static unsafe void WriteMessageAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, uint channel, ushort wszMessage)
        {
            new ExceptionSafeWriteMessageCallback(pmiOperation, callbackContext, channel, wszMessage).InvokeUserCallbackAndCatchInternalErrors();
        }

        internal static unsafe void WriteProgressAppDomainProxy(_MI_Operation* pmiOperation, void* callbackContext, ushort wszActivity, ushort wszCurrentOperation, ushort wszStatusDescription, uint percentageComplete, uint secondsRemaining)
        {
            new ExceptionSafeWriteProgressCallback(pmiOperation, callbackContext, wszActivity, wszCurrentOperation, wszStatusDescription, percentageComplete, secondsRemaining).InvokeUserCallbackAndCatchInternalErrors();
        }
        */

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal ClassCallbackDelegate ClassCallback
        {
            get
            {
                return this._ClassCallback;
            }
            set
            {
                this._ClassCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal IndicationResultCallbackDelegate IndicationResultCallback
        {
            get
            {
                return this._IndicationResultCallback;
            }
            set
            {
                this._IndicationResultCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal InstanceResultCallbackDelegate InstanceResultCallback
        {
            get
            {
                return this._InstanceResultCallback;
            }
            set
            {
                this._InstanceResultCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal InternalErrorCallbackDelegate InternalErrorCallback
        {
            get
            {
                return this._InternalErrorCallback;
            }
            set
            {
                this._InternalErrorCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp and in cs/CimOperationOptions.cs")]
        internal object ManagedOperationContext
        {
            get
            {
                return this._ManagedOperationContext;
            }
            set
            {
                this._ManagedOperationContext = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal PromptUserCallbackDelegate PromptUserCallback
        {
            get
            {
                return this._PromptUserCallback;
            }
            set
            {
                this._PromptUserCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal StreamedParameterCallbackDelegate StreamedParameterCallback
        {
            get
            {
                return this._StreamedParameterCallback;
            }
            set
            {
                this._StreamedParameterCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal WriteErrorCallbackDelegate WriteErrorCallback
        {
            get
            {
                return this._WriteErrorCallback;
            }
            set
            {
                this._WriteErrorCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal WriteMessageCallbackDelegate WriteMessageCallback
        {
            get
            {
                return this._WriteMessageCallback;
            }
            set
            {
                this._WriteMessageCallback = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="False positive from FxCop - this property is used in nativeOperationCallbacks.cpp")]
        internal WriteProgressCallbackDelegate WriteProgressCallback
        {
            get
            {
                return this._WriteProgressCallback;
            }
            set
            {
                this._WriteProgressCallback = value;
            }
        }

        internal unsafe delegate void ClassAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, object pmiClass, byte moreResults, MiResult resultCode, ushort errorString, object pmiErrorDetails, MiResult resultAcknowledgement);

        internal delegate void ClassCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, ClassHandle classHandle, [MarshalAs(UnmanagedType.U1)] bool moreResults, MiResult resultCode, string errorString, InstanceHandle errorDetails);

        internal unsafe delegate void IndicationAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, InstanceHandle pmiInstance, ushort bookmark, ushort machineID, byte moreResults, MiResult resultCode, ushort errorString, object pmiErrorDetails, MiResult resultAcknowledgement);

        internal delegate void IndicationResultCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, string bookmark, string machineID, [MarshalAs(UnmanagedType.U1)] bool moreResults, MiResult resultCode, string errorString, InstanceHandle errorDetails);

        internal unsafe delegate void InstanceResultAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, InstanceHandle pmiInstance, byte moreResults, MiResult resultCode, ushort errorString, object pmiErrorDetails, MiResult resultAcknowledgement);

        internal delegate void InstanceResultCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, [MarshalAs(UnmanagedType.U1)] bool moreResults, MiResult resultCode, string errorString, InstanceHandle errorDetails);

        internal delegate void InternalErrorCallbackDelegate(OperationCallbackProcessingContext callbackContextWhereInternalErrorOccurred, Exception exception);

        internal unsafe delegate void PromptUserAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, ushort wszMessage, MiPromptType promptType, MiResult promptUserResult);

        internal delegate void PromptUserCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, string message, MiPromptType promptType, out MIResponseType response);

        internal delegate void StreamedParameterCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, string parameterName, object parameterValue, MiType parameterType);

        internal unsafe delegate void StreamedParameterResultAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, ushort wszParameterName, MiType miType, object pmiParameterValue, MiResult resultAcknowledgement);

        internal unsafe delegate void WriteErrorAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, object pmiInstance, MiResult writeErrorResult);

        internal delegate void WriteErrorCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, out MIResponseType response);

        internal unsafe delegate void WriteMessageAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, uint channel, ushort wszMessage);

        internal delegate void WriteMessageCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, uint channel, string message);

        internal unsafe delegate void WriteProgressAppDomainProxyDelegate(_MI_Operation* pmiOperation, void* callbackContext, ushort wszActivity, ushort wszCurrentOperation, ushort wszStatusDescription, int percentageComplete, int secondsRemaining);

        internal delegate void WriteProgressCallbackDelegate(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, string activity, string currentOperation, string statusDescription, int percentageComplete, int secondsRemaining);
    }
}

