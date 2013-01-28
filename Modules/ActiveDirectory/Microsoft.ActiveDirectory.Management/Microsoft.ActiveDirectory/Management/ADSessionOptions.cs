using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSessionOptions
	{
		private Hashtable _options;

		public bool? AutoReconnect
		{
			get
			{
				return (bool?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_AUTO_RECONNECT];
			}
			set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_AUTO_RECONNECT] = value;
			}
		}

		public int Count
		{
			get
			{
				return this._options.Count;
			}
		}

		public string DomainName
		{
			get
			{
				return (string)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_DNSDOMAIN_NAME];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_DNSDOMAIN_NAME] = value;
			}
		}

		public string HostName
		{
			get
			{
				return (string)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_HOST_NAME];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_HOST_NAME] = value;
			}
		}

		public ADLocatorFlags? LocatorFlag
		{
			get
			{
				return (ADLocatorFlags?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_GETDSNAME_FLAGS];
			}
			set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_GETDSNAME_FLAGS] = value;
			}
		}

		public int? ProtocolVersion
		{
			get
			{
				return (int?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_VERSION];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_VERSION] = value;
			}
		}

		public ReferralChasingOptions? ReferralChasing
		{
			get
			{
				return (ReferralChasingOptions?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_REFERRALS];
			}
			set
			{
				ReferralChasingOptions? nullable;
				bool hasValue;
				ReferralChasingOptions? nullable1 = value;
				if (nullable1.HasValue)
				{
					nullable = new ReferralChasingOptions?((ReferralChasingOptions)((int)nullable1.GetValueOrDefault() & -97));
				}
				else
				{
					ReferralChasingOptions? nullable2 = null;
					nullable = nullable2;
				}
				ReferralChasingOptions? nullable3 = nullable;
				if (nullable3.GetValueOrDefault() != ReferralChasingOptions.None)
				{
					hasValue = true;
				}
				else
				{
					hasValue = !nullable3.HasValue;
				}
				if (!hasValue)
				{
					this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_REFERRALS] = value;
					return;
				}
				else
				{
					throw new InvalidEnumArgumentException("value", (int)value.Value, typeof(ReferralChasingOptions));
				}
			}
		}

		public bool? Sealing
		{
			get
			{
				return (bool?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_ENCRYPT];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_ENCRYPT] = value;
			}
		}

		public bool? SecureSocketLayer
		{
			get
			{
				return (bool?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_SSL];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_SSL] = value;
			}
		}

		public bool? Signing
		{
			get
			{
				return (bool?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_SIGN];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_SIGN] = value;
			}
		}

		public int? SspiFlag
		{
			get
			{
				return (int?)this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_SSPI_FLAGS];
			}
			private set
			{
				this._options[(object)ADStoreAccess.LdapSessionOption.LDAP_OPT_SSPI_FLAGS] = value;
			}
		}

		public ADSessionOptions()
		{
			this._options = new Hashtable();
		}

		public ADSessionOptions Copy()
		{
			ADSessionOptions aDSessionOption = new ADSessionOptions();
			foreach (DictionaryEntry _option in this._options)
			{
				aDSessionOption._options.Add(_option.Key, _option.Value);
			}
			return aDSessionOption;
		}

		public void CopyValuesTo(ADSessionOptions targetSessionOption)
		{
			bool? autoReconnect = this.AutoReconnect;
			if (autoReconnect.HasValue)
			{
				targetSessionOption.AutoReconnect = this.AutoReconnect;
			}
			ADLocatorFlags? locatorFlag = this.LocatorFlag;
			if (locatorFlag.HasValue)
			{
				ADLocatorFlags? nullable = targetSessionOption.LocatorFlag;
				ADLocatorFlags? locatorFlag1 = this.LocatorFlag;
				ADLocatorFlags? nullable1 = this.LocatorFlag;
				targetSessionOption.LocatorFlag = new ADLocatorFlags?(nullable.GetValueOrDefault(locatorFlag1.Value) | nullable1.Value);
			}
			ReferralChasingOptions? referralChasing = this.ReferralChasing;
			if (referralChasing.HasValue)
			{
				targetSessionOption.ReferralChasing = this.ReferralChasing;
			}
		}

		public static bool MatchConnectionState(ADSessionOptions info1, ADSessionOptions info2, bool ignoreLocatorFlags)
		{
			bool hasValue;
			bool flag;
			if (info1 != null || info2 != null)
			{
				if (info1 == null || info2 == null)
				{
					return false;
				}
				else
				{
					ReferralChasingOptions? referralChasing = info1.ReferralChasing;
					ReferralChasingOptions? nullable = info2.ReferralChasing;
					if (referralChasing.GetValueOrDefault() != nullable.GetValueOrDefault())
					{
						hasValue = false;
					}
					else
					{
						hasValue = referralChasing.HasValue == nullable.HasValue;
					}
					if (hasValue)
					{
						bool? autoReconnect = info1.AutoReconnect;
						bool? autoReconnect1 = info2.AutoReconnect;
						if (autoReconnect.GetValueOrDefault() != autoReconnect1.GetValueOrDefault())
						{
							flag = false;
						}
						else
						{
							flag = autoReconnect.HasValue == autoReconnect1.HasValue;
						}
						if (flag)
						{
							if (ignoreLocatorFlags)
							{
								return true;
							}
							else
							{
								ADLocatorFlags? locatorFlag = info1.LocatorFlag;
								ADLocatorFlags? locatorFlag1 = info2.LocatorFlag;
								if (locatorFlag.GetValueOrDefault() != locatorFlag1.GetValueOrDefault())
								{
									return false;
								}
								else
								{
									return locatorFlag.HasValue == locatorFlag1.HasValue;
								}
							}
						}
					}
					return false;
				}
			}
			else
			{
				return true;
			}
		}
	}
}