using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("A7A1BA28-DE41-466A-AD0A-C4059EAD7428")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Sel")]
	[TypeLibType(0x10c0)]
	public interface IWSManResourceLocator
	{
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
		string Error
		{
			[DispId(9)]
			[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
			[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
			get;
		}

		string FragmentDialect
		{
			[DispId(5)]
			get;
			[DispId(5)]
			set;
		}

		string FragmentPath
		{
			[DispId(4)]
			get;
			[DispId(4)]
			set;
		}

		int MustUnderstandOptions
		{
			[DispId(7)]
			get;
			[DispId(7)]
			set;
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="resource")]
		string resourceUri
		{
			[DispId(1)]
			[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
			[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="resource")]
			get;
			[DispId(1)]
			[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
			[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="resource")]
			set;
		}

		[DispId(6)]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="Option")]
		void AddOption(string OptionName, object OptionValue, int mustComply);

		[DispId(2)]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Sel")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="resource")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="sel")]
		void AddSelector(string resourceSelName, object selValue);

		[DispId(8)]
		void ClearOptions();

		[DispId(3)]
		void ClearSelectors();
	}
}