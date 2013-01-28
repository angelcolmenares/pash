using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmFunctionImport : EdmFunctionBase, IEdmFunctionImport, IEdmFunctionBase, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEdmEntityContainer container;

		private readonly IEdmExpression entitySet;

		private readonly bool isSideEffecting;

		private readonly bool isComposable;

		private readonly bool isBindable;

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
				return this.entitySet;
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

		public EdmFunctionImport(IEdmEntityContainer container, string name, IEdmTypeReference returnType) : this(container, name, returnType, null, true, false, false)
		{
		}

		public EdmFunctionImport(IEdmEntityContainer container, string name, IEdmTypeReference returnType, IEdmExpression entitySet) : this(container, name, returnType, entitySet, true, false, false)
		{
		}

		public EdmFunctionImport(IEdmEntityContainer container, string name, IEdmTypeReference returnType, IEdmExpression entitySet, bool isSideEffecting, bool isComposable, bool isBindable) : base(name, returnType)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityContainer>(container, "container");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.container = container;
			this.entitySet = entitySet;
			this.isSideEffecting = isSideEffecting;
			this.isComposable = isComposable;
			this.isBindable = isBindable;
		}
	}
}