using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.DirectoryServices.ActiveDirectory
{
	public class DirectoryContext
	{
		private string name;

		private DirectoryContextType contextType;

		private NetworkCredential credential;

		internal string serverName;

		internal bool usernameIsNull;

		internal bool passwordIsNull;

		private bool validated;

		private bool contextIsValid;

		private static bool platformSupported;

		private static bool serverBindSupported;

		private static bool dnsgetdcSupported;

		private static bool w2k;

		internal static LoadLibrarySafeHandle ADHandle;

		internal static LoadLibrarySafeHandle ADAMHandle;

		public DirectoryContextType ContextType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.contextType;
			}
		}

		internal NetworkCredential Credential
		{
			get
			{
				return this.credential;
			}
		}

		internal static bool DnsgetdcSupported
		{
			get
			{
				return DirectoryContext.dnsgetdcSupported;
			}
		}

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		internal string Password
		{
			[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				if (!this.passwordIsNull)
				{
					return this.credential.Password;
				}
				else
				{
					return null;
				}
			}
		}

		internal static bool ServerBindSupported
		{
			get
			{
				return DirectoryContext.serverBindSupported;
			}
		}

		public string UserName
		{
			get
			{
				if (!this.usernameIsNull)
				{
					return this.credential.UserName;
				}
				else
				{
					return null;
				}
			}
		}

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		static DirectoryContext()
		{
			DirectoryContext.platformSupported = true;
			DirectoryContext.serverBindSupported = true;
			DirectoryContext.dnsgetdcSupported = true;
			DirectoryContext.w2k = false;
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform == PlatformID.MacOSX || oSVersion.Platform == PlatformID.Unix) return;
			if (oSVersion.Platform != PlatformID.Win32NT || oSVersion.Version.Major < 5)
			{
				DirectoryContext.platformSupported = false;
				DirectoryContext.serverBindSupported = false;
				DirectoryContext.dnsgetdcSupported = false;
				return;
			}
			else
			{
				if (oSVersion.Version.Major == 5 && oSVersion.Version.Minor == 0)
				{
					DirectoryContext.w2k = true;
					DirectoryContext.dnsgetdcSupported = false;
					OSVersionInfoEx oSVersionInfoEx = new OSVersionInfoEx();
					bool versionEx = NativeMethods.GetVersionEx(oSVersionInfoEx);
					if (versionEx)
					{
						if (oSVersionInfoEx.servicePackMajor < 3)
						{
							DirectoryContext.serverBindSupported = false;
						}
					}
					else
					{
						int lastError = NativeMethods.GetLastError();
						object[] objArray = new object[1];
						objArray[0] = lastError;
						throw new SystemException(Res.GetString("VersionFailure", objArray));
					}
				}
				DirectoryContext.GetLibraryHandle();
				return;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public DirectoryContext(DirectoryContextType contextType)
		{
			if (contextType == DirectoryContextType.Domain || contextType == DirectoryContextType.Forest)
			{
				this.InitializeDirectoryContext(contextType, null, null, null);
				return;
			}
			else
			{
				throw new ArgumentException(Res.GetString("OnlyDomainOrForest"), "contextType");
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public DirectoryContext(DirectoryContextType contextType, string name)
		{
			if (contextType < DirectoryContextType.Domain || contextType > DirectoryContextType.ApplicationPartition)
			{
				throw new InvalidEnumArgumentException("contextType", (int)contextType, typeof(DirectoryContextType));
			}
			else
			{
				if (name != null)
				{
					if (name.Length != 0)
					{
						this.InitializeDirectoryContext(contextType, name, null, null);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "name");
					}
				}
				else
				{
					throw new ArgumentNullException("name");
				}
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public DirectoryContext(DirectoryContextType contextType, string username, string password)
		{
			if (contextType == DirectoryContextType.Domain || contextType == DirectoryContextType.Forest)
			{
				this.InitializeDirectoryContext(contextType, null, username, password);
				return;
			}
			else
			{
				throw new ArgumentException(Res.GetString("OnlyDomainOrForest"), "contextType");
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public DirectoryContext(DirectoryContextType contextType, string name, string username, string password)
		{
			if (contextType < DirectoryContextType.Domain || contextType > DirectoryContextType.ApplicationPartition)
			{
				throw new InvalidEnumArgumentException("contextType", (int)contextType, typeof(DirectoryContextType));
			}
			else
			{
				if (name != null)
				{
					if (name.Length != 0)
					{
						this.InitializeDirectoryContext(contextType, name, username, password);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "name");
					}
				}
				else
				{
					throw new ArgumentNullException("name");
				}
			}
		}

		internal DirectoryContext(DirectoryContextType contextType, string name, DirectoryContext context)
		{
			this.name = name;
			this.contextType = contextType;
			if (context == null)
			{
				this.credential = new NetworkCredential(null, "", null);
				this.usernameIsNull = true;
				this.passwordIsNull = true;
				return;
			}
			else
			{
				this.credential = context.Credential;
				this.usernameIsNull = context.usernameIsNull;
				this.passwordIsNull = context.passwordIsNull;
				return;
			}
		}

		internal DirectoryContext(DirectoryContext context)
		{
			this.name = context.Name;
			this.contextType = context.ContextType;
			this.credential = context.Credential;
			this.usernameIsNull = context.usernameIsNull;
			this.passwordIsNull = context.passwordIsNull;
			if (context.ContextType != DirectoryContextType.ConfigurationSet)
			{
				this.serverName = context.serverName;
			}
		}

		internal static string GetDnsDomainName(string domainName)
		{
			DomainControllerInfo domainControllerInfo = null;
			int num = Locator.DsGetDcNameWrapper(null, domainName, null, (long)16, out domainControllerInfo);
			if (num != 0x54b)
			{
				if (num != 0)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num);
				}
			}
			else
			{
				num = Locator.DsGetDcNameWrapper(null, domainName, null, (long)17, out domainControllerInfo);
				if (num != 0x54b)
				{
					if (num != 0)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(num);
					}
				}
				else
				{
					return null;
				}
			}
			return domainControllerInfo.DomainName;
		}

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		[FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
		private static void GetLibraryHandle()
		{
			string systemDirectory = Environment.SystemDirectory;
			IntPtr intPtr = UnsafeNativeMethods.LoadLibrary(string.Concat(systemDirectory, "\\ntdsapi.dll"));
			if (intPtr != (IntPtr)0)
			{
				DirectoryContext.ADHandle = new LoadLibrarySafeHandle(intPtr);
				DirectoryInfo parent = Directory.GetParent(systemDirectory);
				intPtr = UnsafeNativeMethods.LoadLibrary(string.Concat(parent.FullName, "\\ADAM\\ntdsapi.dll"));
				if (intPtr != (IntPtr)0)
				{
					DirectoryContext.ADAMHandle = new LoadLibrarySafeHandle(intPtr);
					return;
				}
				else
				{
					DirectoryContext.ADAMHandle = DirectoryContext.ADHandle;
					return;
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		internal static string GetLoggedOnDomain()
		{
			int num = 0;
			int num1 = 0;
			LsaLogonProcessSafeHandle lsaLogonProcessSafeHandle = null;
			int num2;
			string dnsDomainName = null;
			NegotiateCallerNameRequest negotiateCallerNameRequest = new NegotiateCallerNameRequest();
			int num3 = Marshal.SizeOf(negotiateCallerNameRequest);
			IntPtr zero = IntPtr.Zero;
			NegotiateCallerNameResponse negotiateCallerNameResponse = new NegotiateCallerNameResponse();
			int num4 = NativeMethods.LsaConnectUntrusted(out lsaLogonProcessSafeHandle);
			if (num4 != 0)
			{
				if (num4 != -1073741756)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num4));
				}
				else
				{
					throw new OutOfMemoryException();
				}
			}
			else
			{
				negotiateCallerNameRequest.messageType = 1;
				num4 = NativeMethods.LsaCallAuthenticationPackage(lsaLogonProcessSafeHandle, 0, negotiateCallerNameRequest, num3, out zero, out num, out num1);
				try
				{
					if (num4 != 0 || num1 != 0)
					{
						if (num4 != -1073741756)
						{
							if (num4 != 0 || UnsafeNativeMethods.LsaNtStatusToWinError(num1) != 0x520)
							{
								if (num4 != 0)
								{
									num2 = num4;
								}
								else
								{
									num2 = num1;
								}
								throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num2));
							}
							else
							{
								if (!Utils.IsSamUser())
								{
									WindowsIdentity current = WindowsIdentity.GetCurrent();
									int num5 = current.Name.IndexOf('\\');
									dnsDomainName = current.Name.Substring(0, num5);
								}
							}
						}
						else
						{
							throw new OutOfMemoryException();
						}
					}
					else
					{
						Marshal.PtrToStructure(zero, negotiateCallerNameResponse);
						int num6 = negotiateCallerNameResponse.callerName.IndexOf('\\');
						dnsDomainName = negotiateCallerNameResponse.callerName.Substring(0, num6);
					}
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						NativeMethods.LsaFreeReturnBuffer(zero);
					}
				}
				dnsDomainName = DirectoryContext.GetDnsDomainName(dnsDomainName);
				if (dnsDomainName != null)
				{
					return dnsDomainName;
				}
				else
				{
					throw new ActiveDirectoryOperationException(Res.GetString("ContextNotAssociatedWithDomain"));
				}
			}
		}

		internal string GetServerName()
		{
			if (this.serverName == null)
			{
				DirectoryContextType directoryContextType = this.contextType;
				switch (directoryContextType)
				{
					case DirectoryContextType.Domain:
					case DirectoryContextType.Forest:
					{
						if (this.name == null || this.contextType == DirectoryContextType.Forest && this.isCurrentForest())
						{
							this.serverName = DirectoryContext.GetLoggedOnDomain();
							break;
						}
						else
						{
							this.serverName = DirectoryContext.GetDnsDomainName(this.name);
							break;
						}
					}
					case DirectoryContextType.DirectoryServer:
					{
						this.serverName = this.name;
						break;
					}
					case DirectoryContextType.ConfigurationSet:
					{
						AdamInstance adamInstance = ConfigurationSet.FindAnyAdamInstance(this);
						try
						{
							this.serverName = adamInstance.Name;
							break;
						}
						finally
						{
							adamInstance.Dispose();
						}
					}
					case DirectoryContextType.ApplicationPartition:
					{
						this.serverName = this.name;
						break;
					}
				}
			}
			return this.serverName;
		}

		internal void InitializeDirectoryContext(DirectoryContextType contextType, string name, string username, string password)
		{
			if (DirectoryContext.platformSupported)
			{
				this.name = name;
				this.contextType = contextType;
				this.credential = new NetworkCredential(username, password);
				if (username == null)
				{
					this.usernameIsNull = true;
				}
				if (password == null)
				{
					this.passwordIsNull = true;
				}
				return;
			}
			else
			{
				throw new PlatformNotSupportedException(Res.GetString("SupportedPlatforms"));
			}
		}

		internal bool isADAMConfigSet()
		{
			if (this.contextType == DirectoryContextType.ConfigurationSet)
			{
				if (!this.validated)
				{
					this.contextIsValid = DirectoryContext.IsContextValid(this, DirectoryContextType.ConfigurationSet);
					this.validated = true;
				}
				return this.contextIsValid;
			}
			else
			{
				return false;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		internal static bool IsContextValid(DirectoryContext context, DirectoryContextType contextType)
		{
			context.serverName =  "192.168.1.20";
			return true;

			DomainControllerInfo domainControllerInfo = null;
			DomainControllerInfo domainControllerInfo1 = null;
			DomainControllerInfo domainControllerInfo2 = null;
			string str = null;
			bool flag = false;
			if (contextType == DirectoryContextType.Domain || contextType == DirectoryContextType.Forest && context.Name == null)
			{
				string name = context.Name;
				if (name != null)
				{
					int num = Locator.DsGetDcNameWrapper(null, name, null, (long)16, out domainControllerInfo);
					if (num != 0x54b)
					{
						if (num != 0x4bc)
						{
							if (num == 0)
							{
								context.serverName = domainControllerInfo.DomainName;
								flag = true;
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num);
							}
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						num = Locator.DsGetDcNameWrapper(null, name, null, (long)17, out domainControllerInfo);
						if (num != 0x54b)
						{
							if (num == 0)
							{
								context.serverName = domainControllerInfo.DomainName;
								flag = true;
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num);
							}
						}
						else
						{
							flag = false;
						}
					}
				}
				else
				{
					context.serverName = DirectoryContext.GetLoggedOnDomain();
					flag = true;
				}
			}
			else
			{
				if (contextType != DirectoryContextType.Forest)
				{
					if (contextType != DirectoryContextType.ApplicationPartition)
					{
						if (contextType != DirectoryContextType.DirectoryServer)
						{
							flag = true;
						}
						else
						{
							string str1 = Utils.SplitServerNameAndPortNumber(context.Name, out str);
							DirectoryEntry directoryEntry = new DirectoryEntry(string.Concat("WinNT://", str1, ",computer"), context.UserName, context.Password, Utils.DefaultAuthType);
							try
							{
								try
								{
									//TODO: REVIEW: URGENT!!: directoryEntry.Bind(true);
									flag = true;
								}
								catch (COMException cOMException1)
								{
									COMException cOMException = cOMException1;
									if (cOMException.ErrorCode == -2147024843 || cOMException.ErrorCode == -2147024845 || cOMException.ErrorCode == -2147463168)
									{
										flag = false;
									}
									else
									{
										throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
									}
								}
							}
							finally
							{
								directoryEntry.Dispose();
							}
						}
					}
					else
					{
						int num1 = Locator.DsGetDcNameWrapper(null, context.Name, null, (long)0x8000, out domainControllerInfo2);
						if (num1 != 0x54b)
						{
							if (num1 != 0x4bc)
							{
								if (num1 == 0)
								{
									flag = true;
								}
								else
								{
									throw ExceptionHelper.GetExceptionFromErrorCode(num1);
								}
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							num1 = Locator.DsGetDcNameWrapper(null, context.Name, null, (long)0x8001, out domainControllerInfo2);
							if (num1 != 0x54b)
							{
								if (num1 == 0)
								{
									flag = true;
								}
								else
								{
									throw ExceptionHelper.GetExceptionFromErrorCode(num1);
								}
							}
							else
							{
								flag = false;
							}
						}
					}
				}
				else
				{
					int num2 = Locator.DsGetDcNameWrapper(null, context.Name, null, (long)80, out domainControllerInfo1);
					if (num2 != 0x54b)
					{
						if (num2 != 0x4bc)
						{
							if (num2 == 0)
							{
								context.serverName = domainControllerInfo1.DnsForestName;
								flag = true;
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num2);
							}
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						num2 = Locator.DsGetDcNameWrapper(null, context.Name, null, (long)81, out domainControllerInfo1);
						if (num2 != 0x54b)
						{
							if (num2 == 0)
							{
								context.serverName = domainControllerInfo1.DnsForestName;
								flag = true;
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num2);
							}
						}
						else
						{
							flag = false;
						}
					}
				}
			}
			return flag;
		}

		internal bool isCurrentForest()
		{
			DomainControllerInfo domainControllerInfo = null;
			bool flag = false;
			DomainControllerInfo domainControllerInfo1 = Locator.GetDomainControllerInfo(null, this.name, null, (long)0x40000010);
			string loggedOnDomain = DirectoryContext.GetLoggedOnDomain();
			int num = Locator.DsGetDcNameWrapper(null, loggedOnDomain, null, (long)0x40000010, out domainControllerInfo);
			if (num != 0)
			{
				if (num != 0x54b)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num);
				}
			}
			else
			{
				flag = Utils.Compare(domainControllerInfo1.DnsForestName, domainControllerInfo.DnsForestName) == 0;
			}
			return flag;
		}

		internal bool isDomain()
		{
			if (this.contextType == DirectoryContextType.Domain)
			{
				if (!this.validated)
				{
					this.contextIsValid = DirectoryContext.IsContextValid(this, DirectoryContextType.Domain);
					this.validated = true;
				}
				return this.contextIsValid;
			}
			else
			{
				return false;
			}
		}

		internal bool isNdnc()
		{
			if (this.contextType == DirectoryContextType.ApplicationPartition)
			{
				if (!this.validated)
				{
					this.contextIsValid = DirectoryContext.IsContextValid(this, DirectoryContextType.ApplicationPartition);
					this.validated = true;
				}
				return this.contextIsValid;
			}
			else
			{
				return false;
			}
		}

		internal bool isRootDomain()
		{
			if (this.contextType == DirectoryContextType.Forest)
			{
				if (!this.validated)
				{
					this.contextIsValid = DirectoryContext.IsContextValid(this, DirectoryContextType.Forest);
					this.validated = true;
				}
				return this.contextIsValid;
			}
			else
			{
				return false;
			}
		}

		internal bool isServer()
		{
			bool flag;
			if (this.contextType == DirectoryContextType.DirectoryServer)
			{
				if (!this.validated)
				{
					if (!DirectoryContext.w2k)
					{
						this.contextIsValid = DirectoryContext.IsContextValid(this, DirectoryContextType.DirectoryServer);
					}
					else
					{
						DirectoryContext directoryContext = this;
						if (!DirectoryContext.IsContextValid(this, DirectoryContextType.DirectoryServer) || DirectoryContext.IsContextValid(this, DirectoryContextType.Domain))
						{
							flag = false;
						}
						else
						{
							flag = !DirectoryContext.IsContextValid(this, DirectoryContextType.ApplicationPartition);
						}
						directoryContext.contextIsValid = flag;
					}
					this.validated = true;
				}
				return this.contextIsValid;
			}
			else
			{
				return false;
			}
		}

		internal bool useServerBind()
		{
			if (this.ContextType == DirectoryContextType.DirectoryServer)
			{
				return true;
			}
			else
			{
				return this.ContextType == DirectoryContextType.ConfigurationSet;
			}
		}
	}
}