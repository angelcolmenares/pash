using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsRowTypeExpression : CsdlSemanticsTypeExpression, IEdmRowTypeReference, IEdmStructuredTypeReference, IEdmTypeReference, IEdmElement
	{
		public CsdlSemanticsRowTypeExpression(CsdlExpressionTypeReference expressionUsage, CsdlSemanticsTypeDefinition type) : base(expressionUsage, type)
		{
		}
	}
}