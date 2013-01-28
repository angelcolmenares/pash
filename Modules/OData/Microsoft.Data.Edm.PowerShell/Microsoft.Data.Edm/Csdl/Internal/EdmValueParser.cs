using Microsoft.Data.Edm;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal
{
	internal static class EdmValueParser
	{
		private const string TimeExp = "[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{0,3})?";

		private static Regex TimeValidator;

		static EdmValueParser()
		{
			EdmValueParser.TimeValidator = new Regex("^[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{0,3})?$", RegexOptions.Compiled | RegexOptions.Singleline);
		}

		internal static bool TryParseBinary(string value, out byte[] result)
		{
			byte num = 0;
			byte num1 = 0;
			if (value.Length % 2 == 0)
			{
				result = new byte[value.Length >> 1];
				int num2 = 0;
				while (num2 < value.Length)
				{
					if (EdmValueParser.TryParseCharAsBinary(value[num2], out num))
					{
						int num3 = num2 + 1;
						num2 = num3;
						if (EdmValueParser.TryParseCharAsBinary(value[num3], out num1))
						{
							result[num2 >> 1] = (byte)(num << 4 | num1);
							num2++;
						}
						else
						{
							result = null;
							return false;
						}
					}
					else
					{
						result = null;
						return false;
					}
				}
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		internal static bool TryParseBool(string value, out bool? result)
		{
			int length = value.Length;
			switch (length)
			{
				case 1:
				{
					char chr = value[0];
					switch (chr)
					{
						case '0':
						{
							result = new bool?(false);
							return true;
						}
						case '1':
						{
							result = new bool?(true);
							return true;
						}
						default:
						{
							result = null;
							return false;
						}
					}
				}
				case 2:
				case 3:
				{
					result = null;
					return false;
				}
				case 4:
				{
					if (value[0] != 't' && value[0] != 'T' || value[1] != 'r' && value[1] != 'R' || value[2] != 'u' && value[2] != 'U' || value[3] != 'e' && value[3] != 'E')
					{
						result = null;
						return false;
					}
					result = new bool?(true);
					return true;
				}
				case 5:
				{
					if (value[0] != 'f' && value[0] != 'F' || value[1] != 'a' && value[1] != 'A' || value[2] != 'l' && value[2] != 'L' || value[3] != 's' && value[3] != 'S' || value[4] != 'e' && value[4] != 'E')
					{
						result = null;
						return false;
					}
					result = new bool?(false);
					return true;
				}
				default:
				{
					result = null;
					return false;
				}
			}
		}

		private static bool TryParseCharAsBinary(char ch, out byte b)
		{
			var c = Convert.ToUInt32(ch);
			uint num = c - 48;
			if (num < 0 || num > 9)
			{
				num = c - 65;
				if (num < 0 || num > 5)
				{
					num = c - 97;
				}
				if (num < 0 || num > 5)
				{
					b = 0;
					return false;
				}
				else
				{
					b = (byte)(num + 10);
					return true;
				}
			}
			else
			{
				b = (byte)num;
				return true;
			}
		}

		internal static bool TryParseDateTime(string value, out DateTime? result)
		{
			bool flag;
			try
			{
				result = new DateTime?(PlatformHelper.ConvertStringToDateTime(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseDateTimeOffset(string value, out DateTimeOffset? result)
		{
			bool flag;
			try
			{
				result = new DateTimeOffset?(XmlConvert.ToDateTimeOffset(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseDecimal(string value, out decimal? result)
		{
			bool flag;
			try
			{
				result = new decimal?(XmlConvert.ToDecimal(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseFloat(string value, out double? result)
		{
			bool flag;
			try
			{
				result = new double?(XmlConvert.ToDouble(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseGuid(string value, out Guid? result)
		{
			bool flag;
			try
			{
				result = new Guid?(XmlConvert.ToGuid(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseInt(string value, out int? result)
		{
			bool flag;
			try
			{
				result = new int?(XmlConvert.ToInt32(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseLong(string value, out long? result)
		{
			bool flag;
			try
			{
				result = new long?(XmlConvert.ToInt64(value));
				flag = true;
			}
			catch (FormatException formatException)
			{
				result = null;
				flag = false;
			}
			return flag;
		}

		internal static bool TryParseTime(string value, out TimeSpan? result)
		{
			bool flag;
			if (EdmValueParser.TimeValidator.IsMatch(value))
			{
				try
				{
					result = new TimeSpan?(TimeSpan.Parse(string.Concat("00:", value), CultureInfo.InvariantCulture));
					flag = true;
				}
				catch (FormatException formatException)
				{
					result = null;
					flag = false;
				}
				return flag;
			}
			else
			{
				result = null;
				return false;
			}
		}
	}
}