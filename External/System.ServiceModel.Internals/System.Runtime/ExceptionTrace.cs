using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Diagnostics;
using System.Security;

namespace System.Runtime
{
	internal class ExceptionTrace
	{
		private string eventSourceName;

		private readonly EtwDiagnosticTrace diagnosticTrace;

		private const ushort FailFastEventLogCategory = 6;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ExceptionTrace(string eventSourceName, EtwDiagnosticTrace diagnosticTrace)
		{
			this.eventSourceName = eventSourceName;
			this.diagnosticTrace = diagnosticTrace;
		}

		public ArgumentException Argument(string paramName, string message)
		{
			return this.TraceException<ArgumentException>(new ArgumentException(message, paramName));
		}

		public ArgumentNullException ArgumentNull(string paramName)
		{
			return this.TraceException<ArgumentNullException>(new ArgumentNullException(paramName));
		}

		public ArgumentNullException ArgumentNull(string paramName, string message)
		{
			return this.TraceException<ArgumentNullException>(new ArgumentNullException(paramName, message));
		}

		public ArgumentException ArgumentNullOrEmpty(string paramName)
		{
			return this.Argument(paramName, InternalSR.ArgumentNullOrEmpty(paramName));
		}

		public ArgumentOutOfRangeException ArgumentOutOfRange(string paramName, object actualValue, string message)
		{
			return this.TraceException<ArgumentOutOfRangeException>(new ArgumentOutOfRangeException(paramName, actualValue, message));
		}

		public Exception AsError(Exception exception)
		{
			AggregateException aggregateException = exception as AggregateException;
			if (aggregateException == null)
			{
				TargetInvocationException targetInvocationException = exception as TargetInvocationException;
				if (targetInvocationException == null || targetInvocationException.InnerException == null)
				{
					return this.TraceException<Exception>(exception);
				}
				else
				{
					return this.AsError(targetInvocationException.InnerException);
				}
			}
			else
			{
				return this.AsError<Exception>(aggregateException);
			}
		}

		public Exception AsError(Exception exception, string eventSource)
		{
			AggregateException aggregateException = exception as AggregateException;
			if (aggregateException == null)
			{
				TargetInvocationException targetInvocationException = exception as TargetInvocationException;
				if (targetInvocationException == null || targetInvocationException.InnerException == null)
				{
					return this.TraceException<Exception>(exception, eventSource);
				}
				else
				{
					return this.AsError(targetInvocationException.InnerException, eventSource);
				}
			}
			else
			{
				return this.AsError<Exception>(aggregateException, eventSource);
			}
		}

		public Exception AsError(TargetInvocationException targetInvocationException, string eventSource)
		{
			if (!Fx.IsFatal(targetInvocationException))
			{
				Exception innerException = targetInvocationException.InnerException;
				if (innerException == null)
				{
					return this.TraceException<Exception>(targetInvocationException, eventSource);
				}
				else
				{
					return this.AsError(innerException, eventSource);
				}
			}
			else
			{
				return targetInvocationException;
			}
		}

		public Exception AsError<TPreferredException>(AggregateException aggregateException)
		{
			return this.AsError<TPreferredException>(aggregateException, this.eventSourceName);
		}

		public Exception AsError<TPreferredException>(AggregateException aggregateException, string eventSource)
		{
			Exception innerException;
			if (!Fx.IsFatal(aggregateException))
			{
				ReadOnlyCollection<Exception> innerExceptions = aggregateException.Flatten().InnerExceptions;
				if (innerExceptions.Count != 0)
				{
					Exception item = null;
					foreach (Exception exception in innerExceptions)
					{
						TargetInvocationException targetInvocationException = exception as TargetInvocationException;
						if (targetInvocationException == null || targetInvocationException.InnerException == null)
						{
							innerException = exception;
						}
						else
						{
							innerException = targetInvocationException.InnerException;
						}
						Exception exception1 = innerException;
						if (exception1 is TPreferredException && item == null)
						{
							item = exception1;
						}
						this.TraceException<Exception>(exception1, eventSource);
					}
					if (item == null)
					{
						item = innerExceptions[0];
					}
					return item;
				}
				else
				{
					return this.TraceException<AggregateException>(aggregateException, eventSource);
				}
			}
			else
			{
				return aggregateException;
			}
		}

