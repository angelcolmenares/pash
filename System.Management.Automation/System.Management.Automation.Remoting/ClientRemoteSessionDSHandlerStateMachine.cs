namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Timers;

    internal class ClientRemoteSessionDSHandlerStateMachine
    {
        private Queue<RemoteSessionStateEventArgs> _clientRemoteSessionStateChangeQueue;
        private System.Timers.Timer _keyExchangeTimer;
        private RemoteSessionState _state;
        private EventHandler<RemoteSessionStateMachineEventArgs>[,] _stateMachineHandle;
        [TraceSource("CRSessionFSM", "CRSessionFSM")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSessionFSM", "CRSessionFSM");
        private bool eventsInProcess;
        private Guid id;
        private bool keyExchanged;
        private bool pendingDisconnect;
        private Queue<RemoteSessionStateMachineEventArgs> processPendingEventsQueue;
        private object syncObject;

        internal event EventHandler<RemoteSessionStateEventArgs> StateChanged;

        internal ClientRemoteSessionDSHandlerStateMachine()
        {
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
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray29 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray30 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray31 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray32 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray33 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray34 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray35 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray36 = null;
			EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray37 = null;
            this.processPendingEventsQueue = new Queue<RemoteSessionStateMachineEventArgs>();
            this.syncObject = new object();
            this._clientRemoteSessionStateChangeQueue = new Queue<RemoteSessionStateEventArgs>();
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
				EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray9 = null;
				IntPtr ptr9 = IntPtr.Zero;
                (handlerArray = this._stateMachineHandle)[(int) (ptr = (IntPtr) i), 0x11] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray[(int) ptr, 0x11], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoFatal));
                (handlerArray2 = this._stateMachineHandle)[(int) (ptr2 = (IntPtr) i), 9] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray2[(int) ptr2, 9], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose));
                (handlerArray3 = this._stateMachineHandle)[(int) (ptr3 = (IntPtr) i), 11] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray3[(int) ptr3, 11], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
                (handlerArray4 = this._stateMachineHandle)[(int) (ptr4 = (IntPtr) i), 10] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray4[(int) ptr4, 10], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
                (handlerArray5 = this._stateMachineHandle)[(int) (ptr5 = (IntPtr) i), 14] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray5[(int) ptr5, 14], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
                (handlerArray6 = this._stateMachineHandle)[(int) (ptr6 = (IntPtr) i), 15] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray6[(int) ptr6, 15], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
                (handlerArray7 = this._stateMachineHandle)[(int) (ptr7 = (IntPtr) i), 0x10] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray7[(int) ptr7, 0x10], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
                (handlerArray8 = this._stateMachineHandle)[(int) (ptr8 = (IntPtr) i), 1] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray8[(int) ptr8, 1], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoCreateSession));
                (handlerArray9 = this._stateMachineHandle)[(int) (ptr9 = (IntPtr) i), 2] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray9[(int) ptr9, 2], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoConnectSession));
            }
            (handlerArray10 = this._stateMachineHandle)[1, 3] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray10[1, 3], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationSending));
            (handlerArray11 = this._stateMachineHandle)[1, 4] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray11[1, 4], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationSending));
            (handlerArray12 = this._stateMachineHandle)[4, 5] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray12[4, 5], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray13 = this._stateMachineHandle)[5, 5] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray13[5, 5], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray14 = this._stateMachineHandle)[6, 6] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray14[6, 6], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray15 = this._stateMachineHandle)[7, 7] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray15[7, 7], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray16 = this._stateMachineHandle)[7, 13] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray16[7, 13], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
            (handlerArray17 = this._stateMachineHandle)[2, 12] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray17[2, 12], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray18 = this._stateMachineHandle)[9, 10] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray18[9, 10], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray19 = this._stateMachineHandle)[11, 0x19] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray19[11, 0x19], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoDisconnect));
            (handlerArray20 = this._stateMachineHandle)[0x10, 0x1a] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray20[0x10, 0x1a], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray21 = this._stateMachineHandle)[0x10, 0x1b] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray21[0x10, 0x1b], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray22 = this._stateMachineHandle)[0x11, 0x1c] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray22[0x11, 0x1c], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoReconnect));
            (handlerArray23 = this._stateMachineHandle)[0x12, 0x1d] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray23[0x12, 0x1d], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray24 = this._stateMachineHandle)[0x12, 30] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray24[0x12, 30], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
            (handlerArray25 = this._stateMachineHandle)[0x10, 0x1f] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray25[0x10, 0x1f], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoRCDisconnectStarted));
            (handlerArray26 = this._stateMachineHandle)[0x11, 0x1f] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray26[0x11, 0x1f], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoRCDisconnectStarted));
            (handlerArray27 = this._stateMachineHandle)[11, 0x1f] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray27[11, 0x1f], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoRCDisconnectStarted));
            (handlerArray28 = this._stateMachineHandle)[0x13, 0x1a] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray28[0x13, 0x1a], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray29 = this._stateMachineHandle)[12, 0x19] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray29[12, 0x19], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoDisconnectDuringKeyExchange));
            (handlerArray30 = this._stateMachineHandle)[14, 0x19] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray30[14, 0x19], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoDisconnectDuringKeyExchange));
            (handlerArray31 = this._stateMachineHandle)[11, 0x17] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray31[11, 0x17], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray32 = this._stateMachineHandle)[11, 0x13] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray32[11, 0x13], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray33 = this._stateMachineHandle)[11, 20] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray33[11, 20], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
            (handlerArray34 = this._stateMachineHandle)[12, 0x15] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray34[12, 0x15], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray35 = this._stateMachineHandle)[14, 0x13] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray35[14, 0x13], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler));
            (handlerArray36 = this._stateMachineHandle)[12, 0x16] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray36[12, 0x16], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
            (handlerArray37 = this._stateMachineHandle)[14, 20] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray37[14, 20], new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler));
            for (int j = 0; j < this._stateMachineHandle.GetLength(0); j++)
            {
                for (int k = 0; k < this._stateMachineHandle.GetLength(1); k++)
                {
                    if (this._stateMachineHandle[j, k] == null)
                    {
                        EventHandler<RemoteSessionStateMachineEventArgs>[,] handlerArray38 = null;
                        IntPtr ptr10 = IntPtr.Zero;
						IntPtr ptr11 = IntPtr.Zero;
                        (handlerArray38 = this._stateMachineHandle)[(int) (ptr10 = (IntPtr) j), (int) (ptr11 = (IntPtr) k)] = (EventHandler<RemoteSessionStateMachineEventArgs>) Delegate.Combine(handlerArray38[(int) ptr10, (int) ptr11], new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose));
                    }
                }
            }
			this.id = Guid.NewGuid();
            this.SetState(RemoteSessionState.Idle, null);
        }

        internal bool CanByPassRaiseEvent(RemoteSessionStateMachineEventArgs arg)
        {
            if ((arg.StateEvent != RemoteSessionEvent.MessageReceived) || (((this._state != RemoteSessionState.Established) && (this._state != RemoteSessionState.EstablishedAndKeyReceived)) && (((this._state != RemoteSessionState.EstablishedAndKeySent) && (this._state != RemoteSessionState.Disconnecting)) && (this._state != RemoteSessionState.Disconnected))))
            {
                return false;
            }
            return true;
        }

        private void CleanAll()
        {

        }

        private void DoClose(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            using (_trace.TraceEventHandlers())
            {
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
                    case RemoteSessionState.Disconnecting:
                    case RemoteSessionState.Disconnected:
                    case RemoteSessionState.Reconnecting:
                    case RemoteSessionState.RCDisconnecting:
                        this.SetState(RemoteSessionState.ClosingConnection, arg.Reason);
                        break;
                    case RemoteSessionState.ClosingConnection:
                    case RemoteSessionState.Closed:
                        break;
                    default:
                    {
                        PSRemotingTransportException reason = new PSRemotingTransportException(arg.Reason, RemotingErrorIdStrings.ForceClosed, new object[0]);
                        this.SetState(RemoteSessionState.Closed, reason);
                        break;
                    }
                }
                this.CleanAll();
            }
        }

        private void DoConnectSession(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (this.State == RemoteSessionState.Idle)
                {
                    RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSendingOnConnect);
                    this.RaiseEvent(args, false);
                }
            }
        }

        private void DoCreateSession(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            using (_trace.TraceEventHandlers())
            {
                if (this.State == RemoteSessionState.Idle)
                {
                    RemoteSessionStateMachineEventArgs args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSending);
                    this.RaiseEvent(args, false);
                }
            }
        }

        private void DoDisconnect(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            this.SetState(RemoteSessionState.Disconnecting, null);
        }

        private void DoDisconnectDuringKeyExchange(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            this.pendingDisconnect = true;
        }

        private void DoFatal(object sender, RemoteSessionStateMachineEventArgs eventArgs)
        {
            PSRemotingDataStructureException reason = new PSRemotingDataStructureException(eventArgs.Reason, RemotingErrorIdStrings.FatalErrorCausingClose, new object[0]);
            RemoteSessionStateMachineEventArgs arg = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, reason);
            this.RaiseEvent(arg, false);
        }

        private void DoNegotiationSending(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            if (arg.StateEvent == RemoteSessionEvent.NegotiationSending)
            {
                this.SetState(RemoteSessionState.NegotiationSending, null);
            }
            else if (arg.StateEvent == RemoteSessionEvent.NegotiationSendingOnConnect)
            {
                this.SetState(RemoteSessionState.NegotiationSendingOnConnect, null);
            }
        }

        private void DoRCDisconnectStarted(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            if ((this.State != RemoteSessionState.Disconnecting) && (this.State != RemoteSessionState.Disconnected))
            {
                this.SetState(RemoteSessionState.RCDisconnecting, null);
            }
        }

        private void DoReconnect(object sender, RemoteSessionStateMachineEventArgs arg)
        {
            this.SetState(RemoteSessionState.Reconnecting, null);
        }

        private void HandleKeyExchangeTimeout(object sender, ElapsedEventArgs eventArgs)
        {
            this._keyExchangeTimer.Dispose();
            PSRemotingDataStructureException reason = new PSRemotingDataStructureException(RemotingErrorIdStrings.ClientKeyExchangeFailed);
            this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed, reason), false);
        }

        private void ProcessEvents()
        {
            RemoteSessionStateMachineEventArgs arg = null;
            do
            {
                lock (this.syncObject)
                {
                    if (this.processPendingEventsQueue.Count == 0)
                    {
                        this.eventsInProcess = false;
                        break;
                    }
                    arg = this.processPendingEventsQueue.Dequeue();
                }
                try
                {
                    this.RaiseEventPrivate(arg);
                }
                catch (Exception exception)
                {
                    PSRemotingDataStructureException reason = new PSRemotingDataStructureException(exception, RemotingErrorIdStrings.FatalErrorCausingClose, new object[0]);
                    RemoteSessionStateMachineEventArgs args2 = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, reason);
                    this.RaiseEvent(args2, true);
                }
                this.RaiseStateMachineEvents();
            }
            while (this.eventsInProcess);
        }

        internal void RaiseEvent(RemoteSessionStateMachineEventArgs arg, bool clearQueuedEvents = false)
        {
            lock (this.syncObject)
            {
                _trace.WriteLine("Event recieved : {0} for {1}", new object[] { arg.StateEvent, this.id });
                if (clearQueuedEvents)
                {
                    this.processPendingEventsQueue.Clear();
                }
                this.processPendingEventsQueue.Enqueue(arg);
                if (!this.eventsInProcess)
                {
                    this.eventsInProcess = true;
                }
                else
                {
                    return;
                }
            }
            this.ProcessEvents();
        }

        private void RaiseEventPrivate (RemoteSessionStateMachineEventArgs arg)
		{
			if (arg == null) {
				throw PSTraceSource.NewArgumentNullException ("arg");
			}
			EventHandler<RemoteSessionStateMachineEventArgs> handler = this._stateMachineHandle [(int)this.State, (int)arg.StateEvent];
			if (arg.StateEvent == RemoteSessionEvent.MessageReceived) return;
			if (handler != null) {
				_trace.WriteLine ("Before calling state machine event handler: state = {0}, event = {1}, id = {2}", new object[] {
					this.State,
					arg.StateEvent,
					this.id
				});
				handler (this, arg);
				_trace.WriteLine ("After calling state machine event handler: state = {0}, event = {1}, id = {2}", new object[] {
					this.State,
					arg.StateEvent,
					this.id
				});
			} else {
				_trace.WriteLine ("Could not call state machine event handler: state = {0}, event = {1}, id = {2}", new object[] {
					this.State,
					arg.StateEvent,
					this.id
				});
			}
        }

        private void RaiseStateMachineEvents()
        {
            RemoteSessionStateEventArgs eventArgs = null;
            while (this._clientRemoteSessionStateChangeQueue.Count > 0)
            {
                eventArgs = this._clientRemoteSessionStateChangeQueue.Dequeue();
                this.StateChanged.SafeInvoke<RemoteSessionStateEventArgs>(this, eventArgs);
            }
        }

        private void SetState(RemoteSessionState newState, Exception reason)
        {
            RemoteSessionState state = this._state;
            if (newState != state)
            {
                this._state = newState;
                _trace.WriteLine("state machine state transition: from state {0} to state {1}", new object[] { state, this._state });
                RemoteSessionStateInfo remoteSessionStateInfo = new RemoteSessionStateInfo(this._state, reason);
                RemoteSessionStateEventArgs item = new RemoteSessionStateEventArgs(remoteSessionStateInfo);
                this._clientRemoteSessionStateChangeQueue.Enqueue(item);
            }
        }

        private void SetStateHandler(object sender, RemoteSessionStateMachineEventArgs eventArgs)
        {
            switch (eventArgs.StateEvent)
            {
                case RemoteSessionEvent.NegotiationSendCompleted:
                    this.SetState(RemoteSessionState.NegotiationSent, null);
                    return;

                case RemoteSessionEvent.NegotiationReceived:
                    if (eventArgs.RemoteSessionCapability == null)
                    {
                        throw PSTraceSource.NewArgumentException("eventArgs");
                    }
                    this.SetState(RemoteSessionState.NegotiationReceived, null);
                    return;

                case RemoteSessionEvent.NegotiationCompleted:
                    this.SetState(RemoteSessionState.Established, null);
                    return;

                case RemoteSessionEvent.NegotiationPending:
                case RemoteSessionEvent.Close:
                case RemoteSessionEvent.KeySendFailed:
                case RemoteSessionEvent.KeyReceiveFailed:
                case RemoteSessionEvent.KeyRequestFailed:
                case RemoteSessionEvent.DisconnectStart:
                case RemoteSessionEvent.ReconnectStart:
                    break;

                case RemoteSessionEvent.CloseCompleted:
                    this.SetState(RemoteSessionState.Closed, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.CloseFailed:
                    this.SetState(RemoteSessionState.Closed, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.ConnectFailed:
                    this.SetState(RemoteSessionState.ClosingConnection, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.KeySent:
                    if ((this._state != RemoteSessionState.Established) && (this._state != RemoteSessionState.EstablishedAndKeyRequested))
                    {
                        break;
                    }
                    this.SetState(RemoteSessionState.EstablishedAndKeySent, eventArgs.Reason);
                    this._keyExchangeTimer = new System.Timers.Timer();
                    this._keyExchangeTimer.AutoReset = false;
                    this._keyExchangeTimer.Elapsed += new ElapsedEventHandler(this.HandleKeyExchangeTimeout);
                    this._keyExchangeTimer.Interval = 180000.0;
                    return;

                case RemoteSessionEvent.KeyReceived:
                    if (this._state != RemoteSessionState.EstablishedAndKeySent)
                    {
                        break;
                    }
                    if (this._keyExchangeTimer != null)
                    {
                        this._keyExchangeTimer.Enabled = false;
                        this._keyExchangeTimer.Dispose();
                        this._keyExchangeTimer = null;
                    }
                    this.keyExchanged = true;
                    this.SetState(RemoteSessionState.Established, eventArgs.Reason);
                    if (!this.pendingDisconnect)
                    {
                        break;
                    }
                    this.pendingDisconnect = false;
                    this.DoDisconnect(sender, eventArgs);
                    return;

                case RemoteSessionEvent.KeyRequested:
                    if (this._state != RemoteSessionState.Established)
                    {
                        break;
                    }
                    this.SetState(RemoteSessionState.EstablishedAndKeyRequested, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.DisconnectCompleted:
                    if ((this._state != RemoteSessionState.Disconnecting) && (this._state != RemoteSessionState.RCDisconnecting))
                    {
                        break;
                    }
                    this.SetState(RemoteSessionState.Disconnected, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.DisconnectFailed:
                    if (this._state != RemoteSessionState.Disconnecting)
                    {
                        break;
                    }
                    this.SetState(RemoteSessionState.Disconnected, eventArgs.Reason);
                    return;

                case RemoteSessionEvent.ReconnectCompleted:
                    if (this._state == RemoteSessionState.Reconnecting)
                    {
                        this.SetState(RemoteSessionState.Established, eventArgs.Reason);
                    }
                    break;

                default:
                    return;
            }
        }

        private void SetStateToClosedHandler(object sender, RemoteSessionStateMachineEventArgs eventArgs)
        {
            if ((eventArgs.StateEvent != RemoteSessionEvent.NegotiationTimeout) || (this.State != RemoteSessionState.Established))
            {
                this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, eventArgs.Reason), false);
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

