using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Microsoft.Management.PowerShellWebAccess
{
	public sealed class PswaAuthorizationRule
	{
		private string user;

		private string destination;

		public string ConfigurationName
		{
			get;
			internal set;
		}

		public string Destination
		{
			get
			{
				if (this.destination == null)
				{
					this.destination = this.ResolveDestinationDisplayName();
				}
				return this.destination;
			}
			internal set
			{
				this.destination = value;
			}
		}

		internal string DestinationCanonicalForm
		{
			get;
			set;
		}

		public PswaDestinationType DestinationType
		{
			get;
			internal set;
		}

		public int Id
		{
			get;
			internal set;
		}

		internal bool IsCanonicalDestinationSid
		{
			get;
			set;
		}

		internal bool IsComputerGroupLocal
		{
			get;
			set;
		}

		internal bool IsUserGroupLocal
		{
			get;
			set;
		}

		public string RuleName
		{
			get;
			internal set;
		}

		public string User
		{
			get
			{
				if (this.user == null)
				{
					this.user = this.ResolveUserDisplayName();
				}
				return this.user;
			}
			internal set
			{
				this.user = value;
			}
		}

		internal string UserCanonicalForm
		{
			get;
			set;
		}

		public PswaUserType UserType
		{
			get;
			internal set;
		}

		internal PswaAuthorizationRule()
		{
		}

		internal void DeserializeFromXmlElement(XmlElement rule)
		{
			string str;
			string str1;
			this.Id = Convert.ToInt32(rule.GetElementsByTagName("Id")[0].InnerText, CultureInfo.InvariantCulture);
			this.RuleName = rule.GetElementsByTagName("Name")[0].InnerText;
			this.UserType = (PswaUserType)Enum.Parse(typeof(PswaUserType), rule.GetElementsByTagName("UserType")[0].InnerText);
			PswaAuthorizationRule pswaAuthorizationRule = this;
			if (this.UserType == PswaUserType.All)
			{
				str = "*";
			}
			else
			{
				str = null;
			}
			pswaAuthorizationRule.User = str;
			this.UserCanonicalForm = rule.GetElementsByTagName("UserCanonicalForm")[0].InnerText;
			this.DestinationType = (PswaDestinationType)Enum.Parse(typeof(PswaDestinationType), rule.GetElementsByTagName("DestinationType")[0].InnerText);
			PswaAuthorizationRule pswaAuthorizationRule1 = this;
			if (this.DestinationType == PswaDestinationType.All)
			{
				str1 = "*";
			}
			else
			{
				str1 = null;
			}
			pswaAuthorizationRule1.Destination = str1;
			this.DestinationCanonicalForm = rule.GetElementsByTagName("DestinationCanonicalForm")[0].InnerText;
			this.ConfigurationName = rule.GetElementsByTagName("ConfigurationName")[0].InnerText;
			this.IsUserGroupLocal = Convert.ToBoolean(rule.GetElementsByTagName("IsUserGroupLocal")[0].InnerText, CultureInfo.InvariantCulture);
			this.IsComputerGroupLocal = Convert.ToBoolean(rule.GetElementsByTagName("IsComputerGroupLocal")[0].InnerText, CultureInfo.InvariantCulture);
			this.IsCanonicalDestinationSid = Convert.ToBoolean(rule.GetElementsByTagName("IsCanonicalDestinationSid")[0].InnerText, CultureInfo.InvariantCulture);
		}

		private string ResolveDestinationDisplayName()
		{
			string lower;
			string str;
			if (!this.IsCanonicalDestinationSid)
			{
				return this.DestinationCanonicalForm;
			}
			else
			{
				string str1 = null;
				try
				{
					string accountName = PswaAuthorizationRuleManager.Instance.activeDirectoryHelper.ConvertStringSidToAccountName(this.DestinationCanonicalForm, out str1);
					if (this.DestinationType == PswaDestinationType.Computer)
					{
						char[] chrArray = new char[1];
						chrArray[0] = '$';
						accountName = accountName.TrimEnd(chrArray);
					}
					if (string.IsNullOrEmpty(str1))
					{
						str = accountName;
					}
					else
					{
						str = string.Concat(str1, (char)92, accountName);
					}
					lower = str.ToLower(CultureInfo.CurrentCulture);
				}
				catch (Exception exception)
				{
					lower = this.DestinationCanonicalForm;
				}
				return lower;
			}
		}

		private string ResolveUserDisplayName()
		{
			string lower;
			string str;
			string str1 = null;
			try
			{
				string accountName = PswaAuthorizationRuleManager.Instance.activeDirectoryHelper.ConvertStringSidToAccountName(this.UserCanonicalForm, out str1);
				if (string.IsNullOrEmpty(str1))
				{
					str = accountName;
				}
				else
				{
					str = string.Concat(str1, (char)92, accountName);
				}
				lower = str.ToLower(CultureInfo.CurrentCulture);
			}
			catch (Exception exception)
			{
				lower = this.UserCanonicalForm;
			}
			return lower;
		}

		internal XmlElement SerializeToXmlElement()
		{
			XmlDocument xmlDocument = new XmlDocument();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("<Rule>");
			stringBuilder.AppendLine(string.Concat("<Id>", this.Id, "</Id>"));
			stringBuilder.AppendLine(string.Concat("<Name>", this.RuleName, "</Name>"));
			stringBuilder.AppendLine(string.Concat("<UserCanonicalForm>", this.UserCanonicalForm, "</UserCanonicalForm>"));
			stringBuilder.AppendLine(string.Concat("<UserType>", this.UserType, "</UserType>"));
			stringBuilder.AppendLine(string.Concat("<DestinationCanonicalForm>", this.DestinationCanonicalForm, "</DestinationCanonicalForm>"));
			stringBuilder.AppendLine(string.Concat("<DestinationType>", this.DestinationType, "</DestinationType>"));
			stringBuilder.AppendLine(string.Concat("<ConfigurationName>", this.ConfigurationName, "</ConfigurationName>"));
			bool isUserGroupLocal = this.IsUserGroupLocal;
			stringBuilder.AppendLine(string.Concat("<IsUserGroupLocal>", isUserGroupLocal.ToString().ToLower(CultureInfo.InvariantCulture), "</IsUserGroupLocal>"));
			bool isComputerGroupLocal = this.IsComputerGroupLocal;
			stringBuilder.AppendLine(string.Concat("<IsComputerGroupLocal>", isComputerGroupLocal.ToString().ToLower(CultureInfo.InvariantCulture), "</IsComputerGroupLocal>"));
			bool isCanonicalDestinationSid = this.IsCanonicalDestinationSid;
			stringBuilder.AppendLine(string.Concat("<IsCanonicalDestinationSid>", isCanonicalDestinationSid.ToString().ToLower(CultureInfo.InvariantCulture), "</IsCanonicalDestinationSid>"));
			stringBuilder.AppendLine("</Rule>");
			xmlDocument.LoadXml(stringBuilder.ToString());
			return xmlDocument.DocumentElement;
		}
	}
}