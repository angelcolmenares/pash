using System;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	internal sealed class CredentialValidator
	{
		private bool fastConcurrentSupported;

		private Hashtable connCache;

		private LdapDirectoryIdentifier directoryIdent;

		private object cacheLock;

		private CredentialValidator.AuthMethod lastBindMethod;

		private string serverName;

		private ContextType contextType;

		private ServerProperties serverProperties;

		private const ContextOptions defaultContextOptionsNegotiate = ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing;

		private const ContextOptions defaultContextOptionsSimple = ContextOptions.SimpleBind | ContextOptions.SecureSocketLayer;

		public CredentialValidator(ContextType contextType, string serverName, ServerProperties serverProperties)
		{
			this.fastConcurrentSupported = true;
			this.connCache = new Hashtable(4);
			this.cacheLock = new object();
			this.lastBindMethod = CredentialValidator.AuthMethod.Simple;
			this.fastConcurrentSupported = serverProperties.OsVersion != DomainControllerMode.Win2k;
			if (contextType != ContextType.Machine || serverName != null)
			{
				this.serverName = serverName;
			}
			else
			{
				this.serverName = Environment.MachineName;
			}
			this.contextType = contextType;
			this.serverProperties = serverProperties;
		}

		private bool BindLdap(NetworkCredential creds, ContextOptions contextOptions)
		{
			LdapConnection item;
			int lDAPSSLPORT;
			bool flag;
			int num;
			bool flag1 = (ContextOptions.SecureSocketLayer & contextOptions) > 0;
			if (this.contextType != ContextType.ApplicationDirectory)
			{
				CredentialValidator ldapDirectoryIdentifier = this;
				string str = this.serverName;
				if (flag1)
				{
					lDAPSSLPORT = LdapConstants.LDAP_SSL_PORT;
				}
				else
				{
					lDAPSSLPORT = LdapConstants.LDAP_PORT;
				}
				ldapDirectoryIdentifier.directoryIdent = new LdapDirectoryIdentifier(str, lDAPSSLPORT);
			}
			else
			{
				CredentialValidator credentialValidator = this;
				string str1 = this.serverProperties.dnsHostName;
				if (flag1)
				{
					num = this.serverProperties.portSSL;
				}
				else
				{
					num = this.serverProperties.portLDAP;
				}
				credentialValidator.directoryIdent = new LdapDirectoryIdentifier(str1, num);
			}
			if (!flag1)
			{
				flag = false;
			}
			else
			{
				flag = this.fastConcurrentSupported;
			}
			bool flag2 = flag;
			int num1 = Convert.ToInt32(flag2) * 2 + Convert.ToInt32(flag1);
			if (this.connCache.Contains(num1))
			{
				item = (LdapConnection)this.connCache[(object)num1];
			}
			else
			{
				lock (this.cacheLock)
				{
					if (this.connCache.Contains(num1))
					{
						item = (LdapConnection)this.connCache[(object)num1];
					}
					else
					{
						item = new LdapConnection(this.directoryIdent);
						item.SessionOptions.SecureSocketLayer = flag1;
						if (flag2)
						{
							try
							{
								item.SessionOptions.FastConcurrentBind();
							}
							catch (PlatformNotSupportedException platformNotSupportedException)
							{
								item.Dispose();
								item = null;
								this.fastConcurrentSupported = false;
								num1 = Convert.ToInt32(flag1);
								item = new LdapConnection(this.directoryIdent);
								item.SessionOptions.SecureSocketLayer = flag1;
							}
						}
						this.connCache.Add(num1, item);
					}
				}
			}
			if (!flag2 || !this.fastConcurrentSupported)
			{
				lock (this.cacheLock)
				{
					this.lockedLdapBind(item, creds, contextOptions);
				}
			}
			else
			{
				this.lockedLdapBind(item, creds, contextOptions);
			}
			return true;
		}

		[SecurityCritical]
		private bool BindSam(string target, string userName, string password)
		{
			bool flag;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("WinNT://");
			stringBuilder.Append(this.serverName);
			stringBuilder.Append(",computer");
			Guid guid = new Guid("fd8256d0-fd15-11ce-abc4-02608c9e7553");
			object obj = null;
			int num = 1;
			try
			{
				try
				{
					if (Thread.CurrentThread.GetApartmentState() == ApartmentState.Unknown)
					{
						Thread.CurrentThread.SetApartmentState(ApartmentState.MTA);
					}
					if (userName != null)
					{
						int num1 = userName.IndexOf("\\", StringComparison.Ordinal);
						if (num1 == -1)
						{
							userName = string.Concat(this.serverName, "\\", userName);
						}
					}
					int num2 = UnsafeNativeMethods.ADsOpenObject(stringBuilder.ToString(), userName, password, num, out guid, out obj);
					if (num2 == 0)
					{
						((UnsafeNativeMethods.IADs)obj).Get("name");
					}
					else
					{
						if (num2 != ExceptionHelper.ERROR_HRESULT_LOGON_FAILURE)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(num2);
						}
						else
						{
							flag = false;
							return flag;
						}
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					if (cOMException.ErrorCode != ExceptionHelper.ERROR_HRESULT_LOGON_FAILURE)
					{
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
					else
					{
						flag = false;
						return flag;
					}
				}
				return true;
			}
			finally
			{
				if (obj != null)
				{
					Marshal.ReleaseComObject(obj);
				}
			}
			return flag;
		}

		[SecuritySafeCritical]
		private void lockedLdapBind(LdapConnection current, NetworkCredential creds, ContextOptions contextOptions)
		{
			AuthType authType;
			bool flag;
			bool flag1;
			LdapConnection ldapConnection = current;
			if ((ContextOptions.SimpleBind & contextOptions) > 0)
			{
				authType = AuthType.Basic;
			}
			else
			{
				authType = AuthType.Negotiate;
			}
			ldapConnection.AuthType = authType;
			LdapSessionOptions sessionOptions = current.SessionOptions;
			if ((ContextOptions.Signing & contextOptions) > 0)
			{
				flag = true;
			}
			else
			{
				flag = false;
			}
			sessionOptions.Signing = flag;
			LdapSessionOptions ldapSessionOption = current.SessionOptions;
			if ((ContextOptions.Sealing & contextOptions) > 0)
			{
				flag1 = true;
			}
			else
			{
				flag1 = false;
			}
			ldapSessionOption.Sealing = flag1;
			if (creds.UserName != null || creds.Password != null)
			{
				current.Bind(creds);
				return;
			}
			else
			{
				current.Bind();
				return;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		public bool Validate(string userName, string password)
		{
			bool flag;
			NetworkCredential networkCredential = new NetworkCredential(userName, password);
			if (userName == null || userName.Length != 0)
			{
				if (this.contextType == ContextType.Domain || this.contextType == ContextType.ApplicationDirectory)
				{
					try
					{
						if (this.lastBindMethod != CredentialValidator.AuthMethod.Simple || !this.fastConcurrentSupported && this.contextType != ContextType.ApplicationDirectory)
						{
							try
							{
								this.BindLdap(networkCredential, ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing);
								this.lastBindMethod = CredentialValidator.AuthMethod.Negotiate;
								flag = true;
								return flag;
							}
							catch (LdapException ldapException)
							{
							}
							this.BindLdap(networkCredential, ContextOptions.SimpleBind | ContextOptions.SecureSocketLayer);
							this.lastBindMethod = CredentialValidator.AuthMethod.Simple;
							flag = true;
						}
						else
						{
							try
							{
								this.BindLdap(networkCredential, ContextOptions.SimpleBind | ContextOptions.SecureSocketLayer);
								this.lastBindMethod = CredentialValidator.AuthMethod.Simple;
								flag = true;
								return flag;
							}
							catch (LdapException ldapException1)
							{
							}
							this.BindLdap(networkCredential, ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing);
							this.lastBindMethod = CredentialValidator.AuthMethod.Negotiate;
							flag = true;
						}
					}
					catch (LdapException ldapException3)
					{
						LdapException ldapException2 = ldapException3;
						if ((long)ldapException2.ErrorCode != (long)ExceptionHelper.ERROR_LOGON_FAILURE)
						{
							throw;
						}
						else
						{
							flag = false;
						}
					}
					return flag;
				}
				else
				{
					return this.BindSam(this.serverName, userName, password);
				}
			}
			else
			{
				return false;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		public bool Validate(string userName, string password, ContextOptions connectionMethod)
		{
			bool flag;
			if (userName == null || userName.Length != 0)
			{
				if (this.contextType == ContextType.Domain || this.contextType == ContextType.ApplicationDirectory)
				{
					try
					{
						NetworkCredential networkCredential = new NetworkCredential(userName, password);
						this.BindLdap(networkCredential, connectionMethod);
						flag = true;
					}
					catch (LdapException ldapException1)
					{
						LdapException ldapException = ldapException1;
						if ((long)ldapException.ErrorCode != (long)ExceptionHelper.ERROR_LOGON_FAILURE)
						{
							throw;
						}
						else
						{
							flag = false;
						}
					}
					return flag;
				}
				else
				{
					return this.BindSam(this.serverName, userName, password);
				}
			}
			else
			{
				return false;
			}
		}

		private enum AuthMethod
		{
			Simple = 1,
			Negotiate = 2
		}
	}
}