using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedEnumMember : BadElement, IEdmEnumMember, IEdmNamedElement, IEdmElement
	{
		private string name;

		private IEdmEnumType declaringType;

		private Cache<UnresolvedEnumMember, IEdmPrimitiveValue> @value;

		private static Func<UnresolvedEnumMember, IEdmPrimitiveValue> ComputeValueFunc;

		public IEdmEnumType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public IEdmPrimitiveValue Value
		{
			get
			{
				return this.@value.GetValue(this, UnresolvedEnumMember.ComputeValueFunc, null);
			}
		}

		static UnresolvedEnumMember()
		{
			UnresolvedEnumMember.ComputeValueFunc = (UnresolvedEnumMember me) => me.ComputeValue();
		}

		public UnresolvedEnumMember(string name, IEdmEnumType declaringType, EdmLocation location)
			: base(new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEnumMember, Strings.Bad_UnresolvedEnumMember(name)) })
		{
			this.@value = new Cache<UnresolvedEnumMember, IEdmPrimitiveValue>();
			UnresolvedEnumMember unresolvedEnumMember = this;
			string str = name;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			unresolvedEnumMember.name = empty;
			this.declaringType = declaringType;
		}

		private IEdmPrimitiveValue ComputeValue()
		{
			return new EdmIntegerConstant((long)0);
		}
	}
}