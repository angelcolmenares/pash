namespace System.Management.Automation
{
    using System;
    using System.Transactions;

    public sealed class PSTransaction : IDisposable
    {
        private bool isCommitted;
        private bool isRolledBack;
        private RollbackSeverity rollbackPreference;
        private int subscriberCount;
        private CommittableTransaction transaction;

        internal PSTransaction(RollbackSeverity rollbackPreference, TimeSpan timeout)
        {
            this.transaction = new CommittableTransaction(timeout);
            this.rollbackPreference = rollbackPreference;
            this.subscriberCount = 1;
        }

        internal PSTransaction(CommittableTransaction transaction, RollbackSeverity severity)
        {
            this.transaction = transaction;
            this.rollbackPreference = severity;
            this.subscriberCount = 1;
        }

        internal void Activate()
        {
            Transaction.Current = this.transaction;
        }

        internal void Commit()
        {
            this.transaction.Commit();
            this.isCommitted = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing && (this.transaction != null))
            {
                this.transaction.Dispose();
            }
        }

        ~PSTransaction()
        {
            this.Dispose(false);
        }

        internal void Rollback()
        {
            this.transaction.Rollback();
            this.isRolledBack = true;
        }

        internal bool IsCommitted
        {
            get
            {
                return this.isCommitted;
            }
            set
            {
                this.isCommitted = value;
            }
        }

        internal bool IsRolledBack
        {
            get
            {
                if ((!this.isRolledBack && (this.transaction != null)) && (this.transaction.TransactionInformation.Status == TransactionStatus.Aborted))
                {
                    this.isRolledBack = true;
                }
                return this.isRolledBack;
            }
            set
            {
                this.isRolledBack = value;
            }
        }

        public RollbackSeverity RollbackPreference
        {
            get
            {
                return this.rollbackPreference;
            }
        }

        public PSTransactionStatus Status
        {
            get
            {
                if (this.IsRolledBack)
                {
                    return PSTransactionStatus.RolledBack;
                }
                if (this.IsCommitted)
                {
                    return PSTransactionStatus.Committed;
                }
                return PSTransactionStatus.Active;
            }
        }

        public int SubscriberCount
        {
            get
            {
                if (this.IsRolledBack)
                {
                    this.SubscriberCount = 0;
                }
                return this.subscriberCount;
            }
            set
            {
                this.subscriberCount = value;
            }
        }
    }
}

