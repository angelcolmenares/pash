using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class ArrayType : DataType
	{
		private readonly DataType m_elementType;

		private readonly int? m_length;

		public DataType ElementType
		{
			get
			{
				return this.m_elementType;
			}
		}

		public int? Length
		{
			get
			{
				return this.m_length;
			}
		}

		public override DataTypeType Type
		{
			get
			{
				DataTypeType type = this.ElementType.Type;
				if (type == DataTypeType.ObjectReference)
				{
					return DataTypeType.ObjectReferenceArray;
				}
				else
				{
					switch (type)
					{
						case DataTypeType.Bool:
						{
							return DataTypeType.BoolArray;
						}
						case DataTypeType.BoolArray:
						case DataTypeType.Char16Array:
						case DataTypeType.DateTimeArray:
						case DataTypeType.Real32Array:
						case DataTypeType.Real64Array:
						case DataTypeType.SInt8Array:
						case DataTypeType.SInt16Array:
						case DataTypeType.SInt32Array:
						case DataTypeType.SInt64Array:
						case DataTypeType.StringArray:
						case DataTypeType.UInt8Array:
						case DataTypeType.UInt16Array:
						case DataTypeType.UInt32Array:
						{
							throw new InvalidOperationException();
						}
						case DataTypeType.Char16:
						{
							return DataTypeType.Char16Array;
						}
						case DataTypeType.DateTime:
						{
							return DataTypeType.DateTimeArray;
						}
						case DataTypeType.Real32:
						{
							return DataTypeType.Real32Array;
						}
						case DataTypeType.Real64:
						{
							return DataTypeType.Real64Array;
						}
						case DataTypeType.SInt8:
						{
							return DataTypeType.SInt8Array;
						}
						case DataTypeType.SInt16:
						{
							return DataTypeType.SInt16Array;
						}
						case DataTypeType.SInt32:
						{
							return DataTypeType.SInt32Array;
						}
						case DataTypeType.SInt64:
						{
							return DataTypeType.SInt64Array;
						}
						case DataTypeType.String:
						{
							return DataTypeType.StringArray;
						}
						case DataTypeType.UInt8:
						{
							return DataTypeType.UInt8Array;
						}
						case DataTypeType.UInt16:
						{
							return DataTypeType.UInt16Array;
						}
						case DataTypeType.UInt32:
						{
							return DataTypeType.UInt32Array;
						}
						case DataTypeType.UInt64:
						{
							return DataTypeType.UInt64Array;
						}
						default:
						{
							throw new InvalidOperationException();
						}
					}
				}
			}
		}

		internal ArrayType(DataType elementType, int? length)
		{
			this.m_elementType = elementType;
			this.m_length = length;
		}

		public override string ToString()
		{
			int? mLength = this.m_length;
			if (!mLength.HasValue)
			{
				return string.Concat(this.m_elementType, "[]");
			}
			else
			{
				object[] mElementType = new object[4];
				mElementType[0] = this.m_elementType;
				mElementType[1] = "[";
				mElementType[2] = this.m_length;
				mElementType[3] = "]";
				return string.Concat(mElementType);
			}
		}
	}
}