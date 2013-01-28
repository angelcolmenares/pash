using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlCollectionExpression : CsdlExpressionBase
	{
		private readonly CsdlTypeReference type;

		private readonly List<CsdlExpressionBase> elementValues;

		public IEnumerable<CsdlExpressionBase> ElementValues
		{
			get
			{
				return this.elementValues;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Collection;
			}
		}

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlCollectionExpression(CsdlTypeReference type, IEnumerable<CsdlExpressionBase> elementValues, CsdlLocation location) : base(location)
		{
			this.type = type;
			this.elementValues = new List<CsdlExpressionBase>(elementValues);
		}
	}
}