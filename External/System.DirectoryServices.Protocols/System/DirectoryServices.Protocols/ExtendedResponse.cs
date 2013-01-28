using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class ExtendedResponse : DirectoryResponse
	{
		internal string name;

		internal byte[] @value;

		public string ResponseName
		{
			get
			{
				if (this.dsmlRequest && this.name == null)
				{
					XmlElement xmlElement = (XmlElement)this.dsmlNode.SelectSingleNode("dsml:responseName", this.dsmlNS);
					if (xmlElement != null)
					{
						this.name = xmlElement.InnerText;
					}
				}
				return this.name;
			}
		}

		public byte[] ResponseValue
		{
			get
			{
				if (this.dsmlRequest && this.@value == null)
				{
					XmlElement xmlElement = (XmlElement)this.dsmlNode.SelectSingleNode("dsml:response", this.dsmlNS);
					if (xmlElement != null)
					{
						string innerText = xmlElement.InnerText;
						try
						{
							this.@value = Convert.FromBase64String(innerText);
						}
						catch (FormatException formatException)
						{
							throw new DsmlInvalidDocumentException(Res.GetString("BadBase64Value"));
						}
					}
				}
				if (this.@value != null)
				{
					byte[] numArray = new byte[(int)this.@value.Length];
					for (int i = 0; i < (int)this.@value.Length; i++)
					{
						numArray[i] = this.@value[i];
					}
					return numArray;
				}
				else
				{
					return new byte[0];
				}
			}
		}

		internal ExtendedResponse(XmlNode node) : base(node)
		{
		}

		internal ExtendedResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
		}
	}
}