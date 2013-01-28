using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADTypeConverter
	{
		private const string _debugCategory = "ADTypeConverter";

		private ADSessionInfo _sessionInfo;

		private ADSchema _adSchema;

		private static UTF8Encoding _encoder;

		static ADTypeConverter()
		{
			ADTypeConverter._encoder = new UTF8Encoding();
		}

		public ADTypeConverter()
		{
		}

		public ADTypeConverter(ADSessionInfo sessionInfo)
		{
			this._sessionInfo = sessionInfo;
		}

		internal ADPropertyValueCollection ConvertFromRaw(string propertyName, ADPropertyValueCollection propertyValues)
		{
			string str = null;
			int num = 0;
			ADPropertyValueCollection aDPropertyValueCollection;
			byte[] bytes;
			this.Init();
			if (propertyValues.Count != 0)
			{
				ADObjectSearcher.ContainsRangeRetrievalTag(propertyName, out str, out num);
				ADAttributeSyntax propertyType = this._adSchema.GetPropertyType(str);
				ADAttributeSyntax aDAttributeSyntax = propertyType;
				switch (aDAttributeSyntax)
				{
					case ADAttributeSyntax.DirectoryString:
					case ADAttributeSyntax.DN:
					{
						aDPropertyValueCollection = propertyValues;
						break;
					}
					case ADAttributeSyntax.OctetString:
					{
						aDPropertyValueCollection = propertyValues;
						break;
					}
					case ADAttributeSyntax.SecurityDescriptor:
					{
						aDPropertyValueCollection = new ADPropertyValueCollection(propertyValues.Count);
						IEnumerator enumerator = propertyValues.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								byte[] current = (byte[])enumerator.Current;
								ActiveDirectorySecurity activeDirectorySecurity = new ActiveDirectorySecurity();
								activeDirectorySecurity.SetSecurityDescriptorBinaryForm(current);
								aDPropertyValueCollection.Add(activeDirectorySecurity);
							}
							break;
						}
						finally
						{
							IDisposable disposable = enumerator as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
					}
					case ADAttributeSyntax.Int:
					case ADAttributeSyntax.Enumeration:
					{
						aDPropertyValueCollection = new ADPropertyValueCollection(propertyValues.Count);
						IEnumerator enumerator1 = propertyValues.GetEnumerator();
						try
						{
							while (enumerator1.MoveNext())
							{
								string current1 = (string)enumerator1.Current;
								aDPropertyValueCollection.Add(int.Parse(current1, NumberFormatInfo.InvariantInfo));
							}
							break;
						}
						finally
						{
							IDisposable disposable1 = enumerator1 as IDisposable;
							if (disposable1 != null)
							{
								disposable1.Dispose();
							}
						}
					}
					case ADAttributeSyntax.Int64:
					{
						aDPropertyValueCollection = new ADPropertyValueCollection(propertyValues.Count);
						IEnumerator enumerator2 = propertyValues.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								string str1 = (string)enumerator2.Current;
								aDPropertyValueCollection.Add(long.Parse(str1, NumberFormatInfo.InvariantInfo));
							}
							break;
						}
						finally
						{
							IDisposable disposable2 = enumerator2 as IDisposable;
							if (disposable2 != null)
							{
								disposable2.Dispose();
							}
						}
					}
					case ADAttributeSyntax.Bool:
					{
						aDPropertyValueCollection = new ADPropertyValueCollection(propertyValues.Count);
						IEnumerator enumerator3 = propertyValues.GetEnumerator();
						try
						{
							while (enumerator3.MoveNext())
							{
								string current2 = (string)enumerator3.Current;
								if (string.Compare(current2, "TRUE", StringComparison.OrdinalIgnoreCase) != 0)
								{
									aDPropertyValueCollection.Add(false);
								}
								else
								{
									aDPropertyValueCollection.Add(true);
								}
							}
							break;
						}
						finally
						{
							IDisposable disposable3 = enumerator3 as IDisposable;
							if (disposable3 != null)
							{
								disposable3.Dispose();
							}
						}
					}
					case ADAttributeSyntax.Oid:
					case ADAttributeSyntax.DNWithBinary:
					case ADAttributeSyntax.DNWithString:
					case ADAttributeSyntax.IA5String:
					case ADAttributeSyntax.PrintableString:
					{
						aDPropertyValueCollection = propertyValues;
						break;
					}
					case ADAttributeSyntax.GeneralizedTime:
					case ADAttributeSyntax.UtcTime:
					{
						aDPropertyValueCollection = new ADPropertyValueCollection(propertyValues.Count);
						IEnumerator enumerator4 = propertyValues.GetEnumerator();
						try
						{
							while (enumerator4.MoveNext())
							{
								string str2 = (string)enumerator4.Current;
								aDPropertyValueCollection.Add(ADTypeConverter.ParseDateTimeValue(str2, propertyType));
							}
							break;
						}
						finally
						{
							IDisposable disposable4 = enumerator4 as IDisposable;
							if (disposable4 != null)
							{
								disposable4.Dispose();
							}
						}
					}
					case ADAttributeSyntax.Sid:
					{
						aDPropertyValueCollection = new ADPropertyValueCollection(propertyValues.Count);
						IEnumerator enumerator5 = propertyValues.GetEnumerator();
						try
						{
							while (enumerator5.MoveNext())
							{
								object obj = enumerator5.Current;
								if (obj as string == null)
								{
									bytes = (byte[])obj;
								}
								else
								{
									bytes = ADTypeConverter._encoder.GetBytes((string)obj);
								}
								aDPropertyValueCollection.Add(new SecurityIdentifier(bytes, 0));
							}
							break;
						}
						finally
						{
							IDisposable disposable5 = enumerator5 as IDisposable;
							if (disposable5 != null)
							{
								disposable5.Dispose();
							}
						}
					}
					default:
					{
						aDPropertyValueCollection = propertyValues;
						break;
					}
				}
				return aDPropertyValueCollection;
			}
			else
			{
				return null;
			}
		}

		internal ADPropertyValueCollection ConvertFromRaw(DirectoryAttribute property)
		{
			string str = null;
			int num = 0;
			this.Init();
			if (property == null || property.Count == 0)
			{
				return null;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
				ADObjectSearcher.ContainsRangeRetrievalTag(property.Name, out str, out num);
				ADAttributeSyntax propertyType = this._adSchema.GetPropertyType(str);
				string[] values = null;
				byte[][] numArray = null;
				ADAttributeSyntax aDAttributeSyntax = propertyType;
				switch (aDAttributeSyntax)
				{
					case ADAttributeSyntax.DirectoryString:
					case ADAttributeSyntax.DN:
					{
						aDPropertyValueCollection.AddRange(property.GetValues(typeof(string)));
						break;
					}
					case ADAttributeSyntax.OctetString:
					{
						aDPropertyValueCollection.AddRange(property.GetValues(typeof(byte[])));
						break;
					}
					case ADAttributeSyntax.SecurityDescriptor:
					{
						numArray = (byte[][])property.GetValues(typeof(byte[]));
						byte[][] numArray1 = numArray;
						for (int i = 0; i < (int)numArray1.Length; i++)
						{
							byte[] numArray2 = numArray1[i];
							ActiveDirectorySecurity activeDirectorySecurity = new ActiveDirectorySecurity();
							activeDirectorySecurity.SetSecurityDescriptorBinaryForm(numArray2);
							aDPropertyValueCollection.Add(activeDirectorySecurity);
						}
						break;
					}
					case ADAttributeSyntax.Int:
					case ADAttributeSyntax.Enumeration:
					{
						values = (string[])property.GetValues(typeof(string));
						string[] strArrays = values;
						for (int j = 0; j < (int)strArrays.Length; j++)
						{
							string str1 = strArrays[j];
							aDPropertyValueCollection.Add(int.Parse(str1, NumberFormatInfo.InvariantInfo));
						}
						break;
					}
					case ADAttributeSyntax.Int64:
					{
						values = (string[])property.GetValues(typeof(string));
						string[] strArrays1 = values;
						for (int k = 0; k < (int)strArrays1.Length; k++)
						{
							string str2 = strArrays1[k];
							aDPropertyValueCollection.Add(long.Parse(str2, NumberFormatInfo.InvariantInfo));
						}
						break;
					}
					case ADAttributeSyntax.Bool:
					{
						values = (string[])property.GetValues(typeof(string));
						string[] strArrays2 = values;
						for (int l = 0; l < (int)strArrays2.Length; l++)
						{
							string str3 = strArrays2[l];
							if (string.Compare(str3, "TRUE", StringComparison.OrdinalIgnoreCase) != 0)
							{
								aDPropertyValueCollection.Add(false);
							}
							else
							{
								aDPropertyValueCollection.Add(true);
							}
						}
						break;
					}
					case ADAttributeSyntax.Oid:
					case ADAttributeSyntax.DNWithBinary:
					case ADAttributeSyntax.DNWithString:
					case ADAttributeSyntax.IA5String:
					case ADAttributeSyntax.PrintableString:
					{
						aDPropertyValueCollection.AddRange(property.GetValues(typeof(string)));
						break;
					}
					case ADAttributeSyntax.GeneralizedTime:
					case ADAttributeSyntax.UtcTime:
					{
						values = (string[])property.GetValues(typeof(string));
						string[] strArrays3 = values;
						for (int m = 0; m < (int)strArrays3.Length; m++)
						{
							string str4 = strArrays3[m];
							aDPropertyValueCollection.Add(ADTypeConverter.ParseDateTimeValue(str4, propertyType));
						}
						break;
					}
					case ADAttributeSyntax.Sid:
					{
						numArray = (byte[][])property.GetValues(typeof(byte[]));
						byte[][] numArray3 = numArray;
						for (int n = 0; n < (int)numArray3.Length; n++)
						{
							byte[] numArray4 = numArray3[n];
							aDPropertyValueCollection.Add(new SecurityIdentifier(numArray4, 0));
						}
						break;
					}
					default:
					{
						if (aDAttributeSyntax == ADAttributeSyntax.ReplicaLink)
						{
							aDPropertyValueCollection.AddRange(property.GetValues(typeof(byte[])));
							break;
						}
						aDPropertyValueCollection.AddRange(property.GetValues(typeof(string)));
						break;
					}
				}
				return aDPropertyValueCollection;
			}
		}

		internal ADPropertyValueCollection ConvertFromRawAsString(DirectoryAttribute property)
		{
			if (property == null || property.Count == 0)
			{
				return null;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(property.GetValues(typeof(string)));
				return aDPropertyValueCollection;
			}
		}

		public object ConvertToRaw(string propertyName, object propertyValue)
		{
			object[] objArray;
			if (propertyValue != null)
			{
				Type type = propertyValue.GetType();
				TypeCode typeCode = Type.GetTypeCode(type);
				switch (typeCode)
				{
					case TypeCode.Object:
					{
						if (type != typeof(byte[]))
						{
							if (type != typeof(Guid))
							{
								if (type != typeof(SecurityIdentifier))
								{
									if (type != typeof(ActiveDirectorySecurity))
									{
										if (!typeof(X509Certificate).IsAssignableFrom(type))
										{
											DebugLogger.LogWarning("ADTypeConverter", string.Concat("ConvertToRaw: invalid value type ", type.ToString(), " for ", propertyName));
											object[] objArray1 = new object[1];
											objArray1[0] = type;
											throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray1), propertyName);
										}
										else
										{
											return this.FormatCertificateValue(propertyName, (X509Certificate)propertyValue);
										}
									}
									else
									{
										return ((ActiveDirectorySecurity)propertyValue).GetSecurityDescriptorBinaryForm();
									}
								}
								else
								{
									SecurityIdentifier securityIdentifier = (SecurityIdentifier)propertyValue;
									byte[] numArray = new byte[securityIdentifier.BinaryLength];
									securityIdentifier.GetBinaryForm(numArray, 0);
									return numArray;
								}
							}
							else
							{
								return this.FormatGuidValue(propertyName, (Guid)propertyValue);
							}
						}
						else
						{
							return propertyValue;
						}
					}
					case TypeCode.DBNull:
					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
					case TypeCode.Object | TypeCode.DateTime:
					{
						DebugLogger.LogWarning("ADTypeConverter", string.Concat("ConvertToRaw: invalid value type ", type.ToString(), " for ", propertyName));
						objArray = new object[1];
						objArray[0] = type;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
					}
					case TypeCode.Boolean:
					{
						if (!(bool)propertyValue)
						{
							return "FALSE";
						}
						else
						{
							return "TRUE";
						}
					}
					case TypeCode.Char:
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					{
						IFormattable formattable = propertyValue as IFormattable;
						if (formattable != null)
						{
							return formattable.ToString(string.Empty, NumberFormatInfo.InvariantInfo);
						}
						else
						{
							return propertyValue.ToString();
						}
					}
					case TypeCode.DateTime:
					{
						return this.FormatDateTimeValue(propertyName, (DateTime)propertyValue);
					}
					case TypeCode.String:
					{
						return propertyValue;
					}
					default:
					{
						DebugLogger.LogWarning("ADTypeConverter", string.Concat("ConvertToRaw: invalid value type ", type.ToString(), " for ", propertyName));
						objArray = new object[1];
						objArray[0] = type;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
					}
				}
			}
			else
			{
				return null;
			}
		}

		public object[] ConvertToRaw(string propertyName, IList valueList)
		{
			string[] item;
			byte[][] securityDescriptorBinaryForm;
			object[] objArray;
			string str;
			if (valueList == null || valueList.Count == 0)
			{
				return null;
			}
			else
			{
				Type type = valueList[0].GetType();
				TypeCode typeCode = Type.GetTypeCode(type);
				switch (typeCode)
				{
					case TypeCode.Object:
					{
						if (type != typeof(byte[]))
						{
							if (type != typeof(Guid))
							{
								if (type != typeof(SecurityIdentifier))
								{
									if (type != typeof(ActiveDirectorySecurity))
									{
										if (!typeof(X509Certificate).IsAssignableFrom(type))
										{
											DebugLogger.LogWarning("ADTypeConverter", string.Concat("ConvertToRaw: invalid value type ", type.ToString(), " for ", propertyName));
											object[] objArray1 = new object[1];
											objArray1[0] = type;
											throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray1), propertyName);
										}
										else
										{
											return this.FormatCertificateValue(propertyName, valueList);
										}
									}
									else
									{
										securityDescriptorBinaryForm = new byte[valueList.Count][];
										for (int i = 0; i < valueList.Count; i++)
										{
											securityDescriptorBinaryForm[i] = ((ActiveDirectorySecurity)valueList[i]).GetSecurityDescriptorBinaryForm();
										}
										return securityDescriptorBinaryForm;
									}
								}
								else
								{
									securityDescriptorBinaryForm = new byte[valueList.Count][];
									for (int j = 0; j < valueList.Count; j++)
									{
										SecurityIdentifier securityIdentifier = (SecurityIdentifier)valueList[j];
										byte[] numArray = new byte[securityIdentifier.BinaryLength];
										securityIdentifier.GetBinaryForm(numArray, 0);
										securityDescriptorBinaryForm[j] = numArray;
									}
									return securityDescriptorBinaryForm;
								}
							}
							else
							{
								return this.FormatGuidValue(propertyName, valueList);
							}
						}
						else
						{
							securityDescriptorBinaryForm = new byte[valueList.Count][];
							for (int k = 0; k < valueList.Count; k++)
							{
								securityDescriptorBinaryForm[k] = (byte[])valueList[k];
							}
							return securityDescriptorBinaryForm;
						}
					}
					case TypeCode.DBNull:
					case TypeCode.Object | TypeCode.DateTime:
					{
						DebugLogger.LogWarning("ADTypeConverter", string.Concat("ConvertToRaw: invalid value type ", type.ToString(), " for ", propertyName));
						objArray = new object[1];
						objArray[0] = type;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
					}
					case TypeCode.Boolean:
					{
						item = new string[valueList.Count];
						for (int l = 0; l < valueList.Count; l++)
						{
							if (!(bool)valueList[l])
							{
								item[l] = "FALSE";
							}
							else
							{
								item[l] = "TRUE";
							}
						}
						return item;
					}
					case TypeCode.Char:
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
					{
						item = new string[valueList.Count];
						for (int m = 0; m < valueList.Count; m++)
						{
							IFormattable formattable = valueList[m] as IFormattable;
							string[] strArrays = item;
							int num = m;
							if (formattable != null)
							{
								str = formattable.ToString(string.Empty, NumberFormatInfo.InvariantInfo);
							}
							else
							{
								str = valueList[m].ToString();
							}
							strArrays[num] = str;
						}
						return item;
					}
					case TypeCode.DateTime:
					{
						return this.FormatDateTimeValue(propertyName, valueList);
					}
					case TypeCode.String:
					{
						item = new string[valueList.Count];
						for (int n = 0; n < valueList.Count; n++)
						{
							item[n] = (string)valueList[n];
						}
						return item;
					}
					default:
					{
						DebugLogger.LogWarning("ADTypeConverter", string.Concat("ConvertToRaw: invalid value type ", type.ToString(), " for ", propertyName));
						objArray = new object[1];
						objArray[0] = type;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
					}
				}
			}
		}

		private object FormatCertificateValue(string propertyName, X509Certificate propertyValue)
		{
			ADAttributeSyntax propertyType = this.GetPropertyType(propertyName, ADAttributeSyntax.OctetString);
			ADAttributeSyntax aDAttributeSyntax = propertyType;
			if (aDAttributeSyntax == ADAttributeSyntax.OctetString || aDAttributeSyntax == ADAttributeSyntax.ReplicaLink)
			{
				return propertyValue.GetRawCertData();
			}
			else
			{
				return propertyValue.ToString();
			}
		}

		private object[] FormatCertificateValue(string propertyName, IList valueList)
		{
			object[] rawCertData;
			ADAttributeSyntax propertyType = this.GetPropertyType(propertyName, ADAttributeSyntax.OctetString);
			ADAttributeSyntax aDAttributeSyntax = propertyType;
			if (aDAttributeSyntax == ADAttributeSyntax.OctetString || aDAttributeSyntax == ADAttributeSyntax.ReplicaLink)
			{
				rawCertData = new byte[valueList.Count][];
				for (int i = 0; i < valueList.Count; i++)
				{
					rawCertData[i] = ((X509Certificate)valueList[i]).GetRawCertData();
				}
			}
			else
			{
				rawCertData = new string[valueList.Count];
				for (int j = 0; j < valueList.Count; j++)
				{
					rawCertData[j] = valueList[j].ToString();
				}
			}
			return rawCertData;
		}

		private string FormatDateTimeValue(string propertyName, DateTime propertyValue)
		{
			object[] objArray;
			ADAttributeSyntax propertyType = this.GetPropertyType(propertyName, ADAttributeSyntax.GeneralizedTime);
			ADAttributeSyntax aDAttributeSyntax = propertyType;
			switch (aDAttributeSyntax)
			{
				case ADAttributeSyntax.Int64:
				{
					long fileTimeUtc = propertyValue.ToFileTimeUtc();
					return fileTimeUtc.ToString();
				}
				case ADAttributeSyntax.Bool:
				case ADAttributeSyntax.Oid:
				{
					DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: DateTime value for ", propertyName, " of syntax ", propertyType.ToString()));
					objArray = new object[1];
					objArray[0] = typeof(DateTime);
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
				}
				case ADAttributeSyntax.GeneralizedTime:
				{
					if (propertyValue.Year >= 0x3e8)
					{
						propertyValue = propertyValue.ToUniversalTime();
						return propertyValue.ToString("yyyyMMddHHmmss.0Z");
					}
					else
					{
						int year = propertyValue.Year;
						DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: Invalid year ", year.ToString(), " for ", propertyName));
						throw new ArgumentOutOfRangeException(propertyName);
					}
				}
				case ADAttributeSyntax.UtcTime:
				{
					propertyValue = propertyValue.ToUniversalTime();
					return propertyValue.ToString("yyMMddHHmmssZ");
				}
				default:
				{
					DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: DateTime value for ", propertyName, " of syntax ", (object)propertyType.ToString()));
					objArray = new object[1];
					objArray[0] = typeof(DateTime);
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
				}
			}
		}

		private string[] FormatDateTimeValue(string propertyName, IList valueList)
		{
			DateTime item;
			object[] objArray;
			string[] str = null;
			ADAttributeSyntax propertyType = this.GetPropertyType(propertyName, ADAttributeSyntax.GeneralizedTime);
			ADAttributeSyntax aDAttributeSyntax = propertyType;
			if (aDAttributeSyntax == ADAttributeSyntax.Int64)
			{
				str = new string[valueList.Count];
				for (int i = 0; i < valueList.Count; i++)
				{
					item = (DateTime)valueList[i];
					long fileTimeUtc = item.ToFileTimeUtc();
					str[i] = fileTimeUtc.ToString();
				}
			}
			else if (aDAttributeSyntax == ADAttributeSyntax.Bool || aDAttributeSyntax == ADAttributeSyntax.Oid)
			{
				DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: DateTime value for ", propertyName, " of syntax ", (object)propertyType.ToString()));
				objArray = new object[1];
				objArray[0] = typeof(DateTime);
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
			}
			else if (aDAttributeSyntax == ADAttributeSyntax.GeneralizedTime)
			{
				str = new string[valueList.Count];
				int num = 0;
				while (num < valueList.Count)
				{
					item = (DateTime)valueList[num];
					if (item.Year >= 0x3e8)
					{
						item = item.ToUniversalTime();
						str[num] = item.ToString("yyyyMMddHHmmss.0Z");
						num++;
					}
					else
					{
						int year = item.Year;
						DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: Invalid year ", year.ToString(), " for ", propertyName));
						throw new ArgumentOutOfRangeException(propertyName, (object)item, StringResources.InvalidYear);
					}
				}
			}
			else if (aDAttributeSyntax == ADAttributeSyntax.UtcTime)
			{
				str = new string[valueList.Count];
				for (int j = 0; j < valueList.Count; j++)
				{
					item = (DateTime)valueList[j];
					item = item.ToUniversalTime();
					str[j] = item.ToString("yyMMddHHmmssZ");
				}
			}
			else
			{
				DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: DateTime value for ", propertyName, " of syntax ", (object)propertyType.ToString()));
				objArray = new object[1];
				objArray[0] = typeof(DateTime);
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
			}
			return str;
			DebugLogger.LogWarning("ADTypeConverter", string.Concat("FormatDateTimeValue: DateTime value for ", propertyName, " of syntax ", propertyType.ToString()));
			objArray = new object[1];
			objArray[0] = typeof(DateTime);
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidType, objArray), propertyName);
		}

		private object FormatGuidValue(string propertyName, Guid propertyValue)
		{
			ADAttributeSyntax propertyType = this.GetPropertyType(propertyName, ADAttributeSyntax.OctetString);
			ADAttributeSyntax aDAttributeSyntax = propertyType;
			if (aDAttributeSyntax == ADAttributeSyntax.OctetString || aDAttributeSyntax == ADAttributeSyntax.ReplicaLink)
			{
				return propertyValue.ToByteArray();
			}
			else
			{
				return propertyValue.ToString();
			}
		}

		private object[] FormatGuidValue(string propertyName, IList valueList)
		{
			object[] byteArray;
			ADAttributeSyntax propertyType = this.GetPropertyType(propertyName, ADAttributeSyntax.OctetString);
			ADAttributeSyntax aDAttributeSyntax = propertyType;
			if (aDAttributeSyntax == ADAttributeSyntax.OctetString || aDAttributeSyntax == ADAttributeSyntax.ReplicaLink)
			{
				byteArray = new byte[valueList.Count][];
				for (int i = 0; i < valueList.Count; i++)
				{
					Guid item = (Guid)valueList[i];
					byteArray[i] = item.ToByteArray();
				}
			}
			else
			{
				byteArray = new string[valueList.Count];
				for (int j = 0; j < valueList.Count; j++)
				{
					byteArray[j] = valueList[j].ToString();
				}
			}
			return byteArray;
		}

		public Type GetDotNetPropertyType(string propertyName)
		{
			this.Init();
			ADAttributeSyntax propertyType = this._adSchema.GetPropertyType(propertyName);
			return this.GetDotNetPropertyTypeFromSyntax(propertyType);
		}

		private Type GetDotNetPropertyTypeFromSyntax(ADAttributeSyntax syntax)
		{
			this.Init();
			ADAttributeSyntax aDAttributeSyntax = syntax;
			switch (aDAttributeSyntax)
			{
				case ADAttributeSyntax.DirectoryString:
				case ADAttributeSyntax.DN:
				{
					return typeof(string);
				}
				case ADAttributeSyntax.OctetString:
				{
					return typeof(byte[]);
				}
				case ADAttributeSyntax.SecurityDescriptor:
				{
					return typeof(ActiveDirectorySecurity);
				}
				case ADAttributeSyntax.Int:
				case ADAttributeSyntax.Enumeration:
				{
					return typeof(int);
				}
				case ADAttributeSyntax.Int64:
				{
					return typeof(long);
				}
				case ADAttributeSyntax.Bool:
				{
					return typeof(bool);
				}
				case ADAttributeSyntax.Oid:
				case ADAttributeSyntax.DNWithBinary:
				case ADAttributeSyntax.DNWithString:
				case ADAttributeSyntax.IA5String:
				case ADAttributeSyntax.PrintableString:
				{
					return typeof(string);
				}
				case ADAttributeSyntax.GeneralizedTime:
				case ADAttributeSyntax.UtcTime:
				{
					return typeof(DateTime);
				}
				case ADAttributeSyntax.Sid:
				{
					return typeof(SecurityIdentifier);
				}
				default:
				{
					if (aDAttributeSyntax == ADAttributeSyntax.ReplicaLink)
					{
						return typeof(byte[]);
					}
					return typeof(string);
				}
			}
		}

		public ADAttributeSyntax GetPropertyType(string propertyName)
		{
			this.Init();
			return this._adSchema.GetPropertyType(propertyName);
		}

		public ADAttributeSyntax GetPropertyType(string propertyName, ADAttributeSyntax defaultSyntax)
		{
			this.Init();
			return this._adSchema.GetPropertyType(propertyName, defaultSyntax);
		}

		private void Init()
		{
			if (this._adSchema == null)
			{
				this._adSchema = new ADSchema(this._sessionInfo);
			}
		}

		internal static DateTime ParseDateTimeValue(string value, ADAttributeSyntax syntax)
		{
			if (syntax != ADAttributeSyntax.GeneralizedTime)
			{
				if (syntax != ADAttributeSyntax.UtcTime)
				{
					DebugLogger.LogWarning("ADTypeConverter", string.Concat("ParseDateTimeValue: Not supported syntax ", syntax.ToString()));
					throw new NotSupportedException();
				}
				else
				{
					int num = int.Parse(value.Substring(0, 2), NumberFormatInfo.InvariantInfo);
					int num1 = int.Parse(value.Substring(2, 2), NumberFormatInfo.InvariantInfo);
					int num2 = int.Parse(value.Substring(4, 2), NumberFormatInfo.InvariantInfo);
					int num3 = int.Parse(value.Substring(6, 2), NumberFormatInfo.InvariantInfo);
					int num4 = int.Parse(value.Substring(8, 2), NumberFormatInfo.InvariantInfo);
					int num5 = int.Parse(value.Substring(10, 2), NumberFormatInfo.InvariantInfo);
					DateTime dateTime = new DateTime(num, num1, num2, num3, num4, num5, DateTimeKind.Utc);
					return dateTime.ToLocalTime();
				}
			}
			else
			{
				int num6 = int.Parse(value.Substring(0, 4), NumberFormatInfo.InvariantInfo);
				int num7 = int.Parse(value.Substring(4, 2), NumberFormatInfo.InvariantInfo);
				int num8 = int.Parse(value.Substring(6, 2), NumberFormatInfo.InvariantInfo);
				int num9 = int.Parse(value.Substring(8, 2), NumberFormatInfo.InvariantInfo);
				int num10 = int.Parse(value.Substring(10, 2), NumberFormatInfo.InvariantInfo);
				int num11 = int.Parse(value.Substring(12, 2), NumberFormatInfo.InvariantInfo);
				DateTime dateTime1 = new DateTime(num6, num7, num8, num9, num10, num11, DateTimeKind.Utc);
				return dateTime1.ToLocalTime();
			}
		}
	}
}