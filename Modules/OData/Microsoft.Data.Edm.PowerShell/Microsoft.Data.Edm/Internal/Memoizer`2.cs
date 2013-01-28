using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Data.Edm.Internal
{
	internal sealed class Memoizer<TArg, TResult>
	{
		private readonly Func<TArg, TResult> function;

		private readonly Dictionary<TArg, Memoizer<TArg, TResult>.Result> resultCache;

		private readonly ReaderWriterLockSlim slimLock;

		internal Memoizer(Func<TArg, TResult> function, IEqualityComparer<TArg> argComparer)
		{
			this.function = function;
			this.resultCache = new Dictionary<TArg, Memoizer<TArg, TResult>.Result>(argComparer);
			this.slimLock = new ReaderWriterLockSlim();
		}

		internal TResult Evaluate(TArg arg)
		{
			Memoizer<TArg, TResult>.Result result = null;
			bool flag;
			Func<TResult> func = null;
			this.slimLock.EnterReadLock();
			try
			{
				flag = this.resultCache.TryGetValue(arg, out result);
			}
			finally
			{
				this.slimLock.ExitReadLock();
			}
			if (!flag)
			{
				this.slimLock.EnterWriteLock();
				try
				{
					if (!this.resultCache.TryGetValue(arg, out result))
					{
						if (func == null)
						{
							func = () =>  this.function(arg);
						}
						result = new Memoizer<TArg, TResult>.Result(func);
						this.resultCache.Add(arg, result);
					}
				}
				finally
				{
					this.slimLock.ExitWriteLock();
				}
			}
			return result.GetValue();
		}

		private class Result
		{
			private TResult @value;

			private Func<TResult> createValueDelegate;

			internal Result(Func<TResult> createValueDelegate)
			{
				this.createValueDelegate = createValueDelegate;
			}

			internal TResult GetValue()
			{
				TResult tResult;
				if (this.createValueDelegate != null)
				{
					lock (this)
					{
						if (this.createValueDelegate != null)
						{
							this.@value = this.createValueDelegate();
							this.createValueDelegate = null;
							tResult = this.@value;
						}
						else
						{
							tResult = this.@value;
						}
					}
					return tResult;
				}
				else
				{
					return this.@value;
				}
			}
		}
	}
}