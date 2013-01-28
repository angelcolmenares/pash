using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimResultObserver<T> : IObserver<T>
	{
		private CimSession session;

		private IObservable<object> observable;

		private CimResultContext context;

		protected CimSession CurrentSession
		{
			get
			{
				return this.session;
			}
		}

		public CimResultObserver(CimSession session, IObservable<object> observable)
		{
			this.session = session;
			this.observable = observable;
		}

		public CimResultObserver(CimSession session, IObservable<object> observable, CimResultContext cimResultContext)
		{
			this.session = session;
			this.observable = observable;
			this.context = cimResultContext;
		}

		public virtual void OnCompleted()
		{
			try
			{
				AsyncResultCompleteEventArgs asyncResultCompleteEventArg = new AsyncResultCompleteEventArgs(this.session, this.observable);
				this.OnNewResult(this, asyncResultCompleteEventArg);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.OnError(exception);
				object[] objArray = new object[1];
				objArray[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray);
			}
		}

		public virtual void OnError(Exception error)
		{
			try
			{
				AsyncResultErrorEventArgs asyncResultErrorEventArg = new AsyncResultErrorEventArgs(this.session, this.observable, error, this.context);
				this.OnNewResult(this, asyncResultErrorEventArg);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray = new object[1];
				objArray[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray);
			}
		}

		public virtual void OnNext(T value)
		{
			object[] objArray = new object[1];
			objArray[0] = value;
			DebugHelper.WriteLogEx("value = {0}.", 1, objArray);
			if (value != null)
			{
				this.OnNextCore(value);
				return;
			}
			else
			{
				return;
			}
		}

		protected void OnNextCore(object value)
		{
			object[] objArray = new object[1];
			objArray[0] = value;
			DebugHelper.WriteLogEx("value = {0}.", 1, objArray);
			try
			{
				AsyncResultObjectEventArgs asyncResultObjectEventArg = new AsyncResultObjectEventArgs(this.session, this.observable, value);
				this.OnNewResult(this, asyncResultObjectEventArg);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.OnError(exception);
				object[] objArray1 = new object[1];
				objArray1[0] = exception;
				DebugHelper.WriteLogEx("{0}", 0, objArray1);
			}
		}

		public event CimResultObserver<T>.ResultEventHandler OnNewResult;
		public delegate void ResultEventHandler(object observer, AsyncResultEventArgsBase resultArgs);
	}
}