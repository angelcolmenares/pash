using System;
using System.Diagnostics;
using System.Runtime.Diagnostics;
using System.Security;
using System.Threading;

namespace System.Runtime
{
	internal abstract class ActionItem
	{
		[SecurityCritical]
		private SecurityContext context;

		private bool isScheduled;

		private bool lowPriority;

		public bool LowPriority
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.lowPriority;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			protected set
			{
				this.lowPriority = value;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ActionItem()
		{
		}

		[SecurityCritical]
		private SecurityContext ExtractContext()
		{
			SecurityContext securityContext = this.context;
			this.context = null;
			return securityContext;
		}

		[SecurityCritical]
		protected abstract void Invoke();

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public static void Schedule(Action<object> callback, object state)
		{
			ActionItem.Schedule(callback, state, false);
		}

		[SecuritySafeCritical]
		public static void Schedule(Action<object> callback, object state, bool lowPriority)
		{
			if (PartialTrustHelpers.ShouldFlowSecurityContext || WaitCallbackActionItem.ShouldUseActivity || Fx.Trace.IsEnd2EndActivityTracingEnabled)
			{
				(new ActionItem.DefaultActionItem(callback, state, lowPriority)).Schedule();
				return;
			}
			else
			{
				ActionItem.ScheduleCallback(callback, state, lowPriority);
				return;
			}
		}

		[SecurityCritical]
		protected void Schedule()
		{
			if (!this.isScheduled)
			{
				this.isScheduled = true;
				if (PartialTrustHelpers.ShouldFlowSecurityContext)
				{
					this.context = PartialTrustHelpers.CaptureSecurityContextNoIdentityFlow();
				}
				if (this.context == null)
				{
					this.ScheduleCallback(ActionItem.CallbackHelper.InvokeWithoutContextCallback);
					return;
				}
				else
				{
					this.ScheduleCallback(ActionItem.CallbackHelper.InvokeWithContextCallback);
					return;
				}
			}
			else
			{
				throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.ActionItemIsAlreadyScheduled));
			}
		}

		[SecurityCritical]
		private static void ScheduleCallback(Action<object> callback, object state, bool lowPriority)
		{
			if (!lowPriority)
			{
				IOThreadScheduler.ScheduleCallbackNoFlow(callback, state);
				return;
			}
			else
			{
				IOThreadScheduler.ScheduleCallbackLowPriNoFlow(callback, state);
				return;
			}
		}

		[SecurityCritical]
		private void ScheduleCallback(Action<object> callback)
		{
			ActionItem.ScheduleCallback(callback, this, this.lowPriority);
		}

