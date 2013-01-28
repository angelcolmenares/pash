using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class CsdlSemanticsExpression : CsdlSemanticsElement, IEdmExpression, IEdmElement
	{
		private readonly CsdlSemanticsSchema schema;

		public abstract EdmExpressionKind ExpressionKind
		{
			get;
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.schema.Model;
			}
		}

		public CsdlSemanticsSchema Schema
		{
			get
			{
				return this.schema;
			}
		}

		protected CsdlSemanticsExpression(CsdlSemanticsSchema schema, CsdlExpressionBase element) : base(element)
		{
			this.schema = schema;
		}
	}
}