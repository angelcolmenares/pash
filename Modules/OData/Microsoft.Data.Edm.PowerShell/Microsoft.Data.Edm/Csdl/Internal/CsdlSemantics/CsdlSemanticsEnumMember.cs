using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsEnumMember : CsdlSemanticsElement, IEdmEnumMember, IEdmNamedElement, IEdmElement
	{
		private readonly CsdlEnumMember member;

		private readonly CsdlSemanticsEnumTypeDefinition declaringType;

		private readonly Cache<CsdlSemanticsEnumMember, IEdmPrimitiveValue> valueCache;

		private readonly static Func<CsdlSemanticsEnumMember, IEdmPrimitiveValue> ComputeValueFunc;

		public IEdmEnumType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.member;
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
				return this.member.Name;
			}
		}

		public IEdmPrimitiveValue Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsEnumMember.ComputeValueFunc, null);
			}
		}

		static CsdlSemanticsEnumMember()
		{
			CsdlSemanticsEnumMember.ComputeValueFunc = (CsdlSemanticsEnumMember me) => me.ComputeValue();
		}

		public CsdlSemanticsEnumMember(CsdlSemanticsEnumTypeDefinition declaringType, CsdlEnumMember member) : base(member)
		{
			this.valueCache = new Cache<CsdlSemanticsEnumMember, IEdmPrimitiveValue>();
			this.member = member;
			this.declaringType = declaringType;
		}

		private IEdmPrimitiveValue ComputeValue()
		{
			long? value = this.member.Value;
			return new EdmIntegerConstant(new EdmPrimitiveTypeReference(this.DeclaringType.UnderlyingType, false), value.Value);
		}
	}
}