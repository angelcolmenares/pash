namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class DataServiceQuery<TElement> : DataServiceQuery, IQueryable<TElement>, IEnumerable<TElement>, IQueryable, IEnumerable
    {
        private static readonly MethodInfo expandGenericMethodInfo;
        private static readonly MethodInfo expandMethodInfo;
        private System.Data.Services.Client.QueryComponents queryComponents;
        private readonly System.Linq.Expressions.Expression queryExpression;
        private readonly DataServiceQueryProvider queryProvider;

        static DataServiceQuery()
        {
            DataServiceQuery<TElement>.expandMethodInfo = typeof(DataServiceQuery<TElement>).GetMethod("Expand", new Type[] { typeof(string) });
            DataServiceQuery<TElement>.expandGenericMethodInfo = (MethodInfo) typeof(DataServiceQuery<TElement>).GetMember("Expand*").Single<MemberInfo>(m => (((MethodInfo) m).GetGenericArguments().Count<Type>() == 1));
        }

        private DataServiceQuery(System.Linq.Expressions.Expression expression, DataServiceQueryProvider provider)
        {
            this.queryExpression = expression;
            this.queryProvider = provider;
        }

        public DataServiceQuery<TElement> AddQueryOption(string name, object value)
        {
            Util.CheckArgumentNull<string>(name, "name");
            Util.CheckArgumentNull<object>(value, "value");
            MethodInfo method = typeof(DataServiceQuery<TElement>).GetMethod("AddQueryOption");
            return (DataServiceQuery<TElement>) this.Provider.CreateQuery<TElement>(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(this.Expression, typeof(DataServiceOrderedQuery)), method, new System.Linq.Expressions.Expression[] { System.Linq.Expressions.Expression.Constant(name), System.Linq.Expressions.Expression.Constant(value, typeof(object)) }));
        }

        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            return base.BeginExecute(this, this.queryProvider.Context, callback, state, "Execute");
        }

        internal override IAsyncResult BeginExecuteInternal(AsyncCallback callback, object state)
        {
            return this.BeginExecute(callback, state);
        }

        public IEnumerable<TElement> EndExecute(IAsyncResult asyncResult)
        {
            return DataServiceRequest.EndExecute<TElement>(this, this.queryProvider.Context, "Execute", asyncResult);
        }

        internal override IEnumerable EndExecuteInternal(IAsyncResult asyncResult)
        {
            return this.EndExecute(asyncResult);
        }

        public IEnumerable<TElement> Execute()
        {
            return base.Execute<TElement>(this.queryProvider.Context, this.Translate());
        }

        internal override IEnumerable ExecuteInternal()
        {
            return this.Execute();
        }

        public DataServiceQuery<TElement> Expand<TTarget>(Expression<Func<TElement, TTarget>> navigationPropertyAccessor)
        {
            Util.CheckArgumentNull<Expression<Func<TElement, TTarget>>>(navigationPropertyAccessor, "navigationPropertyAccessor");
            MethodInfo method = DataServiceQuery<TElement>.expandGenericMethodInfo.MakeGenericMethod(new Type[] { typeof(TTarget) });
            return (DataServiceQuery<TElement>) this.Provider.CreateQuery<TElement>(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(this.Expression, typeof(DataServiceOrderedQuery)), method, new System.Linq.Expressions.Expression[] { navigationPropertyAccessor }));
        }

        public DataServiceQuery<TElement> Expand(string path)
        {
            Util.CheckArgumentNullAndEmpty(path, "path");
            return (DataServiceQuery<TElement>) this.Provider.CreateQuery<TElement>(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(this.Expression, typeof(DataServiceOrderedQuery)), DataServiceQuery<TElement>.expandMethodInfo, new System.Linq.Expressions.Expression[] { System.Linq.Expressions.Expression.Constant(path) }));
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.Execute().GetEnumerator();
        }

        public DataServiceQuery<TElement> IncludeTotalCount()
        {
            MethodInfo method = typeof(DataServiceQuery<TElement>).GetMethod("IncludeTotalCount");
            return (DataServiceQuery<TElement>) this.Provider.CreateQuery<TElement>(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(this.Expression, typeof(DataServiceOrderedQuery)), method));
        }

        internal override System.Data.Services.Client.QueryComponents QueryComponents(DataServiceProtocolVersion maxProtocolVersion)
        {
            return this.Translate();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            try
            {
                return this.QueryComponents(this.queryProvider.Context.MaxProtocolVersion).Uri.ToString();
            }
            catch (NotSupportedException exception)
            {
                return System.Data.Services.Client.Strings.ALinq_TranslationError(exception.Message);
            }
        }

        private System.Data.Services.Client.QueryComponents Translate()
        {
            if (this.queryComponents == null)
            {
                this.queryComponents = this.queryProvider.Translate(this.queryExpression);
            }
            return this.queryComponents;
        }

        public override Type ElementType
        {
            get
            {
                return typeof(TElement);
            }
        }

        public override System.Linq.Expressions.Expression Expression
        {
            get
            {
                return this.queryExpression;
            }
        }

        internal override ProjectionPlan Plan
        {
            get
            {
                return null;
            }
        }

        public override IQueryProvider Provider
        {
            get
            {
                return this.queryProvider;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.Translate().Uri;
            }
            internal set
            {
                this.Translate().Uri = value;
            }
        }

        internal class DataServiceOrderedQuery : DataServiceQuery<TElement>, IOrderedQueryable<TElement>, IQueryable<TElement>, IEnumerable<TElement>, IOrderedQueryable, IQueryable, IEnumerable
        {
            internal DataServiceOrderedQuery(Expression expression, DataServiceQueryProvider provider) : base(expression, provider)
            {
            }
        }
    }
}

