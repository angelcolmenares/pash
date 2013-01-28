namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.SymbolStore;
    using System.Globalization;
    using System.Linq;
    using System.Management;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Timers;

    internal class PSLocalEventManager : PSEventManager, IDisposable
    {
        private int _consecutiveIdleSamples;
        private bool _isTimerActive;
        private System.Timers.Timer _timer;
        private bool _timerInitialized;
        private object actionProcessingLock = new object();
        private Queue<EventAction> actionQueue = new Queue<EventAction>();
        private System.Management.Automation.ExecutionContext context;
        private bool debugMode;
        private Dictionary<string, List<PSEventSubscriber>> engineEventSubscribers = new Dictionary<string, List<PSEventSubscriber>>(StringComparer.OrdinalIgnoreCase);
        private AssemblyBuilder eventAssembly;
        private ModuleBuilder eventModule;
        private Dictionary<PSEventSubscriber, Delegate> eventSubscribers = new Dictionary<PSEventSubscriber, Delegate>();
        private static Dictionary<string, Type> GeneratedEventHandlers = new Dictionary<string, Type>();
        private int nextSubscriptionId = 1;
        private EventAction processingAction;
        private int throttleChecks;
        private double throttleLimit = 1.0;
        private int typeId;

        internal override event EventHandler<PSEventArgs> ForwardEvent;

        internal PSLocalEventManager(System.Management.Automation.ExecutionContext context)
        {
            this.context = context;
        }

        private void AddAction(EventAction action, bool processSynchronously)
        {
            if (processSynchronously)
            {
                action.Args.EventProcessed = new ManualResetEventSlim();
            }
            lock (((ICollection) this.actionQueue).SyncRoot)
            {
                this.actionQueue.Enqueue(action);
            }
            this.PulseEngine();
        }

        internal override void AddForwardedEvent(PSEventArgs forwardedEvent)
        {
            forwardedEvent.EventIdentifier = base.GetNextEventId();
            this.ProcessNewEvent(forwardedEvent, false);
        }

        private void AutoUnregisterEventIfNecessary(PSEventSubscriber subscriber)
        {
            bool flag = false;
            if (subscriber.AutoUnregister)
            {
                lock (subscriber)
                {
                    subscriber.RemainingActionsToProcess--;
                    flag = (subscriber.RemainingTriggerCount == 0) && (subscriber.RemainingActionsToProcess == 0);
                }
            }
            if (flag)
            {
                this.UnsubscribeEvent(subscriber, true);
            }
        }

        protected override PSEventArgs CreateEvent(string sourceIdentifier, object sender, object[] args, PSObject extraData)
        {
            return new PSEventArgs(null, this.context.CurrentRunspace.InstanceId, base.GetNextEventId(), sourceIdentifier, sender, args, extraData);
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
                lock (this.eventSubscribers)
                {
                    if (this._timer != null)
                    {
                        this._timer.Dispose();
                    }
                    foreach (PSEventSubscriber subscriber in this.eventSubscribers.Keys.ToList<PSEventSubscriber>())
                    {
                        this.UnsubscribeEvent(subscriber);
                    }
                }
            }
        }

        internal void DrainPendingActions(PSEventSubscriber subscriber)
        {
            if (this.actionQueue.Count != 0)
            {
                lock (this.actionProcessingLock)
                {
                    lock (((ICollection) this.actionQueue).SyncRoot)
                    {
                        if (this.actionQueue.Count != 0)
                        {
                            bool flag = false;
                            do
                            {
                                EventAction[] actionArray = this.actionQueue.ToArray();
                                this.actionQueue.Clear();
                                foreach (EventAction action in actionArray)
                                {
                                    if ((action.Sender == subscriber) && (action != this.processingAction))
                                    {
                                        while (this.IsExecutingEventAction)
                                        {
                                            Thread.Sleep(100);
                                        }
                                        bool addActionBack = false;
                                        this.InvokeAction(action, out addActionBack);
                                        if (addActionBack)
                                        {
                                            flag = true;
                                        }
                                    }
                                    else
                                    {
                                        this.actionQueue.Enqueue(action);
                                    }
                                }
                            }
                            while (flag);
                        }
                    }
                }
            }
        }

        private void EnableTimer()
        {
            try
            {
                this._timer.Enabled = true;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        ~PSLocalEventManager()
        {
            this.Dispose(false);
        }

        private Type GenerateEventHandler(MethodInfo invokeSignature)
        {
            int length = invokeSignature.GetParameters().Length;
            StackFrame frame = new StackFrame(0, true);
            ISymbolDocumentWriter document = null;
            if (this.debugMode)
            {
                document = this.eventModule.DefineDocument(frame.GetFileName(), Guid.Empty, Guid.Empty, Guid.Empty);
            }
            TypeBuilder builder = this.eventModule.DefineType("PSEventHandler_" + this.typeId, TypeAttributes.Public, typeof(PSEventHandler));
            this.typeId++;
            ConstructorInfo constructor = typeof(PSEventHandler).GetConstructor(new Type[] { typeof(PSEventManager), typeof(object), typeof(string), typeof(PSObject) });
            if (this.debugMode)
            {
                Type type = typeof(DebuggableAttribute);
                CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(type.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) }), new object[] { DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default });
                this.eventAssembly.SetCustomAttribute(customBuilder);
            }
            ILGenerator iLGenerator = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(PSEventManager), typeof(object), typeof(string), typeof(PSObject) }).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.Emit(OpCodes.Ldarg_3);
            iLGenerator.Emit(OpCodes.Ldarg, 4);
            iLGenerator.Emit(OpCodes.Call, constructor);
            iLGenerator.Emit(OpCodes.Ret);
            Type[] parameterTypes = new Type[length];
            int index = 0;
            foreach (ParameterInfo info3 in invokeSignature.GetParameters())
            {
                parameterTypes[index] = info3.ParameterType;
                index++;
            }
            MethodBuilder builder4 = builder.DefineMethod("EventDelegate", MethodAttributes.Public, CallingConventions.Standard, invokeSignature.ReturnType, parameterTypes);
            index = 1;
            foreach (ParameterInfo info4 in invokeSignature.GetParameters())
            {
                builder4.DefineParameter(index, info4.Attributes, info4.Name);
                index++;
            }
            ILGenerator generator2 = builder4.GetILGenerator();
            LocalBuilder builder5 = generator2.DeclareLocal(typeof(object[]));
            if (this.debugMode)
            {
                builder5.SetLocalSymInfo("args");
                generator2.MarkSequencePoint(document, frame.GetFileLineNumber() - 1, 1, frame.GetFileLineNumber(), 100);
            }
            generator2.Emit(OpCodes.Ldc_I4, length);
            generator2.Emit(OpCodes.Newarr, typeof(object));
            generator2.Emit(OpCodes.Stloc_0);
            for (int i = 1; i <= length; i++)
            {
                if (this.debugMode)
                {
                    generator2.MarkSequencePoint(document, frame.GetFileLineNumber() - 1, 1, frame.GetFileLineNumber(), 100);
                }
                generator2.Emit(OpCodes.Ldloc_0);
                generator2.Emit(OpCodes.Ldc_I4, (int) (i - 1));
                generator2.Emit(OpCodes.Ldarg, i);
                if (parameterTypes[i - 1].IsValueType)
                {
                    generator2.Emit(OpCodes.Box, parameterTypes[i - 1]);
                }
                generator2.Emit(OpCodes.Stelem_Ref);
            }
            generator2.Emit(OpCodes.Ldarg_0);
            FieldInfo field = typeof(PSEventHandler).GetField("eventManager", BindingFlags.NonPublic | BindingFlags.Instance);
            generator2.Emit(OpCodes.Ldfld, field);
            generator2.Emit(OpCodes.Ldarg_0);
            FieldInfo info6 = typeof(PSEventHandler).GetField("sourceIdentifier", BindingFlags.NonPublic | BindingFlags.Instance);
            generator2.Emit(OpCodes.Ldfld, info6);
            generator2.Emit(OpCodes.Ldarg_0);
            FieldInfo info7 = typeof(PSEventHandler).GetField("sender", BindingFlags.NonPublic | BindingFlags.Instance);
            generator2.Emit(OpCodes.Ldfld, info7);
            generator2.Emit(OpCodes.Ldloc_0);
            generator2.Emit(OpCodes.Ldarg_0);
            FieldInfo info8 = typeof(PSEventHandler).GetField("extraData", BindingFlags.NonPublic | BindingFlags.Instance);
            generator2.Emit(OpCodes.Ldfld, info8);
            MethodInfo method = typeof(PSEventManager).GetMethod("GenerateEvent");
            if (this.debugMode)
            {
                generator2.MarkSequencePoint(document, frame.GetFileLineNumber() - 1, 1, frame.GetFileLineNumber(), 100);
            }
            generator2.Emit(OpCodes.Callvirt, method);
            generator2.Emit(OpCodes.Pop);
            generator2.Emit(OpCodes.Ret);
            return builder.CreateType();
        }

        public override IEnumerable<PSEventSubscriber> GetEventSubscribers(string sourceIdentifier)
        {
            return this.GetEventSubscribers(sourceIdentifier, false);
        }

        private IEnumerable<PSEventSubscriber> GetEventSubscribers(string sourceIdentifier, bool forNewEventProcessing)
        {
            List<PSEventSubscriber> list = new List<PSEventSubscriber>();
            List<PSEventSubscriber> list2 = new List<PSEventSubscriber>();
            lock (this.eventSubscribers)
            {
                foreach (PSEventSubscriber subscriber in this.eventSubscribers.Keys)
                {
                    bool flag = false;
                    if (string.Equals(subscriber.SourceIdentifier, sourceIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        if (forNewEventProcessing)
                        {
                            if (!subscriber.AutoUnregister || (subscriber.RemainingTriggerCount > 0))
                            {
                                flag = true;
                                list.Add(subscriber);
                            }
                        }
                        else
                        {
                            list.Add(subscriber);
                        }
                        if ((forNewEventProcessing && subscriber.AutoUnregister) && (subscriber.RemainingTriggerCount > 0))
                        {
                            lock (subscriber)
                            {
                                subscriber.RemainingTriggerCount--;
                                if (flag)
                                {
                                    subscriber.RemainingActionsToProcess++;
                                }
                                if ((subscriber.RemainingTriggerCount == 0) && (subscriber.RemainingActionsToProcess == 0))
                                {
                                    list2.Add(subscriber);
                                }
                            }
                        }
                    }
                }
            }
            if (list2.Count > 0)
            {
                foreach (PSEventSubscriber subscriber2 in list2)
                {
                    this.UnsubscribeEvent(subscriber2, true);
                }
            }
            return list;
        }

        private void InitializeTimer()
        {
            try
            {
                this._timer = new System.Timers.Timer();
                this._timer.Elapsed += new ElapsedEventHandler(this.OnElapsedEvent);
                this._timer.Interval = 100.0;
                this._timer.Enabled = false;
                this._timer.AutoReset = false;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void InvokeAction(EventAction nextAction, out bool addActionBack)
        {
            lock (this.actionProcessingLock)
            {
                this.processingAction = nextAction;
                addActionBack = false;
                SessionStateInternal engineSessionState = this.context.EngineSessionState;
                if (nextAction.Sender.Action != null)
                {
                    this.context.EngineSessionState = nextAction.Sender.Action.ScriptBlock.SessionStateInternal;
                }
                Runspace defaultRunspace = Runspace.DefaultRunspace;
                try
                {
                    Runspace.DefaultRunspace = this.context.CurrentRunspace;
                    if (nextAction.Sender.Action != null)
                    {
                        nextAction.Sender.Action.Invoke(nextAction.Sender, nextAction.Args);
                    }
                    else
                    {
                        nextAction.Sender.HandlerDelegate(nextAction.Sender, nextAction.Args);
                    }
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    if (exception is PipelineStoppedException)
                    {
                        this.AddAction(nextAction, false);
                        addActionBack = true;
                    }
                }
                finally
                {
                    ManualResetEventSlim eventProcessed = nextAction.Args.EventProcessed;
                    if (!addActionBack && (eventProcessed != null))
                    {
                        eventProcessed.Set();
                    }
                    Runspace.DefaultRunspace = defaultRunspace;
                    this.context.EngineSessionState = engineSessionState;
                    this.processingAction = null;
                }
            }
        }

        private void OnElapsedEvent(object source, ElapsedEventArgs e)
        {
            LocalRunspace currentRunspace = this.context.CurrentRunspace as LocalRunspace;
            if (currentRunspace == null)
            {
                this._consecutiveIdleSamples = 0;
            }
            else
            {
                if (currentRunspace.GetCurrentlyRunningPipeline() == null)
                {
                    this._consecutiveIdleSamples++;
                }
                else
                {
                    this._consecutiveIdleSamples = 0;
                }
                if (this._consecutiveIdleSamples == 4)
                {
                    this._consecutiveIdleSamples = 0;
                    lock (this.engineEventSubscribers)
                    {
                        List<PSEventSubscriber> list = null;
                        if (this.engineEventSubscribers.TryGetValue("PowerShell.OnIdle", out list) && (list.Count > 0))
                        {
                            base.GenerateEvent("PowerShell.OnIdle", null, new object[0], null, false, false);
                            this.EnableTimer();
                        }
                        else
                        {
                            this._isTimerActive = false;
                        }
                        return;
                    }
                }
                this.EnableTimer();
            }
        }

        protected virtual void OnForwardEvent(PSEventArgs e)
        {
            EventHandler<PSEventArgs> forwardEvent = this.ForwardEvent;
            if (forwardEvent != null)
            {
                forwardEvent(this, e);
            }
        }

        protected override void ProcessNewEvent(PSEventArgs newEvent, bool processInCurrentThread)
        {
            this.ProcessNewEvent(newEvent, processInCurrentThread, false);
        }

        protected internal override void ProcessNewEvent(PSEventArgs newEvent, bool processInCurrentThread, bool waitForCompletionWhenInCurrentThread)
        {
            WaitCallback callBack = null;
            if (processInCurrentThread)
            {
                this.ProcessNewEventImplementation(newEvent, true);
                ManualResetEventSlim eventProcessed = newEvent.EventProcessed;
                if (eventProcessed != null)
                {
                    while (waitForCompletionWhenInCurrentThread && !eventProcessed.Wait(250))
                    {
                        this.ProcessPendingActions();
                    }
                    eventProcessed.Dispose();
                }
            }
            else
            {
                if (callBack == null)
                {
                    callBack = unused => this.ProcessNewEventImplementation(newEvent, false);
                }
                ThreadPool.QueueUserWorkItem(callBack);
            }
        }

        private void ProcessNewEventImplementation(PSEventArgs newEvent, bool processSynchronously)
        {
            bool flag = false;
            List<PSEventSubscriber> list = new List<PSEventSubscriber>();
            List<PSEventSubscriber> list2 = new List<PSEventSubscriber>();
            foreach (PSEventSubscriber subscriber in this.GetEventSubscribers(newEvent.SourceIdentifier, true))
            {
                newEvent.ForwardEvent = subscriber.ForwardEvent;
                if (subscriber.Action != null)
                {
                    this.AddAction(new EventAction(subscriber, newEvent), processSynchronously);
                    flag = true;
                }
                else if (subscriber.HandlerDelegate != null)
                {
                    if (subscriber.ShouldProcessInExecutionThread)
                    {
                        this.AddAction(new EventAction(subscriber, newEvent), processSynchronously);
                    }
                    else
                    {
                        list.Add(subscriber);
                    }
                    flag = true;
                }
                else
                {
                    list2.Add(subscriber);
                }
            }
            foreach (PSEventSubscriber subscriber2 in list)
            {
                subscriber2.HandlerDelegate(newEvent.Sender, newEvent);
                this.AutoUnregisterEventIfNecessary(subscriber2);
            }
            if (!flag)
            {
                if (newEvent.ForwardEvent)
                {
                    this.OnForwardEvent(newEvent);
                }
                else
                {
                    lock (base.ReceivedEvents.SyncRoot)
                    {
                        base.ReceivedEvents.Add(newEvent);
                    }
                }
                foreach (PSEventSubscriber subscriber3 in list2)
                {
                    this.AutoUnregisterEventIfNecessary(subscriber3);
                }
            }
        }

        private void ProcessNewSubscriber(PSEventSubscriber subscriber, object source, string eventName, string sourceIdentifier, PSObject data, bool supportEvent, bool forwardEvent)
        {
            Delegate handler = null;
            if (this.eventAssembly == null)
            {
                StackFrame frame = new StackFrame(0, true);
                this.debugMode = frame.GetFileName() != null;
                this.eventAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("PSEventHandler"), AssemblyBuilderAccess.Run);
            }
            if (this.eventModule == null)
            {
                this.eventModule = this.eventAssembly.DefineDynamicModule("PSGenericEventModule", this.debugMode);
            }
            string a = null;
            bool flag = false;
            if (source != null)
            {
                if ((sourceIdentifier != null) && sourceIdentifier.StartsWith("PowerShell.", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(StringUtil.Format(EventingResources.ReservedIdentifier, sourceIdentifier), "sourceIdentifier");
                }
                EventInfo info = null;
                Type type = source as Type;
                if (type == null)
                {
                    type = source.GetType();
                }
                if (WinRTHelper.IsWinRTType(type))
                {
                    throw new InvalidOperationException(EventingResources.WinRTEventsNotSupported);
                }
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                info = type.GetEvent(eventName, bindingAttr);
                if (info == null)
                {
                    throw new ArgumentException(StringUtil.Format(EventingResources.CouldNotFindEvent, eventName), "eventName");
                }
                if (type.GetProperty("EnableRaisingEvents") != null)
                {
                    try
                    {
                        type.InvokeMember("EnableRaisingEvents", BindingFlags.SetProperty, null, source, new object[] { true }, CultureInfo.CurrentCulture);
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (exception.InnerException != null)
                        {
                            throw exception.InnerException;
                        }
                        throw;
                    }
                }
                ManagementEventWatcher watcher = source as ManagementEventWatcher;
                if (watcher != null)
                {
                    watcher.Start();
                }
                MethodInfo method = info.EventHandlerType.GetMethod("Invoke");
                if (method.ReturnType != typeof(void))
                {
                    throw new ArgumentException(EventingResources.NonVoidDelegateNotSupported, "eventName");
                }
                string key = source.GetType().FullName + "|" + eventName;
                Type type2 = null;
                if (GeneratedEventHandlers.ContainsKey(key))
                {
                    type2 = GeneratedEventHandlers[key];
                }
                else
                {
                    lock (GeneratedEventHandlers)
                    {
                        if (GeneratedEventHandlers.ContainsKey(key))
                        {
                            type2 = GeneratedEventHandlers[key];
                        }
                        else
                        {
                            type2 = this.GenerateEventHandler(method);
                            GeneratedEventHandlers[key] = type2;
                        }
                    }
                }
                object target = type2.GetConstructor(new Type[] { typeof(PSEventManager), typeof(object), typeof(string), typeof(PSObject) }).Invoke(new object[] { this, source, sourceIdentifier, data });
                handler = Delegate.CreateDelegate(info.EventHandlerType, target, "EventDelegate");
                info.AddEventHandler(source, handler);
            }
            else if (PSEngineEvent.EngineEvents.Contains(sourceIdentifier))
            {
                a = sourceIdentifier;
                flag = string.Equals(a, "PowerShell.OnIdle", StringComparison.OrdinalIgnoreCase);
            }
            lock (this.eventSubscribers)
            {
                this.eventSubscribers[subscriber] = handler;
                if (a != null)
                {
                    lock (this.engineEventSubscribers)
                    {
                        if (flag && !this._timerInitialized)
                        {
                            this.InitializeTimer();
                            this._timerInitialized = true;
                        }
                        List<PSEventSubscriber> list = null;
                        if (!this.engineEventSubscribers.TryGetValue(a, out list))
                        {
                            list = new List<PSEventSubscriber>();
                            this.engineEventSubscribers.Add(a, list);
                        }
                        list.Add(subscriber);
                        if (flag && !this._isTimerActive)
                        {
                            this.EnableTimer();
                            this._isTimerActive = true;
                        }
                    }
                }
            }
        }

        internal void ProcessPendingActions()
        {
            if (this.actionQueue.Count != 0)
            {
                this.ProcessPendingActionsImpl();
            }
        }

        private void ProcessPendingActionsImpl()
        {
            WaitCallback callBack = null;
            if (!this.IsExecutingEventAction)
            {
                try
                {
                    lock (this.actionProcessingLock)
                    {
                        if (!this.IsExecutingEventAction)
                        {
                            int num = 0;
                            this.throttleChecks++;
                            while ((this.throttleLimit * this.throttleChecks) >= num)
                            {
                                EventAction action;
                                lock (((ICollection) this.actionQueue).SyncRoot)
                                {
                                    if (this.actionQueue.Count == 0)
                                    {
                                        return;
                                    }
                                    action = this.actionQueue.Dequeue();
                                }
                                bool addActionBack = false;
                                this.InvokeAction(action, out addActionBack);
                                num++;
                                if (!addActionBack)
                                {
                                    this.AutoUnregisterEventIfNecessary(action.Sender);
                                }
                            }
                            if (num > 0)
                            {
                                this.throttleChecks = 0;
                            }
                        }
                    }
                }
                finally
                {
                    if (this.actionQueue.Count > 0)
                    {
                        if (callBack == null)
                        {
                            callBack = delegate (object unused) {
                                Thread.Sleep(100);
                                this.PulseEngine();
                            };
                        }
                        ThreadPool.QueueUserWorkItem(callBack);
                    }
                }
            }
        }

        private void PulseEngine()
        {
            try
            {
                ((LocalRunspace) this.context.CurrentRunspace).Pulse();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent)
        {
            return this.SubscribeEvent(source, eventName, sourceIdentifier, data, handlerDelegate, supportEvent, forwardEvent, 0);
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, ScriptBlock action, bool supportEvent, bool forwardEvent)
        {
            return this.SubscribeEvent(source, eventName, sourceIdentifier, data, action, supportEvent, forwardEvent, 0);
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent, int maxTriggerCount)
        {
            PSEventSubscriber subscriber = new PSEventSubscriber(this.context, this.nextSubscriptionId++, source, eventName, sourceIdentifier, handlerDelegate, supportEvent, forwardEvent, maxTriggerCount);
            this.ProcessNewSubscriber(subscriber, source, eventName, sourceIdentifier, data, supportEvent, forwardEvent);
            subscriber.RegisterJob();
            return subscriber;
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, ScriptBlock action, bool supportEvent, bool forwardEvent, int maxTriggerCount)
        {
            PSEventSubscriber subscriber = new PSEventSubscriber(this.context, this.nextSubscriptionId++, source, eventName, sourceIdentifier, action, supportEvent, forwardEvent, maxTriggerCount);
            this.ProcessNewSubscriber(subscriber, source, eventName, sourceIdentifier, data, supportEvent, forwardEvent);
            subscriber.RegisterJob();
            return subscriber;
        }

        internal override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent, bool shouldQueueAndProcessInExecutionThread, int maxTriggerCount = 0)
        {
            PSEventSubscriber subscriber = this.SubscribeEvent(source, eventName, sourceIdentifier, data, handlerDelegate, supportEvent, forwardEvent, maxTriggerCount);
            subscriber.ShouldProcessInExecutionThread = shouldQueueAndProcessInExecutionThread;
            return subscriber;
        }

        public override void UnsubscribeEvent(PSEventSubscriber subscriber)
        {
            this.UnsubscribeEvent(subscriber, false);
        }

        private void UnsubscribeEvent(PSEventSubscriber subscriber, bool skipDraining)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }
            Delegate delegate2 = null;
            lock (this.eventSubscribers)
            {
                if (subscriber.IsBeingUnsubscribed || !this.eventSubscribers.TryGetValue(subscriber, out delegate2))
                {
                    return;
                }
                subscriber.IsBeingUnsubscribed = true;
            }
            if ((delegate2 != null) && (subscriber.SourceObject != null))
            {
                subscriber.OnPSEventUnsubscribed(subscriber.SourceObject, new PSEventUnsubscribedEventArgs(subscriber));
                EventInfo info = null;
                Type sourceObject = subscriber.SourceObject as Type;
                if (sourceObject == null)
                {
                    sourceObject = subscriber.SourceObject.GetType();
                }
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                info = sourceObject.GetEvent(subscriber.EventName, bindingAttr);
                if ((info != null) && (delegate2 != null))
                {
                    info.RemoveEventHandler(subscriber.SourceObject, delegate2);
                }
            }
            if (!skipDraining)
            {
                this.DrainPendingActions(subscriber);
            }
            if (subscriber.Action != null)
            {
                subscriber.Action.NotifyJobStopped();
            }
            lock (this.eventSubscribers)
            {
                this.eventSubscribers[subscriber] = null;
                this.eventSubscribers.Remove(subscriber);
                lock (this.engineEventSubscribers)
                {
                    if (PSEngineEvent.EngineEvents.Contains(subscriber.SourceIdentifier))
                    {
                        this.engineEventSubscribers[subscriber.SourceIdentifier].Remove(subscriber);
                    }
                }
            }
        }

        internal bool IsExecutingEventAction
        {
            get
            {
                return (this.processingAction != null);
            }
        }

        public override List<PSEventSubscriber> Subscribers
        {
            get
            {
                List<PSEventSubscriber> list = new List<PSEventSubscriber>();
                lock (this.eventSubscribers)
                {
                    foreach (PSEventSubscriber subscriber in this.eventSubscribers.Keys)
                    {
                        list.Add(subscriber);
                    }
                }
                return list;
            }
        }
    }
}

