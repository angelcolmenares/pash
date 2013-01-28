using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class CsdlSemanticsTypeExpression : CsdlSemanticsElement, IEdmTypeReference, IEdmElement
	{
		private readonly CsdlExpressionTypeReference expressionUsage;

		private readonly CsdlSemanticsTypeDefinition type;

		public IEdmType Definition
		{
			get
			{
				return this.type;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.expressionUsage;
			}
		}

		public bool IsNullable
		{
			get
			{
				return this.expressionUsage.IsNullable;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.type.Model;
			}
		}

		protected CsdlSemanticsTypeExpression(CsdlExpressionTypeReference expressionUsage, CsdlSemanticsTypeDefinition type) : base(expressionUsage)
		{
			this.expressionUsage = expressionUsage;
			this.type = type;
		}

		public override string ToString()
		{
			return this.ToTraceString();
		}
	}
}