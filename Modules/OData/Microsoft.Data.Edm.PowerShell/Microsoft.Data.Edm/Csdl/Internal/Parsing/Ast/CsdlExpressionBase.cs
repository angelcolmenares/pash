using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlExpressionBase : CsdlElement
	{
		public abstract EdmExpressionKind ExpressionKind
		{
			get;
		}

		public CsdlExpressionBase(CsdlLocation location) : base(location)
		{
		}
	}
}