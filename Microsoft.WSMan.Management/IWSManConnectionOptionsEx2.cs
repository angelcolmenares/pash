using System;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[Guid("F500C9EC-24EE-48ab-B38D-FC9A164C658E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	[TypeLibType(0x10c0)]
	public interface IWSManConnectionOptionsEx2 : IWSManConnectionOptionsEx, IWSManConnectionOptions
	{
		[DispId(10)]
		int ProxyAuthenticationUseBasic();

		[DispId(11)]
		int ProxyAuthenticationUseDigest();

		[DispId(9)]
		int ProxyAuthenticationUseNegotiate();

		[DispId(7)]
		int ProxyAutoDetect();

		[DispId(5)]
		int ProxyIEConfig();

		[DispId(8)]
		int ProxyNoProxyServer();

		[DispId(6)]
		int ProxyWinHttpConfig();

		[DispId(4)]
		void SetProxy(int accessType, int authenticationMechanism, string userName, string password);
	}
}