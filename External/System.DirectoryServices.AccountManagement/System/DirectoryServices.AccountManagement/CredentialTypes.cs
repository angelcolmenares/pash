using System;

namespace System.DirectoryServices.AccountManagement
{
	[Flags]
	internal enum CredentialTypes
	{
		Password = 1,
		Certificate = 2
	}
}