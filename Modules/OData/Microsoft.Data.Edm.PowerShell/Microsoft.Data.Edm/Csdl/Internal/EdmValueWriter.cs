using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Values;
using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal
{
	internal static class EdmValueWriter
	{
		private static char[] Hex;

		static EdmValueWriter()
		{
			char[] chrArray = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
			EdmValueWriter.Hex = chrArray;
		}

		internal static string BinaryAsXml(byte[] binary)
		{
			char[] hex = new char[(int)binary.Length * 2];
			for (int i = 0; i < (int)binary.Length; i++)
			{
				hex[i << 1] = EdmValueWriter.Hex[binary[i] >> 4];
				hex[i << 1 | 1] = EdmValueWriter.Hex[binary[i] & 15];
			}
			return new string(hex);
		}

		internal static string BooleanAsXml(bool b)
		{
			return XmlConvert.ToString(b);
		}

		internal static string BooleanAsXml(bool? b)
		{
			return EdmValueWriter.BooleanAsXml(b.Value);
		}

		internal static string DateTimeAsXml(DateTime d)
		{
			return PlatformHelper.ConvertDateTimeToString(d);
		}

		internal static string DateTimeOffsetAsXml(DateTimeOffset d)
		{
			return XmlConvert.ToString(d);
		}

		internal static string DecimalAsXml(decimal d)
		{
			return XmlConvert.ToString(d);
		}

		internal static string FloatAsXml(double f)
		{
			return XmlConvert.ToString(f);
		}

		internal static string GuidAsXml(Guid g)
		{
			return XmlConvert.ToString(g);
		}

		internal static string IntAsXml(int i)
		{
			return XmlConvert.ToString(i);
		}

		internal static string IntAsXml(int? i)
		{
			return EdmValueWriter.IntAsXml(i.Value);
		}

		internal static string LongAsXml(long l)
		{
			return XmlConvert.ToString(l);
		}

		internal static string PrimitiveValueAsXml(IEdmPrimitiveValue v)
		{
			EdmValueKind valueKind = v.ValueKind;
			switch (valueKind)
			{
				case EdmValueKind.Binary:
				{
					return EdmValueWriter.BinaryAsXml(((IEdmBinaryValue)v).Value);
				}
				case EdmValueKind.Boolean:
				{
					return EdmValueWriter.BooleanAsXml(((IEdmBooleanValue)v).Value);
				}
				case EdmValueKind.Collection:
				case EdmValueKind.Enum:
				case EdmValueKind.Null:
				case EdmValueKind.Structured:
				{
					throw new NotSupportedException(Strings.ValueWriter_NonSerializableValue(v.ValueKind));
				}
				case EdmValueKind.DateTimeOffset:
				{
					return EdmValueWriter.DateTimeOffsetAsXml(((IEdmDateTimeOffsetValue)v).Value);
				}
				case EdmValueKind.DateTime:
				{
					return EdmValueWriter.DateTimeAsXml(((IEdmDateTimeValue)v).Value);
				}
				case EdmValueKind.Decimal:
				{
					return EdmValueWriter.DecimalAsXml(((IEdmDecimalValue)v).Value);
				}
				case EdmValueKind.Floating:
				{
					return EdmValueWriter.FloatAsXml(((IEdmFloatingValue)v).Value);
				}
				case EdmValueKind.Guid:
				{
					return EdmValueWriter.GuidAsXml(((IEdmGuidValue)v).Value);
				}
				case EdmValueKind.Integer:
				{
					return EdmValueWriter.LongAsXml(((IEdmIntegerValue)v).Value);
				}
				case EdmValueKind.String:
				{
					return EdmValueWriter.StringAsXml(((IEdmStringValue)v).Value);
				}
				case EdmValueKind.Time:
				{
					return EdmValueWriter.TimeAsXml(((IEdmTimeValue)v).Value);
				}
				default:
				{
					throw new NotSupportedException(Strings.ValueWriter_NonSerializableValue((object)v.ValueKind));
				}
			}
		}

		internal static string StringAsXml(string s)
		{
			return s;
		}

		internal static string TimeAsXml(TimeSpan d)
		{
			return d.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture);
		}
	}
}