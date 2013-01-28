using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace System.DirectoryServices.Protocols
{
	public sealed class BerConverter
	{
		private BerConverter()
		{
		}

		public static object[] Decode(string format, byte[] value)
		{
			bool flag = false;
			object[] objArray = BerConverter.TryDecode(format, value, out flag);
			if (!flag)
			{
				throw new BerConversionException();
			}
			else
			{
				return objArray;
			}
		}

		private static byte[] DecodingByteArrayHelper(BerSafeHandle berElement, char fmt, ref int error)
		{
			error = 0;
			IntPtr intPtr = (IntPtr)0;
			berval _berval = new berval();
			byte[] numArray = null;
			error = Wldap32.ber_scanf_ptr(berElement, new string(fmt, 1), ref intPtr);
			try
			{
				if (error == 0 && intPtr != (IntPtr)0)
				{
					Marshal.PtrToStructure(intPtr, _berval);
					numArray = new byte[_berval.bv_len];
					Marshal.Copy(_berval.bv_val, numArray, 0, _berval.bv_len);
				}
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Wldap32.ber_bvfree(intPtr);
				}
			}
			return numArray;
		}

		private static byte[][] DecodingMultiByteArrayHelper(BerSafeHandle berElement, char fmt, ref int error)
		{
			error = 0;
			IntPtr intPtr = (IntPtr)0;
			int num = 0;
			ArrayList arrayLists = new ArrayList();
			byte[][] item = null;
			try
			{
				error = Wldap32.ber_scanf_ptr(berElement, new string(fmt, 1), ref intPtr);
				if (error == 0 && intPtr != (IntPtr)0)
				{
					for (IntPtr i = Marshal.ReadIntPtr(intPtr); i != (IntPtr)0; i = Marshal.ReadIntPtr(intPtr, num * Marshal.SizeOf(typeof(IntPtr))))
					{
						berval _berval = new berval();
						Marshal.PtrToStructure(i, _berval);
						byte[] numArray = new byte[_berval.bv_len];
						Marshal.Copy(_berval.bv_val, numArray, 0, _berval.bv_len);
						arrayLists.Add(numArray);
						num++;
					}
					item = new byte[arrayLists.Count][];
					for (int j = 0; j < arrayLists.Count; j++)
					{
						item[j] = (byte[])arrayLists[j];
					}
				}
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Wldap32.ber_bvecfree(intPtr);
				}
			}
			return item;
		}

		public static byte[] Encode(string format, object[] value)
		{
			int num;
			Utility.CheckOSVersion();
			if (format != null)
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				byte[] numArray = null;
				if (value == null)
				{
					value = new object[0];
				}
				BerSafeHandle berSafeHandle = new BerSafeHandle();
				int num1 = 0;
				int num2 = 0;
				int num3 = 0;
				while (num3 < format.Length)
				{
					char chr = format[num3];
					if (chr == '{' || chr == '}' || chr == '[' || chr == ']' || chr == 'n')
					{
						num2 = Wldap32.ber_printf_emptyarg(berSafeHandle, new string(chr, 1));
					}
					else
					{
						if (chr == 't' || chr == 'i' || chr == 'e')
						{
							if (num1 < (int)value.Length)
							{
								if (value[num1] is int)
								{
									num2 = Wldap32.ber_printf_int(berSafeHandle, new string(chr, 1), (int)value[num1]);
									num1++;
								}
								else
								{
									throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
								}
							}
							else
							{
								throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
							}
						}
						else
						{
							if (chr != 'b')
							{
								if (chr != 's')
								{
									if (chr == 'o' || chr == 'X')
									{
										if (num1 < (int)value.Length)
										{
											if (value[num1] == null || value[num1] as byte[] != null)
											{
												byte[] numArray1 = (byte[])value[num1];
												num2 = BerConverter.EncodingByteArrayHelper(berSafeHandle, numArray1, chr);
												num1++;
											}
											else
											{
												throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
											}
										}
										else
										{
											throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
										}
									}
									else
									{
										if (chr != 'v')
										{
											if (chr != 'V')
											{
												throw new ArgumentException(Res.GetString("BerConverterUndefineChar"));
											}
											else
											{
												if (num1 < (int)value.Length)
												{
													if (value[num1] == null || value[num1] as byte[][] != null)
													{
														byte[][] numArray2 = (byte[][])value[num1];
														num2 = BerConverter.EncodingMultiByteArrayHelper(berSafeHandle, numArray2, chr);
														num1++;
													}
													else
													{
														throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
													}
												}
												else
												{
													throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
												}
											}
										}
										else
										{
											if (num1 < (int)value.Length)
											{
												if (value[num1] == null || value[num1] as string[] != null)
												{
													string[] strArrays = (string[])value[num1];
													byte[][] bytes = null;
													if (strArrays != null)
													{
														bytes = new byte[(int)strArrays.Length][];
														for (int i = 0; i < (int)strArrays.Length; i++)
														{
															string str = strArrays[i];
															if (str != null)
															{
																bytes[i] = uTF8Encoding.GetBytes(str);
															}
															else
															{
																bytes[i] = null;
															}
														}
													}
													num2 = BerConverter.EncodingMultiByteArrayHelper(berSafeHandle, bytes, 'V');
													num1++;
												}
												else
												{
													throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
												}
											}
											else
											{
												throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
											}
										}
									}
								}
								else
								{
									if (num1 < (int)value.Length)
									{
										if (value[num1] == null || value[num1] as string != null)
										{
											byte[] bytes1 = null;
											if (value[num1] != null)
											{
												bytes1 = uTF8Encoding.GetBytes((string)value[num1]);
											}
											num2 = BerConverter.EncodingByteArrayHelper(berSafeHandle, bytes1, 'o');
											num1++;
										}
										else
										{
											throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
										}
									}
									else
									{
										throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
									}
								}
							}
							else
							{
								if (num1 < (int)value.Length)
								{
									if (value[num1] is bool)
									{
										BerSafeHandle berSafeHandle1 = berSafeHandle;
										string str1 = new string(chr, 1);
										if ((bool)value[num1])
										{
											num = 1;
										}
										else
										{
											num = 0;
										}
										num2 = Wldap32.ber_printf_int(berSafeHandle1, str1, num);
										num1++;
									}
									else
									{
										throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
									}
								}
								else
								{
									throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
								}
							}
						}
					}
					if (num2 != -1)
					{
						num3++;
					}
					else
					{
						throw new BerConversionException();
					}
				}
				berval _berval = new berval();
				IntPtr intPtr = (IntPtr)0;
				try
				{
					num2 = Wldap32.ber_flatten(berSafeHandle, ref intPtr);
					if (num2 != -1)
					{
						if (intPtr != (IntPtr)0)
						{
							Marshal.PtrToStructure(intPtr, _berval);
						}
						if (_berval == null || _berval.bv_len == 0)
						{
							numArray = new byte[0];
						}
						else
						{
							numArray = new byte[_berval.bv_len];
							Marshal.Copy(_berval.bv_val, numArray, 0, _berval.bv_len);
						}
					}
					else
					{
						throw new BerConversionException();
					}
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						Wldap32.ber_bvfree(intPtr);
					}
				}
				return numArray;
			}
			else
			{
				throw new ArgumentNullException("format");
			}
		}

		private static int EncodingByteArrayHelper(BerSafeHandle berElement, byte[] tempValue, char fmt)
		{
			int num;
			if (tempValue == null)
			{
				num = Wldap32.ber_printf_bytearray(berElement, new string(fmt, 1), new HGlobalMemHandle((IntPtr)0), 0);
			}
			else
			{
				IntPtr intPtr = Marshal.AllocHGlobal((int)tempValue.Length);
				Marshal.Copy(tempValue, 0, intPtr, (int)tempValue.Length);
				HGlobalMemHandle hGlobalMemHandle = new HGlobalMemHandle(intPtr);
				num = Wldap32.ber_printf_bytearray(berElement, new string(fmt, 1), hGlobalMemHandle, (int)tempValue.Length);
			}
			return num;
		}

		private static int EncodingMultiByteArrayHelper(BerSafeHandle berElement, byte[][] tempValue, char fmt)
		{
			IntPtr intPtr;
			IntPtr intPtr1 = (IntPtr)0;
			SafeBerval[] safeBerval = null;
			int num = 0;
			try
			{
				if (tempValue != null)
				{
					int i = 0;
					intPtr1 = Utility.AllocHGlobalIntPtrArray((int)tempValue.Length + 1);
					int num1 = Marshal.SizeOf(typeof(SafeBerval));
					safeBerval = new SafeBerval[(int)tempValue.Length];
					for (i = 0; i < (int)tempValue.Length; i++)
					{
						byte[] numArray = tempValue[i];
						safeBerval[i] = new SafeBerval();
						if (numArray != null)
						{
							safeBerval[i].bv_len = (int)numArray.Length;
							safeBerval[i].bv_val = Marshal.AllocHGlobal((int)numArray.Length);
							Marshal.Copy(numArray, 0, safeBerval[i].bv_val, (int)numArray.Length);
						}
						else
						{
							safeBerval[i].bv_len = 0;
							safeBerval[i].bv_val = (IntPtr)0;
						}
						IntPtr intPtr2 = Marshal.AllocHGlobal(num1);
						Marshal.StructureToPtr(safeBerval[i], intPtr2, false);
						intPtr = (IntPtr)((long)intPtr1 + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
						Marshal.WriteIntPtr(intPtr, intPtr2);
					}
					intPtr = (IntPtr)((long)intPtr1 + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
					Marshal.WriteIntPtr(intPtr, (IntPtr)0);
				}
				num = Wldap32.ber_printf_berarray(berElement, new string(fmt, 1), intPtr1);
				GC.KeepAlive(safeBerval);
			}
			finally
			{
				if (intPtr1 != (IntPtr)0)
				{
					for (int j = 0; j < (int)tempValue.Length; j++)
					{
						IntPtr intPtr3 = Marshal.ReadIntPtr(intPtr1, Marshal.SizeOf(typeof(IntPtr)) * j);
						if (intPtr3 != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr3);
						}
					}
					Marshal.FreeHGlobal(intPtr1);
				}
			}
			return num;
		}

		internal static object[] TryDecode(string format, byte[] value, out bool decodeSucceeded)
		{
			Utility.CheckOSVersion();
			if (format != null)
			{
				UTF8Encoding uTF8Encoding = new UTF8Encoding(false, true);
				berval _berval = new berval();
				ArrayList arrayLists = new ArrayList();
				BerSafeHandle berSafeHandle = null;
				object[] item = null;
				decodeSucceeded = false;
				if (value != null)
				{
					_berval.bv_len = (int)value.Length;
					_berval.bv_val = Marshal.AllocHGlobal((int)value.Length);
					Marshal.Copy(value, 0, _berval.bv_val, (int)value.Length);
				}
				else
				{
					_berval.bv_len = 0;
					_berval.bv_val = (IntPtr)0;
				}
				try
				{
					berSafeHandle = new BerSafeHandle(_berval);
				}
				finally
				{
					if (_berval.bv_val != (IntPtr)0)
					{
						Marshal.FreeHGlobal(_berval.bv_val);
					}
				}
				int num = 0;
				int num1 = 0;
				while (num1 < format.Length)
				{
					char chr = format[num1];
					if (chr == '{' || chr == '}' || chr == '[' || chr == ']' || chr == 'n' || chr == 'x')
					{
						num = Wldap32.ber_scanf(berSafeHandle, new string(chr, 1));
						if (num != 0)
						{
						}
					}
					else
					{
						if (chr == 'i' || chr == 'e' || chr == 'b')
						{
							int num2 = 0;
							num = Wldap32.ber_scanf_int(berSafeHandle, new string(chr, 1), ref num2);
							if (num == 0)
							{
								if (chr != 'b')
								{
									arrayLists.Add(num2);
								}
								else
								{
									bool flag = false;
									if (num2 != 0)
									{
										flag = true;
									}
									else
									{
										flag = false;
									}
									arrayLists.Add(flag);
								}
							}
						}
						else
						{
							if (chr != 'a')
							{
								if (chr != 'O')
								{
									if (chr != 'B')
									{
										if (chr != 'v')
										{
											if (chr != 'V')
											{
												throw new ArgumentException(Res.GetString("BerConverterUndefineChar"));
											}
											else
											{
												byte[][] numArray = BerConverter.DecodingMultiByteArrayHelper(berSafeHandle, chr, ref num);
												if (num == 0)
												{
													arrayLists.Add(numArray);
												}
											}
										}
										else
										{
											string[] str = null;
											byte[][] numArray1 = BerConverter.DecodingMultiByteArrayHelper(berSafeHandle, 'V', ref num);
											if (num == 0)
											{
												if (numArray1 != null)
												{
													str = new string[(int)numArray1.Length];
													for (int i = 0; i < (int)numArray1.Length; i++)
													{
														if (numArray1[i] != null)
														{
															str[i] = uTF8Encoding.GetString(numArray1[i]);
														}
														else
														{
															str[i] = null;
														}
													}
												}
												arrayLists.Add(str);
											}
										}
									}
									else
									{
										IntPtr intPtr = (IntPtr)0;
										int num3 = 0;
										num = Wldap32.ber_scanf_bitstring(berSafeHandle, "B", ref intPtr, ref num3);
										if (num == 0)
										{
											byte[] numArray2 = null;
											if (intPtr != (IntPtr)0)
											{
												numArray2 = new byte[num3];
												Marshal.Copy(intPtr, numArray2, 0, num3);
											}
											arrayLists.Add(numArray2);
										}
									}
								}
								else
								{
									byte[] numArray3 = BerConverter.DecodingByteArrayHelper(berSafeHandle, chr, ref num);
									if (num == 0)
									{
										arrayLists.Add(numArray3);
									}
								}
							}
							else
							{
								byte[] numArray4 = BerConverter.DecodingByteArrayHelper(berSafeHandle, 'O', ref num);
								if (num == 0)
								{
									string str1 = null;
									if (numArray4 != null)
									{
										str1 = uTF8Encoding.GetString(numArray4);
									}
									arrayLists.Add(str1);
								}
							}
						}
					}
					if (num == 0)
					{
						num1++;
					}
					else
					{
						return item;
					}
				}
				item = new object[arrayLists.Count];
				for (int j = 0; j < arrayLists.Count; j++)
				{
					item[j] = arrayLists[j];
				}
				decodeSucceeded = true;
				return item;
			}
			else
			{
				throw new ArgumentNullException("format");
			}
		}
	}
}