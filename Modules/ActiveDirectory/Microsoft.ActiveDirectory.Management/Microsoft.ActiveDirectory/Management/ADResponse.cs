using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADResponse
	{
		private string _dn;

		private DirectoryControl[] _controls;

		private ResultCode _result;

		private string _message;

		private Uri[] _referral;

		public virtual DirectoryControl[] Controls
		{
			get
			{
				if (this._controls != null)
				{
					return this._controls;
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
				return this._message;
			}
		}

		public virtual string MatchedDN
		{
			get
			{
				return this._dn;
			}
		}

		public virtual Uri[] Referral
		{
			get
			{
				if (this._referral != null)
				{
					return this._referral;
				}
				else
				{
					return new Uri[0];
				}
			}
		}

		public virtual ResultCode ResultCode
		{
			get
			{
				return this._result;
			}
		}

		public ADResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral)
		{
			this._result = ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect | ResultCode.SortControlMissing | ResultCode.OffsetRangeError | ResultCode.NamingViolation | ResultCode.ObjectClassViolation | ResultCode.NotAllowedOnNonLeaf | ResultCode.NotAllowedOnRdn | ResultCode.EntryAlreadyExists | ResultCode.ObjectClassModificationsProhibited | ResultCode.ResultsTooLarge | ResultCode.AffectsMultipleDsas | ResultCode.VirtualListViewError | ResultCode.Other;
			this._dn = dn;
			this._controls = controls;
			ADResponse.TransformControls(this._controls);
			this._result = result;
			this._message = message;
			this._referral = referral;
		}

		internal static void TransformControls(DirectoryControl[] controls)
		{
			if (controls != null)
			{
				for (int i = 0; i < (int)controls.Length; i++)
				{
					if (!(controls[i].GetType() != typeof(DirectoryControl)) && controls[i].Type == "1.2.840.113556.1.4.1504")
					{
						byte[] value = controls[i].GetValue();
						object[] objArray = BerConverter.Decode("{e}", value);
						int num = (int)objArray[0];
						ADAsqResponseControl aDAsqResponseControl = new ADAsqResponseControl(num, controls[i].IsCritical, value);
						controls[i] = aDAsqResponseControl;
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}