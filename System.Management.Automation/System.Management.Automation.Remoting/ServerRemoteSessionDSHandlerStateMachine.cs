namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Timers;

    internal class ServerRemoteSessionDSHandlerStateMachine
    {
        private Timer _keyExchangeTimer;
        private ServerRemoteSession _session;
        private RemoteSessionState _state;
        private EventHandler<RemoteSessionStateMachineEventArgs>[,] _stateMachineHandle;
        private object _syncObject;
        [TraceSource("ServerRemoteSessionDSHandlerStateMachine", "ServerRemoteSessionDSHandlerStateMachine")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("ServerRemoteSessionDSHandlerStateMachine", "ServerRemoteSessionDSHandlerStateMachine");
        private bool eventsInProcess;
        private Queue<RemoteSessionStateMachineEventArgs> processPendingEventsQueue;

        internal ServerRemoteSessionDSHandlerStateMachine(ServerRemoteSession session)
        {
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray9 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray10 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray11 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray12 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray13 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray14 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray15 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray16 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray17 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray18 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray19 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray20 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray21 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray22 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray23 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray24 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray25 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray26 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray27 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray28 = null;
            this.processPendingEventsQueue = new Queue<RemoteSessionStateMachineEventArgs>();
            if (session == null)
            {
                throw PSTraceSource.NewArgumentNullException("session");
            }
            this._session = session;
            this._syncObject = new object();
            this._stateMachineHandle = new EventHandler<RemoteSessionStateMachineEventArgs>[20, 0x20];
            for (int i = 0; i < this._stateMachineHandle.GetLength(0); i++)
            {
                EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray = null;
				IntPtr ptr = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray2 = null;
				IntPtr ptr2 = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray3 = null;
				IntPtr ptr3 = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray4 = null;
				IntPtr ptr4 = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray5 = null;
				IntPtr ptr5 = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray6 = null;
				IntPtr ptr6 = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray7 = null;
				IntPtr ptr7 = IntPtr.Zero;
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray8 = null;
				IntPtr ptr8 = IntPtr.Zero;
                (handlerArray = this._stateMachineHandle)[(int) (ptr = (IntPtr) i), 0x11] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray[(int) ptr, 0x11], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoFatalError));
                (handlerArray2 = this._stateMachineHandle)[(int) (ptr2 = (IntPtr) i), 9] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray2[(int) ptr2, 9], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose));
                (handlerArray3 = this._stateMachineHandle)[(int) (ptr3 = (IntPtr) i), 11] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray3[(int) ptr3, 11], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoCloseFailed));
                (handlerArray4 = this._stateMachineHandle)[(int) (ptr4 = (IntPtr) i), 10] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray4[(int) ptr4, 10], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoCloseCompleted));
                (handlerArray5 = this._stateMachineHandle)[(int) (ptr5 = (IntPtr) i), 14] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray5[(int) ptr5, 14], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationTimeout));
                (handlerArray6 = this._stateMachineHandle)[(int) (ptr6 = (IntPtr) i), 15] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray6[(int) ptr6, 15], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoSendFailed));
                (handlerArray7 = this._stateMachineHandle)[(int) (ptr7 = (IntPtr) i), 0x10] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray7[(int) ptr7, 0x10], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoReceiveFailed));
                (handlerArray8 = this._stateMachineHandle)[(int) (ptr8 = (IntPtr) i), 2] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray8[(int) ptr8, 2], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoConnect));
            }
            (handlerArray9 = this._stateMachineHandle)[1, 1] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray9[1, 1], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoCreateSession));
            (handlerArray10 = this._stateMachineHandle)[8, 6] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray10[8, 6], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationReceived));
            (handlerArray11 = this._stateMachineHandle)[7, 3] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray11[7, 3], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationSending));
            (handlerArray12 = this._stateMachineHandle)[4, 5] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray12[4, 5], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationCompleted));
            (handlerArray13 = this._stateMachineHandle)[6, 7] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray13[6, 7], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoEstablished));
            (handlerArray14 = this._stateMachineHandle)[6, 8] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray14[6, 8], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationPending));
            (handlerArray15 = this._stateMachineHandle)[11, 0x12] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray15[11, 0x12], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoMessageReceived));
            (handlerArray16 = this._stateMachineHandle)[7, 13] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray16[7, 13], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationFailed));
            (handlerArray17 = this._stateMachineHandle)[2, 12] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray17[2, 12], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoConnectFailed));
            (handlerArray18 = this._stateMachineHandle)[11, 0x15] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray18[11, 0x15], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray19 = this._stateMachineHandle)[11, 0x17] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray19[11, 0x17], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray20 = this._stateMachineHandle)[11, 0x16] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray20[11, 0x16], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray21 = this._stateMachineHandle)[14, 0x15] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray21[14, 0x15], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray22 = this._stateMachineHandle)[14, 0x13] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray22[14, 0x13], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray23 = this._stateMachineHandle)[14, 0x16] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray23[14, 0x16], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray24 = this._stateMachineHandle)[13, 20] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray24[13, 20], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray25 = this._stateMachineHandle)[13, 0x13] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray25[13, 0x13], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray26 = this._stateMachineHandle)[15, 0x15] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray26[15, 0x15], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray27 = this._stateMachineHandle)[15, 0x17] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray27[15, 0x17], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            (handlerArray28 = this._stateMachineHandle)[15, 0x16] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray28[15, 0x16], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange));
            for (int j = 0; j < this._stateMachineHandle.GetLength(0); j++)
            {
                for (int k = 0; k < this._stateMachineHandle.GetLength(1); k++)
                {
                    if (this._stateMachineHandle[j, k] == null)
                    {
						EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray29 = null;
                        IntPtr ptr9 = IntPtr.Zero;
                        IntPtr ptr10 = IntPtr.Zero;
                        (handlerArray29 = this._stateMachineHandle)[(int) (ptr9 = (IntPtr) j), (int) (ptr10 = (IntPtr) k)] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray29[(int) ptr9, (int) ptr10], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose));
                    }
                }
            }
            this.SetState(RemoteSessionState.Idle, null);
        }

        internal bool CanByPassRaiseEvent(RemoteSessionStateMachineEventArgs arg)
        {
            if ((arg.StateEvent != RemoteSessionEvent.MessageReceived) || (((this._state != RemoteSessionState.Established) && (this._state != RemoteSessionState.EstablishedAndKeySent)) && ((this._state != RemoteSessionState.EstablishedAndKeyReceived) && (this._state != RemoteSessionState.EstablishedAndKeyExchanged))))
            {
                return false;
            }
            return true;
        }

        private void CleanAll()
        {
        }

        private void DoClose(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                switch (this._state)
                {
                    case RemoteSessionState.Connecting:
                    case RemoteSessionState.Connected:
                    case RemoteSessionState.NegotiationSending:
                    case RemoteSessionState.NegotiationSent:
                    case RemoteSessionState.NegotiationReceived:
                    case RemoteSessionState.Established:
                    case RemoteSessionState.EstablishedAndKeySent:
                    case RemoteSessionState.EstablishedAndKeyReceived:
                    case RemoteSessionState.EstablishedAndKeyExchanged:
                        this.SetState(RemoteSessionState.ClosingConnection, fsmEventArg.Reason);
                        this._session.SessionDataStructureHandler.CloseConnectionAsync(fsmEventArg.Reason);
                        break;

                    case RemoteSessionState.ClosingConnection:
                    case RemoteSessionState.Closed:
                        break;

                    default:
                    {
                        Exception reasion = new PSRemotingTransportException(fsmEventArg.Reason, RemotingErrorIdStrings.ForceClosed, new object[0]);
                        this.SetState(RemoteSessionState.Closed, reasion);
                        break;
                    }
                }
                this.CleanAll();
            }
        }

        private void DoCloseCompleted(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                this.SetState(RemoteSessionState.Closed, fsmEventArg.Reason);
                this._session.Close(fsmEventArg);
                this.CleanAll();
            }
        }

        private void DoCloseFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                this.SetState(RemoteSessionState.Closed, fsmEventArg.Reason);
                this.CleanAll();
            }
        }

        private void DoConnect(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            if ((this._state != RemoteSessionState.Closed) && (this._state != RemoteSessionState.ClosingConnection))
            {
                this._session.HandlePostConnect();
            }
        }

        private void DoConnectFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                if (fsmEventArg.StateEvent != RemoteSessionEvent.ConnectFailed)
                {
                    throw PSTraceSource.NewArgumentException("fsmEventArg");
                }
                throw PSTraceSource.NewInvalidOperationException();
            }
        }

        private void DoCreateSession(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                this.DoNegotiationPending(sender, fsmEventArg);
            }
        }

        private void DoEstablished(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                if (fsmEventArg.StateEvent != RemoteSessionEvent.NegotiationCompleted)
                {
                    throw PSTraceSource.NewArgumentException("fsmEventArg");
                }
                if (this._state != RemoteSessionState.NegotiationSent)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                this.SetState(RemoteSessionState.Established, null);
            }
        }

        private void DoFatalError(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                if (fsmEventArg.StateEvent != RemoteSessionEvent.FatalError)
                {
                    throw PSTraceSource.NewArgumentException("fsmEventArg");
                }
                this.DoClose(this, fsmEventArg);
            }
        }

        private void DoKeyExchange(object sender, RemoteSessionStateMachineEventArgs eventArgs)
        {
            switch (eventArgs.StateEvent)
            {
                case RemoteSessionEvent.KeySent:
                    if (this._state != RemoteSessionState.EstablishedAndKeyReceived)
                    {
                        break;
                    }
                    this.SetState(RemoteSessionState.EstablishedAndKeyExchanged, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.KeySendFailed:
                    this.DoClose(this, eventArgs);
                    break;

                case RemoteSessionEvent.KeyReceived:
                    if ((this._state == RemoteSessionState.EstablishedAndKeyRequested) && (this._keyExchangeTimer != null))
                    {
                        this._keyExchangeTimer.Enabled = false;
                        this._keyExchangeTimer.Dispose();
                        this._keyExchangeTimer = null;
                    }
                    this.SetState(RemoteSessionState.EstablishedAndKeyReceived, eventArgs.Reason);
                    this._session.SendEncryptedSessionKey();
                    return;

                case RemoteSessionEvent.KeyReceiveFailed:
                    if ((this._state != RemoteSessionState.Established) && (this._state != RemoteSessionState.EstablishedAndKeyExchanged))
                    {
                        this.DoClose(this, eventArgs);
                        return;
                    }
                    return;

                case RemoteSessionEvent.KeyRequested:
                    if ((this._state != RemoteSessionState.Established) && (this._state != RemoteSessionState.EstablishedAndKeyExchanged))
                    {
                        break;
                    }
                    this.SetState(RemoteSessionState.EstablishedAndKeyRequested, eventArgs.Reason);
                    this._keyExchangeTimer = new Timer();
                    this._keyExchangeTimer.AutoReset = false;
                    this._keyExchangeTimer.Elapsed += new ElapsedEventHandler(this.HandleKeyExchangeTimeout);
                    this._keyExchangeTimer.Interval = 240000.0;
                    return;

                default:
                    return;
            }
        }

        internal void DoMessageReceived(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                Guid runspacePoolId;
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                if (fsmEventArg.RemoteData == null)
                {
                    throw PSTraceSource.NewArgumentException("fsmEventArg");
                }
                RemotingTargetInterface targetInterface = fsmEventArg.RemoteData.TargetInterface;
                RemotingDataType dataType = fsmEventArg.RemoteData.DataType;
                RemoteDataEventArgs arg = null;
                switch (targetInterface)
                {
                    case RemotingTargetInterface.Session:
                        switch (dataType)
                        {
                            case RemotingDataType.CreateRunspacePool:
                            {
                                arg = new RemoteDataEventArgs(fsmEventArg.RemoteData);
                                this._session.SessionDataStructureHandler.RaiseDataReceivedEvent(arg);
                            }
                            break;
                        }
                        return;

                    case RemotingTargetInterface.RunspacePool:
                    {
                        runspacePoolId = fsmEventArg.RemoteData.RunspacePoolId;
                        ServerRunspacePoolDriver runspacePoolDriver = this._session.GetRunspacePoolDriver(runspacePoolId);
                        if (runspacePoolDriver == null)
                        {
                            break;
                        }
                        runspacePoolDriver.DataStructureHandler.ProcessReceivedData(fsmEventArg.RemoteData);
                        return;
                    }
                    case RemotingTargetInterface.PowerShell:
                        runspacePoolId = fsmEventArg.RemoteData.RunspacePoolId;
                        this._session.GetRunspacePoolDriver(runspacePoolId).DataStructureHandler.DispatchMessageToPowerShell(fsmEventArg.RemoteData);
                        return;

                    default:
                        goto Label_0151;
                }
                _trace.WriteLine("Server received data for Runspace (id: {0}), \r\n                                but the Runspace cannot be found", new object[] { runspacePoolId });
                PSRemotingDataStructureException reason = new PSRemotingDataStructureException(RemotingErrorIdStrings.RunspaceCannotBeFound, new object[] { runspacePoolId });
                RemoteSessionStateMachineEventArgs args2 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError, reason);
                this.RaiseEvent(args2);
                return;
            Label_0151:;
                _trace.WriteLine("Server received data unknown targetInterface: {0}", new object[] { targetInterface });
                PSRemotingDataStructureException exception2 = new PSRemotingDataStructureException(RemotingErrorIdStrings.ReceivedUnsupportedRemotingTargetInterfaceType, new object[] { targetInterface });
                RemoteSessionStateMachineEventArgs args3 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError, exception2);
                this.RaiseEvent(args3);
            }
        }

        private void DoNegotiationCompleted(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                this.SetState(RemoteSessionState.NegotiationSent, null);
            }
        }

        private void DoNegotiationFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close);
                this.RaiseEventPrivate(args);
            }
        }

        private void DoNegotiationPending(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                this.SetState(RemoteSessionState.NegotiationPending, null);
            }
        }

        private void DoNegotiationReceived(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                if (fsmEventArg.StateEvent != RemoteSessionEvent.NegotiationReceived)
                {
                    throw PSTraceSource.NewArgumentException("fsmEventArg");
                }
                if (fsmEventArg.RemoteSessionCapability == null)
                {
                    throw PSTraceSource.NewArgumentException("fsmEventArg");
                }
                this.SetState(RemoteSessionState.NegotiationReceived, null);
            }
        }

        private void DoNegotiationSending(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            if (fsmEventArg == null)
            {
                throw PSTraceSource.NewArgumentNullException("fsmEventArg");
            }
            this.SetState(RemoteSessionState.NegotiationSending, null);
            this._session.SessionDataStructureHandler.SendNegotiationAsync();
        }

        private void DoNegotiationTimeout(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                if (this._state != RemoteSessionState.Established)
                {
                    RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close);
                    this.RaiseEventPrivate(args);
                }
            }
        }

        private void DoReceiveFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close);
                this.RaiseEventPrivate(args);
            }
        }

        private void DoSendFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (fsmEventArg == null)
                {
                    throw PSTraceSource.NewArgumentNullException("fsmEventArg");
                }
                RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close);
                this.RaiseEventPrivate(args);
            }
        }

        private void HandleKeyExchangeTimeout(object sender, ElapsedEventArgs eventArgs)
        {
            this._keyExchangeTimer.Dispose();
            PSRemotingDataStructureException reason = new PSRemotingDataStructureException(RemotingErrorIdStrings.ServerKeyExchangeFailed);
            this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed, reason));
        }

        private void ProcessEvents()
        {
            RemoteSessionStateMachineEventArgs fsmEventArg = null;
            do
            {
                lock (this._syncObject)
                {
                    if (this.processPendingEventsQueue.Count == 0)
                    {
                        this.eventsInProcess = false;
                        break;
                    }
                    fsmEventArg = this.processPendingEventsQueue.Dequeue();
                }
                this.RaiseEventPrivate(fsmEventArg);
            }
            while (this.eventsInProcess);
        }

        internal void RaiseEvent(RemoteSessionStateMachineEventArgs fsmEventArg)
        {
            lock (this._syncObject)
            {
                _trace.WriteLine("Event received : {0}", new object[] { fsmEventArg.StateEvent });
                this.processPendingEventsQueue.Enqueue(fsmEventArg);
                if (this.eventsInProcess)
                {
                    return;
                }
                this.eventsInProcess = true;
            }
            this.ProcessEvents();
        }

        private void RaiseEventPrivate (RemoteSessionStateMachineEventArgs fsmEventArg)
		{
			if (fsmEventArg == null) {
				throw PSTraceSource.NewArgumentNullException ("fsmEventArg");
			}
			if (fsmEventArg.StateEvent == RemoteSessionEvent.Close) {
				_trace.WriteLine ("Closing Session");
			}
            EventHandler<RemoteSessionStateMachineEventArgs> handler = this._stateMachineHandle[(int) this._state, (int) fsmEventArg.StateEvent];
            if (handler != null)
            {
                _trace.WriteLine("Before calling state machine event handler: state = {0}, event = {1}", new object[] { this._state, fsmEventArg.StateEvent });
                handler(this, fsmEventArg);
                _trace.WriteLine("After calling state machine event handler: state = {0}, event = {1}", new object[] { this._state, fsmEventArg.StateEvent });
            }
        }

        private void SetState(RemoteSessionState newState, Exception reasion)
        {
            RemoteSessionState state = this._state;
            if (newState != state)
            {
				if (newState == RemoteSessionState.ClosingConnection || state == RemoteSessionState.ClosingConnection)
				{
					_trace.WriteLine ("Closing Server Session");
				}
                this._state = newState;
                _trace.WriteLine("state machine state transition: from state {0} to state {1}", new object[] { state, this._state });
            }
        }

        internal RemoteSessionState State
        {
            get
            {
                return this._state;
            }
        }
    }
}

