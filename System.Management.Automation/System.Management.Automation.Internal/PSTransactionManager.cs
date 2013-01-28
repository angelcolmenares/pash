namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Transactions;

    internal sealed class PSTransactionManager : IDisposable
    {
        private PSTransaction baseTransaction;
        private static bool engineProtectionEnabled;
        private Transaction previousActiveTransaction;
        private Stack<PSTransaction> transactionStack = new Stack<PSTransaction>();

        internal PSTransactionManager()
        {
            this.transactionStack.Push(null);
        }

        internal void ClearBaseTransaction()
        {
            if (this.baseTransaction == null)
            {
                throw new InvalidOperationException(TransactionStrings.BaseTransactionNotSet);
            }
            if (this.transactionStack.Peek() != this.baseTransaction)
            {
                throw new InvalidOperationException(TransactionStrings.BaseTransactionNotActive);
            }
            this.transactionStack.Pop().Dispose();
            this.baseTransaction = null;
        }

        internal void Commit()
        {
            PSTransaction transaction = this.transactionStack.Peek();
            if (transaction == null)
            {
                throw new InvalidOperationException(TransactionStrings.NoTransactionActiveForCommit);
            }
            if (transaction.IsRolledBack)
            {
                throw new TransactionAbortedException(TransactionStrings.TransactionRolledBackForCommit);
            }
            if (transaction.IsCommitted)
            {
                throw new InvalidOperationException(TransactionStrings.CommittedTransactionForCommit);
            }
            if (transaction.SubscriberCount == 1)
            {
                transaction.Commit();
                transaction.SubscriberCount = 0;
            }
            else
            {
                transaction.SubscriberCount--;
            }
            while ((this.transactionStack.Count > 2) && (this.transactionStack.Peek().IsRolledBack || this.transactionStack.Peek().IsCommitted))
            {
                this.transactionStack.Pop().Dispose();
            }
        }

        internal void CreateNew()
        {
            this.CreateNew(RollbackSeverity.Error, TimeSpan.FromMinutes(1.0));
        }

        internal void CreateNew(RollbackSeverity rollbackPreference, TimeSpan timeout)
        {
            this.transactionStack.Push(new PSTransaction(rollbackPreference, timeout));
        }

        internal void CreateOrJoin()
        {
            this.CreateOrJoin(RollbackSeverity.Error, TimeSpan.FromMinutes(1.0));
        }

        internal void CreateOrJoin(RollbackSeverity rollbackPreference, TimeSpan timeout)
        {
            PSTransaction transaction = this.transactionStack.Peek();
            if (transaction != null)
            {
                if (transaction.IsRolledBack || transaction.IsCommitted)
                {
                    this.transactionStack.Pop().Dispose();
                    this.transactionStack.Push(new PSTransaction(rollbackPreference, timeout));
                }
                else
                {
                    transaction.SubscriberCount++;
                }
            }
            else
            {
                this.transactionStack.Push(new PSTransaction(rollbackPreference, timeout));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ResetActive();
                while (this.transactionStack.Peek() != null)
                {
                    PSTransaction transaction = this.transactionStack.Pop();
                    if (transaction != this.baseTransaction)
                    {
                        transaction.Dispose();
                    }
                }
            }
        }

        internal static void EnableEngineProtection()
        {
            engineProtectionEnabled = true;
        }

        ~PSTransactionManager()
        {
            this.Dispose(false);
        }

        internal PSTransaction GetCurrent()
        {
            return this.transactionStack.Peek();
        }

        internal static IDisposable GetEngineProtectionScope()
        {
            if (engineProtectionEnabled && (Transaction.Current != null))
            {
                return new TransactionScope(TransactionScopeOption.Suppress);
            }
            return null;
        }

        internal void ResetActive()
        {
            Transaction.Current = this.previousActiveTransaction;
            this.previousActiveTransaction = null;
        }

        internal void Rollback()
        {
            this.Rollback(false);
        }

        internal void Rollback(bool suppressErrors)
        {
            PSTransaction transaction = this.transactionStack.Peek();
            if (transaction == null)
            {
                throw new InvalidOperationException(TransactionStrings.NoTransactionActiveForRollback);
            }
            if (transaction.IsRolledBack && !suppressErrors)
            {
                throw new TransactionAbortedException(TransactionStrings.TransactionRolledBackForRollback);
            }
            if (transaction.IsCommitted && !suppressErrors)
            {
                throw new InvalidOperationException(TransactionStrings.CommittedTransactionForRollback);
            }
            transaction.SubscriberCount = 0;
            transaction.Rollback();
            while ((this.transactionStack.Count > 2) && (this.transactionStack.Peek().IsRolledBack || this.transactionStack.Peek().IsCommitted))
            {
                this.transactionStack.Pop().Dispose();
            }
        }

        internal void SetActive()
        {
            EnableEngineProtection();
            PSTransaction transaction = this.transactionStack.Peek();
            if (transaction == null)
            {
                throw new InvalidOperationException(TransactionStrings.NoTransactionForActivation);
            }
            if (transaction.IsRolledBack)
            {
                throw new TransactionAbortedException(TransactionStrings.NoTransactionForActivationBecauseRollback);
            }
            this.previousActiveTransaction = Transaction.Current;
            transaction.Activate();
        }

        internal void SetBaseTransaction(CommittableTransaction transaction, RollbackSeverity severity)
        {
            if (this.HasTransaction)
            {
                throw new InvalidOperationException(TransactionStrings.BaseTransactionMustBeFirst);
            }
            this.transactionStack.Peek();
            while ((this.transactionStack.Peek() != null) && (this.transactionStack.Peek().IsRolledBack || this.transactionStack.Peek().IsCommitted))
            {
                this.transactionStack.Pop().Dispose();
            }
            this.baseTransaction = new PSTransaction(transaction, severity);
            this.transactionStack.Push(this.baseTransaction);
        }

        internal bool HasTransaction
        {
            get
            {
                PSTransaction transaction = this.transactionStack.Peek();
                return (((transaction != null) && !transaction.IsCommitted) && !transaction.IsRolledBack);
            }
        }

        internal bool IsLastTransactionCommitted
        {
            get
            {
                PSTransaction transaction = this.transactionStack.Peek();
                return ((transaction != null) && transaction.IsCommitted);
            }
        }

        internal bool IsLastTransactionRolledBack
        {
            get
            {
                PSTransaction transaction = this.transactionStack.Peek();
                return ((transaction != null) && transaction.IsRolledBack);
            }
        }

        internal RollbackSeverity RollbackPreference
        {
            get
            {
                PSTransaction transaction = this.transactionStack.Peek();
                if (transaction == null)
                {
                    throw new InvalidOperationException(TransactionStrings.NoTransactionActive);
                }
                return transaction.RollbackPreference;
            }
        }
    }
}

