using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsPropertyConstructor : CsdlSemanticsElement, IEdmPropertyConstructor, IEdmElement
	{
		private readonly CsdlPropertyValue property;

		private readonly CsdlSemanticsRecordExpression context;

		private readonly Cache<CsdlSemanticsPropertyConstructor, IEdmExpression> valueCache;

		private readonly static Func<CsdlSemanticsPropertyConstructor, IEdmExpression> ComputeValueFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.property;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.property.Property;
			}
		}

		public IEdmExpression Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsPropertyConstructor.ComputeValueFunc, null);
			}
		}

		static CsdlSemanticsPropertyConstructor()
		{
			CsdlSemanticsPropertyConstructor.ComputeValueFunc = (CsdlSemanticsPropertyConstructor me) => me.ComputeValue();
		}

		public CsdlSemanticsPropertyConstructor(CsdlPropertyValue property, CsdlSemanticsRecordExpression context) : base(property)
		{
			this.valueCache = new Cache<CsdlSemanticsPropertyConstructor, IEdmExpression>();
			this.property = property;
			this.context = context;
		}

		private IEdmExpression ComputeValue()
		{
			return CsdlSemanticsModel.WrapExpression(this.property.Expression, this.context.BindingContext, this.context.Schema);
		}
	}
}