using Microsoft.ActiveDirectory;
using System;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSessionInfo
	{
		private readonly static Regex ipv6WithPortRegex;

		private string _server;

		private bool _fullyQualifiedDnsHostName;

		private bool _connectionless;

		private int _defaultPortNumber;

		private TimeSpan? _timeout;

		private ADServerType _storeType;

		private AuthType _authType;

		private PSCredential _credential;

		private ADSessionOptions _options;

		private string _serverNameOnly;

		private int? _appendedPort;

		public AuthType AuthType
		{
			get
			{
				return this._authType;
			}
			set
			{
				if (value < AuthType.Anonymous || value > AuthType.Kerberos)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(AuthType));
				}
				else
				{
					this._authType = value;
					return;
				}
			}
		}

		internal bool ConnectedToGC
		{
			get
			{
				if (this.EffectivePortNumber == LdapConstants.LDAP_GC_PORT)
				{
					return true;
				}
				else
				{
					return this.EffectivePortNumber == LdapConstants.LDAP_SSL_GC_PORT;
				}
			}
		}

		internal bool Connectionless
		{
			get
			{
				return this._connectionless;
			}
			private set
			{
				this._connectionless = value;
			}
		}

		public PSCredential Credential
		{
			get
			{
				return this._credential;
			}
			set
			{
				this._credential = value;
			}
		}

		internal int EffectivePortNumber
		{
			get
			{
				return this._appendedPort.GetValueOrDefault(this._defaultPortNumber);
			}
		}

		public bool FullyQualifiedDnsHostName
		{
			get
			{
				return this._fullyQualifiedDnsHostName;
			}
			private set
			{
				this._fullyQualifiedDnsHostName = value;
			}
		}

		public ADSessionOptions Options
		{
			get
			{
				return this._options;
			}
			set
			{
				this._options = value;
			}
		}

		public string Server
		{
			get
			{
				return this._server;
			}
			set
			{
				this._server = value;
				this.SplitServerNameAndPortNumber(out this._appendedPort, out this._serverNameOnly);
			}
		}

		internal string ServerNameOnly
		{
			get
			{
				return this._serverNameOnly;
			}
		}

		public ADServerType ServerType
		{
			get
			{
				return this._storeType;
			}
			set
			{
				this._storeType = value;
			}
		}

		public TimeSpan? Timeout
		{
			get
			{
				return this._timeout;
			}
			set
			{
				if (value.HasValue)
				{
					TimeSpan timeSpan = value.Value;
					if (timeSpan >= TimeSpan.Zero)
					{
						if (timeSpan.TotalSeconds <= 2147483647)
						{
							this._timeout = value;
							return;
						}
						else
						{
							throw new ArgumentException(StringResources.ExceedMax, "value");
						}
					}
					else
					{
						throw new ArgumentException(StringResources.NoNegativeTime, "value");
					}
				}
				else
				{
					this._timeout = null;
					return;
				}
			}
		}

		internal bool UsingExplicitPort
		{
			get
			{
				return this._appendedPort.HasValue;
			}
		}

		static ADSessionInfo()
		{
			ADSessionInfo.ipv6WithPortRegex = new Regex("^\\[.+\\]:[0-9]+$", RegexOptions.Compiled);
		}

		public ADSessionInfo(string server) : this()
		{
			if (server != null)
			{
				this.Server = server;
			}
		}

		internal ADSessionInfo()
		{
			this._defaultPortNumber = LdapConstants.LDAP_PORT;
			this._timeout = null;
			this._authType = AuthType.Negotiate;
			this._options = new ADSessionOptions();
			this._appendedPort = null;
			this._options.ReferralChasing = new ReferralChasingOptions?(ReferralChasingOptions.None);
			this._options.AutoReconnect = new bool?(true);
		}

		public ADSessionInfo Copy()
		{
			ADSessionInfo aDSessionInfo = new ADSessionInfo(this._server);
			aDSessionInfo._fullyQualifiedDnsHostName = this._fullyQualifiedDnsHostName;
			aDSessionInfo._connectionless = this._connectionless;
			aDSessionInfo._defaultPortNumber = this._defaultPortNumber;
			aDSessionInfo._timeout = this._timeout;
			aDSessionInfo._authType = this._authType;
			aDSessionInfo._appendedPort = this._appendedPort;
			aDSessionInfo._serverNameOnly = this._serverNameOnly;
			if (this._credential != null)
			{
				aDSessionInfo._credential = new PSCredential(this._credential.UserName, this._credential.Password.Copy());
			}
			if (this._options != null)
			{
				aDSessionInfo._options = this._options.Copy();
			}
			return aDSessionInfo;
		}

		public static bool MatchConnectionState(ADSessionInfo info1, ADSessionInfo info2, bool ignoreLocatorFlags)
		{
			bool valueOrDefault;
			TimeSpan? timeout = info1.Timeout;
			TimeSpan? nullable = info2.Timeout;
			if (timeout.HasValue != nullable.HasValue)
			{
				valueOrDefault = false;
			}
			else
			{
				if (!timeout.HasValue)
				{
					valueOrDefault = true;
				}
				else
				{
					valueOrDefault = timeout.GetValueOrDefault() == nullable.GetValueOrDefault();
				}
			}
			if (!valueOrDefault || info1.AuthType != info2.AuthType)
			{
				return false;
			}
			else
			{
				return ADSessionOptions.MatchConnectionState(info1.Options, info2.Options, ignoreLocatorFlags);
			}
		}

		public void SetDefaultPort(int port)
		{
			if (port > 0)
			{
				this._defaultPortNumber = port;
				return;
			}
			else
			{
				throw new ArgumentOutOfRangeException("port");
			}
		}

		public void SetEffectivePort(int port)
		{
			if (port > 0)
			{
				if (this._appendedPort.HasValue)
				{
					this._server = this._serverNameOnly;
				}
				this._appendedPort = null;
				this._defaultPortNumber = port;
				return;
			}
			else
			{
				throw new ArgumentOutOfRangeException("port");
			}
		}

		private void SplitServerNameAndPortNumber(out int? port, out string serverName)
		{
			IPAddress pAddress = null;
			if (string.IsNullOrEmpty(this._server))
			{
				serverName = null;
				port = null;
			}
			else
			{
				if (!IPAddress.TryParse(this._server, out pAddress) || pAddress.AddressFamily != AddressFamily.InterNetworkV6 || ADSessionInfo.ipv6WithPortRegex.IsMatch(this._server))
				{
					int num = this._server.LastIndexOf(':');
					if (num == -1)
					{
						serverName = this._server;
						port = null;
						return;
					}
					else
					{
						try
						{
							serverName = this._server.Substring(0, num);
							port = new int?(Convert.ToInt32(this._server.Substring(num + 1), NumberFormatInfo.InvariantInfo));
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							throw new ArgumentException(this._server, exception);
						}
					}
				}
				else
				{
					serverName = string.Concat("[", pAddress.ToString(), "]");
					port = null;
					return;
				}
			}
		}
	}
}