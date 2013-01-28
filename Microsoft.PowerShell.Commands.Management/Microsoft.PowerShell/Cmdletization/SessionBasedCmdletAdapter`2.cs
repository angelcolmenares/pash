using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Remoting;

namespace Microsoft.PowerShell.Cmdletization
{
	public abstract class SessionBasedCmdletAdapter<TObjectInstance, TSession> : CmdletAdapter<TObjectInstance>, IDisposable
	where TObjectInstance : class
	where TSession : class
	{
        private bool asJob;
        private const string CIMJobType = "CimJob";
        private bool disposed;
        private ThrottlingJob parentJob;
        private TSession[] session;
        private bool sessionWasSpecified;

        internal SessionBasedCmdletAdapter()
        {
        }

        public override void BeginProcessing()
        {
            if (this.AsJob.IsPresent)
            {
                MshCommandRuntime commandRuntime = (MshCommandRuntime)base.Cmdlet.CommandRuntime;
                string str = null;
                if (commandRuntime.WhatIf.IsPresent)
                {
                    str = "WhatIf";
                }
                else if (commandRuntime.Confirm.IsPresent)
                {
                    str = "Confirm";
                }
                if (str != null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, CmdletizationResources.SessionBasedWrapper_ShouldProcessVsJobConflict, new object[] { str }));
                }
            }
            this.parentJob = new ThrottlingJob(Job.GetCommandTextFromInvocationInfo(base.Cmdlet.MyInvocation), this.GenerateParentJobName(), "CimJob", this.ThrottleLimit, !this.AsJob.IsPresent);
        }

        internal abstract StartableJob CreateInstanceMethodInvocationJob(TSession session, TObjectInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru);
        internal abstract StartableJob CreateQueryJob(TSession session, QueryBuilder query);
        internal abstract StartableJob CreateStaticMethodInvocationJob(TSession session, MethodInvocationInfo methodInvocationInfo);
        private static void DiscardJobOutputs<T>(PSDataCollection<T> psDataCollection)
        {
            psDataCollection.DataAdded += (sender, e) => ((PSDataCollection<T>)sender).Clear();
        }

        private static void DiscardJobOutputs(Job job, JobOutputs jobOutputsToDiscard)
        {
            if (JobOutputs.Output == (jobOutputsToDiscard & JobOutputs.Output))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<PSObject>(job.Output);
            }
            if (JobOutputs.Error == (jobOutputsToDiscard & JobOutputs.Error))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<ErrorRecord>(job.Error);
            }
            if (JobOutputs.Warning == (jobOutputsToDiscard & JobOutputs.Warning))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<WarningRecord>(job.Warning);
            }
            if (JobOutputs.Verbose == (jobOutputsToDiscard & JobOutputs.Verbose))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<VerboseRecord>(job.Verbose);
            }
            if (JobOutputs.Debug == (jobOutputsToDiscard & JobOutputs.Debug))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<DebugRecord>(job.Debug);
            }
            if (JobOutputs.Progress == (jobOutputsToDiscard & JobOutputs.Progress))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<ProgressRecord>(job.Progress);
            }
            if (JobOutputs.Results == (jobOutputsToDiscard & JobOutputs.Results))
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs<PSStreamObject>(job.Results);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && (this.parentJob != null))
                {
                    this.parentJob.Dispose();
                    this.parentJob = null;
                }
                this.disposed = true;
            }
        }

        private StartableJob DoCreateInstanceMethodInvocationJob(TSession sessionForJob, TObjectInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru, bool asJob)
        {
            StartableJob job = this.CreateInstanceMethodInvocationJob(sessionForJob, objectInstance, methodInvocationInfo, passThru);
            if (job != null)
            {
                bool discardNonPipelineResults = !asJob;
                this.HandleJobOutput(job, sessionForJob, discardNonPipelineResults, null);
            }
            return job;
        }

        private StartableJob DoCreateQueryJob(TSession sessionForJob, QueryBuilder query, Action<TSession, TObjectInstance> actionAgainstResults)
        {
            Action<PSObject> action = null;
            StartableJob job = this.CreateQueryJob(sessionForJob, query);
            if (job != null)
            {
                if (actionAgainstResults != null)
                {
                    job.SuppressOutputForwarding = true;
                }
                bool discardNonPipelineResults = (actionAgainstResults != null) || !this.AsJob.IsPresent;
                if (actionAgainstResults != null)
                {
                }
                this.HandleJobOutput(job, sessionForJob, discardNonPipelineResults, (action != null) ? null : (action = delegate(PSObject pso)
                {
                    TObjectInstance local = (TObjectInstance)LanguagePrimitives.ConvertTo(pso, typeof(TObjectInstance), CultureInfo.InvariantCulture);
                    actionAgainstResults(sessionForJob, local);
                }));
            }
            return job;
        }

        private StartableJob DoCreateStaticMethodInvocationJob(TSession sessionForJob, MethodInvocationInfo methodInvocationInfo)
        {
            StartableJob job = this.CreateStaticMethodInvocationJob(sessionForJob, methodInvocationInfo);
            if (job != null)
            {
                bool discardNonPipelineResults = !this.AsJob.IsPresent;
                this.HandleJobOutput(job, sessionForJob, discardNonPipelineResults, null);
            }
            return job;
        }

        public override void EndProcessing()
        {
            this.parentJob.EndOfChildJobs();
            if (this.AsJob.IsPresent)
            {
                base.Cmdlet.WriteObject(this.parentJob);
                base.Cmdlet.JobRepository.Add(this.parentJob);
                this.parentJob = null;
            }
            else
            {
                this.parentJob.ForwardAllResultsToCmdlet(base.Cmdlet);
                this.parentJob.Finished.WaitOne();
            }
        }

        protected abstract string GenerateParentJobName();
        private TSession GetImpliedSession()
        {
            TSession local;
            if ((this.PSModuleInfo != null) && PSPrimitiveDictionary.TryPathGet<TSession>(this.PSModuleInfo.PrivateData as IDictionary, out local, new string[] { "CmdletsOverObjects", "DefaultSession" }))
            {
                return local;
            }
            return this.DefaultSession;
        }

        private TSession GetSessionAssociatedWithPipelineObject()
        {
            TObjectInstance local;
            object variableValue = base.Cmdlet.Context.GetVariableValue(SpecialVariables.InputVarPath, null);
            if (variableValue == null)
            {
                return default(TSession);
            }
            IEnumerable source = LanguagePrimitives.GetEnumerable(variableValue);
            if (source == null)
            {
                return default(TSession);
            }
            List<object> list = source.Cast<object>().ToList<object>();
            if (list.Count != 1)
            {
                return default(TSession);
            }
            if (!LanguagePrimitives.TryConvertTo<TObjectInstance>(list[0], CultureInfo.InvariantCulture, out local))
            {
                return default(TSession);
            }
            return this.GetSessionOfOriginFromInstance(local);
        }

        internal virtual TSession GetSessionOfOriginFromInstance(TObjectInstance instance)
        {
            return default(TSession);
        }

        private IEnumerable<TSession> GetSessionsToActAgainst(TObjectInstance objectInstance)
        {
            if (this.sessionWasSpecified)
            {
                return this.Session;
            }
            TSession sessionOfOriginFromInstance = this.GetSessionOfOriginFromInstance(objectInstance);
            if (sessionOfOriginFromInstance != null)
            {
                return new TSession[] { sessionOfOriginFromInstance };
            }
            return new TSession[] { this.GetImpliedSession() };
        }

        private IEnumerable<TSession> GetSessionsToActAgainst(MethodInvocationInfo methodInvocationInfo)
        {
            if (this.sessionWasSpecified)
            {
                return this.Session;
            }
            HashSet<TSession> set = new HashSet<TSession>();
            foreach (TObjectInstance local in methodInvocationInfo.GetArgumentsOfType<TObjectInstance>())
            {
                TSession sessionOfOriginFromInstance = this.GetSessionOfOriginFromInstance(local);
                if (sessionOfOriginFromInstance != null)
                {
                    set.Add(sessionOfOriginFromInstance);
                }
            }
            if (set.Count == 1)
            {
                return set;
            }
            TSession sessionAssociatedWithPipelineObject = this.GetSessionAssociatedWithPipelineObject();
            if (sessionAssociatedWithPipelineObject != null)
            {
                return new TSession[] { sessionAssociatedWithPipelineObject };
            }
            return new TSession[] { this.GetImpliedSession() };
        }

        private IEnumerable<TSession> GetSessionsToActAgainst(QueryBuilder queryBuilder)
        {
            if (this.sessionWasSpecified)
            {
                return this.Session;
            }
            ISessionBoundQueryBuilder<TSession> builder = queryBuilder as ISessionBoundQueryBuilder<TSession>;
            if (builder != null)
            {
                TSession targetSession = builder.GetTargetSession();
                if (targetSession != null)
                {
                    return new TSession[] { targetSession };
                }
            }
            TSession sessionAssociatedWithPipelineObject = this.GetSessionAssociatedWithPipelineObject();
            if (sessionAssociatedWithPipelineObject != null)
            {
                return new TSession[] { sessionAssociatedWithPipelineObject };
            }
            return new TSession[] { this.GetImpliedSession() };
        }

        private void HandleJobOutput(Job job, TSession sessionForJob, bool discardNonPipelineResults, Action<PSObject> outputAction)
        {
            Action<PSObject> processOutput = delegate(PSObject pso)
            {
                if ((pso != null) && (outputAction != null))
                {
                    outputAction(pso);
                }
            };
            job.Output.DataAdded += delegate(object sender, DataAddedEventArgs eventArgs)
            {
                PSDataCollection<PSObject> datas = (PSDataCollection<PSObject>)sender;
                if (discardNonPipelineResults)
                {
                    foreach (PSObject obj2 in datas.ReadAll())
                    {
                        processOutput(obj2);
                    }
                }
                else
                {
                    PSObject obj3 = datas[eventArgs.Index];
                    processOutput(obj3);
                }
            };
            if (discardNonPipelineResults)
            {
                SessionBasedCmdletAdapter<TObjectInstance, TSession>.DiscardJobOutputs(job, JobOutputs.Progress | JobOutputs.Debug | JobOutputs.Verbose | JobOutputs.Warning | JobOutputs.Error);
            }
        }

        public override void ProcessRecord(MethodInvocationInfo methodInvocationInfo)
        {
            if (methodInvocationInfo == null)
            {
                throw new ArgumentNullException("methodInvocationInfo");
            }
            foreach (TSession local in this.GetSessionsToActAgainst(methodInvocationInfo))
            {
                StartableJob childJob = this.DoCreateStaticMethodInvocationJob(local, methodInvocationInfo);
                if (childJob != null)
                {
                    if (!this.AsJob.IsPresent)
                    {
                        this.parentJob.AddChildJobAndPotentiallyBlock(base.Cmdlet, childJob, ThrottlingJob.ChildJobFlags.None);
                    }
                    else
                    {
                        this.parentJob.AddChildJobWithoutBlocking(childJob, ThrottlingJob.ChildJobFlags.None, null);
                    }
                }
            }
        }

        public override void ProcessRecord(QueryBuilder query)
        {
            this.parentJob.DisableFlowControlForPendingCmdletActionsQueue();
            foreach (TSession local in this.GetSessionsToActAgainst(query))
            {
                StartableJob childJob = this.DoCreateQueryJob(local, query, null);
                if (childJob != null)
                {
                    if (!this.AsJob.IsPresent)
                    {
                        this.parentJob.AddChildJobAndPotentiallyBlock(base.Cmdlet, childJob, ThrottlingJob.ChildJobFlags.None);
                    }
                    else
                    {
                        this.parentJob.AddChildJobWithoutBlocking(childJob, ThrottlingJob.ChildJobFlags.None, null);
                    }
                }
            }
        }

        public override void ProcessRecord(TObjectInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru)
        {
            if (objectInstance == null)
            {
                throw new ArgumentNullException("objectInstance");
            }
            if (methodInvocationInfo == null)
            {
                throw new ArgumentNullException("methodInvocationInfo");
            }
            foreach (TSession local in this.GetSessionsToActAgainst(objectInstance))
            {
                StartableJob childJob = this.DoCreateInstanceMethodInvocationJob(local, objectInstance, methodInvocationInfo, passThru, this.AsJob.IsPresent);
                if (childJob != null)
                {
                    if (!this.AsJob.IsPresent)
                    {
                        this.parentJob.AddChildJobAndPotentiallyBlock(base.Cmdlet, childJob, ThrottlingJob.ChildJobFlags.None);
                    }
                    else
                    {
                        this.parentJob.AddChildJobWithoutBlocking(childJob, ThrottlingJob.ChildJobFlags.None, null);
                    }
                }
            }
        }

        public override void ProcessRecord(QueryBuilder query, MethodInvocationInfo methodInvocationInfo, bool passThru)
        {
            Action<TSession, TObjectInstance> actionAgainstResults = null;
            this.parentJob.DisableFlowControlForPendingJobsQueue();
            ThrottlingJob closureOverParentJob = this.parentJob;
            SwitchParameter closureOverAsJob = this.AsJob;
            foreach (TSession local in this.GetSessionsToActAgainst(query))
            {
                if (actionAgainstResults == null)
                {
                    actionAgainstResults = delegate(TSession sessionForMethodInvocationJob, TObjectInstance objectInstance)
                    {
                        StartableJob childJob = ((SessionBasedCmdletAdapter<TObjectInstance, TSession>)this).DoCreateInstanceMethodInvocationJob(sessionForMethodInvocationJob, objectInstance, methodInvocationInfo, passThru, closureOverAsJob.IsPresent);
                        if (childJob != null)
                        {
                            closureOverParentJob.AddChildJobAndPotentiallyBlock(childJob, ThrottlingJob.ChildJobFlags.None);
                        }
                    };
                }
                StartableJob job = this.DoCreateQueryJob(local, query, actionAgainstResults);
                if (job != null)
                {
                    if (!this.AsJob.IsPresent)
                    {
                        this.parentJob.AddChildJobAndPotentiallyBlock(base.Cmdlet, job, ThrottlingJob.ChildJobFlags.CreatesChildJobs);
                    }
                    else
                    {
                        this.parentJob.AddChildJobWithoutBlocking(job, ThrottlingJob.ChildJobFlags.CreatesChildJobs, null);
                    }
                }
            }
        }

        public override void StopProcessing()
        {
            Job parentJob = this.parentJob;
            if (parentJob != null)
            {
                parentJob.StopJob();
            }
            base.StopProcessing();
        }

        [Parameter]
        public SwitchParameter AsJob
        {
            get
            {
                return this.asJob;
            }
            set
            {
                this.asJob = (bool)value;
            }
        }

        protected abstract TSession DefaultSession { get; }

        internal System.Management.Automation.PSModuleInfo PSModuleInfo
        {
            get
            {
                IScriptCommandInfo commandInfo = base.Cmdlet.CommandInfo as IScriptCommandInfo;
                return commandInfo.ScriptBlock.Module;
            }
        }

        protected TSession[] Session
        {
            get
            {
                if (this.session == null)
                {
                    this.session = new TSession[] { this.DefaultSession };
                }
                return this.session;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.session = value;
                this.sessionWasSpecified = true;
            }
        }

        [Parameter]
        public virtual int ThrottleLimit { get; set; }

		[Flags]
		private enum JobOutputs
		{
			Output = 1,
			Error = 2,
			Warning = 4,
			Verbose = 8,
			Debug = 16,
			Progress = 32,
			NonPipelineResults = 63,
			Results = 64,
			PipelineResults = 64
		}
	}
}