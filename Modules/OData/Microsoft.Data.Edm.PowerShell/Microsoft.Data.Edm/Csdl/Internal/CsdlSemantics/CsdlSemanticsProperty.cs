using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsProperty : CsdlSemanticsElement, IEdmStructuralProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		protected CsdlProperty property;

		private readonly CsdlSemanticsStructuredTypeDefinition declaringType;

		private readonly Cache<CsdlSemanticsProperty, IEdmTypeReference> typeCache;

		private readonly static Func<CsdlSemanticsProperty, IEdmTypeReference> ComputeTypeFunc;

		public EdmConcurrencyMode ConcurrencyMode
		{
			get
			{
				if (this.property.IsFixedConcurrency)
				{
					return EdmConcurrencyMode.Fixed;
				}
				else
				{
					return EdmConcurrencyMode.None;
				}
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
				return this.property.DefaultValue;
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
				return this.declaringType.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.property.Name;
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
				return this.typeCache.GetValue(this, CsdlSemanticsProperty.ComputeTypeFunc, null);
			}
		}

		static CsdlSemanticsProperty()
		{
			CsdlSemanticsProperty.ComputeTypeFunc = (CsdlSemanticsProperty me) => me.ComputeType();
		}

		public CsdlSemanticsProperty(CsdlSemanticsStructuredTypeDefinition declaringType, CsdlProperty property) : base(property)
		{
			this.typeCache = new Cache<CsdlSemanticsProperty, IEdmTypeReference>();
			this.property = property;
			this.declaringType = declaringType;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.declaringType.Context);
		}

		private IEdmTypeReference ComputeType()
		{
			return CsdlSemanticsModel.WrapTypeReference(this.declaringType.Context, this.property.Type);
		}
	}
}