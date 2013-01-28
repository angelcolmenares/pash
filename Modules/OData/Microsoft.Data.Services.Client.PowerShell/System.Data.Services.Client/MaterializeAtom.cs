namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client.Materialization;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class MaterializeAtom : IDisposable, IEnumerable, IEnumerator
    {
        private bool calledGetEnumerator;
        private object current;
        private readonly Type elementType;
        private readonly bool expectingPrimitiveValue;
        private readonly ODataMaterializer materializer;
        internal readonly MergeOption MergeOptionValue;
        private bool moved;
        private readonly ResponseInfo responseInfo;
        private TextWriter writer;

        private MaterializeAtom()
        {
        }

        internal MaterializeAtom(ResponseInfo responseInfo, IEnumerable<ODataEntry> entries, Type elementType)
        {
            Type type;
            this.responseInfo = responseInfo;
            this.elementType = elementType;
            this.MergeOptionValue = responseInfo.MergeOption;
            this.expectingPrimitiveValue = PrimitiveType.IsKnownNullableType(elementType);
            Type expectedType = GetTypeForMaterializer(this.expectingPrimitiveValue, this.elementType, responseInfo.MaxProtocolVersion, out type);
            QueryComponents queryComponents = new QueryComponents(null, Util.DataServiceVersionEmpty, elementType, null, null);
            this.materializer = new ODataEntriesEntityMaterializer(entries, responseInfo, queryComponents, expectedType, null);
        }

        internal MaterializeAtom(ResponseInfo responseInfo, QueryComponents queryComponents, ProjectionPlan plan, IODataResponseMessage responseMessage, ODataPayloadKind payloadKind)
        {
            Type type;
            this.responseInfo = responseInfo;
            this.elementType = queryComponents.LastSegmentType;
            this.MergeOptionValue = responseInfo.MergeOption;
            this.expectingPrimitiveValue = PrimitiveType.IsKnownNullableType(this.elementType);
            Type materializerType = GetTypeForMaterializer(this.expectingPrimitiveValue, this.elementType, responseInfo.MaxProtocolVersion, out type);
            this.materializer = ODataMaterializer.CreateMaterializerForMessage(responseMessage, responseInfo, materializerType, queryComponents, plan, payloadKind);
        }

        private void CheckGetEnumerator()
        {
            if (this.calledGetEnumerator)
            {
                throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.Deserialize_GetEnumerator);
            }
            this.calledGetEnumerator = true;
        }

        internal long CountValue()
        {
            return this.materializer.CountValue;
        }

        internal static MaterializeAtom CreateWrapper(DataServiceContext context, IEnumerable results)
        {
            return new ResultsWrapper(context, results, null);
        }

        internal static MaterializeAtom CreateWrapper(DataServiceContext context, IEnumerable results, DataServiceQueryContinuation continuation)
        {
            return new ResultsWrapper(context, results, continuation);
        }

        public void Dispose()
        {
            this.current = null;
            if (this.materializer != null)
            {
                this.materializer.Dispose();
            }
            if (this.writer != null)
            {
                this.writer.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        internal virtual DataServiceQueryContinuation GetContinuation(IEnumerable key)
        {
            DataServiceQueryContinuation continuation;
            if (key == null)
            {
                if ((this.expectingPrimitiveValue && !this.moved) || (!this.expectingPrimitiveValue && !this.materializer.IsEndOfStream))
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_TopLevelLinkNotAvailable);
                }
                if (this.expectingPrimitiveValue || (this.materializer.CurrentFeed == null))
                {
                    return null;
                }
                return DataServiceQueryContinuation.Create(this.materializer.CurrentFeed.NextPageLink, this.materializer.MaterializeEntryPlan);
            }
            if (!this.materializer.NextLinkTable.TryGetValue(key, out continuation))
            {
                throw new ArgumentException(System.Data.Services.Client.Strings.MaterializeFromAtom_CollectionKeyNotPresentInLinkTable);
            }
            return continuation;
        }

        public virtual IEnumerator GetEnumerator()
        {
            this.CheckGetEnumerator();
            return this;
        }

        private static Type GetTypeForMaterializer(bool expectingPrimitiveValue, Type elementType, DataServiceProtocolVersion maxProtocolVersion, out Type implementationType)
        {
            if (!expectingPrimitiveValue && typeof(IEnumerable).IsAssignableFrom(elementType))
            {
                implementationType = ClientTypeUtil.GetImplementationType(elementType, typeof(ICollection<>));
                if (implementationType != null)
                {
                    Type t = implementationType.GetGenericArguments()[0];
                    if (ClientTypeUtil.TypeIsEntity(t, maxProtocolVersion))
                    {
                        return t;
                    }
                }
            }
            implementationType = null;
            return elementType;
        }

        public bool MoveNext()
        {
            bool flag2;
            bool applyingChanges = this.responseInfo.ApplyingChanges;
            try
            {
                this.responseInfo.ApplyingChanges = true;
                flag2 = this.MoveNextInternal();
            }
            finally
            {
                this.responseInfo.ApplyingChanges = applyingChanges;
            }
            return flag2;
        }

        private bool MoveNextInternal()
        {
            Type elementType;
            if (this.materializer == null)
            {
                return false;
            }
            this.current = null;
            this.materializer.ClearLog();
            bool flag = false;
            GetTypeForMaterializer(this.expectingPrimitiveValue, this.elementType, this.responseInfo.MaxProtocolVersion, out elementType);
            if (elementType != null)
            {
                if (this.moved)
                {
                    return false;
                }
                Type type2 = elementType.GetGenericArguments()[0];
                elementType = this.elementType;
                if (elementType.IsInterface())
                {
                    elementType = typeof(Collection<>).MakeGenericType(new Type[] { type2 });
                }
                IList list = (IList) Activator.CreateInstance(elementType);
                while (this.materializer.Read())
                {
                    list.Add(this.materializer.CurrentValue);
                }
                this.moved = true;
                this.current = list;
                flag = true;
            }
            if (this.current == null)
            {
                if (this.expectingPrimitiveValue && this.moved)
                {
                    flag = false;
                }
                else
                {
                    flag = this.materializer.Read();
                    if (flag)
                    {
                        this.current = this.materializer.CurrentValue;
                    }
                    this.moved = true;
                }
            }
            this.materializer.ApplyLogToContext();
            return flag;
        }

        internal static string ReadElementString(XmlReader reader, bool checkNullAttribute)
        {
            // This item is obfuscated and can not be translated.
            bool expressionStack_11_0;
            string str = null;
            if (checkNullAttribute)
            {
                expressionStack_11_0 = !Util.DoesNullAttributeSayTrue(reader);
            }
            else
            {
                expressionStack_11_0 = false;
            }
            bool flag = expressionStack_11_0;
            if (reader.IsEmptyElement)
            {
                if (!flag)
                {
                    return null;
                }
                return string.Empty;
            }
        Label_0091:
            if (!reader.Read())
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_ExpectingSimpleValue);
            }
            switch (reader.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.SignificantWhitespace:
                    if (str != null)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MixedTextWithComment);
                    }
                    str = reader.Value;
                    goto Label_0091;

                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                    goto Label_0091;

                case XmlNodeType.EndElement:
                    string expressionStack_63_0;
                    if (str != null)
                    {
                        return str;
                    }
                    else
                    {
                        expressionStack_63_0 = str;
                    }
                    expressionStack_63_0 = string.Empty;
                    if (flag)
                    {
                        return string.Empty;
                    }
                    return null;
            }
            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_ExpectingSimpleValue);
        }

        internal void SetInsertingObject(object addedObject)
        {
            ((ODataEntityMaterializer) this.materializer).TargetInstance = addedObject;
        }

        void IEnumerator.Reset()
        {
            throw System.Data.Services.Client.Error.NotSupported();
        }

        internal virtual DataServiceContext Context
        {
            get
            {
                return this.responseInfo.Context;
            }
        }

        public object Current
        {
            get
            {
                return this.current;
            }
        }

        internal static MaterializeAtom EmptyResults
        {
            get
            {
                return new ResultsWrapper(null, null, null);
            }
        }

        internal bool IsCountable
        {
            get
            {
                return ((this.materializer != null) && this.materializer.IsCountable);
            }
        }

        private class ResultsWrapper : MaterializeAtom
        {
            private readonly DataServiceContext context;
            private readonly DataServiceQueryContinuation continuation;
            private readonly IEnumerable results;

            internal ResultsWrapper(DataServiceContext context, IEnumerable results, DataServiceQueryContinuation continuation)
            {
                this.context = context;
                this.results = results ?? new object[0];
                this.continuation = continuation;
            }

            internal override DataServiceQueryContinuation GetContinuation(IEnumerable key)
            {
                if (key != null)
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_GetNestLinkForFlatCollection);
                }
                return this.continuation;
            }

            public override IEnumerator GetEnumerator()
            {
                return this.results.GetEnumerator();
            }

            internal override DataServiceContext Context
            {
                get
                {
                    return this.context;
                }
            }
        }
    }
}

