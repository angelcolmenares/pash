using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal
{
	internal static class CimTypeExtensionMethods
	{
		public static Type ToDotNetType(this CimType cimType)
		{
			CimType cimType1 = cimType;
			switch (cimType1)
			{
				case CimType.Unknown:
				{
					throw new ArgumentOutOfRangeException("cimType");
				}
				case CimType.Boolean:
				{
					return typeof(bool);
				}
				case CimType.UInt8:
				{
					return typeof(byte);
				}
				case CimType.SInt8:
				{
					return typeof(sbyte);
				}
				case CimType.UInt16:
				{
					return typeof(ushort);
				}
				case CimType.SInt16:
				{
					return typeof(short);
				}
				case CimType.UInt32:
				{
					return typeof(uint);
				}
				case CimType.SInt32:
				{
					return typeof(int);
				}
				case CimType.UInt64:
				{
					return typeof(ulong);
				}
				case CimType.SInt64:
				{
					return typeof(long);
				}
				case CimType.Real32:
				{
					return typeof(float);
				}
				case CimType.Real64:
				{
					return typeof(double);
				}
				case CimType.Char16:
				{
					return typeof(char);
				}
				case CimType.DateTime:
				{
					return typeof(object);
				}
				case CimType.String:
				{
					return typeof(string);
				}
				case CimType.Reference:
				case CimType.Instance:
				{
					return typeof(CimInstance);
				}
				case CimType.BooleanArray:
				{
					return typeof(bool[]);
				}
				case CimType.UInt8Array:
				{
					return typeof(byte[]);
				}
				case CimType.SInt8Array:
				{
					return typeof(sbyte[]);
				}
				case CimType.UInt16Array:
				{
					return typeof(ushort[]);
				}
				case CimType.SInt16Array:
				{
					return typeof(short[]);
				}
				case CimType.UInt32Array:
				{
					return typeof(uint[]);
				}
				case CimType.SInt32Array:
				{
					return typeof(int[]);
				}
				case CimType.UInt64Array:
				{
					return typeof(ulong[]);
				}
				case CimType.SInt64Array:
				{
					return typeof(long[]);
				}
				case CimType.Real32Array:
				{
					return typeof(float[]);
				}
				case CimType.Real64Array:
				{
					return typeof(double[]);
				}
				case CimType.Char16Array:
				{
					return typeof(char[]);
				}
				case CimType.DateTimeArray:
				{
					return typeof(object[]);
				}
				case CimType.StringArray:
				{
					return typeof(string[]);
				}
				case CimType.ReferenceArray:
				case CimType.InstanceArray:
				{
					return typeof(CimInstance[]);
				}
				default:
				{
					throw new ArgumentOutOfRangeException("cimType");
				}
			}
		}

		public static MiType ToMiType(this CimType cimType)
		{
			CimType cimType1 = cimType;
			switch (cimType1)
			{
				case CimType.Unknown:
				{
					throw new ArgumentOutOfRangeException("cimType");
				}
				case CimType.Boolean:
				{
					return MiType.Boolean;
				}
				case CimType.UInt8:
				{
					return MiType.UInt8;
				}
				case CimType.SInt8:
				{
					return MiType.SInt8;
				}
				case CimType.UInt16:
				{
					return MiType.UInt16;
				}
				case CimType.SInt16:
				{
					return MiType.SInt16;
				}
				case CimType.UInt32:
				{
					return MiType.UInt32;
				}
				case CimType.SInt32:
				{
					return MiType.SInt32;
				}
				case CimType.UInt64:
				{
					return MiType.UInt64;
				}
				case CimType.SInt64:
				{
					return MiType.SInt64;
				}
				case CimType.Real32:
				{
					return MiType.Real32;
				}
				case CimType.Real64:
				{
					return MiType.Real64;
				}
				case CimType.Char16:
				{
					return MiType.Char16;
				}
				case CimType.DateTime:
				{
					return MiType.DateTime;
				}
				case CimType.String:
				{
					return MiType.String;
				}
				case CimType.Reference:
				{
					return MiType.Reference;
				}
				case CimType.Instance:
				{
					return MiType.Instance;
				}
				case CimType.BooleanArray:
				{
					return MiType.BooleanArray;
				}
				case CimType.UInt8Array:
				{
					return MiType.UInt8Array;
				}
				case CimType.SInt8Array:
				{
					return MiType.SInt8Array;
				}
				case CimType.UInt16Array:
				{
					return MiType.UInt16Array;
				}
				case CimType.SInt16Array:
				{
					return MiType.SInt16Array;
				}
				case CimType.UInt32Array:
				{
					return MiType.UInt32Array;
				}
				case CimType.SInt32Array:
				{
					return MiType.SInt32Array;
				}
				case CimType.UInt64Array:
				{
					return MiType.UInt64Array;
				}
				case CimType.SInt64Array:
				{
					return MiType.SInt64Array;
				}
				case CimType.Real32Array:
				{
					return MiType.Real32Array;
				}
				case CimType.Real64Array:
				{
					return MiType.Real64Array;
				}
				case CimType.Char16Array:
				{
					return MiType.Char16Array;
				}
				case CimType.DateTimeArray:
				{
					return MiType.DateTimeArray;
				}
				case CimType.StringArray:
				{
					return MiType.StringArray;
				}
				case CimType.ReferenceArray:
				{
					return MiType.ReferenceArray;
				}
				case CimType.InstanceArray:
				{
					return MiType.InstanceArray;
				}
				default:
				{
					throw new ArgumentOutOfRangeException("cimType");
				}
			}
		}
	}
}