using System;

namespace Microsoft.DnsClient.Commands.dnsdiag
{
	public static class api
	{
		public static int TestDisjointNamespace()
		{
			Console.WriteLine("TestDisjointNamespace");
			return 0;
		}

		public static int TestDnsDirectAccess(bool SKU)
		{
			Console.WriteLine("TestDnsDirectAccess");
			return 0;
		}

		public static int TestDnsRegistration(bool primary, bool connection)
		{
			Console.WriteLine("TestDnsRegistration");
			return 0;
		}

		public static int TestDnsSec()
		{
			Console.WriteLine("TestDnsSec");
			return 0;
		}

		public static int TestDnsServer(string protocol, string adapter, string nrpt, string server)
		{
			Console.WriteLine("TestDnsServer");
			return 0;
		}

		public static int TestSingleLabelNamespace()
		{
			Console.WriteLine("TestSingleLableNamespace");
			return 0;
		}
	}
}