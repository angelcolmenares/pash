using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsEnumTypeDefinition : CsdlSemanticsTypeDefinition, IEdmEnumType, IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
	{
		private readonly CsdlSemanticsSchema context;

		private readonly CsdlEnumType enumeration;

		private readonly Cache<CsdlSemanticsEnumTypeDefinition, IEdmPrimitiveType> underlyingTypeCache;

		private readonly static Func<CsdlSemanticsEnumTypeDefinition, IEdmPrimitiveType> ComputeUnderlyingTypeFunc;

		private readonly Cache<CsdlSemanticsEnumTypeDefinition, IEnumerable<IEdmEnumMember>> membersCache;

		private readonly static Func<CsdlSemanticsEnumTypeDefinition, IEnumerable<IEdmEnumMember>> ComputeMembersFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.enumeration;
			}
		}

		bool Microsoft.Data.Edm.IEdmEnumType.IsFlags
		{
			get
			{
				return this.enumeration.IsFlags;
			}
		}

		IEnumerable<IEdmEnumMember> Microsoft.Data.Edm.IEdmEnumType.Members
		{
			get
			{
				return this.membersCache.GetValue(this, CsdlSemanticsEnumTypeDefinition.ComputeMembersFunc, null);
			}
		}

		IEdmPrimitiveType Microsoft.Data.Edm.IEdmEnumType.UnderlyingType
		{
			get
			{
				return this.underlyingTypeCache.GetValue(this, CsdlSemanticsEnumTypeDefinition.ComputeUnderlyingTypeFunc, null);
			}
		}

		string Microsoft.Data.Edm.IEdmNamedElement.Name
		{
			get
			{
				return this.enumeration.Name;
			}
		}

		EdmSchemaElementKind Microsoft.Data.Edm.IEdmSchemaElement.SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public string Namespace
		{
			get
			{
				return this.context.Namespace;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Enum;
			}
		}

		static CsdlSemanticsEnumTypeDefinition()
		{
			CsdlSemanticsEnumTypeDefinition.ComputeUnderlyingTypeFunc = (CsdlSemanticsEnumTypeDefinition me) => me.ComputeUnderlyingType();
			CsdlSemanticsEnumTypeDefinition.ComputeMembersFunc = (CsdlSemanticsEnumTypeDefinition me) => me.ComputeMembers();
		}

		public CsdlSemanticsEnumTypeDefinition(CsdlSemanticsSchema context, CsdlEnumType enumeration) : base(enumeration)
		{
			this.underlyingTypeCache = new Cache<CsdlSemanticsEnumTypeDefinition, IEdmPrimitiveType>();
			this.membersCache = new Cache<CsdlSemanticsEnumTypeDefinition, IEnumerable<IEdmEnumMember>>();
			this.context = context;
			this.enumeration = enumeration;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.context);
		}

		private IEnumerable<IEdmEnumMember> ComputeMembers()
		{
			IEdmEnumMember csdlSemanticsEnumMember;
			List<IEdmEnumMember> edmEnumMembers = new List<IEdmEnumMember>();
			long value = (long)-1;
			foreach (CsdlEnumMember member in this.enumeration.Members)
			{
				long? nullable = member.Value;
				if (nullable.HasValue)
				{
					long? value1 = member.Value;
					value = value1.Value;
					csdlSemanticsEnumMember = new CsdlSemanticsEnumMember(this, member);
					csdlSemanticsEnumMember.SetIsValueExplicit(this.Model, new bool?(true));
				}
				else
				{
					if (value >= 0x7fffffffffffffffL)
					{
						CsdlSemanticsEnumTypeDefinition csdlSemanticsEnumTypeDefinition = this;
						string name = member.Name;
						EdmError[] edmErrorArray = new EdmError[1];
						EdmError[] edmError = edmErrorArray;
						int num = 0;
						EdmLocation location = member.Location;
						EdmLocation edmLocation = location;
						if (location == null)
						{
							edmLocation = base.Location;
						}
						edmError[num] = new EdmError(edmLocation, EdmErrorCode.EnumMemberValueOutOfRange, Strings.CsdlSemantics_EnumMemberValueOutOfRange);
						csdlSemanticsEnumMember = new BadEnumMember(csdlSemanticsEnumTypeDefinition, name, edmErrorArray);
					}
					else
					{
						long? nullable1 = new long?(value + (long)1);
						value = nullable1.Value;
						member.Value = nullable1;
						csdlSemanticsEnumMember = new CsdlSemanticsEnumMember(this, member);
					}
					csdlSemanticsEnumMember.SetIsValueExplicit(this.Model, new bool?(false));
				}
				edmEnumMembers.Add(csdlSemanticsEnumMember);
			}
			return edmEnumMembers;
		}

		private IEdmPrimitiveType ComputeUnderlyingType()
		{
			if (this.enumeration.UnderlyingTypeName == null)
			{
				return EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
			}
			else
			{
				EdmPrimitiveTypeKind primitiveTypeKind = EdmCoreModel.Instance.GetPrimitiveTypeKind(this.enumeration.UnderlyingTypeName);
				if (primitiveTypeKind != EdmPrimitiveTypeKind.None)
				{
					return EdmCoreModel.Instance.GetPrimitiveType(primitiveTypeKind);
				}
				else
				{
					return new UnresolvedPrimitiveType(this.enumeration.UnderlyingTypeName, base.Location);
				}
			}
		}
	}
}