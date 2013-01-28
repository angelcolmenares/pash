using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;

namespace System.Runtime
{
	internal static class UrlUtility
	{
		private static int HexToInt(char h)
		{
			if (h < '0' || h > '9')
			{
				if (h < 'a' || h > 'f')
				{
					if (h < 'A' || h > 'F')
					{
						return -1;
					}
					else
					{
						return h - 65 + 10;
					}
				}
				else
				{
					return h - 97 + 10;
				}
			}
			else
			{
				return h - 48;
			}
		}

		private static char IntToHex(int n)
		{
			if (n > 9)
			{
				return (ushort)(n - 10 + 97);
			}
			else
			{
				return (ushort)(n + 48);
			}
		}

		private static bool IsNonAsciiByte(byte b)
		{
			if (b >= 127)
			{
				return true;
			}
			else
			{
				return b < 32;
			}
		}

		internal static bool IsSafe(char ch)
		{
			if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z') && (ch < '0' || ch > '9'))
			{
				char chr = ch;
				if (chr != '!')
				{
					if (chr == '\'' || chr == '(' || chr == ')' || chr == '*' || chr == '-' || chr == '.')
					{
						return true;
					}
					else if (chr == '+' || chr == ',')
					{
						return false;
					}
					if (chr == '\u005F')
					{
						return true;
					}
					return false;
				}
				return true;
			}
			else
			{
				return true;
			}
		}

		public static NameValueCollection ParseQueryString(string query)
		{
			return UrlUtility.ParseQueryString(query, Encoding.UTF8);
		}

		public static NameValueCollection ParseQueryString(string query, Encoding encoding)
		{
			if (query != null)
			{
				if (encoding != null)
				{
					if (query.Length > 0 && query[0] == '?')
					{
						query = query.Substring(1);
					}
					return new UrlUtility.HttpValueCollection(query, encoding);
				}
				else
				{
					throw Fx.Exception.ArgumentNull("encoding");
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("query");
			}
		}

		public static string UrlDecode(string str, Encoding e)
		{
			if (str != null)
			{
				return UrlUtility.UrlDecodeStringFromStringInternal(str, e);
			}
			else
			{
				return null;
			}
		}

		private static string UrlDecodeStringFromStringInternal(string s, Encoding e)
		{
			int length = s.Length;
			UrlUtility.UrlDecoder urlDecoder = new UrlUtility.UrlDecoder(length, e);
			for (int i = 0; i < length; i++)
			{
				char chr = s[i];
				if (chr != '+')
				{
					if (chr == '%' && i < length - 2)
					{
						if (s[i + 1] != 'u' || i >= length - 5)
						{
							int num = UrlUtility.HexToInt(s[i + 1]);
							int num1 = UrlUtility.HexToInt(s[i + 2]);
							if (num < 0 || num1 < 0)
							{
								goto Label1;
							}
							byte num2 = (byte)(num << 4 | num1);
							i = i + 2;
							urlDecoder.AddByte(num2);
							goto Label0;
						}
						else
						{
							int num3 = UrlUtility.HexToInt(s[i + 2]);
							int num4 = UrlUtility.HexToInt(s[i + 3]);
							int num5 = UrlUtility.HexToInt(s[i + 4]);
							int num6 = UrlUtility.HexToInt(s[i + 5]);
							if (num3 < 0 || num4 < 0 || num5 < 0 || num6 < 0)
							{
								goto Label1;
							}
							chr = (char)((ushort)(num3 << 12 | num4 << 8 | num5 << 4 | num6));
							i = i + 5;
							urlDecoder.AddChar(chr);
							continue;
						}
					}
				}
				else
				{
					chr = ' ';
				}
			Label1:
				if ((chr & '\uFF80') != 0)
				{
					urlDecoder.AddChar(chr);
				}
				else
				{
					urlDecoder.AddByte((byte)chr);
				}
			}
			return urlDecoder.GetString();
		}

		public static string UrlEncode(string str)
		{
			if (str != null)
			{
				return UrlUtility.UrlEncode(str, Encoding.UTF8);
			}
			else
			{
				return null;
			}
		}

		public static string UrlEncode(string str, Encoding encoding)
		{
			if (str != null)
			{
				return Encoding.ASCII.GetString(UrlUtility.UrlEncodeToBytes(str, encoding));
			}
			else
			{
				return null;
			}
		}

		private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
		{
			int num = 0;
			int num1 = 0;
			for (int i = 0; i < count; i++)
			{
				char chr = (char)bytes[offset + i];
				if (chr != ' ')
				{
					if (!UrlUtility.IsSafe(chr))
					{
						num1++;
					}
				}
				else
				{
					num++;
				}
			}
			if (alwaysCreateReturnValue || num != 0 || num1 != 0)
			{
				byte[] hex = new byte[count + num1 * 2];
				int num2 = 0;
				for (int j = 0; j < count; j++)
				{
					byte num3 = bytes[offset + j];
					char chr1 = (char)num3;
					if (!UrlUtility.IsSafe(chr1))
					{
						if (chr1 != ' ')
						{
							int num4 = num2;
							num2 = num4 + 1;
							hex[num4] = 37;
							int num5 = num2;
							num2 = num5 + 1;
							hex[num5] = (byte)UrlUtility.IntToHex(num3 >> 4 & 15);
							int num6 = num2;
							num2 = num6 + 1;
							hex[num6] = (byte)UrlUtility.IntToHex(num3 & 15);
						}
						else
						{
							int num7 = num2;
							num2 = num7 + 1;
							hex[num7] = 43;
						}
					}
					else
					{
						int num8 = num2;
						num2 = num8 + 1;
						hex[num8] = num3;
					}
				}
				return hex;
			}
			else
			{
				return bytes;
			}
		}

		private static byte[] UrlEncodeBytesToBytesInternalNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (UrlUtility.IsNonAsciiByte(bytes[offset + i]))
				{
					num++;
				}
			}
			if (alwaysCreateReturnValue || num != 0)
			{
				byte[] hex = new byte[count + num * 2];
				int num1 = 0;
				for (int j = 0; j < count; j++)
				{
					byte num2 = bytes[offset + j];
					if (!UrlUtility.IsNonAsciiByte(num2))
					{
						int num3 = num1;
						num1 = num3 + 1;
						hex[num3] = num2;
					}
					else
					{
						int num4 = num1;
						num1 = num4 + 1;
						hex[num4] = 37;
						int num5 = num1;
						num1 = num5 + 1;
						hex[num5] = (byte)UrlUtility.IntToHex(num2 >> 4 & 15);
						int num6 = num1;
						num1 = num6 + 1;
						hex[num6] = (byte)UrlUtility.IntToHex(num2 & 15);
					}
				}
				return hex;
			}
			else
			{
				return bytes;
			}
		}

