using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace Microsoft.Management.Odata.Core
{
	internal class UserContext : IEquatable<UserContext>
	{
		public string AuthenticationType
		{
			get;
			private set;
		}

		public X509Certificate2 ClientCertificate
		{
			get;
			private set;
		}

		public bool IsAuthenticated
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public UserContext(IIdentity identity, X509Certificate2 clientCertificate = null)
		{
			this.Name = identity.Name;
			this.AuthenticationType = identity.AuthenticationType;
			this.IsAuthenticated = identity.IsAuthenticated;
			this.ClientCertificate = clientCertificate;
		}

		private static bool CompareCertificates(X509Certificate2 firstCert, X509Certificate2 secondCert)
		{
			if (firstCert != secondCert)
			{
				if (firstCert == null || secondCert == null)
				{
					return false;
				}
				else
				{
					return firstCert.Equals(secondCert);
				}
			}
			else
			{
				return true;
			}
		}

		public bool Equals(UserContext other)
		{
			bool flag;
			if (other != null)
			{
				if (!string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase))
				{
					flag = false;
				}
				else
				{
					flag = string.Equals(this.AuthenticationType, other.AuthenticationType, StringComparison.OrdinalIgnoreCase);
				}
				bool flag1 = flag;
				if (flag1)
				{
					return UserContext.CompareCertificates(this.ClientCertificate, other.ClientCertificate);
				}
				else
				{
					return flag1;
				}
			}
			else
			{
				return false;
			}
		}

		public override bool Equals(object other)
		{
			UserContext userContext = other as UserContext;
			if (userContext != null)
			{
				return this.Equals(userContext);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			int hashCode = 13;
			hashCode = hashCode * 31 + this.Name.ToLower().GetHashCode();
			hashCode = hashCode * 31 + this.AuthenticationType.ToLower().GetHashCode();
			bool isAuthenticated = this.IsAuthenticated;
			hashCode = hashCode * 31 + isAuthenticated.GetHashCode();
			if (this.ClientCertificate != null)
			{
				hashCode = hashCode * 31 + this.ClientCertificate.Thumbprint.ToLower().GetHashCode();
			}
			return hashCode;
		}

		public IIdentity GetIdentity()
		{
			IIdentity identity = CurrentRequestHelper.Identity;
			if (!string.Equals(this.Name, identity.Name, StringComparison.OrdinalIgnoreCase) || !string.Equals(this.AuthenticationType, identity.AuthenticationType, StringComparison.OrdinalIgnoreCase))
			{
				object[] traceMessage = new object[2];
				traceMessage[0] = this.ToTraceMessage("User Context");
				traceMessage[1] = identity.ToTraceMessage();
				throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.UserNameContextIdentityMismatch, traceMessage));
			}
			else
			{
				return CurrentRequestHelper.Identity;
			}
		}

		public override string ToString()
		{
			object[] name = new object[6];
			name[0] = "Name = ";
			name[1] = this.Name;
			name[2] = " AuthenticationType = ";
			name[3] = this.AuthenticationType;
			name[4] = " IsAuthenticated = ";
			name[5] = this.IsAuthenticated;
			return string.Concat(name);
		}

		public string ToTraceMessage(string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(message);
			stringBuilder.AppendLine(string.Concat("Name = ", this.Name));
			stringBuilder.AppendLine(string.Concat("Authentication Type = ", this.AuthenticationType));
			stringBuilder.AppendLine(string.Concat("IsAuthenticated = ", this.IsAuthenticated));
			if (this.ClientCertificate != null)
			{
				stringBuilder.AppendLine(string.Concat("Client Certificate = ", this.ClientCertificate.ToString()));
			}
			return stringBuilder.ToString();
		}

		public void Trace()
		{
			this.Trace("User Context");
		}

		public void Trace(string message)
		{
			if (TraceHelper.IsEnabled(5))
			{
				TraceHelper.Current.DebugMessage(this.ToTraceMessage(message));
			}
		}
	}
}