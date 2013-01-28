using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal static class ParseTreeExtensionMethods
	{
		public static bool IsArray(this DataTypeType type)
		{
			DataTypeType dataTypeType = type;
			if (dataTypeType != DataTypeType.ObjectReferenceArray)
			{
				if (dataTypeType == DataTypeType.BoolArray || dataTypeType == DataTypeType.Char16Array || dataTypeType == DataTypeType.DateTimeArray || dataTypeType == DataTypeType.Real32Array || dataTypeType == DataTypeType.Real64Array || dataTypeType == DataTypeType.SInt8Array || dataTypeType == DataTypeType.SInt16Array || dataTypeType == DataTypeType.SInt32Array || dataTypeType == DataTypeType.SInt64Array || dataTypeType == DataTypeType.StringArray || dataTypeType == DataTypeType.UInt8Array || dataTypeType == DataTypeType.UInt16Array || dataTypeType == DataTypeType.UInt32Array || dataTypeType == DataTypeType.UInt64Array)
				{
					return true;
				}
				else if (dataTypeType == DataTypeType.Char16 || dataTypeType == DataTypeType.DateTime || dataTypeType == DataTypeType.Real32 || dataTypeType == DataTypeType.Real64 || dataTypeType == DataTypeType.SInt8 || dataTypeType == DataTypeType.SInt16 || dataTypeType == DataTypeType.SInt32 || dataTypeType == DataTypeType.SInt64 || dataTypeType == DataTypeType.String || dataTypeType == DataTypeType.UInt8 || dataTypeType == DataTypeType.UInt16 || dataTypeType == DataTypeType.UInt32 || dataTypeType == DataTypeType.UInt64)
				{
				}
				return false;
			}
			return true;
		}

		public static bool IsArray(this DataType type)
		{
			return type.Type.IsArray();
		}

		public static bool IsScalar(this DataTypeType type)
		{
			DataTypeType dataTypeType = type;
			switch (dataTypeType)
			{
				case DataTypeType.Bool:
				case DataTypeType.Char16:
				case DataTypeType.DateTime:
				case DataTypeType.Real32:
				case DataTypeType.Real64:
				case DataTypeType.SInt8:
				case DataTypeType.SInt16:
				case DataTypeType.SInt32:
				case DataTypeType.SInt64:
				case DataTypeType.String:
				case DataTypeType.UInt8:
				case DataTypeType.UInt16:
				case DataTypeType.UInt32:
				case DataTypeType.UInt64:
				{
					return true;
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
					return false;
				}
				default:
				{
					return false;
				}
			}
		}

		public static bool IsScalar(this DataType type)
		{
			return type.Type.IsScalar();
		}
	}
}