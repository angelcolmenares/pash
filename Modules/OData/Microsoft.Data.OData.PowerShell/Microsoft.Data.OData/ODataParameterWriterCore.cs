namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    internal abstract class ODataParameterWriterCore : ODataParameterWriter, IODataReaderWriterListener, IODataOutputInStreamErrorListener
    {
        private Microsoft.Data.OData.DuplicatePropertyNamesChecker duplicatePropertyNamesChecker;
        private readonly IEdmFunctionImport functionImport;
        private readonly ODataOutputContext outputContext;
        private HashSet<string> parameterNamesWritten = new HashSet<string>(EqualityComparer<string>.Default);
        private Stack<ParameterWriterState> scopes = new Stack<ParameterWriterState>();

        protected ODataParameterWriterCore(ODataOutputContext outputContext, IEdmFunctionImport functionImport)
        {
            this.outputContext = outputContext;
            this.functionImport = functionImport;
            this.scopes.Push(ParameterWriterState.Start);
        }

        public sealed override ODataCollectionWriter CreateCollectionWriter(string parameterName)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(parameterName, "parameterName");
            IEdmTypeReference itemTypeReference = this.VerifyCanCreateCollectionWriter(true, parameterName);
            return this.InterceptException<ODataCollectionWriter>(() => this.CreateCollectionWriterImplementation(parameterName, itemTypeReference));
        }

        public sealed override Task<ODataCollectionWriter> CreateCollectionWriterAsync(string parameterName)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(parameterName, "parameterName");
            IEdmTypeReference itemTypeReference = this.VerifyCanCreateCollectionWriter(false, parameterName);
            return TaskUtils.GetTaskForSynchronousOperation<ODataCollectionWriter>(() => this.InterceptException<ODataCollectionWriter>(() => this.CreateCollectionWriterImplementation(parameterName, itemTypeReference)));
        }

        private ODataCollectionWriter CreateCollectionWriterImplementation(string parameterName, IEdmTypeReference expectedItemType)
        {
            ODataCollectionWriter writer = this.CreateFormatCollectionWriter(parameterName, expectedItemType);
            this.ReplaceScope(ParameterWriterState.ActiveSubWriter);
            return writer;
        }

        protected abstract ODataCollectionWriter CreateFormatCollectionWriter(string parameterName, IEdmTypeReference expectedItemType);
        protected abstract void EndPayload();
        private void EnterErrorScope()
        {
            if (this.State != ParameterWriterState.Error)
            {
                this.EnterScope(ParameterWriterState.Error);
            }
        }

        private void EnterScope(ParameterWriterState newState)
        {
            this.ValidateTransition(newState);
            this.scopes.Push(newState);
        }

        public sealed override void Flush()
        {
            this.VerifyCanFlush(true);
            this.InterceptException(new Action(this.FlushSynchronously));
        }

        public sealed override Task FlushAsync()
        {
            this.VerifyCanFlush(false);
            return this.FlushAsynchronously().FollowOnFaultWith(delegate (Task t) {
                this.EnterErrorScope();
            });
        }

        protected abstract Task FlushAsynchronously();
        protected abstract void FlushSynchronously();
        private IEdmTypeReference GetParameterTypeReference(string parameterName)
        {
            if (this.functionImport == null)
            {
                return null;
            }
            IEdmFunctionParameter parameter = this.functionImport.FindParameter(parameterName);
            if (parameter == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_ParameterNameNotFoundInFunctionImport(parameterName, this.functionImport.Name));
            }
            return parameter.Type;
        }

        private T InterceptException<T>(Func<T> function)
        {
            T local;
            try
            {
                local = function();
            }
            catch
            {
                this.EnterErrorScope();
                throw;
            }
            return local;
        }

        private void InterceptException(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                this.EnterErrorScope();
                throw;
            }
        }

        private void LeaveScope()
        {
            this.ValidateTransition(ParameterWriterState.Completed);
            if (this.State == ParameterWriterState.CanWriteParameter)
            {
                this.scopes.Pop();
            }
            this.ReplaceScope(ParameterWriterState.Completed);
        }

        void IODataOutputInStreamErrorListener.OnInStreamError()
        {
            throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriter_InStreamErrorNotSupported);
        }

        void IODataReaderWriterListener.OnCompleted()
        {
            this.ReplaceScope(ParameterWriterState.CanWriteParameter);
        }

        void IODataReaderWriterListener.OnException()
        {
            this.ReplaceScope(ParameterWriterState.Error);
        }

        private void ReplaceScope(ParameterWriterState newState)
        {
            this.ValidateTransition(newState);
            this.scopes.Pop();
            this.scopes.Push(newState);
        }

        protected abstract void StartPayload();
        private void ValidateTransition(ParameterWriterState newState)
        {
            if ((this.State == ParameterWriterState.Error) || (newState != ParameterWriterState.Error))
            {
                switch (this.State)
                {
                    case ParameterWriterState.Start:
                        if ((newState != ParameterWriterState.CanWriteParameter) && (newState != ParameterWriterState.Completed))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromStart));
                        }
                        break;

                    case ParameterWriterState.CanWriteParameter:
                        if (((newState != ParameterWriterState.CanWriteParameter) && (newState != ParameterWriterState.ActiveSubWriter)) && (newState != ParameterWriterState.Completed))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromCanWriteParameter));
                        }
                        break;

                    case ParameterWriterState.ActiveSubWriter:
                        if (newState != ParameterWriterState.CanWriteParameter)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromActiveSubWriter));
                        }
                        break;

                    case ParameterWriterState.Completed:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromCompleted));

                    case ParameterWriterState.Error:
                        if (newState != ParameterWriterState.Error)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromError));
                        }
                        break;

                    default:
                        throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataParameterWriterCore_ValidateTransition_UnreachableCodePath));
                }
            }
        }

        private void VerifyAllParametersWritten()
        {
            Func<IEdmFunctionParameter, bool> predicate = null;
            if ((this.functionImport != null) && (this.functionImport.Parameters != null))
            {
                IEnumerable<IEdmFunctionParameter> source = null;
                if (this.functionImport.IsBindable)
                {
                    source = this.functionImport.Parameters.Skip<IEdmFunctionParameter>(1);
                }
                else
                {
                    source = this.functionImport.Parameters;
                }
                if (predicate == null)
                {
                    predicate = p => !this.parameterNamesWritten.Contains(p.Name);
                }
                IEnumerable<string> enumerable2 = from p in source.Where<IEdmFunctionParameter>(predicate) select p.Name;
                if (enumerable2.Any<string>())
                {
                    enumerable2 = from name in enumerable2 select string.Format(CultureInfo.InvariantCulture, "'{0}'", new object[] { name });
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_MissingParameterInParameterPayload(string.Join(", ", enumerable2.ToArray<string>()), this.functionImport.Name));
                }
            }
        }

        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                if (!this.outputContext.Synchronous)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_SyncCallOnAsyncWriter);
                }
            }
            else if (this.outputContext.Synchronous)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_AsyncCallOnSyncWriter);
            }
        }

        private IEdmTypeReference VerifyCanCreateCollectionWriter(bool synchronousCall, string parameterName)
        {
            IEdmTypeReference typeReference = this.VerifyCanWriteParameterAndGetTypeReference(synchronousCall, parameterName);
            if ((typeReference != null) && !typeReference.IsNonEntityODataCollectionTypeKind())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotCreateCollectionWriterOnNonCollectionTypeKind(parameterName, typeReference.TypeKind()));
            }
            if (typeReference != null)
            {
                return typeReference.GetCollectionItemType();
            }
            return null;
        }

        private void VerifyCanFlush(bool synchronousCall)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
        }

        private void VerifyCanWriteEnd(bool synchronousCall)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            this.VerifyNotInErrorOrCompletedState();
            if (this.State != ParameterWriterState.CanWriteParameter)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotWriteEnd);
            }
            this.VerifyAllParametersWritten();
        }

        private IEdmTypeReference VerifyCanWriteParameterAndGetTypeReference(bool synchronousCall, string parameterName)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            this.VerifyNotInErrorOrCompletedState();
            if (this.State != ParameterWriterState.CanWriteParameter)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotWriteParameter);
            }
            if (this.parameterNamesWritten.Contains(parameterName))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_DuplicatedParameterNameNotAllowed(parameterName));
            }
            this.parameterNamesWritten.Add(parameterName);
            return this.GetParameterTypeReference(parameterName);
        }

        private void VerifyCanWriteStart(bool synchronousCall)
        {
            this.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);
            if (this.State != ParameterWriterState.Start)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotWriteStart);
            }
        }

        private IEdmTypeReference VerifyCanWriteValueParameter(bool synchronousCall, string parameterName, object parameterValue)
        {
            IEdmTypeReference typeReference = this.VerifyCanWriteParameterAndGetTypeReference(synchronousCall, parameterName);
            if (((typeReference != null) && !typeReference.IsODataPrimitiveTypeKind()) && !typeReference.IsODataComplexTypeKind())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotWriteValueOnNonValueTypeKind(parameterName, typeReference.TypeKind()));
            }
            if (((parameterValue != null) && (!EdmLibraryExtensions.IsPrimitiveType(parameterValue.GetType()) || (parameterValue is Stream))) && !(parameterValue is ODataComplexValue))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotWriteValueOnNonSupportedValueType(parameterName, parameterValue.GetType()));
            }
            return typeReference;
        }

        protected abstract void VerifyNotDisposed();
        private void VerifyNotInErrorOrCompletedState()
        {
            if ((this.State == ParameterWriterState.Error) || (this.State == ParameterWriterState.Completed))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataParameterWriterCore_CannotWriteInErrorOrCompletedState);
            }
        }

        public sealed override void WriteEnd()
        {
            this.VerifyCanWriteEnd(true);
            this.InterceptException(() => this.WriteEndImplementation());
            if (this.State == ParameterWriterState.Completed)
            {
                this.Flush();
            }
        }

        public sealed override Task WriteEndAsync()
        {
            this.VerifyCanWriteEnd(false);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.InterceptException(() => this.WriteEndImplementation());
            }).FollowOnSuccessWithTask(delegate (Task task) {
                if (this.State == ParameterWriterState.Completed)
                {
                    return this.FlushAsync();
                }
                return TaskUtils.CompletedTask;
            });
        }

        private void WriteEndImplementation()
        {
            this.InterceptException(() => this.EndPayload());
            this.LeaveScope();
        }

        public sealed override void WriteStart()
        {
            this.VerifyCanWriteStart(true);
            this.InterceptException(() => this.WriteStartImplementation());
        }

        public sealed override Task WriteStartAsync()
        {
            this.VerifyCanWriteStart(false);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.InterceptException(() => this.WriteStartImplementation());
            });
        }

        private void WriteStartImplementation()
        {
            this.InterceptException(new Action(this.StartPayload));
            this.EnterScope(ParameterWriterState.CanWriteParameter);
        }

        public sealed override void WriteValue(string parameterName, object parameterValue)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(parameterName, "parameterName");
            IEdmTypeReference expectedTypeReference = this.VerifyCanWriteValueParameter(true, parameterName, parameterValue);
            this.InterceptException(() => this.WriteValueImplementation(parameterName, parameterValue, expectedTypeReference));
        }

        public sealed override Task WriteValueAsync(string parameterName, object parameterValue)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(parameterName, "parameterName");
            IEdmTypeReference expectedTypeReference = this.VerifyCanWriteValueParameter(false, parameterName, parameterValue);
            return TaskUtils.GetTaskForSynchronousOperation(delegate {
                this.InterceptException(() => this.WriteValueImplementation(parameterName, parameterValue, expectedTypeReference));
            });
        }

        private void WriteValueImplementation(string parameterName, object parameterValue, IEdmTypeReference expectedTypeReference)
        {
            this.InterceptException(() => this.WriteValueParameter(parameterName, parameterValue, expectedTypeReference));
        }

        protected abstract void WriteValueParameter(string parameterName, object parameterValue, IEdmTypeReference expectedTypeReference);

        protected Microsoft.Data.OData.DuplicatePropertyNamesChecker DuplicatePropertyNamesChecker
        {
            get
            {
                return (this.duplicatePropertyNamesChecker ?? (this.duplicatePropertyNamesChecker = new Microsoft.Data.OData.DuplicatePropertyNamesChecker(false, false)));
            }
        }

        private ParameterWriterState State
        {
            get
            {
                return this.scopes.Peek();
            }
        }

        private enum ParameterWriterState
        {
            Start,
            CanWriteParameter,
            ActiveSubWriter,
            Completed,
            Error
        }
    }
}

