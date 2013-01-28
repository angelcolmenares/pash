using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal sealed class DirectoryControlSerializer
	{
		private DirectoryControlSerializer()
		{
		}

		public static void Deserialize(XmlDictionaryReader reader, out IList<DirectoryControl> controls, bool mustBePresent, bool fullChecks)
		{
			string str = null;
			string str1 = null;
			bool flag;
			byte[] numArray = null;
			controls = new List<DirectoryControl>();
			if (mustBePresent || reader.IsStartElement("controls", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				reader.ReadFullStartElement("controls", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				while (reader.IsStartElement("control", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
				{
					string attribute = reader.GetAttribute("type");
					string attribute1 = reader.GetAttribute("criticality");
					reader.Read();
					if (!reader.IsStartElement("controlValue", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
					{
						numArray = null;
					}
					else
					{
						string attribute2 = reader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
						if (attribute2 != null)
						{
							XmlUtility.SplitPrefix(attribute2, out str, out str1);
							numArray = reader.ReadElementContentAsBase64();
						}
						else
						{
							throw new ArgumentException();
						}
					}
					if (!string.Equals("true", attribute1))
					{
						flag = false;
					}
					else
					{
						flag = true;
					}
					DirectoryControl directoryControl = new DirectoryControl(attribute, numArray, flag, true);
					controls.Add(directoryControl);
					reader.Read();
				}
				return;
			}
			else
			{
				return;
			}
		}

		public static void Serialize(XmlDictionaryWriter writer, IList<DirectoryControl> controls)
		{
			string str;
			if (controls == null || controls.Count == 0)
			{
				return;
			}
			else
			{
				writer.WriteStartElement("ad", "controls", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				foreach (DirectoryControl control in controls)
				{
					byte[] value = control.GetValue();
					writer.WriteStartElement("ad", "control", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
					writer.WriteAttributeString("type", control.Type.ToLower());
					XmlDictionaryWriter xmlDictionaryWriter = writer;
					string str1 = "criticality";
					if (control.IsCritical)
					{
						str = "true";
					}
					else
					{
						str = "false";
					}
					xmlDictionaryWriter.WriteAttributeString(str1, str);
					if (value != null && (int)value.Length > 0)
					{
						writer.WriteStartElement("ad", "controlValue", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
						XmlUtility.WriteXsiTypeAttribute(writer, "base64Binary");
						writer.WriteBase64(value, 0, (int)value.Length);
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				return;
			}
		}
	}
}