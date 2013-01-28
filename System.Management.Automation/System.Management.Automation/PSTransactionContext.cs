namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    public sealed class PSTransactionContext : IDisposable
    {
        private PSTransactionManager transactionManager;

        internal PSTransactionContext(PSTransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
            transactionManager.SetActive();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.transactionManager.ResetActive();
            }
        }

        ~PSTransactionContext()
        {
            this.Dispose(false);
        }
    }
}

