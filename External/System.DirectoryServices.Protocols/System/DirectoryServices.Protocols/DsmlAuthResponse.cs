using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlAuthResponse : DirectoryResponse
	{
		internal DsmlAuthResponse(XmlNode node) : base(node)
		{
		}
	}
}