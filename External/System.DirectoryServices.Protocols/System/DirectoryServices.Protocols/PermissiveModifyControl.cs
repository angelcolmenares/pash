namespace System.DirectoryServices.Protocols
{
	public class PermissiveModifyControl : DirectoryControl
	{
		public PermissiveModifyControl() : base("1.2.840.113556.1.4.1413", null, true, true)
		{
		}
	}
}