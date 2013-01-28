namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [DebuggerVisualizer("Operation={Name}")]
    internal abstract class Operation
    {
        private readonly OperationParameter bindingParameter;
        private bool isReadOnly;
        private readonly OperationKind kind;
        private readonly string method;
        private string mimeType;
        private readonly string name;
        private readonly System.Data.Services.Providers.OperationParameterBindingKind operationParameterBindingKind;
        private readonly ReadOnlyCollection<OperationParameter> operationParameters;
        private readonly System.Data.Services.Providers.ResourceSet resourceSet;
        private readonly ServiceOperationResultKind resultKind;
        private readonly ResourceSetPathExpression resultSetPathExpression;
        private readonly ResourceType returnType;

        internal Operation(string name, ServiceOperationResultKind resultKind, ResourceType returnType, System.Data.Services.Providers.ResourceSet resultSet, ResourceSetPathExpression resultSetPathExpression, string method, IEnumerable<OperationParameter> parameters, System.Data.Services.Providers.OperationParameterBindingKind operationParameterBindingKind, OperationKind kind)
        {
            WebUtil.CheckStringArgumentNullOrEmpty(name, "name");
            WebUtil.CheckServiceOperationResultKind(resultKind, "resultKind");
            WebUtil.CheckStringArgumentNullOrEmpty(method, "method");
            ValidateConstructorArguments(name, returnType, resultSet, resultSetPathExpression, method, operationParameterBindingKind, kind);
            this.name = name;
            this.resultKind = resultKind;
            this.returnType = returnType;
            this.resourceSet = resultSet;
            this.resultSetPathExpression = resultSetPathExpression;
            this.method = method;
            this.kind = kind;
            this.operationParameterBindingKind = operationParameterBindingKind;
            this.operationParameters = ValidateParameters(this.operationParameterBindingKind, parameters);
            if (this.operationParameterBindingKind != System.Data.Services.Providers.OperationParameterBindingKind.Never)
            {
                this.bindingParameter = this.operationParameters.FirstOrDefault<OperationParameter>();
                if (this.bindingParameter == null)
                {
                    throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_BindableOperationMustHaveAtLeastOneParameter, "operationParameterBindingKind");
                }
                if (((resultSetPathExpression != null) && (this.bindingParameter.ParameterType.ResourceTypeKind != ResourceTypeKind.EntityType)) && (this.bindingParameter.ParameterType.ResourceTypeKind != ResourceTypeKind.EntityCollection))
                {
                    throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_BindingParameterMustBeEntityToUsePathExpression("resultSetPathExpression"));
                }
                if (((this.kind == OperationKind.Action) && (this.bindingParameter.ParameterType.ResourceTypeKind != ResourceTypeKind.EntityType)) && (this.bindingParameter.ParameterType.ResourceTypeKind != ResourceTypeKind.EntityCollection))
                {
                    throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_ActionBindingMustBeEntityOrEntityCollection, "parameters");
                }
                if (this.resultSetPathExpression != null)
                {
                    this.resultSetPathExpression.SetBindingParameter(this.bindingParameter);
                }
            }
        }

        internal static ServiceOperationResultKind GetResultKindFromReturnType(ResourceType returnType, bool isComposable)
        {
            if (returnType == null)
            {
                return ServiceOperationResultKind.Void;
            }
            if ((returnType.ResourceTypeKind == ResourceTypeKind.EntityCollection) && isComposable)
            {
                return ServiceOperationResultKind.QueryWithMultipleResults;
            }
            if (((returnType.ResourceTypeKind == ResourceTypeKind.EntityCollection) && !isComposable) || (returnType.ResourceTypeKind == ResourceTypeKind.Collection))
            {
                return ServiceOperationResultKind.Enumeration;
            }
            return ServiceOperationResultKind.DirectValue;
        }

        public void SetReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (OperationParameter parameter in this.OperationParameters)
                {
                    parameter.SetReadOnly();
                }
                this.isReadOnly = true;
            }
        }

        private void ThrowIfSealed()
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.ServiceOperation_Sealed(this.Name));
            }
        }

        private static void ValidateConstructorArguments(string operationName, ResourceType returnType, System.Data.Services.Providers.ResourceSet resultSet, ResourceSetPathExpression resultSetPathExpression, string method, System.Data.Services.Providers.OperationParameterBindingKind operationParameterBindingKind, OperationKind kind)
        {
            if ((returnType != null) && ((returnType.ResourceTypeKind == ResourceTypeKind.EntityType) || (returnType.ResourceTypeKind == ResourceTypeKind.EntityCollection)))
            {
                ResourceType subType = (returnType.ResourceTypeKind == ResourceTypeKind.EntityCollection) ? ((EntityCollectionResourceType) returnType).ItemType : returnType;
                if (((resultSet == null) && (resultSetPathExpression == null)) || ((resultSet != null) && !resultSet.ResourceType.IsAssignableFrom(subType)))
                {
                    if (kind == OperationKind.ServiceOperation)
                    {
                        throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_ResultTypeAndResultSetMustMatch("resultType", "resultSet"));
                    }
                    throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_ReturnTypeAndResultSetMustMatch("returnType", "resultSetPathExpression", "resultSet"));
                }
            }
            else if ((resultSet != null) || (resultSetPathExpression != null))
            {
                string str = (resultSet != null) ? "resultSet" : "resultSetPathExpression";
                if (kind == OperationKind.ServiceOperation)
                {
                    throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_ResultSetMustBeNullForGivenResultType(str, "resultType"));
                }
                throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_ResultSetMustBeNullForGivenReturnType(str, "returnType"));
            }
            if ((returnType != null) && (returnType == ResourceType.GetPrimitiveResourceType(typeof(Stream))))
            {
                string str2;
                string str3;
                if (kind == OperationKind.ServiceOperation)
                {
                    str2 = "resultType";
                    str3 = System.Data.Services.Strings.ServiceOperation_InvalidResultType(returnType.FullName);
                }
                else
                {
                    str2 = "returnType";
                    str3 = System.Data.Services.Strings.ServiceOperation_InvalidReturnType(returnType.FullName);
                }
                throw new ArgumentException(str3, str2);
            }
            if ((string.CompareOrdinal("GET", method) != 0) && (string.CompareOrdinal("POST", method) != 0))
            {
                throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_NotSupportedProtocolMethod(method, operationName), "method");
            }
            if ((resultSetPathExpression != null) && (operationParameterBindingKind == System.Data.Services.Providers.OperationParameterBindingKind.Never))
            {
                throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_MustBeBindableToUsePathExpression("resultSetPathExpression"), "resultSetPathExpression");
            }
        }

        private static ReadOnlyCollection<OperationParameter> ValidateParameters(System.Data.Services.Providers.OperationParameterBindingKind operationParameterBindingKind, IEnumerable<OperationParameter> parameters)
        {
            if (parameters == null)
            {
                return OperationParameter.EmptyOperationParameterCollection;
            }
            ReadOnlyCollection<OperationParameter> onlys = new ReadOnlyCollection<OperationParameter>(new List<OperationParameter>(parameters));
            HashSet<string> set = new HashSet<string>(StringComparer.Ordinal);
            int num = (operationParameterBindingKind != System.Data.Services.Providers.OperationParameterBindingKind.Never) ? 0 : -1;
            for (int i = 0; i < onlys.Count; i++)
            {
                OperationParameter parameter = onlys[i];
                if (!set.Add(parameter.Name))
                {
                    throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_DuplicateParameterName(parameter.Name), "parameters");
                }
                if (i > num)
                {
                    ResourceTypeKind resourceTypeKind = parameter.ParameterType.ResourceTypeKind;
                    switch (resourceTypeKind)
                    {
                        case ResourceTypeKind.EntityType:
                        case ResourceTypeKind.EntityCollection:
                            throw new ArgumentException(System.Data.Services.Strings.ServiceOperation_NonBindingParametersCannotBeEntityorEntityCollection(parameter.Name, resourceTypeKind));
                    }
                }
            }
            return onlys;
        }

        public object CustomState { get; set; }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        internal OperationKind Kind
        {
            get
            {
                return this.kind;
            }
        }

        public string Method
        {
            get
            {
                return this.method;
            }
        }

        public string MimeType
        {
            get
            {
                return this.mimeType;
            }
            set
            {
                this.ThrowIfSealed();
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ServiceOperation_MimeTypeCannotBeEmpty(this.Name));
                }
                if (!WebUtil.IsValidMimeType(value))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.ServiceOperation_MimeTypeNotValid(value, this.Name));
                }
                this.mimeType = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal OperationParameter OperationBindingParameter
        {
            get
            {
                return this.bindingParameter;
            }
        }

        internal System.Data.Services.Providers.OperationParameterBindingKind OperationParameterBindingKind
        {
            get
            {
                return this.operationParameterBindingKind;
            }
        }

        internal ReadOnlyCollection<OperationParameter> OperationParameters
        {
            get
            {
                return this.operationParameters;
            }
        }

        internal ServiceOperationResultKind OperationResultKind
        {
            get
            {
                return this.resultKind;
            }
        }

        internal ResourceSetPathExpression OperationResultSetPathExpression
        {
            get
            {
                return this.resultSetPathExpression;
            }
        }

        internal ResourceType OperationResultType
        {
            get
            {
                EntityCollectionResourceType returnType = this.returnType as EntityCollectionResourceType;
                if (returnType != null)
                {
                    return returnType.ItemType;
                }
                CollectionResourceType type2 = this.returnType as CollectionResourceType;
                if (type2 != null)
                {
                    return type2.ItemType;
                }
                return this.returnType;
            }
        }

        internal ResourceType OperationReturnType
        {
            get
            {
                return this.returnType;
            }
        }

        public System.Data.Services.Providers.ResourceSet ResourceSet
        {
            get
            {
                return this.resourceSet;
            }
        }
    }
}

