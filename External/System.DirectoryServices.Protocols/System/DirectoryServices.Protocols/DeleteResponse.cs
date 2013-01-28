using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DeleteResponse : DirectoryResponse
	{
		internal DeleteResponse(XmlNode node) : base(node)
		{
		}

		internal DeleteResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
		}
	}
}