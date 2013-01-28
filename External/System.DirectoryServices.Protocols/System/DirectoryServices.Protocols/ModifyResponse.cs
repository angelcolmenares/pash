using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class ModifyResponse : DirectoryResponse
	{
		internal ModifyResponse(XmlNode node) : base(node)
		{
		}

		internal ModifyResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
		{
		}
	}
}