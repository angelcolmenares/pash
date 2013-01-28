using Microsoft.Management.Odata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Microsoft.Management.Odata.Common
{
	internal class AsyncDataStore<TItem>
	{
		private const int UnarchivedMaxDataSize = 10;

		private object syncObject;

		private Expression expression;

		private Queue<IEnumerator<TItem>> enumerators;

		private BoundedResetList<TItem> currentDataList;

		private Exception exception;

		private ManualResetEvent dataAvailableEvent;

		private bool isCompleted;

		public AsyncDataStore(Expression expression, bool noStreamingResponse)
		{
			int num;
			this.syncObject = new object();
			this.expression = expression;
			this.dataAvailableEvent = new ManualResetEvent(false);
			this.enumerators = new Queue<IEnumerator<TItem>>();
			if (!noStreamingResponse && this.DoesExpressionNeedCompleteDataSet(expression))
			{
				noStreamingResponse = true;
			}
			AsyncDataStore<TItem> boundedResetList = this;
			if (noStreamingResponse)
			{
				num = 0x7fffffff;
			}
			else
			{
				num = 10;
			}
			boundedResetList.currentDataList = new BoundedResetList<TItem>(num);
			this.currentDataList.PreResetEventHandler += new EventHandler<BoundedResetList<TItem>.PreResetEventArgs>(this.ResetEventHandler);
		}

		public void Add(TItem item)
		{
			lock (this.syncObject)
			{
				this.currentDataList.Add(item);
			}
		}

		private void AddEnumerator(List<TItem> resources)
		{
			try
			{
				if (resources.Count<TItem>() > 0)
				{
					new ListEnumerableUpdateVisitor<TItem>(resources);
					IEnumerator<TItem> enumerator = resources.AsQueryable<TItem>().GetEnumerator();
					this.enumerators.Enqueue(enumerator);
				}
			}
			finally
			{
				this.dataAvailableEvent.Set();
			}
		}

		public void Completed(Exception exception)
		{
			lock (this.syncObject)
			{
				this.isCompleted = true;
				this.exception = exception;
				this.currentDataList.Reset();
			}
		}

		private bool DoesExpressionNeedCompleteDataSet(Expression expression)
		{
			CompleteDataSetRequirementCheckVisitor completeDataSetRequirementCheckVisitor = new CompleteDataSetRequirementCheckVisitor();
			completeDataSetRequirementCheckVisitor.Visit(expression);
			return completeDataSetRequirementCheckVisitor.DoesExpressionNeedCompleteDataSet;
		}

		public IEnumerator<TItem> Get()
		{
			IEnumerator<TItem> enumerator;
			lock (this.syncObject)
			{
				this.ThrowIfErrorOccurred();
				IEnumerator<TItem> enumerator1 = this.GetEnumerator();
				if (enumerator1 != null || this.isCompleted)
				{
					enumerator = enumerator1;
					return enumerator;
				}
			}
			this.dataAvailableEvent.Reset();
			this.dataAvailableEvent.WaitOne();
			lock (this.syncObject)
			{
				this.ThrowIfErrorOccurred();
				enumerator = this.GetEnumerator();
			}
			return enumerator;
		}

		private IEnumerator<TItem> GetEnumerator()
		{
			if (this.enumerators.Count > 0)
			{
				return this.enumerators.Dequeue();
			}
			else
			{
				return null;
			}
		}

		private void ResetEventHandler(object source, BoundedResetList<TItem>.PreResetEventArgs args)
		{
			lock (this.syncObject)
			{
				this.AddEnumerator(args.PreResetList);
			}
		}

		private void ThrowIfErrorOccurred()
		{
			if (this.exception == null)
			{
				return;
			}
			else
			{
				throw this.exception;
			}
		}
	}
}