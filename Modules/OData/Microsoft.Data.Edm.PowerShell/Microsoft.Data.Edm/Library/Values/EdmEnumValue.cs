using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmEnumValue : EdmValue, IEdmEnumValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly IEdmPrimitiveValue @value;

		public IEdmPrimitiveValue Value
		{
			get
			{
				return this.@value;
			}
		}

		public override EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.Enum;
			}
		}

		public EdmEnumValue(IEdmEnumTypeReference type, IEdmEnumMember member) : this(type, member.Value)
		{
		}

		public EdmEnumValue(IEdmEnumTypeReference type, IEdmPrimitiveValue value) : base(type)
		{
			this.@value = value;
		}
	}
}