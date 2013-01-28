using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmBinaryConstant : EdmValue, IEdmBinaryConstantExpression, IEdmExpression, IEdmBinaryValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly byte[] @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.BinaryConstant;
			}
		}

		public byte[] Value
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
				return EdmValueKind.Binary;
			}
		}

		public EdmBinaryConstant(byte[] value) : this(null, value)
		{
		}

		public EdmBinaryConstant(IEdmBinaryTypeReference type, byte[] value) : base(type)
		{
			EdmUtil.CheckArgumentNull<byte[]>(value, "value");
			this.@value = value;
		}
	}
}