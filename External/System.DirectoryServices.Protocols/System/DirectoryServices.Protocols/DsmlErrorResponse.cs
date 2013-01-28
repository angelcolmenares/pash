using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlErrorResponse : DirectoryResponse
	{
		private string message;

		private string detail;

		private ErrorResponseCategory category;

		public override DirectoryControl[] Controls
		{
			get
			{
				throw new NotSupportedException(Res.GetString("NotSupportOnDsmlErrRes"));
			}
		}

		public string Detail
		{
			get
			{
				if (this.detail == null)
				{
					XmlElement xmlElement = (XmlElement)this.dsmlNode.SelectSingleNode("dsml:detail", this.dsmlNS);
					if (xmlElement != null)
					{
						this.detail = xmlElement.InnerXml;
					}
				}
				return this.detail;
			}
		}

		public override string ErrorMessage
		{
			get
			{
				throw new NotSupportedException(Res.GetString("NotSupportOnDsmlErrRes"));
			}
		}

		public override string MatchedDN
		{
			get
			{
				throw new NotSupportedException(Res.GetString("NotSupportOnDsmlErrRes"));
			}
		}

		public string Message
		{
			get
			{
				if (this.message == null)
				{
					XmlElement xmlElement = (XmlElement)this.dsmlNode.SelectSingleNode("dsml:message", this.dsmlNS);
					if (xmlElement != null)
					{
						this.message = xmlElement.InnerText;
					}
				}
				return this.message;
			}
		}

		public override Uri[] Referral
		{
			get
			{
				throw new NotSupportedException(Res.GetString("NotSupportOnDsmlErrRes"));
			}
		}

		public override ResultCode ResultCode
		{
			get
			{
				throw new NotSupportedException(Res.GetString("NotSupportOnDsmlErrRes"));
			}
		}

		public ErrorResponseCategory Type
		{
			get
			{
				if (this.category == (ErrorResponseCategory.CouldNotConnect | ErrorResponseCategory.ConnectionClosed | ErrorResponseCategory.MalformedRequest | ErrorResponseCategory.GatewayInternalError | ErrorResponseCategory.AuthenticationFailed | ErrorResponseCategory.UnresolvableUri | ErrorResponseCategory.Other))
				{
					XmlAttribute xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode("@dsml:type", this.dsmlNS);
					if (xmlAttribute == null)
					{
						xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode("@type", this.dsmlNS);
					}
					if (xmlAttribute != null)
					{
						string value = xmlAttribute.Value;
						string str = value;
						if (value != null)
						{
							if (str == "notAttempted")
							{
								this.category = ErrorResponseCategory.NotAttempted;
								return this.category;
							}
							else if (str == "couldNotConnect")
							{
								this.category = ErrorResponseCategory.CouldNotConnect;
								return this.category;
							}
							else if (str == "connectionClosed")
							{
								this.category = ErrorResponseCategory.ConnectionClosed;
								return this.category;
							}
							else if (str == "malformedRequest")
							{
								this.category = ErrorResponseCategory.MalformedRequest;
								return this.category;
							}
							else if (str == "gatewayInternalError")
							{
								this.category = ErrorResponseCategory.GatewayInternalError;
								return this.category;
							}
							else if (str == "authenticationFailed")
							{
								this.category = ErrorResponseCategory.AuthenticationFailed;
								return this.category;
							}
							else if (str == "unresolvableURI")
							{
								this.category = ErrorResponseCategory.UnresolvableUri;
								return this.category;
							}
							else if (str == "other")
							{
								this.category = ErrorResponseCategory.Other;
								return this.category;
							}
						}
						object[] objArray = new object[1];
						objArray[0] = xmlAttribute.Value;
						throw new DsmlInvalidDocumentException(Res.GetString("ErrorResponseInvalidValue", objArray));
					}
					else
					{
						throw new DsmlInvalidDocumentException(Res.GetString("MissingErrorResponseType"));
					}
				}
				return this.category;
			}
		}

		internal DsmlErrorResponse(XmlNode node) : base(node)
		{
			this.category = ErrorResponseCategory.CouldNotConnect | ErrorResponseCategory.ConnectionClosed | ErrorResponseCategory.MalformedRequest | ErrorResponseCategory.GatewayInternalError | ErrorResponseCategory.AuthenticationFailed | ErrorResponseCategory.UnresolvableUri | ErrorResponseCategory.Other;
		}
	}
}