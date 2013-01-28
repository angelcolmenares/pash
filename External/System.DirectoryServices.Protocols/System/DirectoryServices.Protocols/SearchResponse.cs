using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class SearchResponse : DirectoryResponse
	{
		private SearchResultReferenceCollection referenceCollection;

		private SearchResultEntryCollection entryCollection;

		internal bool searchDone;

		public override DirectoryControl[] Controls
		{
			get
			{
				if (this.dsmlRequest && this.directoryControls == null)
				{
					this.directoryControls = base.ControlsHelper("dsml:searchResultDone/dsml:control");
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

		public SearchResultEntryCollection Entries
		{
			get
			{
				if (this.dsmlRequest && this.entryCollection.Count == 0)
				{
					this.entryCollection = this.EntryHelper();
				}
				return this.entryCollection;
			}
		}

		public override string ErrorMessage
		{
			get
			{
				if (this.dsmlRequest && this.directoryMessage == null)
				{
					this.directoryMessage = base.ErrorMessageHelper("dsml:searchResultDone/dsml:errorMessage");
				}
				return this.directoryMessage;
			}
		}

		public override string MatchedDN
		{
			get
			{
				if (this.dsmlRequest && this.dn == null)
				{
					this.dn = base.MatchedDNHelper("dsml:searchResultDone/@dsml:matchedDN", "dsml:searchResultDone/@matchedDN");
				}
				return this.dn;
			}
		}

		public SearchResultReferenceCollection References
		{
			get
			{
				if (this.dsmlRequest && this.referenceCollection.Count == 0)
				{
					this.referenceCollection = this.ReferenceHelper();
				}
				return this.referenceCollection;
			}
		}

		public override Uri[] Referral
		{
			get
			{
				if (this.dsmlRequest && this.directoryReferral == null)
				{
					this.directoryReferral = base.ReferralHelper("dsml:searchResultDone/dsml:referral");
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

		public override ResultCode ResultCode
		{
			get
			{
				if (this.dsmlRequest && this.result == (ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect | ResultCode.SortControlMissing | ResultCode.OffsetRangeError | ResultCode.NamingViolation | ResultCode.ObjectClassViolation | ResultCode.NotAllowedOnNonLeaf | ResultCode.NotAllowedOnRdn | ResultCode.EntryAlreadyExists | ResultCode.ObjectClassModificationsProhibited | ResultCode.ResultsTooLarge | ResultCode.AffectsMultipleDsas | ResultCode.VirtualListViewError | ResultCode.Other))
				{
					this.result = base.ResultCodeHelper("dsml:searchResultDone/dsml:resultCode/@dsml:code", "dsml:searchResultDone/dsml:resultCode/@code");
				}
				return this.result;
			}
		}

		internal SearchResponse(XmlNode node) : base(node)
		{
			this.referenceCollection = new SearchResultReferenceCollection();
			this.entryCollection = new SearchResultEntryCollection();
		}

		internal SearchResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
			this.referenceCollection = new SearchResultReferenceCollection();
			this.entryCollection = new SearchResultEntryCollection();
		}

		private SearchResultEntryCollection EntryHelper()
		{
			SearchResultEntryCollection searchResultEntryCollection = new SearchResultEntryCollection();
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes("dsml:searchResultEntry", this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					SearchResultEntry searchResultEntry = new SearchResultEntry((XmlElement)xmlNodes);
					searchResultEntryCollection.Add(searchResultEntry);
				}
			}
			return searchResultEntryCollection;
		}

		private SearchResultReferenceCollection ReferenceHelper()
		{
			SearchResultReferenceCollection searchResultReferenceCollection = new SearchResultReferenceCollection();
			XmlNodeList xmlNodeLists = this.dsmlNode.SelectNodes("dsml:searchResultReference", this.dsmlNS);
			if (xmlNodeLists.Count != 0)
			{
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					SearchResultReference searchResultReference = new SearchResultReference((XmlElement)xmlNodes);
					searchResultReferenceCollection.Add(searchResultReference);
				}
			}
			return searchResultReferenceCollection;
		}

		internal void SetEntries(SearchResultEntryCollection col)
		{
			this.entryCollection = col;
		}

		internal void SetReferences(SearchResultReferenceCollection col)
		{
			this.referenceCollection = col;
		}
	}
}