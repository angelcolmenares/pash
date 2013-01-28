using System;
using System.Collections;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADRootDSE : ADEntity
	{
		private static Hashtable _supportedCapabilitiesTable;

		private static Hashtable _supportedControlTable;

		private ADServerType? _serverType;

		public string ConfigurationNamingContext
		{
			get
			{
				return (string)base.GetValue("configurationNamingContext");
			}
		}

		public DateTime? CurrentTime
		{
			get
			{
				string value = (string)base.GetValue("currentTime");
				if (value != null)
				{
					return new DateTime?(ADTypeConverter.ParseDateTimeValue(value, ADAttributeSyntax.GeneralizedTime));
				}
				else
				{
					DateTime? nullable = null;
					return nullable;
				}
			}
		}

		public string DefaultNamingContext
		{
			get
			{
				return (string)base.GetValue("defaultNamingContext");
			}
		}

		public string DNSHostName
		{
			get
			{
				return (string)base.GetValue("dnsHostName");
			}
		}

		public ADDomainControllerMode DomainControllerFunctionality
		{
			get
			{
				string value = (string)base.GetValue("domainControllerFunctionality");
				if (value != null)
				{
					int num = int.Parse(value, NumberFormatInfo.InvariantInfo);
					return (ADDomainControllerMode)num;
				}
				else
				{
					return ADDomainControllerMode.Windows2000;
				}
			}
		}

		public ADDomainMode DomainFunctionality
		{
			get
			{
				string value = (string)base.GetValue("domainFunctionality");
				if (value != null)
				{
					int num = int.Parse(value, NumberFormatInfo.InvariantInfo);
					return (ADDomainMode)num;
				}
				else
				{
					return ADDomainMode.Windows2000Domain;
				}
			}
		}

		public string DSServiceName
		{
			get
			{
				return (string)base.GetValue("dsServiceName");
			}
		}

		public ADForestMode ForestFunctionality
		{
			get
			{
				string value = (string)base.GetValue("forestFunctionality");
				if (value != null)
				{
					int num = int.Parse(value, NumberFormatInfo.InvariantInfo);
					return (ADForestMode)num;
				}
				else
				{
					return ADForestMode.Windows2000Forest;
				}
			}
		}

		public bool? GlobalCatalogReady
		{
			get
			{
				string value = (string)base.GetValue("isGlobalCatalogReady");
				if (value != null)
				{
					if (string.Compare(value, "TRUE", StringComparison.OrdinalIgnoreCase) != 0)
					{
						return new bool?(false);
					}
					else
					{
						return new bool?(true);
					}
				}
				else
				{
					bool? nullable = null;
					return nullable;
				}
			}
		}

		public long? HighestCommittedUSN
		{
			get
			{
				string value = (string)base.GetValue("highestCommittedUSN");
				if (value != null)
				{
					return new long?(long.Parse(value, NumberFormatInfo.InvariantInfo));
				}
				else
				{
					long? nullable = null;
					return nullable;
				}
			}
		}

		public string LDAPServiceName
		{
			get
			{
				return (string)base.GetValue("ldapServiceName");
			}
		}

		public string[] NamingContexts
		{
			get
			{
				string[] item = null;
				ADPropertyValueCollection aDPropertyValueCollection = base["namingContexts"];
				if (aDPropertyValueCollection != null)
				{
					item = new string[aDPropertyValueCollection.Count];
					for (int i = 0; i < aDPropertyValueCollection.Count; i++)
					{
						item[i] = (string)aDPropertyValueCollection[i];
					}
				}
				return item;
			}
		}

		internal int? PortLDAP
		{
			get
			{
				if (this.ServerType != ADServerType.ADLDS)
				{
					return new int?(LdapConstants.LDAP_PORT);
				}
				else
				{
					string value = (string)base.GetValue("msDS-PortLDAP");
					if (value != null)
					{
						return new int?(int.Parse(value, NumberFormatInfo.InvariantInfo));
					}
					else
					{
						int? nullable = null;
						return nullable;
					}
				}
			}
		}

		public string RootDomainNamingContext
		{
			get
			{
				return (string)base.GetValue("rootDomainNamingContext");
			}
		}

		public string SchemaNamingContext
		{
			get
			{
				return (string)base.GetValue("schemaNamingContext");
			}
		}

		public string ServerName
		{
			get
			{
				return (string)base.GetValue("serverName");
			}
		}

		internal ADServerType ServerType
		{
			get
			{
				if (!this._serverType.HasValue)
				{
					this._serverType = new ADServerType?(ADServerType.Unknown);
					ADPropertyValueCollection item = base["supportedCapabilities"];
					if (item != null)
					{
						if (!item.Contains("1.2.840.113556.1.4.1851"))
						{
							if (item.Contains("1.2.840.113556.1.4.800"))
							{
								this._serverType = new ADServerType?(ADServerType.ADDS);
							}
						}
						else
						{
							this._serverType = new ADServerType?(ADServerType.ADLDS);
						}
					}
				}
				return this._serverType.Value;
			}
		}

		public string SubSchemaSubEntry
		{
			get
			{
				return (string)base.GetValue("subschemaSubentry");
			}
		}

		public ADObjectIdentifier[] SupportedCapabilities
		{
			get
			{
				ADObjectIdentifier[] aDObjectIdentifier = null;
				ADPropertyValueCollection item = base["supportedCapabilities"];
				if (item != null)
				{
					aDObjectIdentifier = new ADObjectIdentifier[item.Count];
					for (int i = 0; i < item.Count; i++)
					{
						aDObjectIdentifier[i] = new ADObjectIdentifier((string)item[i], (string)ADRootDSE._supportedCapabilitiesTable[item[i]]);
					}
				}
				return aDObjectIdentifier;
			}
		}

		public ADObjectIdentifier[] SupportedControl
		{
			get
			{
				ADObjectIdentifier[] aDObjectIdentifier = null;
				ADPropertyValueCollection item = base["supportedControl"];
				if (item != null)
				{
					aDObjectIdentifier = new ADObjectIdentifier[item.Count];
					for (int i = 0; i < item.Count; i++)
					{
						aDObjectIdentifier[i] = new ADObjectIdentifier((string)item[i], (string)ADRootDSE._supportedControlTable[item[i]]);
					}
				}
				return aDObjectIdentifier;
			}
		}

		public string[] SupportedLDAPPolicies
		{
			get
			{
				string[] item = null;
				ADPropertyValueCollection aDPropertyValueCollection = base["supportedLDAPPolicies"];
				if (aDPropertyValueCollection != null)
				{
					item = new string[aDPropertyValueCollection.Count];
					for (int i = 0; i < aDPropertyValueCollection.Count; i++)
					{
						item[i] = (string)aDPropertyValueCollection[i];
					}
				}
				return item;
			}
		}

		public int[] SupportedLDAPVersion
		{
			get
			{
				int[] numArray = null;
				ADPropertyValueCollection item = base["supportedLDAPVersion"];
				if (item != null)
				{
					numArray = new int[item.Count];
					for (int i = 0; i < item.Count; i++)
					{
						numArray[i] = int.Parse((string)item[i], NumberFormatInfo.InvariantInfo);
					}
				}
				return numArray;
			}
		}

		public string[] SupportedSASLMechanisms
		{
			get
			{
				string[] item = null;
				ADPropertyValueCollection aDPropertyValueCollection = base["supportedSASLMechanisms"];
				if (aDPropertyValueCollection != null)
				{
					item = new string[aDPropertyValueCollection.Count];
					for (int i = 0; i < aDPropertyValueCollection.Count; i++)
					{
						item[i] = (string)aDPropertyValueCollection[i];
					}
				}
				return item;
			}
		}

		public bool? Synchronized
		{
			get
			{
				string value = (string)base.GetValue("isSynchronized");
				if (value != null)
				{
					if (string.Compare(value, "TRUE", StringComparison.OrdinalIgnoreCase) != 0)
					{
						return new bool?(false);
					}
					else
					{
						return new bool?(true);
					}
				}
				else
				{
					bool? nullable = null;
					return nullable;
				}
			}
		}

		static ADRootDSE()
		{
			ADRootDSE._supportedCapabilitiesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.800", "LDAP_CAP_ACTIVE_DIRECTORY_OID");
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.1670", "LDAP_CAP_ACTIVE_DIRECTORY_V51_OID");
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.1791", "LDAP_CAP_ACTIVE_DIRECTORY_LDAP_INTEG_OID");
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.1935", "LDAP_CAP_ACTIVE_DIRECTORY_V61_OID");
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.1920", "LDAP_CAP_ACTIVE_DIRECTORY_PARTIAL_SECRETS_OID");
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.1851", "LDAP_CAP_ACTIVE_DIRECTORY_ADAM_OID");
			ADRootDSE._supportedCapabilitiesTable.Add("1.2.840.113556.1.4.1880", "LDAP_CAP_ACTIVE_DIRECTORY_ADAM_DIGEST");
			ADRootDSE._supportedControlTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.319", "LDAP_PAGED_RESULT_OID_STRING");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.801", "LDAP_SERVER_SD_FLAGS_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.473", "LDAP_SERVER_SORT_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.528", "LDAP_SERVER_NOTIFICATION_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.417", "LDAP_SERVER_SHOW_DELETED_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.619", "LDAP_SERVER_LAZY_COMMIT_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.841", "LDAP_SERVER_DIRSYNC_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.529", "LDAP_SERVER_EXTENDED_DN_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.805", "LDAP_SERVER_TREE_DELETE_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.521", "LDAP_SERVER_CROSSDOM_MOVE_TARGET_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.970", "LDAP_SERVER_GET_STATS_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1338", "LDAP_SERVER_VERIFY_NAME_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.474", "LDAP_SERVER_RESP_SORT_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1339", "LDAP_SERVER_DOMAIN_SCOPE_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1340", "LDAP_SERVER_SEARCH_OPTIONS_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1413", "LDAP_SERVER_PERMISSIVE_MODIFY_OID");
			ADRootDSE._supportedControlTable.Add("2.16.840.1.113730.3.4.9", "LDAP_CONTROL_VLVREQUEST");
			ADRootDSE._supportedControlTable.Add("2.16.840.1.113730.3.4.10", "LDAP_CONTROL_VLVRESPONSE");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1504", "LDAP_SERVER_ASQ_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1852", "LDAP_SERVER_QUOTA_CONTROL_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.802", "LDAP_SERVER_RANGE_OPTION_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1907", "LDAP_SERVER_SHUTDOWN_NOTIFY_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1948", "LDAP_SERVER_RANGE_RETRIEVAL_NOERR_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1974", "LDAP_SERVER_FORCE_UPDATE_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.1341", "LDAP_SERVER_RODC_DCPROMO_OID");
			ADRootDSE._supportedControlTable.Add("1.2.840.113556.1.4.2026", "LDAP_SERVER_DN_INPUT_OID");
			ADEntity.RegisterMappingTable(typeof(ADRootDSE), null);
		}

		internal ADRootDSE()
		{
			this._serverType = null;
		}

		private bool IsSupportedCapability(string ldapCapabilityOid)
		{
			bool flag = false;
			ADObjectIdentifier[] supportedCapabilities = this.SupportedCapabilities;
			for (int i = 0; i < (int)supportedCapabilities.Length; i++)
			{
				ADObjectIdentifier aDObjectIdentifier = supportedCapabilities[i];
				if (aDObjectIdentifier.Value.Equals(ldapCapabilityOid))
				{
					flag = true;
				}
			}
			return flag;
		}

		internal bool IsWindows2008AndAbove()
		{
			return this.IsSupportedCapability("1.2.840.113556.1.4.1935");
		}

		internal bool IsWritable()
		{
			return !this.IsSupportedCapability("1.2.840.113556.1.4.1920");
		}
	}
}