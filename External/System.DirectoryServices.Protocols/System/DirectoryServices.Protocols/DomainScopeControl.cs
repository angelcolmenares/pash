namespace System.DirectoryServices.Protocols
{
	public class DomainScopeControl : DirectoryControl
	{
		public DomainScopeControl() : base("1.2.840.113556.1.4.1339", null, true, true)
		{
		}
	}
}