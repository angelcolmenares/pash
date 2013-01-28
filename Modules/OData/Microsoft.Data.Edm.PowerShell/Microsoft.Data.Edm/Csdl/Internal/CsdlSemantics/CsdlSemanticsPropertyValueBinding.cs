using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsPropertyValueBinding : CsdlSemanticsElement, IEdmPropertyValueBinding, IEdmElement
	{
		private readonly CsdlPropertyValue property;

		private readonly CsdlSemanticsTypeAnnotation context;

		private readonly Cache<CsdlSemanticsPropertyValueBinding, IEdmExpression> valueCache;

		private readonly static Func<CsdlSemanticsPropertyValueBinding, IEdmExpression> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsPropertyValueBinding, IEdmProperty> boundPropertyCache;

		private readonly static Func<CsdlSemanticsPropertyValueBinding, IEdmProperty> ComputeBoundPropertyFunc;

		public IEdmProperty BoundProperty
		{
			get
			{
				return this.boundPropertyCache.GetValue(this, CsdlSemanticsPropertyValueBinding.ComputeBoundPropertyFunc, null);
			}
		}

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

		public IEdmExpression Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsPropertyValueBinding.ComputeValueFunc, null);
			}
		}

		static CsdlSemanticsPropertyValueBinding()
		{
			CsdlSemanticsPropertyValueBinding.ComputeValueFunc = (CsdlSemanticsPropertyValueBinding me) => me.ComputeValue();
			CsdlSemanticsPropertyValueBinding.ComputeBoundPropertyFunc = (CsdlSemanticsPropertyValueBinding me) => me.ComputeBoundProperty();
		}

		public CsdlSemanticsPropertyValueBinding(CsdlSemanticsTypeAnnotation context, CsdlPropertyValue property) : base(property)
		{
			this.valueCache = new Cache<CsdlSemanticsPropertyValueBinding, IEdmExpression>();
			this.boundPropertyCache = new Cache<CsdlSemanticsPropertyValueBinding, IEdmProperty>();
			this.context = context;
			this.property = property;
		}

		private IEdmProperty ComputeBoundProperty()
		{
			IEdmProperty edmProperty = ((IEdmStructuredType)this.context.Term).FindProperty(this.property.Property);
			IEdmProperty edmProperty1 = edmProperty;
			IEdmProperty unresolvedBoundProperty = edmProperty1;
			if (edmProperty1 == null)
			{
				unresolvedBoundProperty = new CsdlSemanticsPropertyValueBinding.UnresolvedBoundProperty((IEdmStructuredType)this.context.Term, this.property.Property);
			}
			return unresolvedBoundProperty;
		}

		private IEdmExpression ComputeValue()
		{
			return CsdlSemanticsModel.WrapExpression(this.property.Expression, this.context.TargetBindingContext, this.context.Schema);
		}

		private class UnresolvedBoundProperty : EdmElement, IEdmStructuralProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement, IUnresolvedElement
		{
			private readonly IEdmStructuredType declaringType;

			private readonly string name;

			private readonly IEdmTypeReference type;

			public EdmConcurrencyMode ConcurrencyMode
			{
				get
				{
					return EdmConcurrencyMode.None;
				}
			}

			public IEdmStructuredType DeclaringType
			{
				get
				{
					return this.declaringType;
				}
			}

			public string DefaultValueString
			{
				get
				{
					return null;
				}
			}

			public string Name
			{
				get
				{
					return this.name;
				}
			}

			public EdmPropertyKind PropertyKind
			{
				get
				{
					return EdmPropertyKind.Structural;
				}
			}

			public IEdmTypeReference Type
			{
				get
				{
					return this.type;
				}
			}

			public UnresolvedBoundProperty(IEdmStructuredType declaringType, string name)
			{
				this.declaringType = declaringType;
				this.name = name;
				this.type = new CsdlSemanticsPropertyValueBinding.UnresolvedBoundProperty.UnresolvedBoundPropertyType();
			}

			private class UnresolvedBoundPropertyType : IEdmTypeReference, IEdmType, IEdmElement
			{
				public IEdmType Definition
				{
					get
					{
						return this;
					}
				}

				public bool IsNullable
				{
					get
					{
						return true;
					}
				}

				public EdmTypeKind TypeKind
				{
					get
					{
						return EdmTypeKind.None;
					}
				}

				public UnresolvedBoundPropertyType()
				{
				}
			}
		}
	}
}