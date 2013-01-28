namespace System.Management.Automation.Tracing
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Text;

    public sealed class Tracer : EtwActivity
    {
        private static EventDescriptor DebugMessageEvent = new EventDescriptor(0xc000, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        public const long KeywordAll = 0xffffffffL;
        public const byte LevelCritical = 1;
        public const byte LevelError = 2;
        public const byte LevelInformational = 4;
        public const byte LevelVerbose = 5;
        public const byte LevelWarning = 3;
        private static EventDescriptor M3PAbortingWorkflowExecutionEvent = new EventDescriptor(0xb038, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PActivityExecutionFinishedEvent = new EventDescriptor(0xb03f, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PActivityExecutionQueuedEvent = new EventDescriptor(0xb017, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PActivityExecutionStartedEvent = new EventDescriptor(0xb018, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PBeginContainerParentJobExecutionEvent = new EventDescriptor(0xb50c, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginCreateNewJobEvent = new EventDescriptor(0xb503, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginJobLogicEvent = new EventDescriptor(0xb506, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginProxyChildJobEventHandlerEvent = new EventDescriptor(0xb512, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginProxyJobEventHandlerEvent = new EventDescriptor(0xb510, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginProxyJobExecutionEvent = new EventDescriptor(0xb50e, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginRunGarbageCollectionEvent = new EventDescriptor(0xb514, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginStartWorkflowApplicationEvent = new EventDescriptor(0xb501, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PBeginWorkflowExecutionEvent = new EventDescriptor(0xb508, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PCancellingWorkflowExecutionEvent = new EventDescriptor(0xb037, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PChildWorkflowJobAdditionEvent = new EventDescriptor(0xb50a, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndContainerParentJobExecutionEvent = new EventDescriptor(0xb50d, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndCreateNewJobEvent = new EventDescriptor(0xb504, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndJobLogicEvent = new EventDescriptor(0xb507, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndpointDisabledEvent = new EventDescriptor(0xb044, 1, 0x11, 5, 20, 9, 0x4000000000000200L);
        private static EventDescriptor M3PEndpointEnabledEvent = new EventDescriptor(0xb045, 1, 0x11, 5, 20, 9, 0x4000000000000200L);
        private static EventDescriptor M3PEndpointModifiedEvent = new EventDescriptor(0xb042, 1, 0x11, 5, 20, 9, 0x4000000000000200L);
        private static EventDescriptor M3PEndpointRegisteredEvent = new EventDescriptor(0xb041, 1, 0x11, 5, 20, 9, 0x4000000000000200L);
        private static EventDescriptor M3PEndpointUnregisteredEvent = new EventDescriptor(0xb043, 1, 0x11, 5, 20, 9, 0x4000000000000200L);
        private static EventDescriptor M3PEndProxyChildJobEventHandlerEvent = new EventDescriptor(0xb513, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndProxyJobEventHandlerEvent = new EventDescriptor(0xb511, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndProxyJobExecutionEvent = new EventDescriptor(0xb50f, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndRunGarbageCollectionEvent = new EventDescriptor(0xb515, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndStartWorkflowApplicationEvent = new EventDescriptor(0xb502, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PEndWorkflowExecutionEvent = new EventDescriptor(0xb509, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PErrorImportingWorkflowFromXamlEvent = new EventDescriptor(0xb01b, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PForcedWorkflowShutdownErrorEvent = new EventDescriptor(0xb03c, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PForcedWorkflowShutdownFinishedEvent = new EventDescriptor(0xb03b, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PForcedWorkflowShutdownStartedEvent = new EventDescriptor(0xb03a, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PImportedWorkflowFromXamlEvent = new EventDescriptor(0xb01a, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PImportingWorkflowFromXamlEvent = new EventDescriptor(0xb019, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PJobCreationCompleteEvent = new EventDescriptor(0xb032, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PJobErrorEvent = new EventDescriptor(0xb02e, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PJobRemovedEvent = new EventDescriptor(0xb033, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PJobRemoveErrorEvent = new EventDescriptor(0xb034, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PJobStateChangedEvent = new EventDescriptor(0xb02d, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PLoadingWorkflowForExecutionEvent = new EventDescriptor(0xb035, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3POutOfProcessRunspaceStartedEvent = new EventDescriptor(0xb046, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PParameterSplattingWasPerformedEvent = new EventDescriptor(0xb047, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PParentJobCreatedEvent = new EventDescriptor(0xb031, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PPersistenceStoreMaxSizeReachedEvent = new EventDescriptor(0xb516, 1, 0x10, 3, 0, 0, -9223372036854775808L);
        private static EventDescriptor M3PPersistingWorkflowEvent = new EventDescriptor(0xb03d, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PProxyJobRemoteJobAssociationEvent = new EventDescriptor(0xb50b, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PRemoveJobStartedEvent = new EventDescriptor(0xb02c, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PRunspaceAvailabilityChangedEvent = new EventDescriptor(0xb022, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PRunspaceStateChangedEvent = new EventDescriptor(0xb023, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PTrackingGuidContainerParentJobCorrelationEvent = new EventDescriptor(0xb505, 1, 0x12, 4, 0, 0, 0x2000000000000000L);
        private static EventDescriptor M3PUnloadingWorkflowEvent = new EventDescriptor(0xb039, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowActivityExecutionFailedEvent = new EventDescriptor(0xb021, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowActivityValidatedEvent = new EventDescriptor(0xb01f, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowActivityValidationFailedEvent = new EventDescriptor(0xb020, 1, 0x11, 5, 20, 8, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowCleanupPerformedEvent = new EventDescriptor(0xb028, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowDeletedFromDiskEvent = new EventDescriptor(0xb02a, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowEngineStartedEvent = new EventDescriptor(0xb048, 1, 0x11, 5, 20, 5, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowExecutionAbortedEvent = new EventDescriptor(0xb027, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowExecutionCancelledEvent = new EventDescriptor(0xb026, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowExecutionErrorEvent = new EventDescriptor(0xb040, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowExecutionFinishedEvent = new EventDescriptor(0xb036, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowExecutionStartedEvent = new EventDescriptor(0xb008, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowJobCreatedEvent = new EventDescriptor(0xb030, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowLoadedForExecutionEvent = new EventDescriptor(0xb024, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowLoadedFromDiskEvent = new EventDescriptor(0xb029, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowManagerCheckpointEvent = new EventDescriptor(0xb049, 1, 0x12, 4, 0, 0, 0x2000000000000200L);
        private static EventDescriptor M3PWorkflowPersistedEvent = new EventDescriptor(0xb03e, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowPluginRequestedToShutdownEvent = new EventDescriptor(0xb010, 1, 0x11, 5, 20, 5, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowPluginRestartedEvent = new EventDescriptor(0xb011, 1, 0x11, 5, 20, 5, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowPluginStartedEvent = new EventDescriptor(0xb007, 1, 0x11, 5, 20, 5, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowQuotaViolatedEvent = new EventDescriptor(0xb013, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowResumedEvent = new EventDescriptor(0xb014, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowResumingEvent = new EventDescriptor(0xb012, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowRunspacePoolCreatedEvent = new EventDescriptor(0xb016, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowStateChangedEvent = new EventDescriptor(0xb009, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowUnloadedEvent = new EventDescriptor(0xb025, 1, 0x11, 5, 20, 6, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowValidationErrorEvent = new EventDescriptor(0xb01e, 1, 0x11, 5, 20, 8, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowValidationFinishedEvent = new EventDescriptor(0xb01d, 1, 0x11, 5, 20, 8, 0x4000000000000200L);
        private static EventDescriptor M3PWorkflowValidationStartedEvent = new EventDescriptor(0xb01c, 1, 0x11, 5, 20, 8, 0x4000000000000200L);
        private static Guid providerId = Guid.Parse("a0c1853b-5c40-4b15-8766-3cf1c58f985a");
        private static EventDescriptor WriteTransferEventEvent = new EventDescriptor(0x1f05, 1, 0x11, 5, 20, 0, 0x4000000000000000L);

        [EtwEvent(0xb038L)]
        public void AbortingWorkflowExecution(Guid workflowId, string reason)
        {
            base.WriteEvent(M3PAbortingWorkflowExecutionEvent, new object[] { workflowId, reason });
        }

        [EtwEvent(0xb03fL)]
        public void ActivityExecutionFinished(string activityName)
        {
            base.WriteEvent(M3PActivityExecutionFinishedEvent, new object[] { activityName });
        }

        [EtwEvent(0xb017L)]
        public void ActivityExecutionQueued(Guid workflowId, string activityName)
        {
            base.WriteEvent(M3PActivityExecutionQueuedEvent, new object[] { workflowId, activityName });
        }

        [EtwEvent(0xb018L)]
        public void ActivityExecutionStarted(string activityName, string activityTypeName)
        {
            base.WriteEvent(M3PActivityExecutionStartedEvent, new object[] { activityName, activityTypeName });
        }

        [EtwEvent(0xb50cL)]
        public void BeginContainerParentJobExecution(Guid containerParentJobInstanceId)
        {
            base.WriteEvent(M3PBeginContainerParentJobExecutionEvent, new object[] { containerParentJobInstanceId });
        }

        [EtwEvent(0xb503L)]
        public void BeginCreateNewJob(Guid trackingId)
        {
            base.WriteEvent(M3PBeginCreateNewJobEvent, new object[] { trackingId });
        }

        [EtwEvent(0xb506L)]
        public void BeginJobLogic(Guid workflowJobJobInstanceId)
        {
            base.WriteEvent(M3PBeginJobLogicEvent, new object[] { workflowJobJobInstanceId });
        }

        [EtwEvent(0xb512L)]
        public void BeginProxyChildJobEventHandler(Guid proxyChildJobInstanceId)
        {
            base.WriteEvent(M3PBeginProxyChildJobEventHandlerEvent, new object[] { proxyChildJobInstanceId });
        }

        [EtwEvent(0xb510L)]
        public void BeginProxyJobEventHandler(Guid proxyJobInstanceId)
        {
            base.WriteEvent(M3PBeginProxyJobEventHandlerEvent, new object[] { proxyJobInstanceId });
        }

        [EtwEvent(0xb50eL)]
        public void BeginProxyJobExecution(Guid proxyJobInstanceId)
        {
            base.WriteEvent(M3PBeginProxyJobExecutionEvent, new object[] { proxyJobInstanceId });
        }

        [EtwEvent(0xb514L)]
        public void BeginRunGarbageCollection()
        {
            base.WriteEvent(M3PBeginRunGarbageCollectionEvent, new object[0]);
        }

        [EtwEvent(0xb501L)]
        public void BeginStartWorkflowApplication(Guid trackingId)
        {
            base.WriteEvent(M3PBeginStartWorkflowApplicationEvent, new object[] { trackingId });
        }

        [EtwEvent(0xb508L)]
        public void BeginWorkflowExecution(Guid workflowJobJobInstanceId)
        {
            base.WriteEvent(M3PBeginWorkflowExecutionEvent, new object[] { workflowJobJobInstanceId });
        }

        [EtwEvent(0xb037L)]
        public void CancellingWorkflowExecution(Guid workflowId)
        {
            base.WriteEvent(M3PCancellingWorkflowExecutionEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb50aL)]
        public void ChildWorkflowJobAddition(Guid workflowJobInstanceId, Guid containerParentJobInstanceId)
        {
            base.WriteEvent(M3PChildWorkflowJobAdditionEvent, new object[] { workflowJobInstanceId, containerParentJobInstanceId });
        }

        [EtwEvent(0xc000L)]
        public void DebugMessage(Exception exception)
        {
            if (exception != null)
            {
                this.DebugMessage(GetExceptionString(exception));
            }
        }

        [EtwEvent(0xc000L)]
        public void DebugMessage(string message)
        {
            base.WriteEvent(DebugMessageEvent, new object[] { message });
        }

        [EtwEvent(0xb50dL)]
        public void EndContainerParentJobExecution(Guid containerParentJobInstanceId)
        {
            base.WriteEvent(M3PEndContainerParentJobExecutionEvent, new object[] { containerParentJobInstanceId });
        }

        [EtwEvent(0xb504L)]
        public void EndCreateNewJob(Guid trackingId)
        {
            base.WriteEvent(M3PEndCreateNewJobEvent, new object[] { trackingId });
        }

        [EtwEvent(0xb507L)]
        public void EndJobLogic(Guid workflowJobJobInstanceId)
        {
            base.WriteEvent(M3PEndJobLogicEvent, new object[] { workflowJobJobInstanceId });
        }

        [EtwEvent(0xb044L)]
        public void EndpointDisabled(string endpointName, string disabledBy)
        {
            base.WriteEvent(M3PEndpointDisabledEvent, new object[] { endpointName, disabledBy });
        }

        [EtwEvent(0xb045L)]
        public void EndpointEnabled(string endpointName, string enabledBy)
        {
            base.WriteEvent(M3PEndpointEnabledEvent, new object[] { endpointName, enabledBy });
        }

        [EtwEvent(0xb042L)]
        public void EndpointModified(string endpointName, string modifiedBy)
        {
            base.WriteEvent(M3PEndpointModifiedEvent, new object[] { endpointName, modifiedBy });
        }

        [EtwEvent(0xb041L)]
        public void EndpointRegistered(string endpointName, string endpointType, string registeredBy)
        {
            base.WriteEvent(M3PEndpointRegisteredEvent, new object[] { endpointName, endpointType, registeredBy });
        }

        [EtwEvent(0xb043L)]
        public void EndpointUnregistered(string endpointName, string unregisteredBy)
        {
            base.WriteEvent(M3PEndpointUnregisteredEvent, new object[] { endpointName, unregisteredBy });
        }

        [EtwEvent(0xb513L)]
        public void EndProxyChildJobEventHandler(Guid proxyChildJobInstanceId)
        {
            base.WriteEvent(M3PEndProxyChildJobEventHandlerEvent, new object[] { proxyChildJobInstanceId });
        }

        [EtwEvent(0xb511L)]
        public void EndProxyJobEventHandler(Guid proxyJobInstanceId)
        {
            base.WriteEvent(M3PEndProxyJobEventHandlerEvent, new object[] { proxyJobInstanceId });
        }

        [EtwEvent(0xb50fL)]
        public void EndProxyJobExecution(Guid proxyJobInstanceId)
        {
            base.WriteEvent(M3PEndProxyJobExecutionEvent, new object[] { proxyJobInstanceId });
        }

        [EtwEvent(0xb515L)]
        public void EndRunGarbageCollection()
        {
            base.WriteEvent(M3PEndRunGarbageCollectionEvent, new object[0]);
        }

        [EtwEvent(0xb502L)]
        public void EndStartWorkflowApplication(Guid trackingId)
        {
            base.WriteEvent(M3PEndStartWorkflowApplicationEvent, new object[] { trackingId });
        }

        [EtwEvent(0xb509L)]
        public void EndWorkflowExecution(Guid workflowJobJobInstanceId)
        {
            base.WriteEvent(M3PEndWorkflowExecutionEvent, new object[] { workflowJobJobInstanceId });
        }

        [EtwEvent(0xb01bL)]
        public void ErrorImportingWorkflowFromXaml(Guid workflowId, string errorDescription)
        {
            base.WriteEvent(M3PErrorImportingWorkflowFromXamlEvent, new object[] { workflowId, errorDescription });
        }

        [EtwEvent(0xb03cL)]
        public void ForcedWorkflowShutdownError(Guid workflowId, string errorDescription)
        {
            base.WriteEvent(M3PForcedWorkflowShutdownErrorEvent, new object[] { workflowId, errorDescription });
        }

        [EtwEvent(0xb03bL)]
        public void ForcedWorkflowShutdownFinished(Guid workflowId)
        {
            base.WriteEvent(M3PForcedWorkflowShutdownFinishedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb03aL)]
        public void ForcedWorkflowShutdownStarted(Guid workflowId)
        {
            base.WriteEvent(M3PForcedWorkflowShutdownStartedEvent, new object[] { workflowId });
        }

        public static string GetExceptionString(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            while (WriteExceptionText(sb, exception))
            {
                exception = exception.InnerException;
            }
            return sb.ToString();
        }

        [EtwEvent(0xb01aL)]
        public void ImportedWorkflowFromXaml(Guid workflowId, string xamlFile)
        {
            base.WriteEvent(M3PImportedWorkflowFromXamlEvent, new object[] { workflowId, xamlFile });
        }

        [EtwEvent(0xb019L)]
        public void ImportingWorkflowFromXaml(Guid workflowId, string xamlFile)
        {
            base.WriteEvent(M3PImportingWorkflowFromXamlEvent, new object[] { workflowId, xamlFile });
        }

        [EtwEvent(0xb032L)]
        public void JobCreationComplete(Guid jobId, Guid workflowId)
        {
            base.WriteEvent(M3PJobCreationCompleteEvent, new object[] { jobId, workflowId });
        }

        [EtwEvent(0xb02eL)]
        public void JobError(int jobId, Guid workflowId, string errorDescription)
        {
            base.WriteEvent(M3PJobErrorEvent, new object[] { jobId, workflowId, errorDescription });
        }

        [EtwEvent(0xb033L)]
        public void JobRemoved(Guid parentJobId, Guid childJobId, Guid workflowId)
        {
            base.WriteEvent(M3PJobRemovedEvent, new object[] { parentJobId, childJobId, workflowId });
        }

        [EtwEvent(0xb034L)]
        public void JobRemoveError(Guid parentJobId, Guid childJobId, Guid workflowId, string error)
        {
            base.WriteEvent(M3PJobRemoveErrorEvent, new object[] { parentJobId, childJobId, workflowId, error });
        }

        [EtwEvent(0xb02dL)]
        public void JobStateChanged(int jobId, Guid workflowId, string newState, string oldState)
        {
            base.WriteEvent(M3PJobStateChangedEvent, new object[] { jobId, workflowId, newState, oldState });
        }

        [EtwEvent(0xb035L)]
        public void LoadingWorkflowForExecution(Guid workflowId)
        {
            base.WriteEvent(M3PLoadingWorkflowForExecutionEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb046L)]
        public void OutOfProcessRunspaceStarted(string command)
        {
            base.WriteEvent(M3POutOfProcessRunspaceStartedEvent, new object[] { command });
        }

        [EtwEvent(0xb047L)]
        public void ParameterSplattingWasPerformed(string parameters, string computers)
        {
            base.WriteEvent(M3PParameterSplattingWasPerformedEvent, new object[] { parameters, computers });
        }

        [EtwEvent(0xb031L)]
        public void ParentJobCreated(Guid jobId)
        {
            base.WriteEvent(M3PParentJobCreatedEvent, new object[] { jobId });
        }

        [EtwEvent(0xb516L)]
        public void PersistenceStoreMaxSizeReached()
        {
            base.WriteEvent(M3PPersistenceStoreMaxSizeReachedEvent, new object[0]);
        }

        [EtwEvent(0xb03dL)]
        public void PersistingWorkflow(Guid workflowId, string persistPath)
        {
            base.WriteEvent(M3PPersistingWorkflowEvent, new object[] { workflowId, persistPath });
        }

        [EtwEvent(0xb50bL)]
        public void ProxyJobRemoteJobAssociation(Guid proxyJobInstanceId, Guid containerParentJobInstanceId)
        {
            base.WriteEvent(M3PProxyJobRemoteJobAssociationEvent, new object[] { proxyJobInstanceId, containerParentJobInstanceId });
        }

        [EtwEvent(0xb02cL)]
        public void RemoveJobStarted(Guid jobId)
        {
            base.WriteEvent(M3PRemoveJobStartedEvent, new object[] { jobId });
        }

        [EtwEvent(0xb022L)]
        public void RunspaceAvailabilityChanged(string runspaceId, string availability)
        {
            base.WriteEvent(M3PRunspaceAvailabilityChangedEvent, new object[] { runspaceId, availability });
        }

        [EtwEvent(0xb023L)]
        public void RunspaceStateChanged(string runspaceId, string newState, string oldState)
        {
            base.WriteEvent(M3PRunspaceStateChangedEvent, new object[] { runspaceId, newState, oldState });
        }

        [EtwEvent(0xb505L)]
        public void TrackingGuidContainerParentJobCorrelation(Guid trackingId, Guid containerParentJobInstanceId)
        {
            base.WriteEvent(M3PTrackingGuidContainerParentJobCorrelationEvent, new object[] { trackingId, containerParentJobInstanceId });
        }

        [EtwEvent(0xb039L)]
        public void UnloadingWorkflow(Guid workflowId)
        {
            base.WriteEvent(M3PUnloadingWorkflowEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb021L)]
        public void WorkflowActivityExecutionFailed(Guid workflowId, string activityName, string failureDescription)
        {
            base.WriteEvent(M3PWorkflowActivityExecutionFailedEvent, new object[] { workflowId, activityName, failureDescription });
        }

        [EtwEvent(0xb01fL)]
        public void WorkflowActivityValidated(Guid workflowId, string activityDisplayName, string activityType)
        {
            base.WriteEvent(M3PWorkflowActivityValidatedEvent, new object[] { workflowId, activityDisplayName, activityType });
        }

        [EtwEvent(0xb020L)]
        public void WorkflowActivityValidationFailed(Guid workflowId, string activityDisplayName, string activityType)
        {
            base.WriteEvent(M3PWorkflowActivityValidationFailedEvent, new object[] { workflowId, activityDisplayName, activityType });
        }

        [EtwEvent(0xb028L)]
        public void WorkflowCleanupPerformed(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowCleanupPerformedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb02aL)]
        public void WorkflowDeletedFromDisk(Guid workflowId, string path)
        {
            base.WriteEvent(M3PWorkflowDeletedFromDiskEvent, new object[] { workflowId, path });
        }

        [EtwEvent(0xb048L)]
        public void WorkflowEngineStarted(string endpointName)
        {
            base.WriteEvent(M3PWorkflowEngineStartedEvent, new object[] { endpointName });
        }

        [EtwEvent(0xb027L)]
        public void WorkflowExecutionAborted(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowExecutionAbortedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb026L)]
        public void WorkflowExecutionCancelled(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowExecutionCancelledEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb040L)]
        public void WorkflowExecutionError(Guid workflowId, string errorDescription)
        {
            base.WriteEvent(M3PWorkflowExecutionErrorEvent, new object[] { workflowId, errorDescription });
        }

        [EtwEvent(0xb036L)]
        public void WorkflowExecutionFinished(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowExecutionFinishedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb008L)]
        public void WorkflowExecutionStarted(Guid workflowId, string managedNodes)
        {
            base.WriteEvent(M3PWorkflowExecutionStartedEvent, new object[] { workflowId, managedNodes });
        }

        [EtwEvent(0xb030L)]
        public void WorkflowJobCreated(Guid parentJobId, Guid childJobId, Guid childWorkflowId)
        {
            base.WriteEvent(M3PWorkflowJobCreatedEvent, new object[] { parentJobId, childJobId, childWorkflowId });
        }

        [EtwEvent(0xb024L)]
        public void WorkflowLoadedForExecution(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowLoadedForExecutionEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb029L)]
        public void WorkflowLoadedFromDisk(Guid workflowId, string path)
        {
            base.WriteEvent(M3PWorkflowLoadedFromDiskEvent, new object[] { workflowId, path });
        }

        [EtwEvent(0xb049L)]
        public void WorkflowManagerCheckpoint(string checkpointPath, string configProviderId, string userName, string path)
        {
            base.WriteEvent(M3PWorkflowManagerCheckpointEvent, new object[] { checkpointPath, configProviderId, userName, path });
        }

        [EtwEvent(0xb03eL)]
        public void WorkflowPersisted(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowPersistedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb010L)]
        public void WorkflowPluginRequestedToShutdown(string endpointName)
        {
            base.WriteEvent(M3PWorkflowPluginRequestedToShutdownEvent, new object[] { endpointName });
        }

        [EtwEvent(0xb011L)]
        public void WorkflowPluginRestarted(string endpointName)
        {
            base.WriteEvent(M3PWorkflowPluginRestartedEvent, new object[] { endpointName });
        }

        [EtwEvent(0xb007L)]
        public void WorkflowPluginStarted(string endpointName, string user, string hostingMode, string protocol, string configuration)
        {
            base.WriteEvent(M3PWorkflowPluginStartedEvent, new object[] { endpointName, user, hostingMode, protocol, configuration });
        }

        [EtwEvent(0xb013L)]
        public void WorkflowQuotaViolated(string endpointName, string configName, string allowedValue, string valueInQuestion)
        {
            base.WriteEvent(M3PWorkflowQuotaViolatedEvent, new object[] { endpointName, configName, allowedValue, valueInQuestion });
        }

        [EtwEvent(0xb014L)]
        public void WorkflowResumed(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowResumedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb012L)]
        public void WorkflowResuming(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowResumingEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb016L)]
        public void WorkflowRunspacePoolCreated(Guid workflowId, string managedNode)
        {
            base.WriteEvent(M3PWorkflowRunspacePoolCreatedEvent, new object[] { workflowId, managedNode });
        }

        [EtwEvent(0xb009L)]
        public void WorkflowStateChanged(Guid workflowId, string newState, string oldState)
        {
            base.WriteEvent(M3PWorkflowStateChangedEvent, new object[] { workflowId, newState, oldState });
        }

        [EtwEvent(0xb025L)]
        public void WorkflowUnloaded(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowUnloadedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb01eL)]
        public void WorkflowValidationError(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowValidationErrorEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb01dL)]
        public void WorkflowValidationFinished(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowValidationFinishedEvent, new object[] { workflowId });
        }

        [EtwEvent(0xb01cL)]
        public void WorkflowValidationStarted(Guid workflowId)
        {
            base.WriteEvent(M3PWorkflowValidationStartedEvent, new object[] { workflowId });
        }

        private static bool WriteExceptionText(StringBuilder sb, Exception e)
        {
            if (e == null)
            {
                return false;
            }
            sb.Append(e.GetType().Name);
            sb.Append(Environment.NewLine);
            sb.Append(e.Message);
            sb.Append(Environment.NewLine);
            return true;
        }

        [EtwEvent(0x1f05L)]
        public void WriteTransferEvent(Guid currentActivityId, Guid parentActivityId)
        {
            base.WriteEvent(WriteTransferEventEvent, new object[] { currentActivityId, parentActivityId });
        }

        protected override Guid ProviderId
        {
            get
            {
                return providerId;
            }
        }

        protected override EventDescriptor TransferEvent
        {
            get
            {
                return WriteTransferEventEvent;
            }
        }
    }
}

