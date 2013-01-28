using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("2D53BDAA-798E-49E6-A1AA-74D01256F411")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cred")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="str")]
	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="Username")]
	[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
	[TypeLibType(0x10d0)]
	public interface IWSManEx
	{
		string CommandLine
		{
			[DispId(3)]
			get;
		}

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
		string Error
		{
			[DispId(4)]
			[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
			get;
		}

		[DispId(2)]
		object CreateConnectionOptions();

		[DispId(5)]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="str")]
		object CreateResourceLocator(string strResourceLocator);

		[DispId(1)]
		object CreateSession(string connection, int flags, object connectionOptions);

		[DispId(29)]
		int EnumerationFlagAssociatedInstance();

		[DispId(28)]
		int EnumerationFlagAssociationInstance();

		[DispId(21)]
		int EnumerationFlagHierarchyDeep();

		[DispId(23)]
		int EnumerationFlagHierarchyDeepBasePropsOnly();

		[DispId(22)]
		int EnumerationFlagHierarchyShallow();

		[DispId(17)]
		int EnumerationFlagNonXmlText();

		[DispId(18)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="EPR")]
		int EnumerationFlagReturnEPR();

		[DispId(24)]
		int EnumerationFlagReturnObject();

		[DispId(19)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="EPR")]
		int EnumerationFlagReturnObjectAndEPR();

		[DispId(20)]
		string GetErrorMessage(uint errorNumber);

		[DispId(7)]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="Username")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cred")]
		int SessionFlagCredUsernamePassword();

		[DispId(15)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SPN")]
		int SessionFlagEnableSPNServerPort();

		[DispId(14)]
		int SessionFlagNoEncryption();

		[DispId(8)]
		int SessionFlagSkipCACheck();

		[DispId(9)]
		int SessionFlagSkipCNCheck();

		[DispId(12)]
		int SessionFlagUseBasic();

		[DispId(10)]
		int SessionFlagUseDigest();

		[DispId(13)]
		int SessionFlagUseKerberos();

		[DispId(11)]
		int SessionFlagUseNegotiate();

		[DispId(16)]
		int SessionFlagUseNoAuthentication();

		[DispId(6)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="UTF")]
		int SessionFlagUTF8();
	}
}