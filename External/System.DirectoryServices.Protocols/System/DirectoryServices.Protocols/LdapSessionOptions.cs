using System;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace System.DirectoryServices.Protocols
{
	public class LdapSessionOptions
	{
		private LdapConnection connection;

		private ReferralCallback callbackRoutine;

		internal QueryClientCertificateCallback clientCertificateDelegate;

		private VerifyServerCertificateCallback serverCertificateDelegate;

		private QUERYFORCONNECTIONInternal queryDelegate;

		private NOTIFYOFNEWCONNECTIONInternal notifiyDelegate;

		private DEREFERENCECONNECTIONInternal dereferenceDelegate;

		private VERIFYSERVERCERT serverCertificateRoutine;

		public bool AutoReconnect
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_AUTO_RECONNECT);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int num;
				if (!value)
				{
					num = 0;
				}
				else
				{
					num = 1;
				}
				this.SetIntValueHelper(LdapOption.LDAP_OPT_AUTO_RECONNECT, num);
			}
		}

		internal DereferenceAlias DerefAlias
		{
			get
			{
				return (DereferenceAlias)this.GetIntValueHelper(LdapOption.LDAP_OPT_DEREF);
			}
			set
			{
				this.SetIntValueHelper(LdapOption.LDAP_OPT_DEREF, (int)value);
			}
		}

		public string DomainName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetStringValueHelper(LdapOption.LDAP_OPT_DNSDOMAIN_NAME, true);
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetStringValueHelper(LdapOption.LDAP_OPT_DNSDOMAIN_NAME, value);
			}
		}

		internal bool FQDN
		{
			set
			{
				this.SetIntValueHelper(LdapOption.LDAP_OPT_AREC_EXCLUSIVE, 1);
			}
		}

		public string HostName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetStringValueHelper(LdapOption.LDAP_OPT_HOST_NAME, false);
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetStringValueHelper(LdapOption.LDAP_OPT_HOST_NAME, value);
			}
		}

		public bool HostReachable
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_HOST_REACHABLE);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public LocatorFlags LocatorFlag
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_GETDSNAME_FLAGS);
				return (LocatorFlags)((long)intValueHelper);
			}
			set
			{
				this.SetIntValueHelper(LdapOption.LDAP_OPT_GETDSNAME_FLAGS, (int)value);
			}
		}

		public TimeSpan PingKeepAliveTimeout
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_PING_KEEP_ALIVE);
				return new TimeSpan((long)intValueHelper * (long)0x989680);
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					if (value.TotalSeconds <= 2147483647)
					{
						int ticks = (int)(value.Ticks / (long)0x989680);
						this.SetIntValueHelper(LdapOption.LDAP_OPT_PING_KEEP_ALIVE, ticks);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
				}
			}
		}

		public int PingLimit
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetIntValueHelper(LdapOption.LDAP_OPT_PING_LIMIT);
			}
			set
			{
				if (value >= 0)
				{
					this.SetIntValueHelper(LdapOption.LDAP_OPT_PING_LIMIT, value);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

		public TimeSpan PingWaitTimeout
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_PING_WAIT_TIME);
				return new TimeSpan((long)intValueHelper * (long)0x2710);
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					if (value.TotalMilliseconds <= 2147483647)
					{
						int ticks = (int)(value.Ticks / (long)0x2710);
						this.SetIntValueHelper(LdapOption.LDAP_OPT_PING_WAIT_TIME, ticks);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
				}
			}
		}

		public int ProtocolVersion
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetIntValueHelper(LdapOption.LDAP_OPT_VERSION);
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetIntValueHelper(LdapOption.LDAP_OPT_VERSION, value);
			}
		}

		public QueryClientCertificateCallback QueryClientCertificate
		{
			get
			{
				if (!this.connection.disposed)
				{
					return this.clientCertificateDelegate;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.connection.disposed)
				{
					if (value != null)
					{
						int num = Wldap32.ldap_set_option_clientcert(this.connection.ldapHandle, LdapOption.LDAP_OPT_CLIENT_CERTIFICATE, this.connection.clientCertificateRoutine);
						if (num == 0)
						{
							this.connection.automaticBind = false;
						}
						else
						{
							if (!Utility.IsLdapError((LdapError)num))
							{
								throw new LdapException(num);
							}
							else
							{
								string str = LdapErrorMappings.MapResultCode(num);
								throw new LdapException(num, str);
							}
						}
					}
					this.clientCertificateDelegate = value;
					return;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ReferralCallback ReferralCallback
		{
			get
			{
				if (!this.connection.disposed)
				{
					return this.callbackRoutine;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.connection.disposed)
				{
					ReferralCallback referralCallback = new ReferralCallback();
					if (value == null)
					{
						referralCallback.QueryForConnection = null;
						referralCallback.NotifyNewConnection = null;
						referralCallback.DereferenceConnection = null;
					}
					else
					{
						referralCallback.QueryForConnection = value.QueryForConnection;
						referralCallback.NotifyNewConnection = value.NotifyNewConnection;
						referralCallback.DereferenceConnection = value.DereferenceConnection;
					}
					this.ProcessCallBackRoutine(referralCallback);
					this.callbackRoutine = value;
					return;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ReferralChasingOptions ReferralChasing
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_REFERRALS);
				if (intValueHelper != 1)
				{
					return (ReferralChasingOptions)intValueHelper;
				}
				else
				{
					return ReferralChasingOptions.All;
				}
			}
			set
			{
				if (((int)value & -97) == (int)ReferralChasingOptions.None)
				{
					this.SetIntValueHelper(LdapOption.LDAP_OPT_REFERRALS, (int)value);
					return;
				}
				else
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ReferralChasingOptions));
				}
			}
		}

		public int ReferralHopLimit
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetIntValueHelper(LdapOption.LDAP_OPT_REFERRAL_HOP_LIMIT);
			}
			set
			{
				if (value >= 0)
				{
					this.SetIntValueHelper(LdapOption.LDAP_OPT_REFERRAL_HOP_LIMIT, value);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValue"), "value");
				}
			}
		}

		public bool RootDseCache
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_ROOTDSE_CACHE);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int num;
				if (!value)
				{
					num = 0;
				}
				else
				{
					num = 1;
				}
				this.SetIntValueHelper(LdapOption.LDAP_OPT_ROOTDSE_CACHE, num);
			}
		}

		public string SaslMethod
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetStringValueHelper(LdapOption.LDAP_OPT_SASL_METHOD, true);
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetStringValueHelper(LdapOption.LDAP_OPT_SASL_METHOD, value);
			}
		}

		public bool Sealing
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_ENCRYPT);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int num;
				if (!value)
				{
					num = 0;
				}
				else
				{
					num = 1;
				}
				this.SetIntValueHelper(LdapOption.LDAP_OPT_ENCRYPT, num);
			}
		}

		public bool SecureSocketLayer
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_SSL);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int num;
				if (!value)
				{
					num = 0;
				}
				else
				{
					num = 1;
				}
				this.SetIntValueHelper(LdapOption.LDAP_OPT_SSL, num);
			}
		}

		public object SecurityContext
		{
			get
			{
				if (!this.connection.disposed)
				{
					SecurityHandle securityHandle = new SecurityHandle();
					int num = Wldap32.ldap_get_option_sechandle(this.connection.ldapHandle, LdapOption.LDAP_OPT_SECURITY_CONTEXT, ref securityHandle);
					ErrorChecking.CheckAndSetLdapError(num);
					return securityHandle;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public TimeSpan SendTimeout
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_SEND_TIMEOUT);
				return new TimeSpan((long)intValueHelper * (long)0x989680);
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					if (value.TotalSeconds <= 2147483647)
					{
						int ticks = (int)(value.Ticks / (long)0x989680);
						this.SetIntValueHelper(LdapOption.LDAP_OPT_SEND_TIMEOUT, ticks);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
				}
			}
		}

		internal string ServerErrorMessage
		{
			get
			{
				return this.GetStringValueHelper(LdapOption.LDAP_OPT_SERVER_ERROR, true);
			}
		}

		public bool Signing
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_SIGN);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int num;
				if (!value)
				{
					num = 0;
				}
				else
				{
					num = 1;
				}
				this.SetIntValueHelper(LdapOption.LDAP_OPT_SIGN, num);
			}
		}

		public SecurityPackageContextConnectionInformation SslInformation
		{
			get
			{
				if (!this.connection.disposed)
				{
					SecurityPackageContextConnectionInformation securityPackageContextConnectionInformation = new SecurityPackageContextConnectionInformation();
					int num = Wldap32.ldap_get_option_secInfo(this.connection.ldapHandle, LdapOption.LDAP_OPT_SSL_INFO, securityPackageContextConnectionInformation);
					ErrorChecking.CheckAndSetLdapError(num);
					return securityPackageContextConnectionInformation;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public int SspiFlag
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.GetIntValueHelper(LdapOption.LDAP_OPT_SSPI_FLAGS);
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetIntValueHelper(LdapOption.LDAP_OPT_SSPI_FLAGS, value);
			}
		}

		public bool TcpKeepAlive
		{
			get
			{
				int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_TCP_KEEPALIVE);
				if (intValueHelper != 1)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int num;
				if (!value)
				{
					num = 0;
				}
				else
				{
					num = 1;
				}
				this.SetIntValueHelper(LdapOption.LDAP_OPT_TCP_KEEPALIVE, num);
			}
		}

		public VerifyServerCertificateCallback VerifyServerCertificate
		{
			get
			{
				if (!this.connection.disposed)
				{
					return this.serverCertificateDelegate;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.connection.disposed)
				{
					if (value != null)
					{
						int num = Wldap32.ldap_set_option_servercert(this.connection.ldapHandle, LdapOption.LDAP_OPT_SERVER_CERTIFICATE, this.serverCertificateRoutine);
						ErrorChecking.CheckAndSetLdapError(num);
					}
					this.serverCertificateDelegate = value;
					return;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		internal LdapSessionOptions(LdapConnection connection)
		{
			this.callbackRoutine = new ReferralCallback();
			this.connection = connection;
			this.queryDelegate = new QUERYFORCONNECTIONInternal(this.ProcessQueryConnection);
			this.notifiyDelegate = new NOTIFYOFNEWCONNECTIONInternal(this.ProcessNotifyConnection);
			this.dereferenceDelegate = new DEREFERENCECONNECTIONInternal(this.ProcessDereferenceConnection);
			this.serverCertificateRoutine = new VERIFYSERVERCERT(this.ProcessServerCertificate);
		}

		public void FastConcurrentBind()
		{
			if (!this.connection.disposed)
			{
				int num = 1;
				this.ProtocolVersion = 3;
				int num1 = Wldap32.ldap_set_option_int(this.connection.ldapHandle, LdapOption.LDAP_OPT_FAST_CONCURRENT_BIND, ref num);
				if (num1 != 89 || Utility.IsWin2k3AboveOS)
				{
					ErrorChecking.CheckAndSetLdapError(num1);
					return;
				}
				else
				{
					throw new PlatformNotSupportedException(Res.GetString("ConcurrentBindNotSupport"));
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private int GetIntValueHelper(LdapOption option)
		{
			if (!this.connection.disposed)
			{
				int num = 0;
				int num1 = Wldap32.ldap_get_option_int(this.connection.ldapHandle, option, ref num);
				ErrorChecking.CheckAndSetLdapError(num1);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private string GetStringValueHelper(LdapOption option, bool releasePtr)
		{
			if (!this.connection.disposed)
			{
				IntPtr intPtr = new IntPtr(0);
				int num = Wldap32.ldap_get_option_ptr(this.connection.ldapHandle, option, ref intPtr);
				ErrorChecking.CheckAndSetLdapError(num);
				string stringUni = null;
				if (intPtr != (IntPtr)0)
				{
					stringUni = Marshal.PtrToStringUni(intPtr);
				}
				if (releasePtr)
				{
					Wldap32.ldap_memfree(intPtr);
				}
				return stringUni;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private void ProcessCallBackRoutine(ReferralCallback tempCallback)
		{
			QUERYFORCONNECTIONInternal qUERYFORCONNECTIONInternal;
			NOTIFYOFNEWCONNECTIONInternal nOTIFYOFNEWCONNECTIONInternal;
			DEREFERENCECONNECTIONInternal dEREFERENCECONNECTIONInternal;
			LdapReferralCallback ldapReferralCallback = new LdapReferralCallback();
			ldapReferralCallback.sizeofcallback = Marshal.SizeOf(typeof(LdapReferralCallback));
			LdapReferralCallback ldapReferralCallbackPointer = ldapReferralCallback;
			if (tempCallback.QueryForConnection == null)
			{
				qUERYFORCONNECTIONInternal = null;
			}
			else
			{
				qUERYFORCONNECTIONInternal = this.queryDelegate;
			}
			ldapReferralCallbackPointer.query = qUERYFORCONNECTIONInternal;
			LdapReferralCallback ldapReferralCallbackPointer1 = ldapReferralCallback;
			if (tempCallback.NotifyNewConnection == null)
			{
				nOTIFYOFNEWCONNECTIONInternal = null;
			}
			else
			{
				nOTIFYOFNEWCONNECTIONInternal = this.notifiyDelegate;
			}
			ldapReferralCallbackPointer1.notify = nOTIFYOFNEWCONNECTIONInternal;
			LdapReferralCallback ldapReferralCallbackPointer2 = ldapReferralCallback;
			if (tempCallback.DereferenceConnection == null)
			{
				dEREFERENCECONNECTIONInternal = null;
			}
			else
			{
				dEREFERENCECONNECTIONInternal = this.dereferenceDelegate;
			}
			ldapReferralCallbackPointer2.dereference = dEREFERENCECONNECTIONInternal;
			int num = Wldap32.ldap_set_option_referral(this.connection.ldapHandle, LdapOption.LDAP_OPT_REFERRAL_CALLBACK, ref ldapReferralCallback);
			ErrorChecking.CheckAndSetLdapError(num);
		}

		private int ProcessDereferenceConnection(IntPtr PrimaryConnection, IntPtr ConnectionToDereference)
		{
			LdapConnection ldapConnection;
			WeakReference item;
			if (ConnectionToDereference != (IntPtr)0 && this.callbackRoutine.DereferenceConnection != null)
			{
				lock (LdapConnection.objectLock)
				{
					item = (WeakReference)LdapConnection.handleTable[(object)ConnectionToDereference];
				}
				if (item == null || !item.IsAlive)
				{
					ldapConnection = new LdapConnection((LdapDirectoryIdentifier)this.connection.Directory, this.connection.GetCredential(), this.connection.AuthType, ConnectionToDereference);
				}
				else
				{
					ldapConnection = (LdapConnection)item.Target;
				}
				this.callbackRoutine.DereferenceConnection(this.connection, ldapConnection);
			}
			return 1;
		}

		private bool ProcessNotifyConnection(IntPtr PrimaryConnection, IntPtr ReferralFromConnection, IntPtr NewDNPtr, string HostName, IntPtr NewConnection, int PortNumber, SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentity, Luid CurrentUser, int ErrorCodeFromBind)
		{
			WeakReference item;
			string stringUni = null;
			if (!(NewConnection != (IntPtr)0) || this.callbackRoutine.NotifyNewConnection == null)
			{
				return false;
			}
			else
			{
				if (NewDNPtr != (IntPtr)0)
				{
					stringUni = Marshal.PtrToStringUni(NewDNPtr);
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(HostName);
				stringBuilder.Append(":");
				stringBuilder.Append(PortNumber);
				LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(stringBuilder.ToString());
				NetworkCredential networkCredential = this.ProcessSecAuthIdentity(SecAuthIdentity);
				LdapConnection ldapConnection = null;
				LdapConnection target = null;
				lock (LdapConnection.objectLock)
				{
					if (ReferralFromConnection != (IntPtr)0)
					{
						item = (WeakReference)LdapConnection.handleTable[(object)ReferralFromConnection];
						if (item == null || !item.IsAlive)
						{
							if (item != null)
							{
								LdapConnection.handleTable.Remove(ReferralFromConnection);
							}
							target = new LdapConnection((LdapDirectoryIdentifier)this.connection.Directory, this.connection.GetCredential(), this.connection.AuthType, ReferralFromConnection);
							LdapConnection.handleTable.Add(ReferralFromConnection, new WeakReference(target));
						}
						else
						{
							target = (LdapConnection)item.Target;
						}
					}
					if (NewConnection != (IntPtr)0)
					{
						item = (WeakReference)LdapConnection.handleTable[(object)NewConnection];
						if (item == null || !item.IsAlive)
						{
							if (item != null)
							{
								LdapConnection.handleTable.Remove(NewConnection);
							}
							ldapConnection = new LdapConnection(ldapDirectoryIdentifier, networkCredential, this.connection.AuthType, NewConnection);
							LdapConnection.handleTable.Add(NewConnection, new WeakReference(ldapConnection));
						}
						else
						{
							ldapConnection = (LdapConnection)item.Target;
						}
					}
				}
				long lowPart = (long)CurrentUser.LowPart + ((long)CurrentUser.HighPart << 32);
				bool errorCodeFromBind = this.callbackRoutine.NotifyNewConnection(this.connection, target, stringUni, ldapDirectoryIdentifier, ldapConnection, networkCredential, lowPart, ErrorCodeFromBind);
				if (errorCodeFromBind)
				{
					ldapConnection.needDispose = true;
				}
				return errorCodeFromBind;
			}
		}

		private int ProcessQueryConnection(IntPtr PrimaryConnection, IntPtr ReferralFromConnection, IntPtr NewDNPtr, string HostName, int PortNumber, SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentity, Luid CurrentUserToken, ref ConnectionHandle ConnectionToUse)
		{
			ConnectionToUse = null;
			string stringUni = null;
			if (this.callbackRoutine.QueryForConnection == null)
			{
				return 1;
			}
			else
			{
				if (NewDNPtr != (IntPtr)0)
				{
					stringUni = Marshal.PtrToStringUni(NewDNPtr);
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(HostName);
				stringBuilder.Append(":");
				stringBuilder.Append(PortNumber);
				LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(stringBuilder.ToString());
				NetworkCredential networkCredential = this.ProcessSecAuthIdentity(SecAuthIdentity);
				LdapConnection ldapConnection = null;
				if (ReferralFromConnection != (IntPtr)0)
				{
					lock (LdapConnection.objectLock)
					{
						WeakReference item = (WeakReference)LdapConnection.handleTable[(object)ReferralFromConnection];
						if (item == null || !item.IsAlive)
						{
							if (item != null)
							{
								LdapConnection.handleTable.Remove(ReferralFromConnection);
							}
							ldapConnection = new LdapConnection((LdapDirectoryIdentifier)this.connection.Directory, this.connection.GetCredential(), this.connection.AuthType, ReferralFromConnection);
							LdapConnection.handleTable.Add(ReferralFromConnection, new WeakReference(ldapConnection));
						}
						else
						{
							ldapConnection = (LdapConnection)item.Target;
						}
					}
				}
				long lowPart = (long)CurrentUserToken.LowPart + ((long)CurrentUserToken.HighPart << 32);
				LdapConnection queryForConnection = this.callbackRoutine.QueryForConnection(this.connection, ldapConnection, stringUni, ldapDirectoryIdentifier, networkCredential, lowPart);
				if (queryForConnection != null)
				{
					ConnectionToUse = queryForConnection.ldapHandle;
				}
				return 0;
			}
		}

		private NetworkCredential ProcessSecAuthIdentity(SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentit)
		{
			if (SecAuthIdentit != null)
			{
				string secAuthIdentit = SecAuthIdentit.user;
				string str = SecAuthIdentit.domain;
				string secAuthIdentit1 = SecAuthIdentit.password;
				return new NetworkCredential(secAuthIdentit, secAuthIdentit1, str);
			}
			else
			{
				return new NetworkCredential();
			}
		}

		private bool ProcessServerCertificate(IntPtr Connection, IntPtr pServerCert)
		{
			bool flag = true;
			if (this.serverCertificateDelegate != null)
			{
				IntPtr intPtr = (IntPtr)0;
				X509Certificate x509Certificate = null;
				try
				{
					intPtr = Marshal.ReadIntPtr(pServerCert);
					x509Certificate = new X509Certificate(intPtr);
				}
				finally
				{
					Wldap32.CertFreeCRLContext(intPtr);
				}
				flag = this.serverCertificateDelegate(this.connection, x509Certificate);
			}
			return flag;
		}

		private void SetIntValueHelper(LdapOption option, int value)
		{
			if (!this.connection.disposed)
			{
				int num = value;
				int num1 = Wldap32.ldap_set_option_int(this.connection.ldapHandle, option, ref num);
				ErrorChecking.CheckAndSetLdapError(num1);
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private void SetStringValueHelper(LdapOption option, string value)
		{
			if (!this.connection.disposed)
			{
				IntPtr intPtr = new IntPtr(0);
				if (value != null)
				{
					intPtr = Marshal.StringToHGlobalUni(value);
				}
				try
				{
					int num = Wldap32.ldap_set_option_ptr(this.connection.ldapHandle, option, ref intPtr);
					ErrorChecking.CheckAndSetLdapError(num);
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public unsafe void StartTransportLayerSecurity(DirectoryControlCollection controls)
		{
			IntPtr intPtr;
			IntPtr intPtr1;
			IntPtr intPtr2 = (IntPtr)0;
			LdapControl[] ldapControlArray = null;
			IntPtr intPtr3 = (IntPtr)0;
			LdapControl[] ldapControlArray1 = null;
			IntPtr intPtr4 = (IntPtr)0;
			IntPtr intPtr5 = (IntPtr)0;
			int num = 0;
			Uri[] uri = null;
			if (!Utility.IsWin2kOS)
			{
				if (!this.connection.disposed)
				{
					try
					{
						ldapControlArray = this.connection.BuildControlArray(controls, true);
						int num1 = Marshal.SizeOf(typeof(LdapControl));
						if (ldapControlArray != null)
						{
							intPtr2 = Utility.AllocHGlobalIntPtrArray((int)ldapControlArray.Length + 1);
							for (int i = 0; i < (int)ldapControlArray.Length; i++)
							{
								intPtr = Marshal.AllocHGlobal(num1);
								Marshal.StructureToPtr(ldapControlArray[i], intPtr, false);
								intPtr1 = (IntPtr)((long)intPtr2 + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
								Marshal.WriteIntPtr(intPtr1, intPtr);
							}
							intPtr1 = (IntPtr)((long)intPtr2 + (long)(Marshal.SizeOf(typeof(IntPtr)) * (int)ldapControlArray.Length));
							Marshal.WriteIntPtr(intPtr1, (IntPtr)0);
						}
						ldapControlArray1 = this.connection.BuildControlArray(controls, false);
						if (ldapControlArray1 != null)
						{
							intPtr3 = Utility.AllocHGlobalIntPtrArray((int)ldapControlArray1.Length + 1);
							for (int j = 0; j < (int)ldapControlArray1.Length; j++)
							{
								intPtr = Marshal.AllocHGlobal(num1);
								Marshal.StructureToPtr(ldapControlArray1[j], intPtr, false);
								intPtr1 = (IntPtr)((long)intPtr3 + (long)(Marshal.SizeOf(typeof(IntPtr)) * j));
								Marshal.WriteIntPtr(intPtr1, intPtr);
							}
							intPtr1 = (IntPtr)((long)intPtr3 + (long)(Marshal.SizeOf(typeof(IntPtr)) * (int)ldapControlArray1.Length));
							Marshal.WriteIntPtr(intPtr1, (IntPtr)0);
						}
						int num2 = Wldap32.ldap_start_tls(this.connection.ldapHandle, ref num, ref intPtr4, intPtr2, intPtr3);
						if (intPtr4 != (IntPtr)0)
						{
							int num3 = Wldap32.ldap_parse_result_referral(this.connection.ldapHandle, intPtr4, (IntPtr)0, (IntPtr)0, (IntPtr)0, ref intPtr5, (IntPtr)0, 0);
							if (num3 == 0 && intPtr5 != (IntPtr)0)
							{
								char** chrPointer = (char**)((void*)intPtr5);
								char* chrPointer1 = (char*)((void*)(*(chrPointer)));
								int num4 = 0;
								ArrayList arrayLists = new ArrayList();
								while (chrPointer1 != null)
								{
									string stringUni = Marshal.PtrToStringUni((IntPtr)chrPointer1);
									arrayLists.Add(stringUni);
									num4++;
									chrPointer1 = (char*)((void*)(*(chrPointer + num4 * sizeof(char*))));
								}
								if (intPtr5 != (IntPtr)0)
								{
									Wldap32.ldap_value_free(intPtr5);
									intPtr5 = (IntPtr)0;
								}
								if (arrayLists.Count > 0)
								{
									uri = new Uri[arrayLists.Count];
									for (int k = 0; k < arrayLists.Count; k++)
									{
										uri[k] = new Uri((string)arrayLists[k]);
									}
								}
							}
						}
						if (num2 != 0)
						{
							string str = Res.GetString("DefaultLdapError");
							if (!Utility.IsResultCode((ResultCode)num2))
							{
								if (Utility.IsLdapError((LdapError)num2))
								{
									str = LdapErrorMappings.MapResultCode(num2);
									throw new LdapException(num2, str);
								}
							}
							else
							{
								if (num2 == 80)
								{
									num2 = num;
								}
								str = OperationErrorMappings.MapResultCode(num2);
								ExtendedResponse extendedResponse = new ExtendedResponse(null, null, (ResultCode)num2, str, uri);
								extendedResponse.name = "1.3.6.1.4.1.1466.20037";
								throw new TlsOperationException(extendedResponse);
							}
						}
					}
					finally
					{
						if (intPtr2 != (IntPtr)0)
						{
							for (int l = 0; l < (int)ldapControlArray.Length; l++)
							{
								IntPtr intPtr6 = Marshal.ReadIntPtr(intPtr2, Marshal.SizeOf(typeof(IntPtr)) * l);
								if (intPtr6 != (IntPtr)0)
								{
									Marshal.FreeHGlobal(intPtr6);
								}
							}
							Marshal.FreeHGlobal(intPtr2);
						}
						if (ldapControlArray != null)
						{
							for (int m = 0; m < (int)ldapControlArray.Length; m++)
							{
								if (ldapControlArray[m].ldctl_oid != (IntPtr)0)
								{
									Marshal.FreeHGlobal(ldapControlArray[m].ldctl_oid);
								}
								if (ldapControlArray[m].ldctl_value != null && ldapControlArray[m].ldctl_value.bv_val != (IntPtr)0)
								{
									Marshal.FreeHGlobal(ldapControlArray[m].ldctl_value.bv_val);
								}
							}
						}
						if (intPtr3 != (IntPtr)0)
						{
							for (int n = 0; n < (int)ldapControlArray1.Length; n++)
							{
								IntPtr intPtr7 = Marshal.ReadIntPtr(intPtr3, Marshal.SizeOf(typeof(IntPtr)) * n);
								if (intPtr7 != (IntPtr)0)
								{
									Marshal.FreeHGlobal(intPtr7);
								}
							}
							Marshal.FreeHGlobal(intPtr3);
						}
						if (ldapControlArray1 != null)
						{
							for (int o = 0; o < (int)ldapControlArray1.Length; o++)
							{
								if (ldapControlArray1[o].ldctl_oid != (IntPtr)0)
								{
									Marshal.FreeHGlobal(ldapControlArray1[o].ldctl_oid);
								}
								if (ldapControlArray1[o].ldctl_value != null && ldapControlArray1[o].ldctl_value.bv_val != (IntPtr)0)
								{
									Marshal.FreeHGlobal(ldapControlArray1[o].ldctl_value.bv_val);
								}
							}
						}
						if (intPtr5 != (IntPtr)0)
						{
							Wldap32.ldap_value_free(intPtr5);
						}
					}
					return;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			else
			{
				throw new PlatformNotSupportedException(Res.GetString("TLSNotSupported"));
			}
		}

		public void StopTransportLayerSecurity()
		{
			if (!Utility.IsWin2kOS)
			{
				if (!this.connection.disposed)
				{
					byte num = Wldap32.ldap_stop_tls(this.connection.ldapHandle);
					if (num != 0)
					{
						return;
					}
					else
					{
						throw new TlsOperationException(null, Res.GetString("TLSStopFailure"));
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			else
			{
				throw new PlatformNotSupportedException(Res.GetString("TLSNotSupported"));
			}
		}
	}
}