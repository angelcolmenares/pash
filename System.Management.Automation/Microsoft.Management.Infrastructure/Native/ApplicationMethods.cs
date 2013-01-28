using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Management.Infrastructure.Native
{
    public sealed class ApplicationMethods
    {

		internal static string protocol_WSMan;

		internal static string protocol_DCOM;

		static ApplicationMethods()
		{
			ApplicationMethods.protocol_WSMan = "WinRM";
			ApplicationMethods.protocol_DCOM = "WMIDCOM";
		}

		private ApplicationMethods()
		{

		}

        internal static void GetCimErrorFromMiResult(MiResult errorCode, string errorMessage, out InstanceHandle errorDetailsHandle)
        {
			errorDetailsHandle = new InstanceHandle(IntPtr.Zero, false);
        }

        internal static void SupressFurtherCallbacks()
        {
            
		}

        internal static MiResult Initialize(int p1, string p2, out InstanceHandle instanceHandle, out ApplicationHandle applicationHandle)
        {
			NativeCimApplication app = new NativeCimApplication();
			app.Id = p1;
			app.Target = p2;
			IntPtr appPtr = CimNativeApi.MarshalledObject.Create<NativeCimApplication>(app);
			applicationHandle = new ApplicationHandle(appPtr);
			NewInstance (applicationHandle, p2, null, out instanceHandle);
			return MiResult.OK;
        }

        internal static MiResult NewInstance(ApplicationHandle applicationHandle, string p1, object p2, out InstanceHandle instanceHandle)
        {
			NativeCimInstance instance = new NativeCimInstance();
			instance.CimClassName = p1;
			instance.ClassName = p1;
			instance.Properties = NativeCimPropertiesHelper.Serialize (new NativeCimProperties());
			instance.SystemProperties = NativeCimPropertiesHelper.Serialize (new NativeCimProperties());
			instance.Namespace = "root/cimv2";
			instance.ServerName = "localhost";
			instance.Qualifiers = NativeCimQualifiersHelper.Serialize (new NativeCimQualifiers());
			IntPtr instancePtr = CimNativeApi.MarshalledObject.Create<NativeCimInstance>(instance);
			instanceHandle = new InstanceHandle(instancePtr, false);
			return MiResult.OK;
        }

		internal static MiResult NewSession (ApplicationHandle handle, string protocol, string str1, DestinationOptionsHandle destinationOptionsHandle, out InstanceHandle instanceHandle, out SessionHandle sessionHandle)
		{
			string p1 = string.IsNullOrEmpty (str1) ? "localhost" : str1; //.Substring(1, str1.Length - 2);
			NewInstance (handle, p1, null, out instanceHandle);
			NativeCimSession session = new NativeCimSession();
			session.Protocol = protocol;
			session.ServerName = p1;
			session.DestinationOptions = destinationOptionsHandle == null ? IntPtr.Zero : destinationOptionsHandle.DangerousGetHandle ();
			IntPtr sessionPtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimSession>(session);
			sessionHandle = new SessionHandle(sessionPtr);
			return MiResult.OK;
		}

		internal static MiResult NewSubscriptionDeliveryOptions (ApplicationHandle handle, MiSubscriptionDeliveryType miSubscriptionDeliveryType, out Microsoft.Management.Infrastructure.Native.SubscriptionDeliveryOptionsHandle subscriptionDeliveryOptionsHandle)
		{
			subscriptionDeliveryOptionsHandle = new SubscriptionDeliveryOptionsHandle(handle.DangerousGetHandle());
			return MiResult.OK;
		}
    }
}
