using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsFunction : CsdlSemanticsFunctionBase, IEdmFunction, IEdmFunctionBase, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlFunction function;

		public string DefiningExpression
		{
			get
			{
				return this.function.DefiningExpression;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.function;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return base.Context.Model;
			}
		}

		public string Namespace
		{
			get
			{
				return base.Context.Namespace;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.Function;
			}
		}

		public CsdlSemanticsFunction(CsdlSemanticsSchema context, CsdlFunction function) : base(context, function)
		{
			this.function = function;
		}
	}
}