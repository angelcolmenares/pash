using Microsoft.Management.PowerShellWebAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public sealed class PowwaAuthorizationManager
	{
		private readonly static int defaultMaxSessionsAllowedPerUser;

		private readonly static PowwaAuthorizationManager instance;

		private int userSessionsLimit;

		internal IActiveDirectoryHelper activeDirectoryHelper;

		private int invalidRules;

		public static PowwaAuthorizationManager Instance
		{
			get
			{
				return PowwaAuthorizationManager.instance;
			}
		}

		public int UserSessionsLimit
		{
			get
			{
				return this.userSessionsLimit;
			}
			set
			{
				if (value > 0)
				{
					this.userSessionsLimit = value;
				}
			}
		}

		static PowwaAuthorizationManager()
		{
			PowwaAuthorizationManager.defaultMaxSessionsAllowedPerUser = 3;
			PowwaAuthorizationManager.instance = new PowwaAuthorizationManager();
		}

		private PowwaAuthorizationManager()
		{
			this.userSessionsLimit = PowwaAuthorizationManager.defaultMaxSessionsAllowedPerUser;
			this.activeDirectoryHelper = new ActiveDirectoryHelper();
		}

		private bool AccessCheck(string domainUserName, string password)
		{
			string str;
			string str1;
			bool flag;
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = domainUserName.IndexOf('\\');
				if (0 > num || num >= domainUserName.Length - 1)
				{
					str = null;
					str1 = domainUserName;
				}
				else
				{
					str = domainUserName.Substring(0, num);
					str1 = domainUserName.Substring(num + 1);
				}
				flag = NativeMethods.LogonUser(str1, str, password, 3, 0, ref zero);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(zero);
				}
			}
			return flag;
		}

		internal bool AuthenticateUser(string userName, string password)
		{
			bool flag;
			string statusSuccess;
			PowwaEvents.PowwaEVENT_AUTHENTICATION_START(userName);
			bool flag1 = false;
			try
			{
				try
				{
					bool flag2 = this.AccessCheck(userName, password);
					flag1 = flag2;
					flag = flag2;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					PowwaEvents.PowwaEVENT_DEBUG_LOG0(exception.Message);
					bool flag3 = false;
					flag1 = flag3;
					flag = flag3;
				}
			}
			finally
			{
				string str = userName;
				if (flag1)
				{
					statusSuccess = Resources.Status_Success;
				}
				else
				{
					statusSuccess = Resources.Status_Failure;
				}
				PowwaEvents.PowwaEVENT_AUTHENTICATION_STOP(str, statusSuccess);
			}
			return flag;
		}

		internal bool AuthorizeSession(string userName, string computerName, Uri connectionUri, string configuration)
		{
			bool flag;
			object obj;
			string statusSuccess;
			PowwaEvents.PowwaEVENT_GATEWAY_AUTHORIZATION_START(userName);
			bool length = false;
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.TestRuleInvalidRule += new EventHandler<TestRuleInvalidRuleEventArgs>(this.OnInvalidRule);
			try
			{
				ArrayList arrayLists = new ArrayList();
				SortedList<int, PswaAuthorizationRule> nums = PswaAuthorizationRuleManager.Instance.LoadFromFile(arrayLists);
				this.CheckLoadError(arrayLists);
				if (nums != null)
				{
					PswaAuthorizationRule[] array = nums.Values.ToArray<PswaAuthorizationRule>();
					this.invalidRules = 0;
					if (computerName == null)
					{
						length = (int)PswaAuthorizationRuleManager.Instance.TestRule(array, userName, connectionUri, configuration, false, MatchingWildcard.None).Length > 0;
					}
					else
					{
						length = (int)PswaAuthorizationRuleManager.Instance.TestRule(array, userName, computerName, configuration, false, MatchingWildcard.None).Length > 0;
					}
					if (length || this.invalidRules <= 0)
					{
						flag = length;
					}
					else
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						string str = "Test-PswaAuthorizationRule -UserName '{0}' -{1} '{2}' -ConfigurationName '{3}'";
						object[] objArray = new object[4];
						objArray[0] = userName;
						object[] objArray1 = objArray;
						int num = 1;
						if (computerName != null)
						{
							obj = "ComputerName";
						}
						else
						{
							obj = "ConnectionUri";
						}
						objArray1[num] = obj;
						objArray[2] = computerName;
						objArray[3] = configuration;
						string str1 = string.Format(invariantCulture, str, objArray);
						PowwaEvents.PowwaEVENT_AUTHORIZATION_FAILURE_INVALID_RULES(str1);
						throw PowwaException.CreateLogOnFailureException(Resources.GatewayAuthorizationFailureInvalidRules);
					}
				}
				else
				{
					bool flag1 = false;
					length = flag1;
					flag = flag1;
				}
			}
			finally
			{
				PswaAuthorizationRuleManager pswaAuthorizationRuleManager = PswaAuthorizationRuleManager.Instance;
				pswaAuthorizationRuleManager.TestRuleInvalidRule -= new EventHandler<TestRuleInvalidRuleEventArgs>(this.OnInvalidRule);
				string str2 = userName;
				if (length)
				{
					statusSuccess = Resources.Status_Success;
				}
				else
				{
					statusSuccess = Resources.Status_Failure;
				}
				PowwaEvents.PowwaEVENT_GATEWAY_AUTHORIZATION_STOP(str2, statusSuccess);
			}
			return flag;
		}

		private void CheckLoadError(ArrayList loadError)
		{
			if (loadError.Count <= 0)
			{
				return;
			}
			else
			{
				DataFileLoadError item = (DataFileLoadError)loadError[0];
				if (item.status != DataFileLoadError.ErrorStatus.Warning)
				{
					if (item.status != DataFileLoadError.ErrorStatus.Error)
					{
						throw new Exception(string.Empty);
					}
					else
					{
						throw new Exception(item.exception.Message);
					}
				}
				else
				{
					throw new Exception(item.message);
				}
			}
		}

		public void CheckLogOnCredential(string userName, string password, string computerName, Uri connectionUri, string configuration, string sourceIPAddressRemoteAddr, string sourceIPAddressHttpXForwardedFor)
		{
			string statusSuccess;
			string originalString;
			if (computerName != null ^ connectionUri != null)
			{
				if (this.AuthenticateUser(userName, password))
				{
					if (this.AuthorizeSession(userName, computerName, connectionUri, configuration))
					{
						bool flag = this.CheckUserSessionLimit(userName);
						string str = userName;
						if (flag)
						{
							statusSuccess = Resources.Status_Success;
						}
						else
						{
							statusSuccess = Resources.Status_Failure;
						}
						PowwaEvents.PowwaEVENT_SESSION_LIMIT_CHECK(str, statusSuccess);
						if (flag)
						{
							return;
						}
						else
						{
							PowwaEvents.PowwaEVENT_SESSION_LIMIT_REACHED(userName);
							object[] objArray = new object[1];
							objArray[0] = userName;
							throw PowwaException.CreateLogOnFailureException(string.Format(CultureInfo.CurrentCulture, Resources.UserActiveSessionLimitReached, objArray));
						}
					}
					else
					{
						string str1 = userName;
						string str2 = sourceIPAddressRemoteAddr;
						string str3 = sourceIPAddressHttpXForwardedFor;
						string gatewayAuthorizationFailure = Resources.GatewayAuthorizationFailure;
						if (connectionUri != null)
						{
							originalString = connectionUri.OriginalString;
						}
						else
						{
							originalString = computerName;
						}
						string str4 = configuration;
						string empty = str4;
						if (str4 == null)
						{
							empty = string.Empty;
						}
						PowwaEvents.PowwaEVENT_GATEWAY_AUTHORIZATION_FAILURE(str1, str2, str3, gatewayAuthorizationFailure, originalString, empty);
						throw PowwaException.CreateLogOnFailureException(Resources.GatewayAuthorizationFailure);
					}
				}
				else
				{
					PowwaEvents.PowwaEVENT_AUTHENTICATION_FAILURE(userName, sourceIPAddressRemoteAddr, sourceIPAddressHttpXForwardedFor, Resources.LoginFailure);
					throw PowwaException.CreateLogOnFailureException(Resources.LoginFailure);
				}
			}
			else
			{
				throw new ArgumentException("computerName and connectionUri are mutually exclusive", "computerName");
			}
		}

		internal bool CheckUserSessionLimit(string userName)
		{
			bool flag = false;
			string str = null;
			string stringSid = this.activeDirectoryHelper.ConvertAccountNameToStringSid(userName, out flag, out str);
			int userActiveSessions = PowwaSessionManager.Instance.GetUserActiveSessions(stringSid);
			return userActiveSessions < this.UserSessionsLimit;
		}

		private void OnInvalidRule(object sender, TestRuleInvalidRuleEventArgs e)
		{
			PowwaAuthorizationManager powwaAuthorizationManager = this;
			powwaAuthorizationManager.invalidRules = powwaAuthorizationManager.invalidRules + 1;
		}
	}
}