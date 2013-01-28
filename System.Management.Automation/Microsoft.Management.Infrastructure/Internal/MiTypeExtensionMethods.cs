using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal
{
	internal static class MiTypeExtensionMethods
	{
		public static CimType ToCimType(this MiType miType)
		{
			MiType miType1 = miType;
			switch (miType1)
			{
				case MiType.Boolean:
				{
					return CimType.Boolean;
				}
				case MiType.UInt8:
				{
					return CimType.UInt8;
				}
				case MiType.SInt8:
				{
					return CimType.SInt8;
				}
				case MiType.UInt16:
				{
					return CimType.UInt16;
				}
				case MiType.SInt16:
				{
					return CimType.SInt16;
				}
				case MiType.UInt32:
				{
					return CimType.UInt32;
				}
				case MiType.SInt32:
				{
					return CimType.SInt32;
				}
				case MiType.UInt64:
				{
					return CimType.UInt64;
				}
				case MiType.SInt64:
				{
					return CimType.SInt64;
				}
				case MiType.Real32:
				{
					return CimType.Real32;
				}
				case MiType.Real64:
				{
					return CimType.Real64;
				}
				case MiType.Char16:
				{
					return CimType.Char16;
				}
				case MiType.DateTime:
				{
					return CimType.DateTime;
				}
				case MiType.String:
				{
					return CimType.String;
				}
				case MiType.Reference:
				{
					return CimType.Reference;
				}
				case MiType.Instance:
				{
					return CimType.Instance;
				}
				case MiType.BooleanArray:
				{
					return CimType.BooleanArray;
				}
				case MiType.UInt8Array:
				{
					return CimType.UInt8Array;
				}
				case MiType.SInt8Array:
				{
					return CimType.SInt8Array;
				}
				case MiType.UInt16Array:
				{
					return CimType.UInt16Array;
				}
				case MiType.SInt16Array:
				{
					return CimType.SInt16Array;
				}
				case MiType.UInt32Array:
				{
					return CimType.UInt32Array;
				}
				case MiType.SInt32Array:
				{
					return CimType.SInt32Array;
				}
				case MiType.UInt64Array:
				{
					return CimType.UInt64Array;
				}
				case MiType.SInt64Array:
				{
					return CimType.SInt64Array;
				}
				case MiType.Real32Array:
				{
					return CimType.Real32Array;
				}
				case MiType.Real64Array:
				{
					return CimType.Real64Array;
				}
				case MiType.Char16Array:
				{
					return CimType.Char16Array;
				}
				case MiType.DateTimeArray:
				{
					return CimType.DateTimeArray;
				}
				case MiType.StringArray:
				{
					return CimType.StringArray;
				}
				case MiType.ReferenceArray:
				{
					return CimType.ReferenceArray;
				}
				case MiType.InstanceArray:
				{
					return CimType.InstanceArray;
				}
			}
			throw new ArgumentOutOfRangeException("miType");
		}
	}
}