using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsFunctionBase : CsdlSemanticsElement, IEdmFunctionBase, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlSemanticsSchema context;

		private readonly CsdlFunctionBase functionBase;

		private readonly Cache<CsdlSemanticsFunctionBase, IEdmTypeReference> returnTypeCache;

		private readonly static Func<CsdlSemanticsFunctionBase, IEdmTypeReference> ComputeReturnTypeFunc;

		private readonly Cache<CsdlSemanticsFunctionBase, IEnumerable<IEdmFunctionParameter>> parametersCache;

		private readonly static Func<CsdlSemanticsFunctionBase, IEnumerable<IEdmFunctionParameter>> ComputeParametersFunc;

		public EdmContainerElementKind ContainerElementKind
		{
			get
			{
				return EdmContainerElementKind.FunctionImport;
			}
		}

		public CsdlSemanticsSchema Context
		{
			get
			{
				return this.context;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.functionBase;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.Context.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.functionBase.Name;
			}
		}

		public IEnumerable<IEdmFunctionParameter> Parameters
		{
			get
			{
				return this.parametersCache.GetValue(this, CsdlSemanticsFunctionBase.ComputeParametersFunc, null);
			}
		}

		public IEdmTypeReference ReturnType
		{
			get
			{
				return this.returnTypeCache.GetValue(this, CsdlSemanticsFunctionBase.ComputeReturnTypeFunc, null);
			}
		}

		static CsdlSemanticsFunctionBase()
		{
			CsdlSemanticsFunctionBase.ComputeReturnTypeFunc = (CsdlSemanticsFunctionBase me) => me.ComputeReturnType();
			CsdlSemanticsFunctionBase.ComputeParametersFunc = (CsdlSemanticsFunctionBase me) => me.ComputeParameters();
		}

		public CsdlSemanticsFunctionBase(CsdlSemanticsSchema context, CsdlFunctionBase functionBase) : base(functionBase)
		{
			this.returnTypeCache = new Cache<CsdlSemanticsFunctionBase, IEdmTypeReference>();
			this.parametersCache = new Cache<CsdlSemanticsFunctionBase, IEnumerable<IEdmFunctionParameter>>();
			this.context = context;
			this.functionBase = functionBase;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.Context);
		}

		private IEnumerable<IEdmFunctionParameter> ComputeParameters()
		{
			List<IEdmFunctionParameter> edmFunctionParameters = new List<IEdmFunctionParameter>();
			foreach (CsdlFunctionParameter parameter in this.functionBase.Parameters)
			{
				edmFunctionParameters.Add(new CsdlSemanticsFunctionParameter(this, parameter));
			}
			return edmFunctionParameters;
		}

		private IEdmTypeReference ComputeReturnType()
		{
			return CsdlSemanticsModel.WrapTypeReference(this.Context, this.functionBase.ReturnType);
		}

		public IEdmFunctionParameter FindParameter(string name)
		{
			return this.Parameters.SingleOrDefault<IEdmFunctionParameter>((IEdmFunctionParameter p) => p.Name == name);
		}
	}
}