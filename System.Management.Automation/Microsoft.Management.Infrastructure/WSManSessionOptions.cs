using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.Options
{
	public class WSManSessionOptions : CimSessionOptions
	{
		public bool CertCACheck
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult certCACheck = DestinationOptionsMethods.GetCertCACheck(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(certCACheck);
				return flag;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetCertCACheck(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool CertCNCheck
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult certCNCheck = DestinationOptionsMethods.GetCertCNCheck(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(certCNCheck);
				return flag;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetCertCNCheck(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool CertRevocationCheck
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult certRevocationCheck = DestinationOptionsMethods.GetCertRevocationCheck(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(certRevocationCheck);
				return flag;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetCertRevocationCheck(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public uint DestinationPort
		{
			get
			{
				uint num = 0;
				base.AssertNotDisposed();
				MiResult destinationPort = DestinationOptionsMethods.GetDestinationPort(base.DestinationOptionsHandleOnDemand, out num);
				CimException.ThrowIfMiResultFailure(destinationPort);
				return num;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetDestinationPort(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool EncodePortInServicePrincipalName
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult encodePortInSPN = DestinationOptionsMethods.GetEncodePortInSPN(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(encodePortInSPN);
				return flag;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetEncodePortInSPN(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public Uri HttpUrlPrefix
		{
			get
			{
				string str = null;
				Uri uri;
				base.AssertNotDisposed();
				MiResult httpUrlPrefix = DestinationOptionsMethods.GetHttpUrlPrefix(base.DestinationOptionsHandleOnDemand, out str);
				if (httpUrlPrefix == MiResult.OK)
				{
					try
					{
						try
						{
							uri = new Uri(str, UriKind.Relative);
						}
						catch (UriFormatException uriFormatException)
						{
							uri = new Uri(str, UriKind.Absolute);
						}
					}
					catch (UriFormatException uriFormatException1)
					{
						uri = null;
					}
					catch (ArgumentException argumentException)
					{
						uri = null;
					}
					return uri;
				}
				else
				{
					return null;
				}
			}
			set
			{
				if (value != null)
				{
					base.AssertNotDisposed();
					MiResult miResult = DestinationOptionsMethods.SetHttpUrlPrefix(base.DestinationOptionsHandleOnDemand, value.ToString());
					CimException.ThrowIfMiResultFailure(miResult);
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public uint MaxEnvelopeSize
		{
			get
			{
				uint num = 0;
				base.AssertNotDisposed();
				MiResult maxEnvelopeSize = DestinationOptionsMethods.GetMaxEnvelopeSize(base.DestinationOptionsHandleOnDemand, out num);
				CimException.ThrowIfMiResultFailure(maxEnvelopeSize);
				return num;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetMaxEnvelopeSize(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool NoEncryption
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult packetPrivacy = DestinationOptionsMethods.GetPacketPrivacy(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(packetPrivacy);
				bool flag1 = !flag;
				return flag1;
			}
			set
			{
				base.AssertNotDisposed();
				bool flag = value;
				bool flag1 = !flag;
				MiResult miResult = DestinationOptionsMethods.SetPacketPrivacy(base.DestinationOptionsHandleOnDemand, flag1);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public PacketEncoding PacketEncoding
		{
			get
			{
				string str = null;
				base.AssertNotDisposed();
				MiResult packetEncoding = DestinationOptionsMethods.GetPacketEncoding(base.DestinationOptionsHandleOnDemand, out str);
				CimException.ThrowIfMiResultFailure(packetEncoding);
				return PacketEncodingExtensionMethods.FromNativeType(str);
			}
			set
			{
				base.AssertNotDisposed();
				string nativeType = value.ToNativeType();
				MiResult miResult = DestinationOptionsMethods.SetPacketEncoding(base.DestinationOptionsHandleOnDemand, nativeType);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public ProxyType ProxyType
		{
			get
			{
				string str = null;
				base.AssertNotDisposed();
				MiResult proxyType = DestinationOptionsMethods.GetProxyType(base.DestinationOptionsHandleOnDemand, out str);
				CimException.ThrowIfMiResultFailure(proxyType);
				return ProxyTypeExtensionMethods.FromNativeType(str);
			}
			set
			{
				base.AssertNotDisposed();
				string nativeType = value.ToNativeType();
				MiResult miResult = DestinationOptionsMethods.SetProxyType(base.DestinationOptionsHandleOnDemand, nativeType);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool UseSsl
		{
			get
			{
				string str = null;
				base.AssertNotDisposed();
				MiResult transport = DestinationOptionsMethods.GetTransport(base.DestinationOptionsHandleOnDemand, out str);
				CimException.ThrowIfMiResultFailure(transport);
				if (string.Compare(str, DestinationOptionsMethods.transport_Https, true, CultureInfo.CurrentCulture) != 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				base.AssertNotDisposed();
				if (!value)
				{
					MiResult miResult = DestinationOptionsMethods.SetTransport(base.DestinationOptionsHandleOnDemand, DestinationOptionsMethods.transport_Http);
					CimException.ThrowIfMiResultFailure(miResult);
					return;
				}
				else
				{
					MiResult miResult1 = DestinationOptionsMethods.SetTransport(base.DestinationOptionsHandleOnDemand, DestinationOptionsMethods.transport_Https);
					CimException.ThrowIfMiResultFailure(miResult1);
					return;
				}
			}
		}

		public WSManSessionOptions() : base(ApplicationMethods.protocol_WSMan)
		{
			DestinationOptionsHandle handle;
			DestinationOptionsMethods.Clone (base.DestinationOptionsHandle, out handle);
			DestinationOptionsHandleOnDemand = handle;
		}

		public WSManSessionOptions(WSManSessionOptions optionsToClone) : base(optionsToClone)
		{
			DestinationOptionsHandle handle;
			DestinationOptionsMethods.Clone (base.DestinationOptionsHandle, out handle);
			DestinationOptionsHandleOnDemand = handle;
		}

		public void AddProxyCredentials(CimCredential credential)
		{
			if (credential != null)
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.AddProxyCredentials(base.DestinationOptionsHandleOnDemand, credential.GetCredential());
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("credential");
			}
		}

		public override void AddDestinationCredentials(CimCredential credential)
		{
			if (credential != null)
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.AddDestinationCredentials(base.DestinationOptionsHandleOnDemand, credential.GetCredential());
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("credential");
			}
		}

	}
}