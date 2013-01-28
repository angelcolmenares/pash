namespace System.DirectoryServices.ActiveDirectory
{
	internal enum TRUSTED_INFORMATION_CLASS
	{
		TrustedDomainNameInformation = 1,
		TrustedControllersInformation = 2,
		TrustedPosixOffsetInformation = 3,
		TrustedPasswordInformation = 4,
		TrustedDomainInformationBasic = 5,
		TrustedDomainInformationEx = 6,
		TrustedDomainAuthInformation = 7,
		TrustedDomainFullInformation = 8,
		TrustedDomainAuthInformationInternal = 9,
		TrustedDomainFullInformationInternal = 10,
		TrustedDomainInformationEx2Internal = 11,
		TrustedDomainFullInformation2Internal = 12
	}
}