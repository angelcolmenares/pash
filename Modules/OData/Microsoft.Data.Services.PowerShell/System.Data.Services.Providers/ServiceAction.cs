namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerVisualizer("ServiceAction={Name}")]
    internal class ServiceAction : Operation
    {
        private readonly ReadOnlyCollection<ServiceActionParameter> parameters;

        public ServiceAction(string name, ResourceType returnType, OperationParameterBindingKind operationParameterBindingKind, IEnumerable<ServiceActionParameter> parameters, ResourceSetPathExpression resultSetPathExpression) : this(name, returnType, null, resultSetPathExpression, parameters, operationParameterBindingKind)
        {
        }

        public ServiceAction(string name, ResourceType returnType, ResourceSet resultSet, OperationParameterBindingKind operationParameterBindingKind, IEnumerable<ServiceActionParameter> parameters) : this(name, returnType, resultSet, null, parameters, operationParameterBindingKind)
        {

        }

		private ServiceAction(string name, ResourceType returnType, ResourceSet resultSet, ResourceSetPathExpression resultSetPathExpression, IEnumerable<ServiceActionParameter> parameters, OperationParameterBindingKind operationParameterBindingKind) : base(name, Operation.GetResultKindFromReturnType(returnType, false), returnType, resultSet, resultSetPathExpression, "POST", parameters, operationParameterBindingKind, OperationKind.Action)
        {
            if (base.OperationParameters == OperationParameter.EmptyOperationParameterCollection)
            {
                this.parameters = ServiceActionParameter.EmptyServiceActionParameterCollection;
            }
            else
            {
                this.parameters = new ReadOnlyCollection<ServiceActionParameter>(base.OperationParameters.Cast<ServiceActionParameter>().ToList<ServiceActionParameter>());
            }
        }

        public ServiceActionParameter BindingParameter
        {
            get
            {
                return (ServiceActionParameter) base.OperationBindingParameter;
            }
        }

        public ReadOnlyCollection<ServiceActionParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public ResourceSetPathExpression ResultSetPathExpression
        {
            get
            {
                return base.OperationResultSetPathExpression;
            }
        }

        public ResourceType ReturnType
        {
            get
            {
                return base.OperationReturnType;
            }
        }
    }
}

