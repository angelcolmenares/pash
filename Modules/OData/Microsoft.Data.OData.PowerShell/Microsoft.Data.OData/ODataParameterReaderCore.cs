namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal abstract class ODataParameterReaderCore : ODataParameterReader, IODataReaderWriterListener
    {
        private readonly IEdmFunctionImport functionImport;
        private readonly ODataInputContext inputContext;
        private readonly HashSet<string> parametersRead = new HashSet<string>(StringComparer.Ordinal);
        private readonly Stack<Scope> scopes = new Stack<Scope>();
        private SubReaderState subReaderState;

        protected ODataParameterReaderCore(ODataInputContext inputContext, IEdmFunctionImport functionImport)
        {
            this.inputContext = inputContext;
            this.functionImport = functionImport;
            this.EnterScope(ODataParameterReaderState.Start, null, null);
        }

        public override ODataCollectionReader CreateCollectionReader()
        {
            this.VerifyCanCreateSubReader(ODataParameterReaderState.Collection);
            this.subReaderState = SubReaderState.Active;
            IEdmTypeReference elementType = ((IEdmCollectionType) this.GetParameterTypeReference(this.Name).Definition).ElementType;
            return this.CreateCollectionReader(elementType);
        }

        protected abstract ODataCollectionReader CreateCollectionReader(IEdmTypeReference expectedItemTypeReference);
        protected void EnterScope(ODataParameterReaderState state, string name, object value)
        {
            if (((state == ODataParameterReaderState.Value) && (value != null)) && (!(value is ODataComplexValue) && !EdmLibraryExtensions.IsPrimitiveType(value.GetType())))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterReaderCore_ValueMustBePrimitiveOrComplexOrNull));
            }
            if ((this.scopes.Count == 0) || (this.State != ODataParameterReaderState.Exception))
            {
                if (state == ODataParameterReaderState.Completed)
                {
                    List<string> list = new List<string>();
                    foreach (IEdmFunctionParameter parameter in this.FunctionImport.Parameters.Skip<IEdmFunctionParameter>(this.FunctionImport.IsBindable ? 1 : 0))
                    {
                        if (!this.parametersRead.Contains(parameter.Name))
                        {
                            list.Add(parameter.Name);
                        }
                    }
                    if (list.Count > 0)
                    {
                        this.scopes.Push(new Scope(ODataParameterReaderState.Exception, null, null));
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_ParametersMissingInPayload(this.FunctionImport.Name, string.Join(",", list.ToArray())));
                    }
                }
                this.scopes.Push(new Scope(state, name, value));
            }
        }

        private static string GetCreateReaderMethodName(ODataParameterReaderState state)
        {
            return ("Create" + state.ToString() + "Reader");
        }

        protected IEdmTypeReference GetParameterTypeReference(string parameterName)
        {
            IEdmFunctionParameter parameter = this.FunctionImport.FindParameter(parameterName);
            if (parameter == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_ParameterNameNotInMetadata(parameterName, this.FunctionImport.Name));
            }
            return parameter.Type;
        }

        private T InterceptException<T>(Func<T> action)
        {
            T local;
            try
            {
                local = action();
            }
            catch (Exception exception)
            {
                if (ExceptionUtils.IsCatchableExceptionType(exception))
                {
                    this.EnterScope(ODataParameterReaderState.Exception, null, null);
                }
                throw;
            }
            return local;
        }

        void IODataReaderWriterListener.OnCompleted()
        {
            this.subReaderState = SubReaderState.Completed;
        }

        void IODataReaderWriterListener.OnException()
        {
            this.EnterScope(ODataParameterReaderState.Exception, null, null);
        }

        protected void PopScope(ODataParameterReaderState state)
        {
            this.scopes.Pop();
        }

        public sealed override bool Read()
        {
            this.VerifyCanRead(true);
            return this.InterceptException<bool>(new Func<bool>(this.ReadSynchronously));
        }

        public sealed override Task<bool> ReadAsync()
        {
            this.VerifyCanRead(false);
            return this.ReadAsynchronously().FollowOnFaultWith<bool>(delegate (Task<bool> t) {
                this.EnterScope(ODataParameterReaderState.Exception, null, null);
            });
        }

        protected Task<bool> ReadAsynchronously()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(new Func<bool>(this.ReadImplementation));
        }

        protected abstract bool ReadAtStartImplementation();
        protected bool ReadImplementation()
        {
            bool flag = false;
            switch (this.State)
            {
                case ODataParameterReaderState.Start:
                    flag = this.ReadAtStartImplementation();
                    break;

                case ODataParameterReaderState.Value:
                case ODataParameterReaderState.Collection:
                    this.subReaderState = SubReaderState.None;
                    flag = this.ReadNextParameterImplementation();
                    break;

                case ODataParameterReaderState.Exception:
                case ODataParameterReaderState.Completed:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterReader_ReadImplementation));

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterReader_ReadImplementation));
            }
            if (this.State != ODataParameterReaderState.Completed)
            {
                if (this.parametersRead.Contains(this.Name))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_DuplicateParametersInPayload(this.Name));
                }
                this.parametersRead.Add(this.Name);
            }
            return flag;
        }

        protected abstract bool ReadNextParameterImplementation();
        protected bool ReadSynchronously()
        {
            return this.ReadImplementation();
        }

        private void VerifyAsynchronousCallAllowed()
        {
            if (this.inputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_AsyncCallOnSyncReader);
            }
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                this.VerifySynchronousCallAllowed();
            }
            else
            {
                this.VerifyAsynchronousCallAllowed();
            }
        }

        private void VerifyCanCreateSubReader(ODataParameterReaderState expectedState)
        {
            this.inputContext.VerifyNotDisposed();
            if (this.State != expectedState)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_InvalidCreateReaderMethodCalledForState(GetCreateReaderMethodName(expectedState), this.State));
            }
            if (this.subReaderState != SubReaderState.None)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_CreateReaderAlreadyCalled(GetCreateReaderMethodName(expectedState), this.Name));
            }
        }

        private void VerifyCanRead(bool synchronousCall)
        {
            this.inputContext.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            if ((this.State == ODataParameterReaderState.Exception) || (this.State == ODataParameterReaderState.Completed))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_ReadOrReadAsyncCalledInInvalidState(this.State));
            }
            if (this.State == ODataParameterReaderState.Collection)
            {
                if (this.subReaderState == SubReaderState.None)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_SubReaderMustBeCreatedAndReadToCompletionBeforeTheNextReadOrReadAsyncCall(this.State, GetCreateReaderMethodName(this.State)));
                }
                if (this.subReaderState == SubReaderState.Active)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_SubReaderMustBeInCompletedStateBeforeTheNextReadOrReadAsyncCall(this.State, GetCreateReaderMethodName(this.State)));
                }
            }
        }

        private void VerifySynchronousCallAllowed()
        {
            if (!this.inputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterReaderCore_SyncCallOnAsyncReader);
            }
        }

        protected IEdmFunctionImport FunctionImport
        {
            get
            {
                return this.functionImport;
            }
        }

        public override string Name
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().Name;
            }
        }

        public sealed override ODataParameterReaderState State
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().State;
            }
        }

        public override object Value
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                return this.scopes.Peek().Value;
            }
        }

        protected sealed class Scope
        {
            private readonly string name;
            private readonly ODataParameterReaderState state;
            private readonly object value;

            public Scope(ODataParameterReaderState state, string name, object value)
            {
                this.state = state;
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public ODataParameterReaderState State
            {
                get
                {
                    return this.state;
                }
            }

            public object Value
            {
                get
                {
                    return this.value;
                }
            }
        }

        private enum SubReaderState
        {
            None,
            Active,
            Completed
        }
    }
}

