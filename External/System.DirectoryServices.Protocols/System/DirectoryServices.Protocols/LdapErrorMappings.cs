using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	internal class LdapErrorMappings
	{
		private static Hashtable ResultCodeHash;

		static LdapErrorMappings()
		{
			LdapErrorMappings.ResultCodeHash = new Hashtable();
			LdapErrorMappings.ResultCodeHash.Add(LdapError.IsLeaf, Res.GetString("LDAP_IS_LEAF"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.InvalidCredentials, Res.GetString("LDAP_INVALID_CREDENTIALS"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.ServerDown, Res.GetString("LDAP_SERVER_DOWN"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.LocalError, Res.GetString("LDAP_LOCAL_ERROR"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.EncodingError, Res.GetString("LDAP_ENCODING_ERROR"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.DecodingError, Res.GetString("LDAP_DECODING_ERROR"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.TimeOut, Res.GetString("LDAP_TIMEOUT"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.AuthUnknown, Res.GetString("LDAP_AUTH_UNKNOWN"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.FilterError, Res.GetString("LDAP_FILTER_ERROR"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.UserCancelled, Res.GetString("LDAP_USER_CANCELLED"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.ParameterError, Res.GetString("LDAP_PARAM_ERROR"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.NoMemory, Res.GetString("LDAP_NO_MEMORY"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.ConnectError, Res.GetString("LDAP_CONNECT_ERROR"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.NotSupported, Res.GetString("LDAP_NOT_SUPPORTED"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.NoResultsReturned, Res.GetString("LDAP_NO_RESULTS_RETURNED"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.ControlNotFound, Res.GetString("LDAP_CONTROL_NOT_FOUND"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.MoreResults, Res.GetString("LDAP_MORE_RESULTS_TO_RETURN"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.ClientLoop, Res.GetString("LDAP_CLIENT_LOOP"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.ReferralLimitExceeded, Res.GetString("LDAP_REFERRAL_LIMIT_EXCEEDED"));
			LdapErrorMappings.ResultCodeHash.Add(LdapError.SendTimeOut, Res.GetString("LDAP_SEND_TIMEOUT"));
		}

		public LdapErrorMappings()
		{
		}

		public static string MapResultCode(int errorCode)
		{
			return (string)LdapErrorMappings.ResultCodeHash[(object)((LdapError)errorCode)];
		}
	}
}