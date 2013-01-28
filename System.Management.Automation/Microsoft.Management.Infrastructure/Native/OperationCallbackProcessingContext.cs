namespace Microsoft.Management.Infrastructure.Native
{
    using System;
    using System.Runtime.InteropServices;

    internal class OperationCallbackProcessingContext
    {
        private bool inUserCode = false;
        private object managedOperationContext;

        internal OperationCallbackProcessingContext(object managedOperationContext)
        {
            this.managedOperationContext = managedOperationContext;
        }

        internal bool InUserCode
        {
            [return: MarshalAs(UnmanagedType.U1)]
            get
            {
                return this.inUserCode;
            }
            [param: MarshalAs(UnmanagedType.U1)]
            set
            {
                this.inUserCode = value;
            }
        }

        internal object ManagedOperationContext
        {
            get
            {
                return this.managedOperationContext;
            }
        }
    }
}

