using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADReplicationUtil
	{
		internal static void ToExtendedAttributeMetadataValue(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = directoryObj["sourceXmlAttribute"].Value as string;
			if (!string.Equals("msDS-ReplAttributeMetaData", value, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals("msDS-ReplValueMetaData", value, StringComparison.OrdinalIgnoreCase))
				{
					AttributeConverters.ToExtendedObject(extendedAttribute, directoryAttributes, userObj, directoryObj, cmdletSessionInfo);
				}
				return;
			}
			else
			{
				string str = directoryObj["pszAttributeName"].Value as string;
				if (string.IsNullOrEmpty(str))
				{
					userObj.SetValue(extendedAttribute, null);
					return;
				}
				else
				{
					userObj.SetValue(extendedAttribute, directoryObj[str]);
					return;
				}
			}
		}

		internal static void ToExtendedIsLinkValue(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = directoryObj[directoryAttributes[0]].Value as string;
			if (!string.Equals("msDS-ReplAttributeMetaData", value, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals("msDS-ReplValueMetaData", value, StringComparison.OrdinalIgnoreCase))
				{
					userObj.Add(extendedAttribute, true);
				}
				return;
			}
			else
			{
				userObj.Add(extendedAttribute, false);
				return;
			}
		}

		internal static void ToExtendedReplicationFailureType(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = directoryObj[directoryAttributes[0]].Value as string;
			if (!string.Equals("msDS-ReplConnectionFailures", value, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals("msDS-ReplLinkFailures", value, StringComparison.OrdinalIgnoreCase))
				{
					userObj.Add(extendedAttribute, ADReplicationFailureType.Link);
				}
				return;
			}
			else
			{
				userObj.Add(extendedAttribute, ADReplicationFailureType.Connection);
				return;
			}
		}

		internal static void ToExtendedReplicationPartnerType(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = directoryObj[directoryAttributes[0]].Value as string;
			if (!string.Equals("msDS-NCReplInboundNeighbors", value, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals("msDS-NCReplOutboundNeighbors", value, StringComparison.OrdinalIgnoreCase))
				{
					userObj.Add(extendedAttribute, ADPartnerType.Outbound);
				}
				return;
			}
			else
			{
				userObj.Add(extendedAttribute, ADPartnerType.Inbound);
				return;
			}
		}

		internal static void ToExtendedServerFromSessionInfo(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			userObj.Add(extendedAttribute, cmdletSessionInfo.ADRootDSE.DNSHostName);
		}

		internal static void ToExtendedTransportTypeFromDrsOptions(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool hasValue;
			bool flag;
			AttributeConverters.ToExtendedFlagFromInt(128, false, extendedAttribute, directoryAttributes, userObj, directoryObj, cmdletSessionInfo);
			bool? value = (bool?)(userObj[extendedAttribute].Value as bool?);
			bool? nullable = value;
			if (!nullable.GetValueOrDefault())
			{
				hasValue = false;
			}
			else
			{
				hasValue = nullable.HasValue;
			}
			if (!hasValue)
			{
				bool? nullable1 = value;
				if (nullable1.GetValueOrDefault())
				{
					flag = false;
				}
				else
				{
					flag = nullable1.HasValue;
				}
				if (flag)
				{
					userObj.SetValue(extendedAttribute, ADInterSiteTransportProtocolType.IP);
				}
				return;
			}
			else
			{
				userObj.SetValue(extendedAttribute, ADInterSiteTransportProtocolType.SMTP);
				return;
			}
		}

		internal static class DrsOptions
		{
			internal const int DRS_ASYNC_OP = 1;

			internal const int DRS_GETCHG_CHECK = 2;

			internal const int DRS_UPDATE_NOTIFICATION = 2;

			internal const int DRS_ADD_REF = 4;

			internal const int DRS_SYNC_ALL = 8;

			internal const int DRS_DEL_REF = 8;

			internal const int DRS_WRIT_REP = 16;

			internal const int DRS_INIT_SYNC = 32;

			internal const int DRS_PER_SYNC = 64;

			internal const int DRS_MAIL_REP = 128;

			internal const int DRS_ASYNC_REP = 0x100;

			internal const int DRS_IGNORE_ERROR = 0x100;

			internal const int DRS_TWOWAY_SYNC = 0x200;

			internal const int DRS_CRITICAL_ONLY = 0x400;

			internal const int DRS_GET_ANC = 0x800;

			internal const int DRS_GET_NC_SIZE = 0x1000;

			internal const int DRS_LOCAL_ONLY = 0x1000;

			internal const int DRS_NONGC_RO_REP = 0x2000;

			internal const int DRS_SYNC_BYNAME = 0x4000;

			internal const int DRS_REF_OK = 0x4000;

			internal const int DRS_FULL_SYNC_NOW = 0x8000;

			internal const int DRS_NO_SOURCE = 0x8000;

			internal const int DRS_FULL_SYNC_IN_PROGRESS = 0x10000;

			internal const int DRS_FULL_SYNC_PACKET = 0x20000;

			internal const int DRS_SYNC_REQUEUE = 0x40000;

			internal const int DRS_SYNC_URGENT = 0x80000;

			internal const int DRS_REF_GCSPN = 0x100000;

			internal const int DRS_NO_DISCARD = 0x100000;

			internal const int DRS_NEVER_SYNCED = 0x200000;

			internal const int DRS_SPECIAL_SECRET_PROCESSING = 0x400000;

			internal const int DRS_INIT_SYNC_NOW = 0x800000;

			internal const int DRS_PREEMPTED = 0x1000000;

			internal const int DRS_SYNC_FORCED = 0x2000000;

			internal const int DRS_DISABLE_AUTO_SYNC = 0x4000000;

			internal const int DRS_DISABLE_PERIODIC_SYNC = 0x8000000;

			internal const int DRS_USE_COMPRESSION = 0x10000000;

			internal const int DRS_NEVER_NOTIFY = 0x20000000;

			internal const int DRS_SYNC_PAS = 0x20000000;

			internal const int DRS_GET_ALL_GROUP_MEMBERSHIP = -2147483648;

		}
	}
}