		public void AsInformation(Exception exception)
		{
			string str;
			EtwDiagnosticTrace etwDiagnosticTrace = this.diagnosticTrace;
			if (exception != null)
			{
				str = exception.ToString();
			}
			else
			{
				str = string.Empty;
			}
			TraceCore.HandledException(etwDiagnosticTrace, str, exception);
		}

		public void AsWarning(Exception exception)
		{
			string str;
			EtwDiagnosticTrace etwDiagnosticTrace = this.diagnosticTrace;
			if (exception != null)
			{
				str = exception.ToString();
			}
			else
			{
				str = string.Empty;
			}
			TraceCore.HandledExceptionWarning(etwDiagnosticTrace, str, exception);
		}

		[SecuritySafeCritical]
		private void BreakOnException(Exception exception)
		{
		}

		public ObjectDisposedException ObjectDisposed(string message)
		{
			return this.TraceException<ObjectDisposedException>(new ObjectDisposedException(null, message));
		}

		public void TraceEtwException(Exception exception, TraceEventType eventType)
		{
			string str;
			string empty;
			string str1;
			TraceEventType traceEventType = eventType;
			switch (traceEventType)
			{
				case TraceEventType.Critical:
				{
					if (!TraceCore.UnhandledExceptionIsEnabled(this.diagnosticTrace))
					{
						break;
					}
					EtwDiagnosticTrace etwDiagnosticTrace = this.diagnosticTrace;
					if (exception != null)
					{
						str = exception.ToString();
					}
					else
					{
						str = string.Empty;
					}
					TraceCore.EtwUnhandledException(etwDiagnosticTrace, str, exception);
					return;
				}
				case TraceEventType.Error:
				case TraceEventType.Warning:
				{
					if (!TraceCore.ThrowingExceptionIsEnabled(this.diagnosticTrace))
					{
						break;
					}
					EtwDiagnosticTrace etwDiagnosticTrace1 = this.diagnosticTrace;
					string str2 = this.eventSourceName;
					if (exception != null)
					{
						empty = exception.ToString();
					}
					else
					{
						empty = string.Empty;
					}
					TraceCore.ThrowingEtwException(etwDiagnosticTrace1, str2, empty, exception);
					return;
				}
				case TraceEventType.Critical | TraceEventType.Error:
				{
					if (!TraceCore.ThrowingExceptionVerboseIsEnabled(this.diagnosticTrace))
					{
						break;
					}
					EtwDiagnosticTrace etwDiagnosticTrace2 = this.diagnosticTrace;
					string str3 = this.eventSourceName;
					if (exception != null)
					{
						str1 = exception.ToString();
					}
					else
					{
						str1 = string.Empty;
					}
					TraceCore.ThrowingEtwExceptionVerbose(etwDiagnosticTrace2, str3, str1, exception);
					break;
				}
				default:
				{
				if (!TraceCore.ThrowingExceptionVerboseIsEnabled(this.diagnosticTrace))
				{
					break;
				}
				EtwDiagnosticTrace etwDiagnosticTrace2 = this.diagnosticTrace;
				string str3 = this.eventSourceName;
				if (exception != null)
				{
					str1 = exception.ToString();
				}
				else
				{
					str1 = string.Empty;
				}
				TraceCore.ThrowingEtwExceptionVerbose(etwDiagnosticTrace2, str3, str1, exception);
				break;
				}
			}
		}

		private TException TraceException<TException>(TException exception)
		where TException : Exception
		{
			return this.TraceException<TException>(exception, this.eventSourceName);
		}

		[SecuritySafeCritical]
		private TException TraceException<TException>(TException exception, string eventSource)
		where TException : Exception
		{
			string str;
			if (TraceCore.ThrowingExceptionIsEnabled(this.diagnosticTrace))
			{
				EtwDiagnosticTrace etwDiagnosticTrace = this.diagnosticTrace;
				string str1 = eventSource;
				if (exception != null)
				{
					str = exception.ToString();
				}
				else
				{
					str = string.Empty;
				}
				TraceCore.ThrowingException(etwDiagnosticTrace, str1, str, exception);
			}
			this.BreakOnException(exception);
			return exception;
		}

