namespace System.DirectoryServices.Protocols
{
	public enum SecurityProtocol
	{
		Pct1Server = 1,
		Pct1Client = 2,
		Ssl2Server = 4,
		Ssl2Client = 8,
		Ssl3Server = 16,
		Ssl3Client = 32,
		Tls1Server = 64,
		Tls1Client = 128
	}
}