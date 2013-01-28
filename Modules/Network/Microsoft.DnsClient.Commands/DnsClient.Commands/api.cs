using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Net;

namespace Microsoft.DnsClient.Commands
{
	internal class api
	{
		private const QueryOptions DnsOnlyQueryFlags = QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP;

		private const QueryOptions DnsClientQueryFlags = QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS;

		public api()
		{
		}

		private static string AppendSuffix(string stub, string suffix)
		{
			string str;
			if (!stub.EndsWith(".") || !suffix.StartsWith("."))
			{
				if (stub.EndsWith(".") || suffix.StartsWith("."))
				{
					str = string.Concat(stub, suffix);
				}
				else
				{
					str = string.Concat(stub, ".", suffix);
				}
			}
			else
			{
				char[] chrArray = new char[1];
				chrArray[0] = '.';
				str = string.Concat(stub.TrimEnd(chrArray), suffix);
			}
			return str;
		}

		public static List<DnsRecord> SendDnsQuery(Cmdlet HostCmdlet, string NameQueried, RecordType DNSQueryType, string[] DNSServerAddresses, api.QueryParameters Switches)
		{
			List<string> strs = new List<string>();
			QueryOptions queryOption = QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS;
			api.ValidateParameters(Switches);
			if (Switches.DnsOnly)
			{
				queryOption = QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP;
			}
			if (Switches.CacheOnly)
			{
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption | QueryOptions.DNS_QUERY_NO_WIRE_QUERY;
				queryOption = queryOption | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH;
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
			}
			if (Switches.DnssecOk)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_DNSSEC_OK;
			}
			if (Switches.DnssecCd)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED;
			}
			if (Switches.NoHostsFile)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_NO_HOSTS_FILE;
			}
			if (Switches.LlmnrNetbiosOnly)
			{
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption | QueryOptions.DNS_QUERY_MULTICAST_ONLY;
				queryOption = queryOption | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS;
			}
			if (Switches.LlmnrFallback)
			{
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
			}
			if (Switches.LlmnrOnly)
			{
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption | QueryOptions.DNS_QUERY_MULTICAST_ONLY;
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
			}
			if (Switches.NetbiosFallback)
			{
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_TREAT_AS_FQDN | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption & (QueryOptions.DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE | QueryOptions.DNS_QUERY_USE_TCP_ONLY | QueryOptions.DNS_QUERY_NO_RECURSION | QueryOptions.DNS_QUERY_BYPASS_CACHE | QueryOptions.DNS_QUERY_NO_WIRE_QUERY | QueryOptions.DNS_QUERY_NO_LOCAL_NAME | QueryOptions.DNS_QUERY_NO_HOSTS_FILE | QueryOptions.DNS_QUERY_NO_NETBT | QueryOptions.DNS_QUERY_WIRE_ONLY | QueryOptions.DNS_QUERY_RETURN_MESSAGE | QueryOptions.DNS_QUERY_MULTICAST_ONLY | QueryOptions.DNS_QUERY_NO_MULTICAST | QueryOptions.DNS_QUERY_ADDRCONFIG | QueryOptions.DNS_QUERY_DUAL_ADDR | QueryOptions.DNS_QUERY_DONT_RESET_TTL_VALUES | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING | QueryOptions.DNS_QUERY_APPEND_MULTILABEL | QueryOptions.DNS_QUERY_RESERVED | QueryOptions.DNS_QUERY_CACHE_ONLY | QueryOptions.DNS_QUERY_ACCEPT_PARTIAL_UDP | QueryOptions.DNS_QUERY_CACHE_NO_FLAGS_MATCH | QueryOptions.DNS_QUERY_ALLOW_EMPTY_AUTH_RESP | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS | QueryOptions.DNS_QUERY_MULTICAST_VERIFY | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS | QueryOptions.DNS_QUERY_ALLOW_MULTIPLE_MESSAGES | QueryOptions.DNS_QUERY_DNSSEC_OK | QueryOptions.DNS_QUERY_DNSSEC_CHECKING_DISABLED | QueryOptions.DNS_QUERY_DNSSEC_REQUIRED | QueryOptions.DNS_QUERY_FILESERVER | QueryOptions.DNS_QUERY_MULTICAST_WAIT | QueryOptions.DNS_QUERY_USING_POLICY_TABLE | QueryOptions.DNS_QUERY_NETBIOS_ONLY | QueryOptions.DNS_QUERY_API_ANSI_UTF8);
				queryOption = queryOption | QueryOptions.DNS_QUERY_FALLBACK_NETBIOS;
			}
			if (Switches.NoIdn)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_DISABLE_IDN_ENCODING;
			}
			if (Switches.NoRecursion)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_NO_RECURSION;
			}
			if (Switches.QuickTimeout)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_USE_QUICK_TIMEOUTS;
			}
			if (Switches.TcpOnly)
			{
				queryOption = queryOption | QueryOptions.DNS_QUERY_USE_TCP_ONLY;
			}
			if (strs.Count == 0)
			{
				strs.Add(NameQueried);
			}
			if (DNSServerAddresses != null)
			{
				for (int i = 0; i < (int)DNSServerAddresses.Length; i++)
				{
					DNSServerAddresses[i] = Dns.GetHostAddresses(DNSServerAddresses[i])[0].ToString();
				}
			}
			List<DnsRecord> dnsRecords = new List<DnsRecord>();
			foreach (string str in strs)
			{
				try
				{
					if (DNSQueryType != RecordType.UNKNOWN)
					{
						dnsRecords.AddRange(win32.GetDNSRecords(str, DNSServerAddresses, queryOption, DNSQueryType));
					}
					else
					{
						dnsRecords.AddRange(win32.GetDNSRecords(str, DNSServerAddresses, queryOption | QueryOptions.DNS_QUERY_DUAL_ADDR, RecordType.AAAA));
					}
					if (HostCmdlet != null)
					{
						object[] array = dnsRecords.ToArray();
						HostCmdlet.WriteVerbose(str);
						HostCmdlet.WriteDebug(queryOption.ToString());
						object[] objArray = array;
						for (int j = 0; j < (int)objArray.Length; j++)
						{
							object obj = objArray[j];
							HostCmdlet.WriteObject(obj);
						}
					}
				}
				catch (Win32Exception win32Exception1)
				{
					Win32Exception win32Exception = win32Exception1;
					if (HostCmdlet != null)
					{
						if (win32Exception.NativeErrorCode != 0x5b4)
						{
							HostCmdlet.WriteError(new ErrorRecord(win32Exception, win32Exception.NativeErrorCode.ToString(), ErrorCategory.ResourceUnavailable, str));
						}
						else
						{
							HostCmdlet.WriteError(new ErrorRecord(win32Exception, win32Exception.NativeErrorCode.ToString(), ErrorCategory.OperationTimeout, str));
						}
					}
				}
			}
			return dnsRecords;
		}

		internal static void ValidateParameters(api.QueryParameters Parameters)
		{
			if (!Parameters.LlmnrOnly || !Parameters.LlmnrFallback && !Parameters.LlmnrNetbiosOnly && !Parameters.NetbiosFallback && !Parameters.DnsOnly && !Parameters.DnssecOk && !Parameters.DnssecCd)
			{
				if (!Parameters.LlmnrNetbiosOnly || !Parameters.DnssecOk && !Parameters.DnssecCd && !Parameters.DnsOnly && !Parameters.LlmnrFallback)
				{
					if (!Parameters.DnsOnly || !Parameters.NetbiosFallback && !Parameters.LlmnrFallback)
					{
						return;
					}
					else
					{
						throw new NotSupportedException();
					}
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public struct QueryParameters
		{
			public bool DnsOnly;

			public bool CacheOnly;

			public bool DnssecOk;

			public bool DnssecCd;

			public bool NoHostsFile;

			public bool LlmnrNetbiosOnly;

			public bool LlmnrFallback;

			public bool LlmnrOnly;

			public bool NetbiosFallback;

			public bool NoIdn;

			public bool NoRecursion;

			public bool QuickTimeout;

			public bool TcpOnly;

		}
	}
}