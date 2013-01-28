using System;
using System.Text;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal abstract class MofDataType : DataType
	{
		public static MofDataType Bool
		{
			get
			{
				return MofDataType.BoolType.Instance;
			}
		}

		public static MofDataType Char16
		{
			get
			{
				return MofDataType.Char16Type.Instance;
			}
		}

		public static MofDataType DateTime
		{
			get
			{
				return MofDataType.DateTimeType.Instance;
			}
		}

		public static MofDataType Real32
		{
			get
			{
				return MofDataType.Real32Type.Instance;
			}
		}

		public static MofDataType Real64
		{
			get
			{
				return MofDataType.Real64Type.Instance;
			}
		}

		public static MofDataType SInt16
		{
			get
			{
				return MofDataType.SInt16Type.Instance;
			}
		}

		public static MofDataType SInt32
		{
			get
			{
				return MofDataType.SInt32Type.Instance;
			}
		}

		public static MofDataType SInt64
		{
			get
			{
				return MofDataType.SInt64Type.Instance;
			}
		}

		public static MofDataType SInt8
		{
			get
			{
				return MofDataType.SInt8Type.Instance;
			}
		}

		public static MofDataType String
		{
			get
			{
				return MofDataType.StringType.Instance;
			}
		}

		public static MofDataType UInt16
		{
			get
			{
				return MofDataType.UInt16Type.Instance;
			}
		}

		public static MofDataType UInt32
		{
			get
			{
				return MofDataType.UInt32Type.Instance;
			}
		}

		public static MofDataType UInt64
		{
			get
			{
				return MofDataType.UInt64Type.Instance;
			}
		}

		public static MofDataType UInt8
		{
			get
			{
				return MofDataType.UInt8Type.Instance;
			}
		}

		private MofDataType()
		{
		}

		internal static object QuoteAndEscapeIfString(object value)
		{
			string str = value as string;
			if (str == null)
			{
				return value;
			}
			else
			{
				return MofDataType.QuoteAndEscapeString(str);
			}
		}

		internal static string QuoteAndEscapeString(string str)
		{
			StringBuilder stringBuilder = new StringBuilder(str.Length + 10);
			stringBuilder.Append("\"");
			string str1 = str;
			for (int i = 0; i < str1.Length; i++)
			{
				char chr = str1[i];
				char chr1 = chr;
				switch (chr1)
				{
					case '\a':
					{
						stringBuilder.Append("\\a");
						break;
					}
					case '\b':
					{
						stringBuilder.Append("\\b");
						break;
					}
					case '\t':
					{
						stringBuilder.Append("\\t");
						break;
					}
					case '\n':
					{
						stringBuilder.Append("\\n");
						break;
					}
					case '\v':
					{
						stringBuilder.Append("\\v");
						break;
					}
					case '\f':
					{
						stringBuilder.Append("\\f");
						break;
					}
					case '\r':
					{
						stringBuilder.Append("\\r");
						break;
					}
					default:
					{
						if (chr1 == '\"' || chr1 == '\\')
						{
							stringBuilder.Append("\\");
							stringBuilder.Append(chr);
							break;
						}
						else
						{
							stringBuilder.Append(chr);
							break;
						}
					}
				}
			}
			stringBuilder.Append("\"");
			return stringBuilder.ToString();
		}

		private sealed class BoolType : MofDataType
		{
			public readonly static MofDataType.BoolType Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.Bool;
				}
			}

			static BoolType()
			{
				MofDataType.BoolType.Instance = new MofDataType.BoolType();
			}

			public BoolType()
			{
			}

			public override string ToString()
			{
				return "boolean";
			}
		}

		private sealed class Char16Type : MofDataType
		{
			public readonly static MofDataType.Char16Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.Char16;
				}
			}

			static Char16Type()
			{
				MofDataType.Char16Type.Instance = new MofDataType.Char16Type();
			}

			public Char16Type()
			{
			}

			public override string ToString()
			{
				return "char16";
			}
		}

		private sealed class DateTimeType : MofDataType
		{
			public readonly static MofDataType.DateTimeType Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.DateTime;
				}
			}

			static DateTimeType()
			{
				MofDataType.DateTimeType.Instance = new MofDataType.DateTimeType();
			}

			public DateTimeType()
			{
			}

			public override string ToString()
			{
				return "datetime";
			}
		}

		private sealed class Real32Type : MofDataType
		{
			public readonly static MofDataType.Real32Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.Real32;
				}
			}

			static Real32Type()
			{
				MofDataType.Real32Type.Instance = new MofDataType.Real32Type();
			}

			public Real32Type()
			{
			}

			public override string ToString()
			{
				return "real32";
			}
		}

		private sealed class Real64Type : MofDataType
		{
			public readonly static MofDataType.Real64Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.Real64;
				}
			}

			static Real64Type()
			{
				MofDataType.Real64Type.Instance = new MofDataType.Real64Type();
			}

			public Real64Type()
			{
			}

			public override string ToString()
			{
				return "real64";
			}
		}

		private sealed class SInt16Type : MofDataType
		{
			public readonly static MofDataType.SInt16Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.SInt16;
				}
			}

			static SInt16Type()
			{
				MofDataType.SInt16Type.Instance = new MofDataType.SInt16Type();
			}

			public SInt16Type()
			{
			}

			public override string ToString()
			{
				return "sint16";
			}
		}

		private sealed class SInt32Type : MofDataType
		{
			public readonly static MofDataType.SInt32Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.SInt32;
				}
			}

			static SInt32Type()
			{
				MofDataType.SInt32Type.Instance = new MofDataType.SInt32Type();
			}

			public SInt32Type()
			{
			}

			public override string ToString()
			{
				return "sint32";
			}
		}

		private sealed class SInt64Type : MofDataType
		{
			public readonly static MofDataType.SInt64Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.SInt64;
				}
			}

			static SInt64Type()
			{
				MofDataType.SInt64Type.Instance = new MofDataType.SInt64Type();
			}

			public SInt64Type()
			{
			}

			public override string ToString()
			{
				return "sint64";
			}
		}

		private sealed class SInt8Type : MofDataType
		{
			public readonly static MofDataType.SInt8Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.SInt8;
				}
			}

			static SInt8Type()
			{
				MofDataType.SInt8Type.Instance = new MofDataType.SInt8Type();
			}

			public SInt8Type()
			{
			}

			public override string ToString()
			{
				return "sint8";
			}
		}

		private sealed class StringType : MofDataType
		{
			public readonly static MofDataType.StringType Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.String;
				}
			}

			static StringType()
			{
				MofDataType.StringType.Instance = new MofDataType.StringType();
			}

			public StringType()
			{
			}

			public override string ToString()
			{
				return "string";
			}
		}

		private sealed class UInt16Type : MofDataType
		{
			public readonly static MofDataType.UInt16Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.UInt16;
				}
			}

			static UInt16Type()
			{
				MofDataType.UInt16Type.Instance = new MofDataType.UInt16Type();
			}

			public UInt16Type()
			{
			}

			public override string ToString()
			{
				return "uint16";
			}
		}

		private sealed class UInt32Type : MofDataType
		{
			public readonly static MofDataType.UInt32Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.UInt32;
				}
			}

			static UInt32Type()
			{
				MofDataType.UInt32Type.Instance = new MofDataType.UInt32Type();
			}

			public UInt32Type()
			{
			}

			public override string ToString()
			{
				return "uint32";
			}
		}

		private sealed class UInt64Type : MofDataType
		{
			public readonly static MofDataType.UInt64Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.UInt64;
				}
			}

			static UInt64Type()
			{
				MofDataType.UInt64Type.Instance = new MofDataType.UInt64Type();
			}

			public UInt64Type()
			{
			}

			public override string ToString()
			{
				return "uint64";
			}
		}

		private sealed class UInt8Type : MofDataType
		{
			public readonly static MofDataType.UInt8Type Instance;

			public override DataTypeType Type
			{
				get
				{
					return DataTypeType.UInt8;
				}
			}

			static UInt8Type()
			{
				MofDataType.UInt8Type.Instance = new MofDataType.UInt8Type();
			}

			public UInt8Type()
			{
			}

			public override string ToString()
			{
				return "uint8";
			}
		}
	}
}