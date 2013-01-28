namespace Microsoft.Management.Infrastructure.Native
{
    using Microsoft.Management.Infrastructure.Internal;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Diagnostics;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading;

    internal class OperationHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private IntPtr handleToSecurityTokenUsedForCreation;
        private Action m_ExtraFinalizationAction;
        private unsafe void* m_OperationCallback;

        internal unsafe OperationHandle(IntPtr handle, [MarshalAs(UnmanagedType.U1)] bool ownsHandle) : base(ownsHandle)
        {
            try
            {
                base.handle = handle;
                this.m_OperationCallback = IntPtr.Zero.ToPointer();
                if (ownsHandle)
                {
                    IntPtr currentSecurityToken = Helpers.GetCurrentSecurityToken();
                    this.handleToSecurityTokenUsedForCreation = currentSecurityToken;
                }
                else
                {
                    this.handleToSecurityTokenUsedForCreation = IntPtr.Zero;
                }
            }
            catch
            {
                base.Dispose(true);
            }
        }

        [Conditional("DEBUG")]
        internal unsafe void AssertValidInternalState()
        {
            void* handle = (void*) this.handle;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            if (this.m_ExtraFinalizationAction != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(OperationHandle.ReleaseHandleWhenSynchronousEnumeratorWasAbandoned), this);
                return true;
            }
            return this.ReleaseHandleCore();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        private unsafe bool ReleaseHandleCore()
        {
            if (this.m_ExtraFinalizationAction != null)
            {
                this.m_ExtraFinalizationAction();
                this.m_ExtraFinalizationAction = null;
            }
            /*
            _MI_Operation* handle = (_MI_Operation*) this.handle;
            base.handle = IntPtr.Zero;
            _MI_Result result = ?A0xf5a90918.MI_Operation_Close(handle);
            meminit(handle, 0, 0x18L);
            free((void*) handle);
            void* operationCallback = this.m_OperationCallback;
            if (operationCallback != null)
            {
                OperationCallbacks.ReleaseOperationCallbacksProxy((OperationCallbacksProxy*) operationCallback);
                this.m_OperationCallback = 0L;
            }
            WindowsImpersonationContext introduced3 = WindowsIdentity.Impersonate(this.handleToSecurityTokenUsedForCreation);
            if (CloseHandle((void*) this.handleToSecurityTokenUsedForCreation) != 0)
            {
                this.handleToSecurityTokenUsedForCreation = IntPtr.Zero;
            }
            else
            {
                result = (result == ((_MI_Result) 0)) ? ((_MI_Result) 1) : result;
            }
            introduced3.Undo();
            return (bool) ((byte) (result == ((_MI_Result) 0)));
             */

            return true;
        }

        private static void ReleaseHandleWhenSynchronousEnumeratorWasAbandoned(object state)
        {
            ((OperationHandle) state).ReleaseHandleCore();
        }

        internal void SetExtraFinalizationAction(Action extraFinalizationAction)
        {
            this.m_ExtraFinalizationAction = extraFinalizationAction;
        }

        internal unsafe void SetOperationCallback(void* operationCallback)
        {
            this.m_OperationCallback = operationCallback;
        }
    }
}

