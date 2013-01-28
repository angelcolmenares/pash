namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Host;
    using System.Runtime.InteropServices;

    internal class DefaultCommandRuntime : ICommandRuntime
    {
        private PSHost host;
        private ArrayList output;

        public DefaultCommandRuntime(ArrayList outputArrayList)
        {
            if (outputArrayList == null)
            {
                throw new ArgumentNullException("outputArrayList");
            }
            this.output = outputArrayList;
        }

        public bool ShouldContinue(string query, string caption)
        {
            return true;
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            return true;
        }

        public bool ShouldProcess(string target)
        {
            return true;
        }

        public bool ShouldProcess(string target, string action)
        {
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            return true;
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            if (errorRecord.Exception != null)
            {
                throw errorRecord.Exception;
            }
            throw new InvalidOperationException(errorRecord.ToString());
        }

        public bool TransactionAvailable()
        {
            return false;
        }

        public void WriteCommandDetail(string text)
        {
        }

        public void WriteDebug(string text)
        {
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            if (errorRecord.Exception != null)
            {
                throw errorRecord.Exception;
            }
            throw new InvalidOperationException(errorRecord.ToString());
        }

        public void WriteObject(object sendToPipeline)
        {
            this.output.Add(sendToPipeline);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!enumerateCollection)
            {
                this.output.Add(sendToPipeline);
            }
            else
            {
                IEnumerator enumerator = LanguagePrimitives.GetEnumerator(sendToPipeline);
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        this.output.Add(enumerator.Current);
                    }
                }
                else
                {
                    this.output.Add(sendToPipeline);
                }
            }
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
        }

        public void WriteVerbose(string text)
        {
        }

        public void WriteWarning(string text)
        {
        }

        public PSTransactionContext CurrentPSTransaction
        {
            get
            {
                throw new InvalidOperationException(TransactionStrings.CmdletRequiresUseTx);
            }
        }

        public PSHost Host
        {
            get
            {
                return this.host;
            }
            set
            {
                this.host = value;
            }
        }
    }
}

