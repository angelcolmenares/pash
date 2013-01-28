namespace System.Data.Services.Providers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Expressions;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Expressions;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Linq;

    internal sealed class MetadataProviderEdmFunctionImport : EdmElement, IEdmFunctionImport, IEdmFunctionBase, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
    {
        private readonly MetadataProviderEdmEntityContainer container;
        private const bool DefaultIsBindable = false;
        private const bool DefaultIsComposable = false;
        private const bool DefaultIsSideEffecting = true;
        private readonly string entitySetPath;
        private readonly bool isBindable;
        private readonly bool isComposable;
        private readonly bool isSideEffecting;
        private readonly MetadataProviderEdmModel model;
        private readonly OperationWrapper operation;
        private readonly ReadOnlyCollection<IEdmFunctionParameter> parameters;

        internal MetadataProviderEdmFunctionImport(MetadataProviderEdmModel model, MetadataProviderEdmEntityContainer container, OperationWrapper operation)
        {
            this.container = container;
            this.model = model;
            this.operation = operation;
            if (operation.Kind == OperationKind.Action)
            {
                this.isSideEffecting = true;
                this.isComposable = false;
                this.isBindable = this.operation.BindingParameter != null;
            }
            else
            {
                this.isComposable = false;
                this.isSideEffecting = true;
                this.isBindable = false;
            }
            ResourceSetPathExpression resultSetPathExpression = operation.ResultSetPathExpression;
            this.entitySetPath = (resultSetPathExpression == null) ? null : resultSetPathExpression.PathExpression;
            if (operation.Kind == OperationKind.ServiceOperation)
            {
                model.SetHttpMethod(this, operation.Method);
            }
            string mimeType = operation.MimeType;
            if (!string.IsNullOrEmpty(mimeType))
            {
                model.SetMimeType(this, mimeType);
            }
            if (operation.OperationParameterBindingKind == OperationParameterBindingKind.Always)
            {
                model.SetIsAlwaysBindable(this, true);
            }
            ReadOnlyCollection<OperationParameter> parameters = operation.Parameters;
            if ((parameters != null) && (parameters.Count > 0))
            {
                List<IEdmFunctionParameter> list = new List<IEdmFunctionParameter>(parameters.Count);
                foreach (OperationParameter parameter in parameters)
                {
                    IEdmTypeReference typeReference = this.model.EnsureTypeReference(parameter.ParameterType, null);
                    if (!typeReference.IsNullable && (this.model.GetEdmVersion() < DataServiceProtocolVersion.V3.ToVersion()))
                    {
                        typeReference = typeReference.Clone(true);
                    }
                    EdmFunctionParameter item = new EdmFunctionParameter(this, parameter.Name, typeReference, EdmFunctionParameterMode.In);
                    list.Add(item);
                }
                this.parameters = new ReadOnlyCollection<IEdmFunctionParameter>(list);
            }
        }

        public IEdmFunctionParameter FindParameter(string name)
        {
            if (this.parameters == null)
            {
                return null;
            }
            return this.parameters.FirstOrDefault<IEdmFunctionParameter>(p => (p.Name == name));
        }

        public IEdmEntityContainer Container
        {
            get
            {
                return this.container;
            }
        }

        public EdmContainerElementKind ContainerElementKind
        {
            get
            {
                return EdmContainerElementKind.FunctionImport;
            }
        }

        public IEdmExpression EntitySet
        {
            get
            {
                ResourceSetWrapper resourceSet = this.operation.ResourceSet;
                if (resourceSet != null)
                {
                    return new EdmEntitySetReferenceExpression(this.model.FindEntitySet(resourceSet.ResourceSet));
                }
                if (this.entitySetPath != null)
                {
                    return new EdmPathExpression(this.entitySetPath.Split(new char[] { '/' }));
                }
                return null;
            }
        }

        public bool IsBindable
        {
            get
            {
                return this.isBindable;
            }
        }

        public bool IsComposable
        {
            get
            {
                return this.isComposable;
            }
        }

        public bool IsSideEffecting
        {
            get
            {
                return this.isSideEffecting;
            }
        }

        public string Name
        {
            get
            {
                return this.operation.Name;
            }
        }

        public IEnumerable<IEdmFunctionParameter> Parameters
        {
            get
            {
                return (this.parameters ?? Enumerable.Empty<IEdmFunctionParameter>());
            }
        }

        public IEdmTypeReference ReturnType
        {
            get
            {
                if (this.operation.ReturnType == null)
                {
                    return null;
                }
                ResourceType resultType = this.operation.ResultType;
                if ((this.operation.ResultKind != ServiceOperationResultKind.QueryWithSingleResult) && (this.operation.ResultKind != ServiceOperationResultKind.DirectValue))
                {
                    return this.model.EnsureEntityPrimitiveOrComplexCollectionTypeReference(resultType, null);
                }
                return this.model.EnsureTypeReference(resultType, null);
            }
        }

        internal OperationWrapper ServiceOperation
        {
            get
            {
                return this.operation;
            }
        }
    }
}

