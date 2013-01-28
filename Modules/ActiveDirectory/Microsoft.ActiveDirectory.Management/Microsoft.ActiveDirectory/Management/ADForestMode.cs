namespace Microsoft.ActiveDirectory.Management
{
	public enum ADForestMode
	{
		UnknownForest = -1,
		Windows2000Forest = 0,
		Windows2003InterimForest = 1,
		Windows2003Forest = 2,
		Windows2008Forest = 3,
		Windows2008R2Forest = 4,
		Windows2012Forest = 5
	}
}