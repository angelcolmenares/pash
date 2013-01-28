namespace System.DirectoryServices.Protocols
{
	public class ShowDeletedControl : DirectoryControl
	{
		public ShowDeletedControl() : base("1.2.840.113556.1.4.417", null, true, true)
		{
		}
	}
}