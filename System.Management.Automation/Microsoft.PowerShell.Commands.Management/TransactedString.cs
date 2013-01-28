namespace Microsoft.PowerShell.Commands.Management
{
    using System;
    using System.Text;
    using System.Transactions;

    public class TransactedString : IEnlistmentNotification
    {
        private Transaction enlistedTransaction;
        private StringBuilder m_TemporaryValue;
        private StringBuilder m_Value;

        public TransactedString() : this("")
        {
        }

        public TransactedString(string value)
        {
            this.m_Value = new StringBuilder(value);
            this.m_TemporaryValue = null;
        }

        public void Append(string text)
        {
            this.ValidateTransactionOrEnlist();
            if (this.enlistedTransaction != null)
            {
                this.m_TemporaryValue.Append(text);
            }
            else
            {
                this.m_Value.Append(text);
            }
        }

        public void Remove(int startIndex, int length)
        {
            this.ValidateTransactionOrEnlist();
            if (this.enlistedTransaction != null)
            {
                this.m_TemporaryValue.Remove(startIndex, length);
            }
            else
            {
                this.m_Value.Remove(startIndex, length);
            }
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            this.m_Value = new StringBuilder(this.m_TemporaryValue.ToString());
            this.m_TemporaryValue = null;
            this.enlistedTransaction = null;
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            this.m_TemporaryValue = null;
            this.enlistedTransaction = null;
            enlistment.Done();
        }

        public override string ToString()
        {
            if ((Transaction.Current != null) && !(this.enlistedTransaction != Transaction.Current))
            {
                return this.m_TemporaryValue.ToString();
            }
            return this.m_Value.ToString();
        }

        private void ValidateTransactionOrEnlist()
        {
            if (Transaction.Current != null)
            {
                if (this.enlistedTransaction == null)
                {
                    Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                    this.enlistedTransaction = Transaction.Current;
                    this.m_TemporaryValue = new StringBuilder(this.m_Value.ToString());
                }
                else if (Transaction.Current != this.enlistedTransaction)
                {
                    throw new InvalidOperationException("Cannot modify string. It has been modified by another transaction.");
                }
            }
            else if (this.enlistedTransaction != null)
            {
                throw new InvalidOperationException("Cannot modify string. It has been modified by another transaction.");
            }
        }

        public int Length
        {
            get
            {
                if ((Transaction.Current != null) && !(this.enlistedTransaction != Transaction.Current))
                {
                    return this.m_TemporaryValue.Length;
                }
                return this.m_Value.Length;
            }
        }
    }
}

