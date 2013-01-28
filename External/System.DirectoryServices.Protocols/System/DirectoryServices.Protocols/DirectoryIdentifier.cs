namespace System.DirectoryServices.Protocols
{
	public abstract class DirectoryIdentifier
	{
		protected DirectoryIdentifier()
		{
			Utility.CheckOSVersion();
		}
	}
}