		private static string UrlEncodeNonAscii(string str, Encoding e)
		{
			if (!string.IsNullOrEmpty(str))
			{
				if (e == null)
				{
					e = Encoding.UTF8;
				}
				byte[] bytes = e.GetBytes(str);
				bytes = UrlUtility.UrlEncodeBytesToBytesInternalNonAscii(bytes, 0, (int)bytes.Length, false);
				return Encoding.ASCII.GetString(bytes);
			}
			else
			{
				return str;
			}
		}

		private static string UrlEncodeSpaces(string str)
		{
			if (str != null && str.IndexOf(' ') >= 0)
			{
				str = str.Replace(" ", "%20");
			}
			return str;
		}

		public static byte[] UrlEncodeToBytes(string str, Encoding e)
		{
			if (str != null)
			{
				byte[] bytes = e.GetBytes(str);
				return UrlUtility.UrlEncodeBytesToBytesInternal(bytes, 0, (int)bytes.Length, false);
			}
			else
			{
				return null;
			}
		}

		public static string UrlEncodeUnicode(string str)
		{
			if (str != null)
			{
				return UrlUtility.UrlEncodeUnicodeStringToStringInternal(str, false);
			}
			else
			{
				return null;
			}
		}

		private static string UrlEncodeUnicodeStringToStringInternal(string s, bool ignoreAscii)
		{
			int length = s.Length;
			StringBuilder stringBuilder = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				char chr = s[i];
				if ((chr & '\uFF80') != 0)
				{
					stringBuilder.Append("%u");
					stringBuilder.Append(UrlUtility.IntToHex(chr >> '\f' & 15));
					stringBuilder.Append(UrlUtility.IntToHex(chr >> '\b' & 15));
					stringBuilder.Append(UrlUtility.IntToHex(chr >> '\u0004' & 15));
					stringBuilder.Append(UrlUtility.IntToHex(chr & '\u000F'));
				}
				else
				{
					if (ignoreAscii || UrlUtility.IsSafe(chr))
					{
						stringBuilder.Append(chr);
					}
					else
					{
						if (chr != ' ')
						{
							stringBuilder.Append('%');
							stringBuilder.Append(UrlUtility.IntToHex(chr >> '\u0004' & 15));
							stringBuilder.Append(UrlUtility.IntToHex(chr & '\u000F'));
						}
						else
						{
							stringBuilder.Append('+');
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		public static string UrlPathEncode(string str)
		{
			if (str != null)
			{
				int num = str.IndexOf('?');
				if (num < 0)
				{
					return UrlUtility.UrlEncodeSpaces(UrlUtility.UrlEncodeNonAscii(str, Encoding.UTF8));
				}
				else
				{
					return string.Concat(UrlUtility.UrlPathEncode(str.Substring(0, num)), str.Substring(num));
				}
			}
			else
			{
				return null;
			}
		}

		[Serializable]
		private class HttpValueCollection : NameValueCollection
		{
			internal HttpValueCollection(string str, Encoding encoding) : base(StringComparer.OrdinalIgnoreCase)
			{
				if (!string.IsNullOrEmpty(str))
				{
					this.FillFromString(str, true, encoding);
				}
				base.IsReadOnly = false;
			}

			protected HttpValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
			{
			}

			internal void FillFromString(string s, bool urlencoded, Encoding encoding)
			{
				string str;
				int length;
				if (s != null)
				{
					length = s.Length;
				}
				else
				{
					length = 0;
				}
				int num = length;
				for (int i = 0; i < num; i++)
				{
					int num1 = i;
					int num2 = -1;
					while (i < num)
					{
						char chr = s[i];
						if (chr != '=')
						{
							if (chr == '&')
							{
								break;
							}
						}
						else
						{
							if (num2 < 0)
							{
								num2 = i;
							}
						}
						i++;
					}
					string str1 = null;
					if (num2 < 0)
					{
						str = s.Substring(num1, i - num1);
					}
					else
					{
						str1 = s.Substring(num1, num2 - num1);
						str = s.Substring(num2 + 1, i - num2 - 1);
					}
					if (!urlencoded)
					{
						base.Add(str1, str);
					}
					else
					{
						base.Add(UrlUtility.UrlDecode(str1, encoding), UrlUtility.UrlDecode(str, encoding));
					}
					if (i == num - 1 && s[i] == '&')
					{
						base.Add(null, string.Empty);
					}
				}
			}

			public override string ToString()
			{
				return this.ToString(true, null);
			}

			private string ToString(bool urlencoded, IDictionary excludeKeys)
			{
				string item;
				string empty;
				int count;
				int num = this.Count;
				if (num != 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < num; i++)
					{
						string key = this.GetKey(i);
						if (excludeKeys == null || key == null || excludeKeys[key] == null)
						{
							if (urlencoded)
							{
								key = UrlUtility.UrlEncodeUnicode(key);
							}
							if (!string.IsNullOrEmpty(key))
							{
								empty = string.Concat(key, "=");
							}
							else
							{
								empty = string.Empty;
							}
							string str = empty;
							ArrayList arrayLists = (ArrayList)base.BaseGet(i);
							if (arrayLists != null)
							{
								count = arrayLists.Count;
							}
							else
							{
								count = 0;
							}
							int num1 = count;
							if (stringBuilder.Length > 0)
							{
								stringBuilder.Append('&');
							}
							if (num1 != 1)
							{
								if (num1 != 0)
								{
									for (int j = 0; j < num1; j++)
									{
										if (j > 0)
										{
											stringBuilder.Append('&');
										}
										stringBuilder.Append(str);
										item = (string)arrayLists[j];
										if (urlencoded)
										{
											item = UrlUtility.UrlEncodeUnicode(item);
										}
										stringBuilder.Append(item);
									}
								}
								else
								{
									stringBuilder.Append(str);
								}
							}
							else
							{
								stringBuilder.Append(str);
								item = (string)arrayLists[0];
								if (urlencoded)
								{
									item = UrlUtility.UrlEncodeUnicode(item);
								}
								stringBuilder.Append(item);
							}
						}
					}
					return stringBuilder.ToString();
				}
				else
				{
					return string.Empty;
				}
			}
		}

		private class UrlDecoder
		{
			private int _bufferSize;

			private int _numChars;

			private char[] _charBuffer;

			private int _numBytes;

			private byte[] _byteBuffer;

			private Encoding _encoding;

			internal UrlDecoder(int bufferSize, Encoding encoding)
			{
				this._bufferSize = bufferSize;
				this._encoding = encoding;
				this._charBuffer = new char[bufferSize];
			}

			internal void AddByte(byte b)
			{
				if (this._byteBuffer == null)
				{
					this._byteBuffer = new byte[this._bufferSize];
				}
				UrlUtility.UrlDecoder urlDecoder = this;
				int num = urlDecoder._numBytes;
				int num1 = num;
				urlDecoder._numBytes = num + 1;
				this._byteBuffer[num1] = b;
			}

			internal void AddChar(char ch)
			{
				if (this._numBytes > 0)
				{
					this.FlushBytes();
				}
				UrlUtility.UrlDecoder urlDecoder = this;
				int num = urlDecoder._numChars;
				int num1 = num;
				urlDecoder._numChars = num + 1;
				this._charBuffer[num1] = ch;
			}

			private void FlushBytes()
			{
				if (this._numBytes > 0)
				{
					UrlUtility.UrlDecoder chars = this;
					chars._numChars = chars._numChars + this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
					this._numBytes = 0;
				}
			}

			internal string GetString()
			{
				if (this._numBytes > 0)
				{
					this.FlushBytes();
				}
				if (this._numChars <= 0)
				{
					return string.Empty;
				}
				else
				{
					return new string(this._charBuffer, 0, this._numChars);
				}
			}
		}
	}
}