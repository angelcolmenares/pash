namespace System.Management.Automation
{
    using System;
    using System.Threading;

    internal class ServerSteppablePipelineSubscriber
    {
        private PSLocalEventManager eventManager;
        private bool initialized;
        private PSEventSubscriber processSubscriber;
        private PSEventSubscriber startSubscriber;
        private object syncObject = new object();

        public event EventHandler<EventArgs> RunProcessRecord;

        public event EventHandler<EventArgs> StartSteppablePipeline;

        internal void FireHandleProcessRecord(ServerSteppablePipelineDriver driver)
        {
            lock (this.syncObject)
            {
                if (this.eventManager != null)
                {
                    this.eventManager.GenerateEvent(this.processSubscriber.SourceIdentifier, this, new object[] { new ServerSteppablePipelineDriverEventArg(driver) }, null, true, false);
                }
            }
        }

        internal void FireStartSteppablePipeline(ServerSteppablePipelineDriver driver)
        {
            lock (this.syncObject)
            {
                if (this.eventManager != null)
                {
                    this.eventManager.GenerateEvent(this.startSubscriber.SourceIdentifier, this, new object[] { new ServerSteppablePipelineDriverEventArg(driver) }, null, true, false);
                }
            }
        }

        private void HandleProcessRecord(object sender, PSEventArgs args)
        {
            ServerSteppablePipelineDriverEventArg sourceEventArgs = args.SourceEventArgs as ServerSteppablePipelineDriverEventArg;
            ServerSteppablePipelineDriver steppableDriver = sourceEventArgs.SteppableDriver;
            lock (steppableDriver.SyncObject)
            {
                if ((steppableDriver.SteppablePipeline == null) || steppableDriver.ProcessingInput)
                {
                    return;
                }
                steppableDriver.ProcessingInput = true;
                steppableDriver.Pulsed = false;
            }
            bool flag = false;
            Exception reason = null;
            try
            {
                using (ExecutionContextForStepping.PrepareExecutionContext(steppableDriver.LocalPowerShell.GetContextFromTLS(), steppableDriver.LocalPowerShell.InformationalBuffers, steppableDriver.RemoteHost))
                {
                    bool flag2 = false;
                Label_0086:
                    if (steppableDriver.PipelineState != PSInvocationState.Running)
                    {
                        steppableDriver.SetState(steppableDriver.PipelineState, null);
                        return;
                    }
                    if (!steppableDriver.InputEnumerator.MoveNext())
                    {
                        flag = true;
                        if (!steppableDriver.NoInput || flag2)
                        {
                            goto Label_0203;
                        }
                    }
                    flag2 = true;
                    Array array = new int[0];
                    if (steppableDriver.NoInput)
                    {
                        array = steppableDriver.SteppablePipeline.Process();
                    }
                    else
                    {
                        array = steppableDriver.SteppablePipeline.Process(steppableDriver.InputEnumerator.Current);
                    }
                    foreach (object obj2 in array)
                    {
                        if (steppableDriver.PipelineState != PSInvocationState.Running)
                        {
                            steppableDriver.SetState(steppableDriver.PipelineState, null);
                            return;
                        }
                        steppableDriver.DataStructureHandler.SendOutputDataToClient(PSObject.AsPSObject(obj2));
                    }
                    lock (steppableDriver.SyncObject)
                    {
                        steppableDriver.TotalObjectsProcessed++;
                        if (steppableDriver.TotalObjectsProcessed < steppableDriver.Input.Count)
                        {
                            goto Label_0086;
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
                reason = exception2;
            }
            finally
            {
                lock (steppableDriver.SyncObject)
                {
                    steppableDriver.ProcessingInput = false;
                    steppableDriver.CheckAndPulseForProcessing(false);
                }
                if (steppableDriver.PipelineState == PSInvocationState.Stopping)
                {
                    steppableDriver.PerformStop();
                }
            }
        Label_0203:
            if (flag)
            {
                try
                {
                    using (ExecutionContextForStepping.PrepareExecutionContext(steppableDriver.LocalPowerShell.GetContextFromTLS(), steppableDriver.LocalPowerShell.InformationalBuffers, steppableDriver.RemoteHost))
                    {
                        foreach (object obj3 in steppableDriver.SteppablePipeline.End())
                        {
                            if (steppableDriver.PipelineState != PSInvocationState.Running)
                            {
                                steppableDriver.SetState(steppableDriver.PipelineState, null);
                                return;
                            }
                            steppableDriver.DataStructureHandler.SendOutputDataToClient(PSObject.AsPSObject(obj3));
                        }
                        steppableDriver.SetState(PSInvocationState.Completed, null);
                        return;
                    }
                }
                catch (Exception exception3)
                {
                    CommandProcessorBase.CheckForSevereException(exception3);
                    reason = exception3;
                }
                finally
                {
                    if (steppableDriver.PipelineState == PSInvocationState.Stopping)
                    {
                        steppableDriver.PerformStop();
                    }
                }
            }
            if (reason != null)
            {
                steppableDriver.SetState(PSInvocationState.Failed, reason);
            }
        }

        private void HandleStartEvent(object sender, PSEventArgs args)
        {
            ServerSteppablePipelineDriverEventArg sourceEventArgs = args.SourceEventArgs as ServerSteppablePipelineDriverEventArg;
            ServerSteppablePipelineDriver steppableDriver = sourceEventArgs.SteppableDriver;
            Exception reason = null;
            try
            {
                using (ExecutionContextForStepping.PrepareExecutionContext(steppableDriver.LocalPowerShell.GetContextFromTLS(), steppableDriver.LocalPowerShell.InformationalBuffers, steppableDriver.RemoteHost))
                {
                    steppableDriver.SteppablePipeline = steppableDriver.LocalPowerShell.GetSteppablePipeline();
                    steppableDriver.SteppablePipeline.Begin(!steppableDriver.NoInput);
                }
                if (steppableDriver.NoInput)
                {
                    steppableDriver.HandleInputEndReceived(this, EventArgs.Empty);
                }
            }
            catch (Exception exception2)
            {
                reason = exception2;
            }
            if (reason != null)
            {
                steppableDriver.SetState(PSInvocationState.Failed, reason);
            }
        }

        internal void SubscribeEvents(ServerSteppablePipelineDriver driver)
        {
            lock (this.syncObject)
            {
                if (!this.initialized)
                {
                    this.eventManager = driver.LocalPowerShell.Runspace.Events as PSLocalEventManager;
                    if (this.eventManager != null)
                    {
                        this.startSubscriber = this.eventManager.SubscribeEvent(this, "StartSteppablePipeline", Guid.NewGuid().ToString(), null, new PSEventReceivedEventHandler(this.HandleStartEvent), true, false, true, 0);
                        this.processSubscriber = this.eventManager.SubscribeEvent(this, "RunProcessRecord", Guid.NewGuid().ToString(), null, new PSEventReceivedEventHandler(this.HandleProcessRecord), true, false, true, 0);
                    }
                    this.initialized = true;
                }
            }
        }
    }
}

