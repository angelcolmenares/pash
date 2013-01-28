using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsRecordExpression : CsdlSemanticsExpression, IEdmRecordExpression, IEdmExpression, IEdmElement
	{
		private readonly CsdlRecordExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsRecordExpression, IEdmStructuredTypeReference> declaredTypeCache;

		private readonly static Func<CsdlSemanticsRecordExpression, IEdmStructuredTypeReference> ComputeDeclaredTypeFunc;

		private readonly Cache<CsdlSemanticsRecordExpression, IEnumerable<IEdmPropertyConstructor>> propertiesCache;

		private readonly static Func<CsdlSemanticsRecordExpression, IEnumerable<IEdmPropertyConstructor>> ComputePropertiesFunc;

		public IEdmEntityType BindingContext
		{
			get
			{
				return this.bindingContext;
			}
		}

		public IEdmStructuredTypeReference DeclaredType
		{
			get
			{
				return this.declaredTypeCache.GetValue(this, CsdlSemanticsRecordExpression.ComputeDeclaredTypeFunc, null);
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.expression;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Record;
			}
		}

		public IEnumerable<IEdmPropertyConstructor> Properties
		{
			get
			{
				return this.propertiesCache.GetValue(this, CsdlSemanticsRecordExpression.ComputePropertiesFunc, null);
			}
		}

		static CsdlSemanticsRecordExpression()
		{
			CsdlSemanticsRecordExpression.ComputeDeclaredTypeFunc = (CsdlSemanticsRecordExpression me) => me.ComputeDeclaredType();
			CsdlSemanticsRecordExpression.ComputePropertiesFunc = (CsdlSemanticsRecordExpression me) => me.ComputeProperties();
		}

		public CsdlSemanticsRecordExpression(CsdlRecordExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.declaredTypeCache = new Cache<CsdlSemanticsRecordExpression, IEdmStructuredTypeReference>();
			this.propertiesCache = new Cache<CsdlSemanticsRecordExpression, IEnumerable<IEdmPropertyConstructor>>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmStructuredTypeReference ComputeDeclaredType()
		{
			if (this.expression.Type != null)
			{
				return CsdlSemanticsModel.WrapTypeReference(base.Schema, this.expression.Type).AsStructured();
			}
			else
			{
				return null;
			}
		}

		private IEnumerable<IEdmPropertyConstructor> ComputeProperties()
		{
			List<IEdmPropertyConstructor> edmPropertyConstructors = new List<IEdmPropertyConstructor>();
			foreach (CsdlPropertyValue propertyValue in this.expression.PropertyValues)
			{
				edmPropertyConstructors.Add(new CsdlSemanticsPropertyConstructor(propertyValue, this));
			}
			return edmPropertyConstructors;
		}
	}
}