using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("FC84FC58-1286-40C4-9DA0-C8EF6EC241E0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
	[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
	[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Get")]
	[TypeLibType(0x10c0)]
	public interface IWSManSession
	{
		int BatchItems
		{
			[DispId(9)]
			get;
			[DispId(9)]
			set;
		}

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
		string Error
		{
			[DispId(8)]
			[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error")]
			get;
		}

		int Timeout
		{
			[DispId(10)]
			get;
			[DispId(10)]
			set;
		}

		[DispId(3)]
		string Create(object resourceUri, string resource, int flags);

		[DispId(4)]
		void Delete(object resourceUri, int flags);

		[DispId(6)]
		object Enumerate(object resourceUri, string filter, string dialect, int flags);

		[DispId(1)]
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Get")]
		string Get(object resourceUri, int flags);

		[DispId(7)]
		string Identify(int flags);

		[DispId(5)]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#")]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="URI")]
		string Invoke(string actionURI, object resourceUri, string parameters, int flags);

		[DispId(2)]
		string Put(object resourceUri, string resource, int flags);
	}
}