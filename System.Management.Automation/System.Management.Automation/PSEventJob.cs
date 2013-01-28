namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;

    public class PSEventJob : Job
    {
        private System.Management.Automation.ScriptBlock action;
        private PSEventManager eventManager;
        private int highestErrorIndex;
        private bool moreData;
        private string statusMessage;
        private PSEventSubscriber subscriber;

        public PSEventJob(PSEventManager eventManager, PSEventSubscriber subscriber, System.Management.Automation.ScriptBlock action, string name) : base((action == null) ? null : action.ToString(), name)
        {
            if (eventManager == null)
            {
                throw new ArgumentNullException("eventManager");
            }
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
            base.UsesResultsCollection = true;
            this.action = action;
            this.eventManager = eventManager;
            this.subscriber = subscriber;
        }

        internal void Invoke(PSEventSubscriber eventSubscriber, PSEventArgs eventArgs)
        {
            if (!base.IsFinishedState(base.JobStateInfo.State))
            {
                base.SetJobState(JobState.Running);
                SessionState publicSessionState = this.action.SessionStateInternal.PublicSessionState;
                publicSessionState.PSVariable.Set("eventSubscriber", eventSubscriber);
                publicSessionState.PSVariable.Set("event", eventArgs);
                publicSessionState.PSVariable.Set("sender", eventArgs.Sender);
                publicSessionState.PSVariable.Set("eventArgs", eventArgs.SourceEventArgs);
                ArrayList resultList = new ArrayList();
                try
                {
                    Pipe outputPipe = new Pipe(resultList);
                    this.action.InvokeWithPipe(false, System.Management.Automation.ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, AutomationNull.Value, AutomationNull.Value, AutomationNull.Value, outputPipe, null, eventArgs.SourceArgs);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    if (!(exception is PipelineStoppedException))
                    {
                        this.LogErrorsAndOutput(resultList, publicSessionState);
                        base.SetJobState(JobState.Failed);
                    }
                    throw;
                }
                this.LogErrorsAndOutput(resultList, publicSessionState);
                this.moreData = true;
            }
        }

        private void LogErrorsAndOutput(ArrayList results, SessionState actionState)
        {
            foreach (object obj2 in results)
            {
                this.WriteObject(obj2);
            }
            base.Error.Clear();
            int num = 0;
            ArrayList list = (ArrayList) actionState.PSVariable.Get("error").Value;
            list.Reverse();
            foreach (ErrorRecord record in list)
            {
                if (num == this.highestErrorIndex)
                {
                    this.WriteError(record);
                    this.highestErrorIndex++;
                }
                num++;
            }
        }

        internal void NotifyJobStopped()
        {
            base.SetJobState(JobState.Stopped);
            this.moreData = false;
        }

        public override void StopJob()
        {
            this.eventManager.UnsubscribeEvent(this.subscriber);
        }

        public override bool HasMoreData
        {
            get
            {
                return this.moreData;
            }
        }

        public override string Location
        {
            get
            {
                return null;
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                return this.action.Module;
            }
        }

        internal System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return this.action;
            }
        }

        public override string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }
        }
    }
}

