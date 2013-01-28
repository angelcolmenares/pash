using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Options
{
	public class DComSessionOptions : CimSessionOptions
	{
		public ImpersonationType Impersonation
		{
			get
			{
				DestinationOptionsMethods.MiImpersonationType miImpersonationType = DestinationOptionsMethods.MiImpersonationType.Default;
				base.AssertNotDisposed();
				MiResult impersonationType = DestinationOptionsMethods.GetImpersonationType(base.DestinationOptionsHandleOnDemand, out miImpersonationType);
				CimException.ThrowIfMiResultFailure(impersonationType);
				return (ImpersonationType)miImpersonationType;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetImpersonationType(base.DestinationOptionsHandleOnDemand, value.ToNativeType());
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool PacketIntegrity
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult packetIntegrity = DestinationOptionsMethods.GetPacketIntegrity(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(packetIntegrity);
				return flag;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetPacketIntegrity(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public bool PacketPrivacy
		{
			get
			{
				bool flag = false;
				base.AssertNotDisposed();
				MiResult packetPrivacy = DestinationOptionsMethods.GetPacketPrivacy(base.DestinationOptionsHandleOnDemand, out flag);
				CimException.ThrowIfMiResultFailure(packetPrivacy);
				return flag;
			}
			set
			{
				base.AssertNotDisposed();
				MiResult miResult = DestinationOptionsMethods.SetPacketPrivacy(base.DestinationOptionsHandleOnDemand, value);
				CimException.ThrowIfMiResultFailure(miResult);
			}
		}

		public DComSessionOptions() : base(ApplicationMethods.protocol_DCOM)
		{
		}

		public DComSessionOptions(DComSessionOptions optionsToClone) : base(optionsToClone)
		{
		}
	}
}