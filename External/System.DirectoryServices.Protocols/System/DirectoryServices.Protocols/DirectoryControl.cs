using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DirectoryControl
	{
		internal byte[] directoryControlValue;

		private string directoryControlType;

		private bool directoryControlCriticality;

		private bool directoryControlServerSide;

		public bool IsCritical
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryControlCriticality;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.directoryControlCriticality = value;
			}
		}

		public bool ServerSide
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryControlServerSide;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.directoryControlServerSide = value;
			}
		}

		public string Type
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryControlType;
			}
		}

		public DirectoryControl(string type, byte[] value, bool isCritical, bool serverSide)
		{
			this.directoryControlType = "";
			this.directoryControlCriticality = true;
			this.directoryControlServerSide = true;
			Utility.CheckOSVersion();
			this.directoryControlType = type;
			if (type != null)
			{
				if (value != null)
				{
					this.directoryControlValue = new byte[(int)value.Length];
					for (int i = 0; i < (int)value.Length; i++)
					{
						this.directoryControlValue[i] = value[i];
					}
				}
				this.directoryControlCriticality = isCritical;
				this.directoryControlServerSide = serverSide;
				return;
			}
			else
			{
				throw new ArgumentNullException("type");
			}
		}

		internal DirectoryControl(XmlElement el)
		{
			this.directoryControlType = "";
			this.directoryControlCriticality = true;
			this.directoryControlServerSide = true;
			XmlNamespaceManager dsmlNamespaceManager = NamespaceUtils.GetDsmlNamespaceManager();
			XmlAttribute xmlAttribute = (XmlAttribute)el.SelectSingleNode("@dsml:criticality", dsmlNamespaceManager);
			if (xmlAttribute == null)
			{
				xmlAttribute = (XmlAttribute)el.SelectSingleNode("@criticality", dsmlNamespaceManager);
			}
			if (xmlAttribute != null)
			{
				string value = xmlAttribute.Value;
				if (value == "true" || value == "1")
				{
					this.directoryControlCriticality = true;
				}
				else
				{
					if (value == "false" || value == "0")
					{
						this.directoryControlCriticality = false;
					}
					else
					{
						throw new DsmlInvalidDocumentException(Res.GetString("BadControl"));
					}
				}
			}
			else
			{
				this.directoryControlCriticality = false;
			}
			XmlAttribute xmlAttribute1 = (XmlAttribute)el.SelectSingleNode("@dsml:type", dsmlNamespaceManager);
			if (xmlAttribute1 == null)
			{
				xmlAttribute1 = (XmlAttribute)el.SelectSingleNode("@type", dsmlNamespaceManager);
			}
			if (xmlAttribute1 != null)
			{
				this.directoryControlType = xmlAttribute1.Value;
				XmlElement xmlElement = (XmlElement)el.SelectSingleNode("dsml:controlValue", dsmlNamespaceManager);
				if (xmlElement != null)
				{
					try
					{
						this.directoryControlValue = Convert.FromBase64String(xmlElement.InnerText);
					}
					catch (FormatException formatException)
					{
						throw new DsmlInvalidDocumentException(Res.GetString("BadControl"));
					}
				}
				return;
			}
			else
			{
				throw new DsmlInvalidDocumentException(Res.GetString("BadControl"));
			}
		}

		public virtual byte[] GetValue()
		{
			if (this.directoryControlValue != null)
			{
				byte[] numArray = new byte[(int)this.directoryControlValue.Length];
				for (int i = 0; i < (int)this.directoryControlValue.Length; i++)
				{
					numArray[i] = this.directoryControlValue[i];
				}
				return numArray;
			}
			else
			{
				return new byte[0];
			}
		}

		internal XmlElement ToXmlNode(XmlDocument doc)
		{
			string str;
			XmlElement xmlElement = doc.CreateElement("control", "urn:oasis:names:tc:DSML:2:0:core");
			XmlAttribute type = doc.CreateAttribute("type", null);
			type.InnerText = this.Type;
			xmlElement.Attributes.Append(type);
			XmlAttribute xmlAttribute = doc.CreateAttribute("criticality", null);
			XmlAttribute xmlAttribute1 = xmlAttribute;
			if (this.IsCritical)
			{
				str = "true";
			}
			else
			{
				str = "false";
			}
			xmlAttribute1.InnerText = str;
			xmlElement.Attributes.Append(xmlAttribute);
			byte[] value = this.GetValue();
			if ((int)value.Length != 0)
			{
				XmlElement xmlElement1 = doc.CreateElement("controlValue", "urn:oasis:names:tc:DSML:2:0:core");
				XmlAttribute xmlAttribute2 = doc.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
				xmlAttribute2.InnerText = "xsd:base64Binary";
				xmlElement1.Attributes.Append(xmlAttribute2);
				string base64String = Convert.ToBase64String(value);
				xmlElement1.InnerText = base64String;
				xmlElement.AppendChild(xmlElement1);
			}
			return xmlElement;
		}

		internal static void TransformControls(DirectoryControl[] controls)
		{
			object[] objArray;
			object[] objArray1;
			int num;
			bool flag = false;
			int num1;
			int num2;
			int num3;
			object[] objArray2;
			bool flag1;
			for (int i = 0; i < (int)controls.Length; i++)
			{
				byte[] value = controls[i].GetValue();
				if (controls[i].Type != "1.2.840.113556.1.4.319")
				{
					if (controls[i].Type != "1.2.840.113556.1.4.1504")
					{
						if (controls[i].Type != "1.2.840.113556.1.4.841")
						{
							if (controls[i].Type != "1.2.840.113556.1.4.474")
							{
								if (controls[i].Type == "2.16.840.1.113730.3.4.10")
								{
									byte[] numArray = null;
									bool flag2 = false;
									if (!Utility.IsWin2kOS)
									{
										objArray2 = BerConverter.TryDecode("{iieO}", value, out flag2);
									}
									else
									{
										objArray2 = BerConverter.TryDecode("{iiiO}", value, out flag2);
									}
									if (!flag2)
									{
										if (!Utility.IsWin2kOS)
										{
											objArray2 = BerConverter.Decode("{iie}", value);
										}
										else
										{
											objArray2 = BerConverter.Decode("{iii}", value);
										}
										num1 = (int)objArray2[0];
										num2 = (int)objArray2[1];
										num3 = (int)objArray2[2];
									}
									else
									{
										num1 = (int)objArray2[0];
										num2 = (int)objArray2[1];
										num3 = (int)objArray2[2];
										numArray = (byte[])objArray2[3];
									}
									VlvResponseControl vlvResponseControl = new VlvResponseControl(num1, num2, numArray, (ResultCode)num3, controls[i].IsCritical, controls[i].GetValue());
									controls[i] = vlvResponseControl;
								}
							}
							else
							{
								string str = null;
								if (!Utility.IsWin2kOS)
								{
									objArray1 = BerConverter.TryDecode("{ea}", value, out flag);
								}
								else
								{
									objArray1 = BerConverter.TryDecode("{ia}", value, out flag);
								}
								if (!flag)
								{
									if (!Utility.IsWin2kOS)
									{
										objArray1 = BerConverter.Decode("{e}", value);
									}
									else
									{
										objArray1 = BerConverter.Decode("{i}", value);
									}
									num = (int)objArray1[0];
								}
								else
								{
									num = (int)objArray1[0];
									str = (string)objArray1[1];
								}
								SortResponseControl sortResponseControl = new SortResponseControl((ResultCode)num, str, controls[i].IsCritical, controls[i].GetValue());
								controls[i] = sortResponseControl;
							}
						}
						else
						{
							object[] objArray3 = BerConverter.Decode("{iiO}", value);
							int num4 = (int)objArray3[0];
							int num5 = (int)objArray3[1];
							byte[] numArray1 = (byte[])objArray3[2];
							byte[] numArray2 = numArray1;
							if (num4 == 0)
							{
								flag1 = false;
							}
							else
							{
								flag1 = true;
							}
							DirSyncResponseControl dirSyncResponseControl = new DirSyncResponseControl(numArray2, flag1, num5, controls[i].IsCritical, controls[i].GetValue());
							controls[i] = dirSyncResponseControl;
						}
					}
					else
					{
						if (!Utility.IsWin2kOS)
						{
							objArray = BerConverter.Decode("{e}", value);
						}
						else
						{
							objArray = BerConverter.Decode("{i}", value);
						}
						int num6 = (int)objArray[0];
						AsqResponseControl asqResponseControl = new AsqResponseControl(num6, controls[i].IsCritical, controls[i].GetValue());
						controls[i] = asqResponseControl;
					}
				}
				else
				{
					object[] objArray4 = BerConverter.Decode("{iO}", value);
					int num7 = (int)objArray4[0];
					byte[] numArray3 = (byte[])objArray4[1];
					if (numArray3 == null)
					{
						numArray3 = new byte[0];
					}
					PageResultResponseControl pageResultResponseControl = new PageResultResponseControl(num7, numArray3, controls[i].IsCritical, controls[i].GetValue());
					controls[i] = pageResultResponseControl;
				}
			}
		}
	}
}