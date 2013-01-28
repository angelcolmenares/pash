using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.Protocols
{
	[ComVisible(false)]
	[SuppressUnmanagedCodeSecurity]
	internal class Wldap32
	{
		public const int SEC_WINNT_AUTH_IDENTITY_UNICODE = 2;

		public const int SEC_WINNT_AUTH_IDENTITY_VERSION = 0x200;

		public const string MICROSOFT_KERBEROS_NAME_W = "Kerberos";

		public Wldap32()
		{
		}

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ber_alloc(int option);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_bvecfree(IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_bvfree(IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_flatten(BerSafeHandle berElement, ref IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ber_free(IntPtr berelement, int option);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ber_init(berval value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_printf_berarray(BerSafeHandle berElement, string format, IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_printf_bytearray(BerSafeHandle berElement, string format, HGlobalMemHandle value, int length);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_printf_emptyarg(BerSafeHandle berElement, string format);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_printf_int(BerSafeHandle berElement, string format, int value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_scanf(BerSafeHandle berElement, string format);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_scanf_bitstring(BerSafeHandle berElement, string format, ref IntPtr value, ref int length);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_scanf_int(BerSafeHandle berElement, string format, ref int value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ber_scanf_ptr(BerSafeHandle berElement, string format, ref IntPtr value);

		[DllImport("Crypt32.dll", CharSet=CharSet.Unicode)]
		public static extern int CertFreeCRLContext(IntPtr certContext);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr cldap_open(string hostName, int portNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_abandon(ConnectionHandle ldapHandle, int messagId);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_add(ConnectionHandle ldapHandle, string dn, IntPtr attrs, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_bind_s(ConnectionHandle ldapHandle, string dn, SEC_WINNT_AUTH_IDENTITY_EX credentials, BindMethod method);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_compare(ConnectionHandle ldapHandle, string dn, string attributeName, string strValue, berval binaryValue, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_connect(ConnectionHandle ldapHandle, LDAP_TIMEVAL timeout);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_control_free(IntPtr control);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_controls_free(IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_create_sort_control(ConnectionHandle handle, IntPtr keys, byte critical, ref IntPtr control);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_delete_ext(ConnectionHandle ldapHandle, string dn, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_extended_operation(ConnectionHandle ldapHandle, string oid, berval data, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_first_attribute(ConnectionHandle ldapHandle, IntPtr result, ref IntPtr address);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_first_entry(ConnectionHandle ldapHandle, IntPtr result);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_first_reference(ConnectionHandle ldapHandle, IntPtr result);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_get_dn(ConnectionHandle ldapHandle, IntPtr result);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_get_option_int(ConnectionHandle ldapHandle, LdapOption option, ref int outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_get_option_ptr(ConnectionHandle ldapHandle, LdapOption option, ref IntPtr outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_get_option_sechandle(ConnectionHandle ldapHandle, LdapOption option, ref SecurityHandle outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_get_option_secInfo(ConnectionHandle ldapHandle, LdapOption option, SecurityPackageContextConnectionInformation outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_get_values_len(ConnectionHandle ldapHandle, IntPtr result, string name);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_init(string hostName, int portNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern void ldap_memfree(IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_modify(ConnectionHandle ldapHandle, string dn, IntPtr attrs, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_msgfree(IntPtr result);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_next_attribute(ConnectionHandle ldapHandle, IntPtr result, IntPtr address);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_next_entry(ConnectionHandle ldapHandle, IntPtr result);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_next_reference(ConnectionHandle ldapHandle, IntPtr result);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_parse_extended_result(ConnectionHandle ldapHandle, IntPtr result, ref IntPtr oid, ref IntPtr data, byte freeIt);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_parse_reference(ConnectionHandle ldapHandle, IntPtr result, ref IntPtr referrals);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_parse_result(ConnectionHandle ldapHandle, IntPtr result, ref int serverError, ref IntPtr dn, ref IntPtr message, ref IntPtr referral, ref IntPtr control, byte freeIt);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_parse_result_referral(ConnectionHandle ldapHandle, IntPtr result, IntPtr serverError, IntPtr dn, IntPtr message, ref IntPtr referral, IntPtr control, byte freeIt);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_rename(ConnectionHandle ldapHandle, string dn, string newRdn, string newParentDn, int deleteOldRdn, IntPtr servercontrol, IntPtr clientcontrol, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_result(ConnectionHandle ldapHandle, int messageId, int all, LDAP_TIMEVAL timeout, ref IntPtr Mesage);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_result2error(ConnectionHandle ldapHandle, IntPtr result, int freeIt);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_search(ConnectionHandle ldapHandle, string dn, int scope, string filter, IntPtr attributes, bool attributeOnly, IntPtr servercontrol, IntPtr clientcontrol, int timelimit, int sizelimit, ref int messageNumber);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_set_option_clientcert(ConnectionHandle ldapHandle, LdapOption option, QUERYCLIENTCERT outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_set_option_int(ConnectionHandle ldapHandle, LdapOption option, ref int inValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_set_option_ptr(ConnectionHandle ldapHandle, LdapOption option, ref IntPtr inValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_set_option_referral(ConnectionHandle ldapHandle, LdapOption option, ref LdapReferralCallback outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_set_option_servercert(ConnectionHandle ldapHandle, LdapOption option, VERIFYSERVERCERT outValue);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_simple_bind_s(ConnectionHandle ldapHandle, string distinguishedName, string password);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_start_tls(ConnectionHandle ldapHandle, ref int ServerReturnValue, ref IntPtr Message, IntPtr ServerControls, IntPtr ClientControls);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern byte ldap_stop_tls(ConnectionHandle ldapHandle);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_unbind(IntPtr ldapHandle);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern int ldap_value_free(IntPtr value);

		[DllImport("wldap32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr ldap_value_free_len(IntPtr berelement);

		[DllImport("wldap32.dll", CharSet=CharSet.None)]
		public static extern int LdapGetLastError();
	}
}