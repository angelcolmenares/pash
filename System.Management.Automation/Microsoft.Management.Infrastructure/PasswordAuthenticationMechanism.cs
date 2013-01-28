namespace Microsoft.Management.Infrastructure.Options
{
	public enum PasswordAuthenticationMechanism
	{
		Default,
		Digest,
		Negotiate,
		Basic,
		Kerberos,
		NtlmDomain,
		CredSsp
	}
}