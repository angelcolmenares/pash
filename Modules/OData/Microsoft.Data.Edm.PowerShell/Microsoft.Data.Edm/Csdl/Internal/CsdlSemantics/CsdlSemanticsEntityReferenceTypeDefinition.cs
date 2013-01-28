using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsEntityReferenceTypeDefinition : CsdlSemanticsTypeDefinition, IEdmEntityReferenceType, IEdmType, IEdmElement
	{
		private readonly CsdlSemanticsSchema schema;

		private readonly Cache<CsdlSemanticsEntityReferenceTypeDefinition, IEdmEntityType> entityTypeCache;

		private readonly static Func<CsdlSemanticsEntityReferenceTypeDefinition, IEdmEntityType> ComputeEntityTypeFunc;

		private readonly CsdlEntityReferenceType entityTypeReference;

		public override CsdlElement Element
		{
			get
			{
				return this.entityTypeReference;
			}
		}

		public IEdmEntityType EntityType
		{
			get
			{
				return this.entityTypeCache.GetValue(this, CsdlSemanticsEntityReferenceTypeDefinition.ComputeEntityTypeFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.schema.Model;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.EntityReference;
			}
		}

		static CsdlSemanticsEntityReferenceTypeDefinition()
		{
			CsdlSemanticsEntityReferenceTypeDefinition.ComputeEntityTypeFunc = (CsdlSemanticsEntityReferenceTypeDefinition me) => me.ComputeEntityType();
		}

		public CsdlSemanticsEntityReferenceTypeDefinition(CsdlSemanticsSchema schema, CsdlEntityReferenceType entityTypeReference) : base(entityTypeReference)
		{
			this.entityTypeCache = new Cache<CsdlSemanticsEntityReferenceTypeDefinition, IEdmEntityType>();
			this.schema = schema;
			this.entityTypeReference = entityTypeReference;
		}

		private IEdmEntityType ComputeEntityType()
		{
			IEdmTypeReference edmTypeReference = CsdlSemanticsModel.WrapTypeReference(this.schema, this.entityTypeReference.EntityType);
			if (edmTypeReference.TypeKind() == EdmTypeKind.Entity)
			{
				return edmTypeReference.AsEntity().EntityDefinition();
			}
			else
			{
				return new UnresolvedEntityType(this.schema.UnresolvedName(edmTypeReference.FullName()), base.Location);
			}
		}
	}
}