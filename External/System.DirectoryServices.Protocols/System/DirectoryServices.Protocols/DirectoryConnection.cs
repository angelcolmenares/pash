using System;
using System.DirectoryServices;
using System.Net;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.DirectoryServices.Protocols
{
	public abstract class DirectoryConnection
	{
		internal NetworkCredential directoryCredential;

		internal X509CertificateCollection certificatesCollection;

		internal TimeSpan connectionTimeOut;

		internal DirectoryIdentifier directoryIdentifier;

		public X509CertificateCollection ClientCertificates
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.certificatesCollection;
			}
		}

		public virtual NetworkCredential Credential
		{
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
			[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				NetworkCredential networkCredential;
				DirectoryConnection directoryConnection = this;
				if (value != null)
				{
					networkCredential = new NetworkCredential(value.UserName, value.Password, value.Domain);
				}
				else
				{
					networkCredential = null;
				}
				directoryConnection.directoryCredential = networkCredential;
			}
		}

		public virtual DirectoryIdentifier Directory
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryIdentifier;
			}
		}

		public virtual TimeSpan Timeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.connectionTimeOut;
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					this.connectionTimeOut = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
				}
			}
		}

		protected DirectoryConnection()
		{
			this.connectionTimeOut = new TimeSpan(0, 0, 30);
			Utility.CheckOSVersion();
			this.certificatesCollection = new X509CertificateCollection();
		}

		internal NetworkCredential GetCredential()
		{
			return this.directoryCredential;
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract DirectoryResponse SendRequest(DirectoryRequest request);
	}
}