		[SecurityCritical]
		protected void ScheduleWithContext(SecurityContext context)
		{
			if (context != null)
			{
				if (!this.isScheduled)
				{
					this.isScheduled = true;
					this.context = context.CreateCopy();
					this.ScheduleCallback(ActionItem.CallbackHelper.InvokeWithContextCallback);
					return;
				}
				else
				{
					throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.ActionItemIsAlreadyScheduled));
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("context");
			}
		}

		[SecurityCritical]
		protected void ScheduleWithoutContext()
		{
			if (!this.isScheduled)
			{
				this.isScheduled = true;
				this.ScheduleCallback(ActionItem.CallbackHelper.InvokeWithoutContextCallback);
				return;
			}
			else
			{
				throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.ActionItemIsAlreadyScheduled));
			}
		}

		[SecurityCritical]
		private static class CallbackHelper
		{
			private static Action<object> invokeWithContextCallback;

			private static Action<object> invokeWithoutContextCallback;

			private static ContextCallback onContextAppliedCallback;

			public static Action<object> InvokeWithContextCallback
			{
				get
				{
					if (ActionItem.CallbackHelper.invokeWithContextCallback == null)
					{
						ActionItem.CallbackHelper.invokeWithContextCallback = new Action<object>(ActionItem.CallbackHelper.InvokeWithContext);
					}
					return ActionItem.CallbackHelper.invokeWithContextCallback;
				}
			}

			public static Action<object> InvokeWithoutContextCallback
			{
				get
				{
					if (ActionItem.CallbackHelper.invokeWithoutContextCallback == null)
					{
						ActionItem.CallbackHelper.invokeWithoutContextCallback = new Action<object>(ActionItem.CallbackHelper.InvokeWithoutContext);
					}
					return ActionItem.CallbackHelper.invokeWithoutContextCallback;
				}
			}

			public static ContextCallback OnContextAppliedCallback
			{
				get
				{
					if (ActionItem.CallbackHelper.onContextAppliedCallback == null)
					{
						ActionItem.CallbackHelper.onContextAppliedCallback = new ContextCallback(ActionItem.CallbackHelper.OnContextApplied);
					}
					return ActionItem.CallbackHelper.onContextAppliedCallback;
				}
			}

			private static void InvokeWithContext(object state)
			{
				SecurityContext securityContext = ((ActionItem)state).ExtractContext();
				SecurityContext.Run(securityContext, ActionItem.CallbackHelper.OnContextAppliedCallback, state);
			}

			private static void InvokeWithoutContext(object state)
			{
				((ActionItem)state).Invoke();
				((ActionItem)state).isScheduled = false;
			}

			private static void OnContextApplied(object o)
			{
				((ActionItem)o).Invoke();
				((ActionItem)o).isScheduled = false;
			}
		}

		private class DefaultActionItem : ActionItem
		{
			[SecurityCritical]
			private Action<object> callback;

			[SecurityCritical]
			private object state;

			private bool flowLegacyActivityId;

			private Guid activityId;

			private EventTraceActivity eventTraceActivity;

			[SecuritySafeCritical]
			public DefaultActionItem(Action<object> callback, object state, bool isLowPriority)
			{
				base.LowPriority = isLowPriority;
				this.callback = callback;
				this.state = state;
				if (WaitCallbackActionItem.ShouldUseActivity)
				{
					this.flowLegacyActivityId = true;
					this.activityId = DiagnosticTraceBase.ActivityId;
				}
				if (Fx.Trace.IsEnd2EndActivityTracingEnabled)
				{
					this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate(false);
					if (TraceCore.ActionItemScheduledIsEnabled(Fx.Trace))
					{
						TraceCore.ActionItemScheduled(Fx.Trace, this.eventTraceActivity);
					}
				}
			}

			[SecurityCritical]
			protected override void Invoke()
			{
				if (this.flowLegacyActivityId || Fx.Trace.IsEnd2EndActivityTracingEnabled)
				{
					this.TraceAndInvoke();
					return;
				}
				else
				{
					this.callback(this.state);
					return;
				}
			}

			[SecurityCritical]
			private void TraceAndInvoke()
			{
				if (!this.flowLegacyActivityId)
				{
					Guid empty = Guid.Empty;
					bool flag = false;
					try
					{
						if (this.eventTraceActivity != null)
						{
							empty = Trace.CorrelationManager.ActivityId;
							flag = true;
							Trace.CorrelationManager.ActivityId = this.eventTraceActivity.ActivityId;
							if (TraceCore.ActionItemCallbackInvokedIsEnabled(Fx.Trace))
							{
								TraceCore.ActionItemCallbackInvoked(Fx.Trace, this.eventTraceActivity);
							}
						}
						this.callback(this.state);
					}
					finally
					{
						if (flag)
						{
							Trace.CorrelationManager.ActivityId = empty;
						}
					}
				}
				else
				{
					Guid activityId = DiagnosticTraceBase.ActivityId;
					try
					{
						DiagnosticTraceBase.ActivityId = this.activityId;
						this.callback(this.state);
					}
					finally
					{
						DiagnosticTraceBase.ActivityId = activityId;
					}
				}
			}
		}
	}
}