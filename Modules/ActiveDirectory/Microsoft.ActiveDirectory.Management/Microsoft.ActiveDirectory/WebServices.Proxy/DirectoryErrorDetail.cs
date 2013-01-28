using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[Serializable]
	[XmlType(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	public class DirectoryErrorDetail
	{
		private string messageField;

		private string errorCodeField;

		private string extendedErrorMessageField;

		private string matchedDNField;

		private string[] referralField;

		private string win32ErrorCodeField;

		private string shortMessageField;

		[XmlElement(Order=1)]
		public string ErrorCode
		{
			get
			{
				return this.errorCodeField;
			}
			set
			{
				this.errorCodeField = value;
			}
		}

		[XmlElement(Order=2)]
		public string ExtendedErrorMessage
		{
			get
			{
				return this.extendedErrorMessageField;
			}
			set
			{
				this.extendedErrorMessageField = value;
			}
		}

		[XmlElement(Order=3)]
		public string MatchedDN
		{
			get
			{
				return this.matchedDNField;
			}
			set
			{
				this.matchedDNField = value;
			}
		}

		[XmlElement(Order=0)]
		public string Message
		{
			get
			{
				return this.messageField;
			}
			set
			{
				this.messageField = value;
			}
		}

		[XmlElement("Referral", Order=4)]
		public string[] Referral
		{
			get
			{
				return this.referralField;
			}
			set
			{
				this.referralField = value;
			}
		}

		[XmlElement(Order=6)]
		public string ShortMessage
		{
			get
			{
				return this.shortMessageField;
			}
			set
			{
				this.shortMessageField = value;
			}
		}

		[XmlElement(Order=5)]
		public string Win32ErrorCode
		{
			get
			{
				return this.win32ErrorCodeField;
			}
			set
			{
				this.win32ErrorCodeField = value;
			}
		}

		public DirectoryErrorDetail()
		{
		}
	}
}