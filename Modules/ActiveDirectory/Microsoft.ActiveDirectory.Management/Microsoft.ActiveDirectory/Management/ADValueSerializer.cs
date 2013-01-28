using Microsoft.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal sealed class ADValueSerializer
	{
		private static Dictionary<string, Type> _typeMappingToClr;

		static ADValueSerializer()
		{
			ADValueSerializer._typeMappingToClr = new Dictionary<string, Type>(6);
			ADValueSerializer._typeMappingToClr.Add("string", typeof(string));
			ADValueSerializer._typeMappingToClr.Add("base64Binary", typeof(byte[]));
			ADValueSerializer._typeMappingToClr.Add("boolean", typeof(bool));
			ADValueSerializer._typeMappingToClr.Add("int", typeof(int));
			ADValueSerializer._typeMappingToClr.Add("long", typeof(long));
			ADValueSerializer._typeMappingToClr.Add("dateTime", typeof(DateTime));
		}

		private ADValueSerializer()
		{
		}

		private static void Deserialize(XmlReader reader, bool useInternStrings, out object value, out Type type)
		{
			string str = null;
			string str1 = null;
			object array;
			object obj = null;
			List<object> objs = null;
			int num = 0;
			type = null;
			value = null;
			while (reader.IsStartElement("value", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				num++;
				if (num > 1 && objs == null)
				{
					objs = new List<object>();
					objs.Add(obj);
				}
				string attribute = reader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
				XmlUtility.SplitPrefix(attribute, out str, out str1);
				type = ADValueSerializer._typeMappingToClr[str1];
				obj = reader.ReadElementContentAs(type, null, "value", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				if (objs == null)
				{
					continue;
				}
				if (!useInternStrings || !(type == typeof(string)))
				{
					objs.Add(obj);
				}
				else
				{
					objs.Add(string.Intern((string)obj));
				}
			}
			object objPointer = value;
			if (objs == null)
			{
				array = obj;
			}
			else
			{
				array = objs.ToArray();
			}
			objPointer = array;
		}

		public static void Deserialize(XmlReader reader, bool useInternStrings, out object value)
		{
			Type type = null;
			ADValueSerializer.Deserialize(reader, useInternStrings, out value, out type);
		}

		public static void DeserializeSingleValue<T>(XmlReader reader, out T value)
		{
			object obj = null;
			Type type = null;
			ADValueSerializer.Deserialize(reader, false, out obj, out type);
			value = (T)obj;
		}

		public static void Serialize(XmlDictionaryWriter writer, object value)
		{
			string str;
			byte[] numArray;
			object obj;
			bool flag = false;
			bool flag1 = false;
			writer.LookupPrefix("http://www.w3.org/2001/XMLSchema");
			writer.WriteStartElement("value", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			if (value.GetType() != typeof(byte[]))
			{
				TypeCode typeCode = Type.GetTypeCode(value.GetType());
				if (typeCode == TypeCode.Boolean)
				{
					str = "boolean";
					flag = false;
					if ((bool)value)
					{
						obj = "true";
					}
					else
					{
						obj = "false";
					}
					value = obj;
					XmlUtility.WriteXsiTypeAttribute(writer, str);
					if (!flag)
					{
						writer.WriteString(value.ToString());
					}
					else
					{
						if (!flag1)
						{
							numArray = new byte[1];
							numArray[0] = (byte)value;
						}
						else
						{
							numArray = (byte[])value;
						}
						writer.WriteBase64(numArray, 0, (int)numArray.Length);
					}
					writer.WriteEndElement();
					return;
				}
				else if (typeCode == TypeCode.Char || typeCode == TypeCode.SByte || typeCode == TypeCode.Int16 || typeCode == TypeCode.UInt16 || typeCode == TypeCode.Int32 || typeCode == TypeCode.UInt32)
				{
					str = "int";
					flag = false;
					XmlUtility.WriteXsiTypeAttribute(writer, str);
					if (!flag)
					{
						writer.WriteString(value.ToString());
					}
					else
					{
						if (!flag1)
						{
							numArray = new byte[1];
							numArray[0] = (byte)value;
						}
						else
						{
							numArray = (byte[])value;
						}
						writer.WriteBase64(numArray, 0, (int)numArray.Length);
					}
					writer.WriteEndElement();
					return;
				}
				else if (typeCode == TypeCode.Byte)
				{
					str = "base64Binary";
					flag = true;
					flag1 = false;
					XmlUtility.WriteXsiTypeAttribute(writer, str);
					if (!flag)
					{
						writer.WriteString(value.ToString());
					}
					else
					{
						if (!flag1)
						{
							numArray = new byte[1];
							numArray[0] = (byte)value;
						}
						else
						{
							numArray = (byte[])value;
						}
						writer.WriteBase64(numArray, 0, (int)numArray.Length);
					}
					writer.WriteEndElement();
					return;
				}
				else if (typeCode == TypeCode.Int64 || typeCode == TypeCode.UInt64)
				{
					str = "long";
					flag = false;
					XmlUtility.WriteXsiTypeAttribute(writer, str);
					if (!flag)
					{
						writer.WriteString(value.ToString());
					}
					else
					{
						if (!flag1)
						{
							numArray = new byte[1];
							numArray[0] = (byte)value;
						}
						else
						{
							numArray = (byte[])value;
						}
						writer.WriteBase64(numArray, 0, (int)numArray.Length);
					}
					writer.WriteEndElement();
					return;
				}
				else if (typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Decimal || typeCode == (TypeCode.Object | TypeCode.DateTime))
				{
				}
				else if (typeCode == TypeCode.DateTime)
				{
					str = "dateTime";
					flag = false;
					value = XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc);
					XmlUtility.WriteXsiTypeAttribute(writer, str);
					if (!flag)
					{
						writer.WriteString(value.ToString());
					}
					else
					{
						if (!flag1)
						{
							numArray = new byte[1];
							numArray[0] = (byte)value;
						}
						else
						{
							numArray = (byte[])value;
						}
						writer.WriteBase64(numArray, 0, (int)numArray.Length);
					}
					writer.WriteEndElement();
					return;
				}
				else if (typeCode == TypeCode.String)
				{
					str = "string";
					flag = false;
					XmlUtility.WriteXsiTypeAttribute(writer, str);
					if (!flag)
					{
						writer.WriteString(value.ToString());
					}
					else
					{
						if (!flag1)
						{
							numArray = new byte[1];
							numArray[0] = (byte)value;
						}
						else
						{
							numArray = (byte[])value;
						}
						writer.WriteBase64(numArray, 0, (int)numArray.Length);
					}
					writer.WriteEndElement();
					return;
				}
				object[] objArray = new object[1];
				objArray[0] = value.GetType().ToString();
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADWSXmlParserUnexpectedElement, objArray));
			}
			else
			{
				flag = true;
				flag1 = true;
				str = "base64Binary";
			}
			XmlUtility.WriteXsiTypeAttribute(writer, str);
			if (!flag)
			{
				writer.WriteString(value.ToString());
			}
			else
			{
				if (!flag1)
				{
					numArray = new byte[1];
					numArray[0] = (byte)value;
				}
				else
				{
					numArray = (byte[])value;
				}
				writer.WriteBase64(numArray, 0, (int)numArray.Length);
			}
			writer.WriteEndElement();
		}
	}
}