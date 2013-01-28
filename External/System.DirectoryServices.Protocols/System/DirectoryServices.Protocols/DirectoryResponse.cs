using System;
using System.Globalization;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public abstract class DirectoryResponse : DirectoryOperation
	{
		internal XmlNode dsmlNode;

		internal XmlNamespaceManager dsmlNS;

		internal bool dsmlRequest;

		internal string dn;

		internal DirectoryControl[] directoryControls;

		internal ResultCode result;

		internal string directoryMessage;

		internal Uri[] directoryReferral;

		private string requestID;

		public virtual DirectoryControl[] Controls
		{
			get
			{
				if (this.dsmlRequest && this.directoryControls == null)
				{
					this.directoryControls = this.ControlsHelper("dsml:control");
				}
				if (this.directoryControls != null)
				{
					DirectoryControl[] directoryControl = new DirectoryControl[(int)this.directoryControls.Length];
					for (int i = 0; i < (int)this.directoryControls.Length; i++)
					{
						directoryControl[i] = new DirectoryControl(this.directoryControls[i].Type, this.directoryControls[i].GetValue(), this.directoryControls[i].IsCritical, this.directoryControls[i].ServerSide);
					}
					DirectoryControl.TransformControls(directoryControl);
					return directoryControl;
				}
				else
				{
					return new DirectoryControl[0];
				}
			}
		}

		public virtual string ErrorMessage
		{
			get
			{
				if (this.dsmlRequest && this.directoryMessage == null)
				{
					this.directoryMessage = this.ErrorMessageHelper("dsml:errorMessage");
				}
				return this.directoryMessage;
			}
		}

		public virtual string MatchedDN
		{
			get
			{
				if (this.dsmlRequest && this.dn == null)
				{
					this.dn = this.MatchedDNHelper("@dsml:matchedDN", "@matchedDN");
				}
				return this.dn;
			}
		}

		public virtual Uri[] Referral
		{
			get
			{
				if (this.dsmlRequest && this.directoryReferral == null)
				{
					this.directoryReferral = this.ReferralHelper("dsml:referral");
				}
				if (this.directoryReferral != null)
				{
					Uri[] uri = new Uri[(int)this.directoryReferral.Length];
					for (int i = 0; i < (int)this.directoryReferral.Length; i++)
					{
						uri[i] = new Uri(this.directoryReferral[i].AbsoluteUri);
					}
					return uri;
				}
				else
				{
					return new Uri[0];
				}
			}
		}

		public string RequestId
		{
			get
			{
				if (this.dsmlRequest && this.requestID == null)
				{
					XmlAttribute xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode("@dsml:requestID", this.dsmlNS);
					if (xmlAttribute == null)
					{
						xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode("@requestID", this.dsmlNS);
					}
					if (xmlAttribute != null)
					{
						this.requestID = xmlAttribute.Value;
					}
				}
				return this.requestID;
			}
		}

		public virtual ResultCode ResultCode
		{
			get
			{
				if (this.dsmlRequest && this.result == (ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect | ResultCode.SortControlMissing | ResultCode.OffsetRangeError | ResultCode.NamingViolation | ResultCode.ObjectClassViolation | ResultCode.NotAllowedOnNonLeaf | ResultCode.NotAllowedOnRdn | ResultCode.EntryAlreadyExists | ResultCode.ObjectClassModificationsProhibited | ResultCode.ResultsTooLarge | ResultCode.AffectsMultipleDsas | ResultCode.VirtualListViewError | ResultCode.Other))
				{
					this.result = this.ResultCodeHelper("dsml:resultCode/@dsml:code", "dsml:resultCode/@code");
				}
				return this.result;
			}
		}

		internal DirectoryResponse(XmlNode node)
		{
			this.result = ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect | ResultCode.SortControlMissing | ResultCode.OffsetRangeError | ResultCode.NamingViolation | ResultCode.ObjectClassViolation | ResultCode.NotAllowedOnNonLeaf | ResultCode.NotAllowedOnRdn | ResultCode.EntryAlreadyExists | ResultCode.ObjectClassModificationsProhibited | ResultCode.ResultsTooLarge | ResultCode.AffectsMultipleDsas | ResultCode.VirtualListViewError | ResultCode.Other;
			this.dsmlNode = node;
			this.dsmlNS = NamespaceUtils.GetDsmlNamespaceManager();
			this.dsmlRequest = true;
		}

		internal DirectoryResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral)
		{
			this.result = ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect | ResultCode.SortControlMissing | ResultCode.OffsetRangeError | ResultCode.NamingViolation | ResultCode.ObjectClassViolation | ResultCode.NotAllowedOnNonLeaf | ResultCode.NotAllowedOnRdn | ResultCode.EntryAlreadyExists | ResultCode.ObjectClassModificationsProhibited | ResultCode.ResultsTooLarge | ResultCode.AffectsMultipleDsas | ResultCode.VirtualListViewError | ResultCode.Other;
			this.dn = dn;
			this.directoryControls = controls;
			this.result = result;
			this.directoryMessage = message;
			this.directoryReferral = referral;
		}

		internal DirectoryControl[] ControlsHelper(string primaryXPath)
		{
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes(primaryXPath, this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				DirectoryControl[] directoryControl = new DirectoryControl[xmlNodeLists.Count];
				int num = 0;
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					directoryControl[num] = new DirectoryControl((XmlElement)xmlNodes);
					num++;
				}
				return directoryControl;
			}
			else
			{
				return new DirectoryControl[0];
			}
		}

		internal string ErrorMessageHelper(string primaryXPath)
		{
			XmlElement xmlElement = (XmlElement)this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
			if (xmlElement == null)
			{
				return null;
			}
			else
			{
				return xmlElement.InnerText;
			}
		}

		internal string MatchedDNHelper(string primaryXPath, string secondaryXPath)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
			if (xmlAttribute != null)
			{
				return xmlAttribute.Value;
			}
			else
			{
				xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode(secondaryXPath, this.dsmlNS);
				if (xmlAttribute != null)
				{
					return xmlAttribute.Value;
				}
				else
				{
					return null;
				}
			}
		}

		internal Uri[] ReferralHelper(string primaryXPath)
		{
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes(primaryXPath, this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				Uri[] uri = new Uri[xmlNodeLists.Count];
				int num = 0;
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					uri[num] = new Uri(xmlNodes.InnerText);
					num++;
				}
				return uri;
			}
			else
			{
				return new Uri[0];
			}
		}

		internal ResultCode ResultCodeHelper(string primaryXPath, string secondaryXPath)
		{
			int num;
			XmlAttribute xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode(primaryXPath, this.dsmlNS);
			if (xmlAttribute == null)
			{
				xmlAttribute = (XmlAttribute)this.dsmlNode.SelectSingleNode(secondaryXPath, this.dsmlNS);
				if (xmlAttribute == null)
				{
					throw new DsmlInvalidDocumentException(Res.GetString("MissingOperationResponseResultCode"));
				}
			}
			string value = xmlAttribute.Value;
			try
			{
				num = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
			}
			catch (FormatException formatException)
			{
				object[] objArray = new object[1];
				objArray[0] = value;
				throw new DsmlInvalidDocumentException(Res.GetString("BadOperationResponseResultCode", objArray));
			}
			catch (OverflowException overflowException)
			{
				object[] objArray1 = new object[1];
				objArray1[0] = value;
				throw new DsmlInvalidDocumentException(Res.GetString("BadOperationResponseResultCode", objArray1));
			}
			if (Utility.IsResultCode((ResultCode)num))
			{
				ResultCode resultCode = (ResultCode)num;
				return resultCode;
			}
			else
			{
				object[] objArray2 = new object[1];
				objArray2[0] = value;
				throw new DsmlInvalidDocumentException(Res.GetString("BadOperationResponseResultCode", objArray2));
			}
		}
	}
}