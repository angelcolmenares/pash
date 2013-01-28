namespace Microsoft.PowerShell.Commands.Internal
{
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Management.Automation;
    using System.Security;
    using System.Transactions;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeTransactionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string resBaseName = "RegistryProviderStrings";

        private SafeTransactionHandle(IntPtr handle) : base(true)
        {
            base.handle = handle;
        }

        internal static SafeTransactionHandle Create()
        {
            return Create(Transaction.Current);
        }

        internal static SafeTransactionHandle Create(Transaction managedTransaction)
        {
            IntPtr ptr;
            if (managedTransaction == null)
            {
                throw new InvalidOperationException(RegistryProviderStrings.InvalidOperation_NeedTransaction);
            }
            if (RemotingCommandUtil.IsWinPEHost() || PsUtils.IsRunningOnProcessorArchitectureARM())
            {
                throw new NotSupportedException(RegistryProviderStrings.NotSupported_KernelTransactions);
            }
            IKernelTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(managedTransaction) as IKernelTransaction;
            if (dtcTransaction == null)
            {
                throw new NotSupportedException(RegistryProviderStrings.NotSupported_KernelTransactions);
            }
            HandleError(dtcTransaction.GetHandle(out ptr));
            return new SafeTransactionHandle(ptr);
        }

        private static void HandleError(int error)
        {
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
        }

        protected override bool ReleaseHandle()
        {
            return Win32Native.CloseHandle(base.handle);
        }
    }
}

