namespace Microsoft.ActiveDirectory.Management
{
	public enum ADDomainMode
	{
		UnknownDomain = -1,
		Windows2000Domain = 0,
		Windows2003InterimDomain = 1,
		Windows2003Domain = 2,
		Windows2008Domain = 3,
		Windows2008R2Domain = 4,
		Windows2012Domain = 5
	}
}