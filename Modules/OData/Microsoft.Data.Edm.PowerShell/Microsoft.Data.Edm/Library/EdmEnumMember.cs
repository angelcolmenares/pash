using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEnumMember : EdmNamedElement, IEdmEnumMember, IEdmNamedElement, IEdmElement
	{
		private readonly IEdmEnumType declaringType;

		private IEdmPrimitiveValue @value;

		public IEdmEnumType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public IEdmPrimitiveValue Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmEnumMember(IEdmEnumType declaringType, string name, IEdmPrimitiveValue value) : base(name)
		{
			EdmUtil.CheckArgumentNull<IEdmEnumType>(declaringType, "declaringType");
			EdmUtil.CheckArgumentNull<IEdmPrimitiveValue>(value, "value");
			this.declaringType = declaringType;
			this.@value = value;
		}
	}
}