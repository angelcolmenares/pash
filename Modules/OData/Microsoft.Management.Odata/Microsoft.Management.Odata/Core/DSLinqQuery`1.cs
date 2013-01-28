using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class DSLinqQuery<TElement> : IOrderedQueryable<TElement>, IQueryable<TElement>, IEnumerable<TElement>, IOrderedQueryable, IQueryable, IEnumerable
	{
		public Type ElementType
		{
			get
			{
				return typeof(TElement);
			}
		}
	

		public Expression Expression
		{
			get;set;
		}


		public IQueryProvider Provider
		{
			get;set;
		}

		internal DSLinqQuery(DSLinqQueryProvider queryProvider, Expression queryExpression)
		{
			this.Provider = queryProvider;
			this.Expression = queryExpression;
		}

		public IEnumerator<TElement> GetEnumerator()
		{
			return this.Provider.Execute<IEnumerable<TElement>>(this.Expression).GetEnumerator();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.Provider.Execute<IEnumerable>(this.Expression).GetEnumerator();
		}
	}
}