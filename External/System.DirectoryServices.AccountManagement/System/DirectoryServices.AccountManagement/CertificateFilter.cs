using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class CertificateFilter : FilterBase
	{
		public const string PropertyNameStatic = "AuthenticablePrincipal.Certificates";

		public override string PropertyName
		{
			get
			{
				return "AuthenticablePrincipal.Certificates";
			}
		}

		public CertificateFilter()
		{
		}
	}
}