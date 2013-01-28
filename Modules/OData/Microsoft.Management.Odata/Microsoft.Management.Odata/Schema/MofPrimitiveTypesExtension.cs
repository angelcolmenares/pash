using Microsoft.Management.Odata;
using Microsoft.Management.Odata.MofParser.ParseTree;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Management.Odata.Schema
{
	internal static class MofPrimitiveTypesExtension
	{
		private static Dictionary<DataTypeType, Type> mofPrimitiveToClrType;

		static MofPrimitiveTypesExtension()
		{
			MofPrimitiveTypesExtension.mofPrimitiveToClrType = new Dictionary<DataTypeType, Type>();
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Bool, typeof(bool));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.BoolArray, typeof(bool));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Char16, typeof(short));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Char16Array, typeof(short));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.DateTime, typeof(DateTime));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.DateTimeArray, typeof(DateTime));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Real32, typeof(float));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Real32Array, typeof(float));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Real64, typeof(double));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.Real64Array, typeof(double));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt8, typeof(sbyte));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt8Array, typeof(sbyte));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt16, typeof(short));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt16Array, typeof(short));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt32, typeof(int));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt32Array, typeof(int));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt64, typeof(long));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.SInt64Array, typeof(long));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.String, typeof(string));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.StringArray, typeof(string));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt8, typeof(byte));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt8Array, typeof(byte));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt16, typeof(int));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt16Array, typeof(int));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt32, typeof(long));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt32Array, typeof(long));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt64, typeof(decimal));
			MofPrimitiveTypesExtension.mofPrimitiveToClrType.Add(DataTypeType.UInt64Array, typeof(decimal));
		}

		public static Type GetClrType(this DataTypeType dataType)
		{
			if (!MofPrimitiveTypesExtension.mofPrimitiveToClrType.ContainsKey(dataType))
			{
				object[] str = new object[1];
				str[0] = dataType.ToString();
				throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, Resources.UnsupportedPrimitiveTypeInMofSchema, str));
			}
			else
			{
				return MofPrimitiveTypesExtension.mofPrimitiveToClrType[dataType];
			}
		}
	}
}