namespace System.Data.Services.Internal
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Services;
    using System.Linq;
    using System.Linq.Expressions;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal abstract class ProjectedWrapper : IProjectedResult
    {
        private static readonly Type[] precreatedProjectedWrapperTypes = new Type[] { typeof(ProjectedWrapper0), typeof(ProjectedWrapper1), typeof(ProjectedWrapper2), typeof(ProjectedWrapper3), typeof(ProjectedWrapper4), typeof(ProjectedWrapper5), typeof(ProjectedWrapper6), typeof(ProjectedWrapper7), typeof(ProjectedWrapper8) };
        private static readonly string[] projectedPropertyNames = new string[] { "ProjectedProperty0", "ProjectedProperty1", "ProjectedProperty2", "ProjectedProperty3", "ProjectedProperty4", "ProjectedProperty5", "ProjectedProperty6", "ProjectedProperty7" };
        private string propertyNameList;
        private string[] propertyNames;
        private string resourceTypeName;

        protected ProjectedWrapper()
        {
        }

        internal static MemberBinding[] Bind(Expression[] bindingExpressions, Type projectedWrapperType)
        {
            MemberBinding[] bindingArray;
            int length = bindingExpressions.Length;
            if (length <= (precreatedProjectedWrapperTypes.Length + 1))
            {
                bindingArray = new MemberBinding[length];
                BindResourceTypeAndPropertyNameList(projectedWrapperType, bindingArray, bindingExpressions);
                for (int i = 0; i < (length - 2); i++)
                {
                    bindingArray[i + 2] = BindToProjectedProperty(projectedWrapperType, i, bindingExpressions[i + 2]);
                }
                return bindingArray;
            }
            bindingArray = new MemberBinding[precreatedProjectedWrapperTypes.Length + 2];
            BindResourceTypeAndPropertyNameList(projectedWrapperType, bindingArray, bindingExpressions);
            BindToProjectedWrapperMany(bindingExpressions, 2, bindingArray, 2);
            return bindingArray;
        }

        private static void BindResourceTypeAndPropertyNameList(Type projectedWrapperType, MemberBinding[] bindings, Expression[] bindingExpressions)
        {
            bindings[0] = Expression.Bind(projectedWrapperType.GetProperty("ResourceTypeName"), bindingExpressions[0]);
            bindings[1] = Expression.Bind(projectedWrapperType.GetProperty("PropertyNameList"), bindingExpressions[1]);
        }

        private static MemberAssignment BindToProjectedProperty(Type projectedWrapperType, int propertyIndex, Expression expression)
        {
            return Expression.Bind(projectedWrapperType.GetProperty(projectedPropertyNames[propertyIndex]), expression);
        }

        private static void BindToProjectedWrapperMany(Expression[] bindingExpressions, int expressionStartIndex, MemberBinding[] bindings, int bindingStartIndex)
        {
            int propertyIndex = 0;
            while ((propertyIndex < (precreatedProjectedWrapperTypes.Length - 1)) && ((propertyIndex + expressionStartIndex) < bindingExpressions.Length))
            {
                bindings[bindingStartIndex + propertyIndex] = BindToProjectedProperty(typeof(ProjectedWrapperMany), propertyIndex, bindingExpressions[expressionStartIndex + propertyIndex]);
                propertyIndex++;
            }
            if (bindingExpressions.Length <= ((precreatedProjectedWrapperTypes.Length - 1) + expressionStartIndex))
            {
                while (propertyIndex < (precreatedProjectedWrapperTypes.Length - 1))
                {
                    bindings[bindingStartIndex + propertyIndex] = BindToProjectedProperty(typeof(ProjectedWrapperMany), propertyIndex, Expression.Constant(string.Empty, typeof(string)));
                    propertyIndex++;
                }
                bindings[(bindingStartIndex + precreatedProjectedWrapperTypes.Length) - 1] = Expression.Bind(typeof(ProjectedWrapperMany).GetProperty("Next"), Expression.MemberInit(Expression.New(typeof(ProjectedWrapperManyEnd)), new MemberBinding[] { Expression.Bind(typeof(ProjectedWrapperManyEnd).GetProperty("ResourceTypeName"), Expression.Constant(string.Empty, typeof(string))) }));
            }
            else
            {
                int length = bindingExpressions.Length - ((precreatedProjectedWrapperTypes.Length - 1) + expressionStartIndex);
                if (length > (precreatedProjectedWrapperTypes.Length - 1))
                {
                    length = precreatedProjectedWrapperTypes.Length;
                }
                MemberBinding[] bindingArray = new MemberBinding[precreatedProjectedWrapperTypes.Length + 2];
                bindingArray[0] = Expression.Bind(typeof(ProjectedWrapperMany).GetProperty("ResourceTypeName"), Expression.Constant(string.Empty, typeof(string)));
                bindingArray[1] = Expression.Bind(typeof(ProjectedWrapperMany).GetProperty("PropertyNameList"), Expression.Constant(string.Empty, typeof(string)));
                BindToProjectedWrapperMany(bindingExpressions, (expressionStartIndex + precreatedProjectedWrapperTypes.Length) - 1, bindingArray, 2);
                Expression expression = Expression.MemberInit(Expression.New(typeof(ProjectedWrapperMany)), bindingArray);
                bindings[(bindingStartIndex + precreatedProjectedWrapperTypes.Length) - 1] = Expression.Bind(typeof(ProjectedWrapperMany).GetProperty("Next"), expression);
            }
        }

        public object GetProjectedPropertyValue(string propertyName)
        {
            WebUtil.CheckArgumentNull<string>(propertyName, "propertyName");
            if (this.propertyNames == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BasicExpandProvider_ProjectedPropertiesNotInitialized);
            }
            int propertyIndex = -1;
            for (int i = 0; i < this.propertyNames.Length; i++)
            {
                if (this.propertyNames[i] == propertyName)
                {
                    propertyIndex = i;
                    break;
                }
            }
            return this.InternalGetProjectedPropertyValue(propertyIndex);
        }

        internal static Type GetProjectedWrapperType(int projectedPropertyCount)
        {
            if (projectedPropertyCount >= precreatedProjectedWrapperTypes.Length)
            {
                return typeof(ProjectedWrapperMany);
            }
            return precreatedProjectedWrapperTypes[projectedPropertyCount];
        }

        protected abstract object InternalGetProjectedPropertyValue(int propertyIndex);
        internal static object ProcessResultEnumeration(object resource)
        {
            IEnumerable enumerable;
            if (!WebUtil.IsElementIEnumerable(resource, out enumerable))
            {
                return resource;
            }
            return new EnumerableWrapper(enumerable);
        }

        internal static object ProcessResultInstance(object resource)
        {
            ProjectedWrapper wrapper = resource as ProjectedWrapper;
            if ((wrapper != null) && string.IsNullOrEmpty(wrapper.resourceTypeName))
            {
                return null;
            }
            return resource;
        }

        internal static IEnumerator UnwrapEnumerator(IEnumerator enumerator)
        {
            EnumeratorWrapper wrapper = enumerator as EnumeratorWrapper;
            if (wrapper != null)
            {
                enumerator = wrapper.InnerEnumerator;
            }
            return enumerator;
        }

        internal static IQueryable WrapQueryable(IQueryable queryable)
        {
            return new QueryableWrapper(queryable);
        }

        public string PropertyNameList
        {
            get
            {
                return this.propertyNameList;
            }
            set
            {
                this.propertyNameList = WebUtil.CheckArgumentNull<string>(value, "value");
                this.propertyNames = WebUtil.StringToSimpleArray(this.propertyNameList);
            }
        }

        public string ResourceTypeName
        {
            get
            {
                return this.resourceTypeName;
            }
            set
            {
                this.resourceTypeName = value;
            }
        }

        private class EnumerableWrapper : IEnumerable
        {
            private readonly IEnumerable enumerable;

            internal EnumerableWrapper(IEnumerable enumerable)
            {
                this.enumerable = enumerable;
            }

            public IEnumerator GetEnumerator()
            {
                return new ProjectedWrapper.EnumeratorWrapper(this.enumerable.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private sealed class EnumeratorWrapper : IEnumerator, IDisposable
        {
            private readonly IEnumerator enumerator;

            internal EnumeratorWrapper(IEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public void Dispose()
            {
                WebUtil.Dispose(this.enumerator);
            }

            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            public object Current
            {
                get
                {
                    return ProjectedWrapper.ProcessResultInstance(this.enumerator.Current);
                }
            }

            internal IEnumerator InnerEnumerator
            {
                get
                {
                    return this.enumerator;
                }
            }
        }

        private sealed class QueryableWrapper : ProjectedWrapper.EnumerableWrapper, IQueryable, IEnumerable
        {
            private readonly IQueryable queryable;

            internal QueryableWrapper(IQueryable queryable) : base(queryable)
            {
                this.queryable = queryable;
            }

            public Type ElementType
            {
                get
                {
                    return this.queryable.ElementType;
                }
            }

            public System.Linq.Expressions.Expression Expression
            {
                get
                {
                    return this.queryable.Expression;
                }
            }

            public IQueryProvider Provider
            {
                get
                {
                    throw System.Data.Services.Error.NotSupported();
                }
            }
        }
    }
}

