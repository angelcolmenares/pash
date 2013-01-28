using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	[Serializable]
	public class ADReferralException : ADException
	{
		private Uri[] _referral;

		public Uri[] Referral
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

		public ADReferralException()
		{
		}

		public ADReferralException(string message) : base(message)
		{
		}

		public ADReferralException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ADReferralException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			int num = info.GetInt32("referralCount");
			if (num > 0)
			{
				this._referral = new Uri[num];
				for (int i = 0; i < num; i++)
				{
					this._referral[i] = (Uri)info.GetValue(string.Concat("referral", i), typeof(Uri));
				}
			}
		}

		public ADReferralException(string message, int errorCode) : base(message, errorCode)
		{
		}

		public ADReferralException(string message, Exception inner, int errorCode) : base(message, inner, errorCode)
		{
		}

		public ADReferralException(string message, Uri[] referral) : base(message)
		{
			this._referral = referral;
		}

		public ADReferralException(string message, int errorCode, Uri[] referral) : base(message, errorCode)
		{
			this._referral = referral;
		}

		public ADReferralException(string message, Exception inner, int errorCode, Uri[] referral) : base(message, inner, errorCode)
		{
			this._referral = referral;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext streamingContext)
		{
			if (info != null)
			{
				int length = 0;
				if (this._referral != null)
				{
					length = (int)this._referral.Length;
				}
				info.AddValue("referralCount", length);
				for (int i = 0; i < length; i++)
				{
					info.AddValue(string.Concat("referral", i), this._referral[i], typeof(Uri));
				}
				base.GetObjectData(info, streamingContext);
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}
	}
}