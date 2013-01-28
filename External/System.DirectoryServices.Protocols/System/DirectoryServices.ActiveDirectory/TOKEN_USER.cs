namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class TOKEN_USER
	{
		public SID_AND_ATTR sidAndAttributes;

		public TOKEN_USER()
		{
			this.sidAndAttributes = new SID_AND_ATTR();
		}
	}
}