		internal void TraceFailFast(string message)
		{
			EventLogger eventLogger = new EventLogger(this.eventSourceName, this.diagnosticTrace);
			this.TraceFailFast(message, eventLogger);
		}

		internal void TraceFailFast(string message, EventLogger logger)
		{
			if (logger != null)
			{
				try
				{
					string str = null;
					try
					{
						try
						{
							str = (new StackTrace()).ToString();
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							str = exception.Message;
							if (Fx.IsFatal(exception))
							{
								throw;
							}
						}
					}
					finally
					{
						string[] strArrays = new string[2];
						strArrays[0] = message;
						strArrays[1] = str;
						logger.LogEvent(TraceEventType.Critical, 6, -1073676186, strArrays);
					}
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					string[] str1 = new string[1];
					str1[0] = exception2.ToString();
					logger.LogEvent(TraceEventType.Critical, 6, -1073676185, str1);
					if (Fx.IsFatal(exception2))
					{
						throw;
					}
				}
			}
		}

		public void TraceHandledException(Exception exception, TraceEventType traceEventType)
		{
			string str;
			string empty;
			string str1;
			string empty1;
			TraceEventType traceEventType1 = traceEventType;
			switch (traceEventType1)
			{
				case TraceEventType.Error:
				{
					if (!TraceCore.HandledExceptionErrorIsEnabled(this.diagnosticTrace))
					{
						break;
					}
					EtwDiagnosticTrace etwDiagnosticTrace = this.diagnosticTrace;
					if (exception != null)
					{
						str = exception.ToString();
					}
					else
					{
						str = string.Empty;
					}
					TraceCore.HandledExceptionError(etwDiagnosticTrace, str, exception);
					return;
				}
				case TraceEventType.Critical | TraceEventType.Error:
				{
					if (!TraceCore.HandledExceptionIsEnabled(this.diagnosticTrace))
					{
						break;
					}
					EtwDiagnosticTrace etwDiagnosticTrace1 = this.diagnosticTrace;
					if (exception != null)
					{
						empty = exception.ToString();
					}
					else
					{
						empty = string.Empty;
					}
					TraceCore.HandledException(etwDiagnosticTrace1, empty, exception);
					break;
				}
				case TraceEventType.Warning:
				{
					if (!TraceCore.HandledExceptionWarningIsEnabled(this.diagnosticTrace))
					{
						break;
					}
					EtwDiagnosticTrace etwDiagnosticTrace2 = this.diagnosticTrace;
					if (exception != null)
					{
						str1 = exception.ToString();
					}
					else
					{
						str1 = string.Empty;
					}
					TraceCore.HandledExceptionWarning(etwDiagnosticTrace2, str1, exception);
					return;
				}
				default:
				{
					if (traceEventType1 == TraceEventType.Verbose)
					{
						if (!TraceCore.HandledExceptionVerboseIsEnabled(this.diagnosticTrace))
						{
							break;
						}
						EtwDiagnosticTrace etwDiagnosticTrace3 = this.diagnosticTrace;
						if (exception != null)
						{
							empty1 = exception.ToString();
						}
						else
						{
							empty1 = string.Empty;
						}
						TraceCore.HandledExceptionVerbose(etwDiagnosticTrace3, empty1, exception);
						return;
					}
					else
					{
						if (!TraceCore.HandledExceptionIsEnabled(this.diagnosticTrace))
						{
							break;
						}
						EtwDiagnosticTrace etwDiagnosticTrace1 = this.diagnosticTrace;
						if (exception != null)
						{
							empty = exception.ToString();
						}
						else
						{
							empty = string.Empty;
						}
						TraceCore.HandledException(etwDiagnosticTrace1, empty, exception);
						break;
					}
				}
			}
		}

		public void TraceUnhandledException(Exception exception)
		{
			string str;
			EtwDiagnosticTrace etwDiagnosticTrace = this.diagnosticTrace;
			if (exception != null)
			{
				str = exception.ToString();
			}
			else
			{
				str = string.Empty;
			}
			TraceCore.UnhandledException(etwDiagnosticTrace, str, exception);
		}
	}
}