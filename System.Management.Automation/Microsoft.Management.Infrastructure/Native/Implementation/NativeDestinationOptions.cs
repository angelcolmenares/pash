using System;

namespace Microsoft.Management.Infrastructure.Native
{
	/// <summary>
	/// Native destination options.
	/// </summary>
	internal struct NativeDestinationOptions
	{
		public string ServerName { get; set; }

		public string UserName { get;set; }

		public string Password { get; set; }

		public string Domain { get;set; }

		public string ProxyUserName { get;set; }
		
		public string ProxyPassword { get; set; }
		
		public string ProxyDomain { get;set; }

		public int ImpersonationType { get; set; }

		public string UrlPrefix { get; set; }

		public bool PacketPrivacy { get; set; }

		public string Encoding { get; set; }

		public bool PacketIntegrity { get; set; }

		public uint MaxEnvelopeSize { get; set; }

		public string Transport { get;set; }

		public int ProxyType { get; set; }

		public long Timeout { get; set; }

		public bool EncodePort
		{
			get;set;
		}

		public int DestinationPort
		{
			get;set;
		}

		public bool CertCACheck
		{
			get;set;
		}

		public bool CertCNCheck
		{
			get;set;
		}

		public bool CertRevocationCheck
		{
			get;set;
		}

		public string Locale
		{
			get;set;
		}

		public string UILocale
		{
			get;set;
		}

	}
}

