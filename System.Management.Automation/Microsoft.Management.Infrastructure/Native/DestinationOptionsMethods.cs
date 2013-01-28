using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class DestinationOptionsMethods
	{
		internal static string packetEncoding_Default;

		internal static string packetEncoding_UTF8;

		internal static string packetEncoding_UTF16;

		internal static string transport_Http;

		internal static string transport_Https;

		internal static string proxyType_IE;

		internal static string proxyType_WinHTTP;

		internal static string proxyType_Auto;

		internal static string proxyType_None;

		static DestinationOptionsMethods()
		{
			DestinationOptionsMethods.packetEncoding_Default = "default";
			DestinationOptionsMethods.packetEncoding_UTF8 = "UTF8";
			DestinationOptionsMethods.packetEncoding_UTF16 = "UTF16";
			DestinationOptionsMethods.transport_Http = "HTTP";
			DestinationOptionsMethods.transport_Https = "HTTPS";
			DestinationOptionsMethods.proxyType_IE = "IE";
			DestinationOptionsMethods.proxyType_WinHTTP = "WinHTTP";
			DestinationOptionsMethods.proxyType_Auto = "Auto";
			DestinationOptionsMethods.proxyType_None = "None";
		}

		private DestinationOptionsMethods()
		{
		}

		internal static unsafe MiResult AddDestinationCredentials(DestinationOptionsHandle destinationOptionsHandle, NativeCimCredentialHandle credentials)
		{
			NativeDestinationOptions options = CimNativeApi.MarshalledObject.FromPointer<NativeDestinationOptions>(destinationOptionsHandle.DangerousGetHandle ());
			string userName = Marshal.PtrToStringUni(credentials.DangerousGetHandle ());
			SecureString pwd = credentials.GetSecureString ();
			
			options.UserName = userName;
			options.Password = CimNativeApi.GetPassword(pwd);
			IntPtr ptr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeDestinationOptions>(options);
			destinationOptionsHandle.DangerousSetHandle (ptr);
			/*
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			_MI_Result _MIResult = 0;
			DangerousHandleAccessor dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				_MI_DestinationOptions* _MIDestinationOptionsPointer = (_MI_DestinationOptions*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				if (_MIDestinationOptionsPointer != null)
				{
					if ((long)(*(_MIDestinationOptionsPointer + (long)16)) != 0)
					{
						DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst)1 = new DangerousHandleAccessor(credentials);
						try
						{
							dangerousHandleAccessor1 = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst)1;
							_MI_UserCredentials* _MIUserCredentialsPointer = (_MI_UserCredentials*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
							SecureString secureString = credentials.GetSecureString();
							IntPtr zero = IntPtr.Zero;
							if (secureString == null || secureString.Length <= 0)
							{
								*(_MIUserCredentialsPointer + (long)24) = (long)0;
							}
							else
							{
								IntPtr globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(secureString);
								zero = globalAllocUnicode;
								*(_MIUserCredentialsPointer + (long)24) = (void*)globalAllocUnicode;
							}
							try
							{
								_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_AddDestinationCredentials(_MIDestinationOptionsPointer, _MIUserCredentialsPointer);
							}
							finally
							{
								if (zero != IntPtr.Zero)
								{
									Marshal.ZeroFreeGlobalAllocUnicode(zero);
								}
							}
						}
						dangerousHandleAccessor1.Dispose();
						dangerousHandleAccessor.Dispose();
						return (MiResult)_MIResult;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			*/
			return MiResult.OK;
		}

		internal static unsafe MiResult AddProxyCredentials(DestinationOptionsHandle destinationOptionsHandle, NativeCimCredentialHandle credentials)
		{
			NativeDestinationOptions options = CimNativeApi.MarshalledObject.FromPointer<NativeDestinationOptions>(destinationOptionsHandle.DangerousGetHandle ());
			string userName = Marshal.PtrToStringUni (credentials.DangerousGetHandle ());
			SecureString pwd = credentials.GetSecureString ();

			options.ProxyUserName = userName;
			options.ProxyPassword = CimNativeApi.GetPassword(pwd);
			IntPtr ptr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeDestinationOptions>(options);
			destinationOptionsHandle.DangerousSetHandle (ptr);

			/*
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor1 = null;
			_MI_Result _MIResult = 0;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MI_DestinationOptions* _MIDestinationOptionsPointer = (_MI_DestinationOptions*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				if (_MIDestinationOptionsPointer != null)
				{
					if ((long)(*(_MIDestinationOptionsPointer + (long)16)) != 0)
					{
						DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst)1 = new DangerousHandleAccessor(credentials);
						try
						{
							dangerousHandleAccessor1 = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst)1;
							_MI_UserCredentials* _MIUserCredentialsPointer = (_MI_UserCredentials*)((void*)dangerousHandleAccessor1.DangerousGetHandle());
							SecureString secureString = credentials.GetSecureString();
							IntPtr zero = IntPtr.Zero;
							if (secureString == null || secureString.Length <= 0)
							{
								*(_MIUserCredentialsPointer + (long)24) = (long)0;
							}
							else
							{
								IntPtr globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(secureString);
								zero = globalAllocUnicode;
								*(_MIUserCredentialsPointer + (long)24) = (void*)globalAllocUnicode;
							}
							try
							{
								_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_AddProxyCredentials(_MIDestinationOptionsPointer, _MIUserCredentialsPointer);
							}
							finally
							{
								if (zero != IntPtr.Zero)
								{
									Marshal.ZeroFreeGlobalAllocUnicode(zero);
								}
							}
						}
						dangerousHandleAccessor1.Dispose();
						dangerousHandleAccessor.Dispose();
						return (MiResult)_MIResult;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			*/
			return MiResult.INVALID_PARAMETER;
		}

		internal static unsafe MiResult Clone (DestinationOptionsHandle destinationOptionsHandle, out DestinationOptionsHandle newDestinationOptionsHandle)
		{
			if (destinationOptionsHandle != null) {
				newDestinationOptionsHandle = destinationOptionsHandle;
			} else {
				NativeDestinationOptions options = new NativeDestinationOptions();
				IntPtr optionsPtr = (IntPtr)CimNativeApi.MarshalledObject.Create<NativeDestinationOptions>(options);
				newDestinationOptionsHandle = new DestinationOptionsHandle(optionsPtr);
			}

			/*
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MI_DestinationOptions* _MIDestinationOptionsPointer = (_MI_DestinationOptions*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				if (_MIDestinationOptionsPointer != null)
				{
					if ((long)(*(_MIDestinationOptionsPointer + (long)16)) != 0)
					{
						newDestinationOptionsHandle = null;
						_MI_DestinationOptions* _MIDestinationOptionsPointer1 = (_MI_DestinationOptions*)<Module>.Microsoft.Management.Infrastructure.Native.MI_CLI_malloc_core((long)24);
						_MI_Result _MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_Clone(_MIDestinationOptionsPointer, _MIDestinationOptionsPointer1);
						if (_MIResult != 0)
						{
							<Module>.free(_MIDestinationOptionsPointer1);
							dangerousHandleAccessor.Dispose();
							return (MiResult)_MIResult;
						}
						else
						{
							IntPtr intPtr = (IntPtr)_MIDestinationOptionsPointer1;
							newDestinationOptionsHandle = new DestinationOptionsHandle(intPtr);
							dangerousHandleAccessor.Dispose();
							return (MiResult)_MIResult;
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			*/
			return MiResult.OK;
		}

		internal static MiResult GetCertCACheck(DestinationOptionsHandle destinationOptionsHandle, out bool check)
		{
			check = false;
			/*
			_MI_Result _MIResult;
			byte num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			check = false;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetCertCACheck((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					bool flag = num == 1;
					check = flag;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetCertCNCheck(DestinationOptionsHandle destinationOptionsHandle, out bool check)
		{
			check = false;
			/*
			_MI_Result _MIResult;
			byte num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			check = false;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetCertCNCheck((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					bool flag = num == 1;
					check = flag;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetCertRevocationCheck(DestinationOptionsHandle destinationOptionsHandle, out bool check)
		{
			check = false;
			/*
			_MI_Result _MIResult;
			byte num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			check = false;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetCertRevocationCheck((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					bool flag = num == 1;
					check = flag;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
				return MiResult.NOT_SUPPORTED;
		}

		internal static unsafe MiResult GetDataLocale(DestinationOptionsHandle destinationOptionsHandle, out string locale)
		{
			locale = CultureInfo.CurrentCulture.Name;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			locale = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetDataLocale((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
					locale = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetDestinationPort(DestinationOptionsHandle destinationOptionsHandle, out uint port)
		{
			port = 80;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			port = 0;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				uint num = 0;
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetDestinationPort((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					port = num;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetEncodePortInSPN(DestinationOptionsHandle destinationOptionsHandle, out bool encodePort)
		{
			encodePort = false;
			/*
			_MI_Result _MIResult;
			byte num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			encodePort = false;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetEncodePortInSPN((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					bool flag = num == 1;
					encodePort = flag;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static unsafe MiResult GetHttpUrlPrefix(DestinationOptionsHandle destinationOptionsHandle, out string prefix)
		{
			prefix = "http";
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			prefix = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetHttpUrlPrefix((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
					prefix = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetImpersonationType(DestinationOptionsHandle destinationOptionsHandle, out DestinationOptionsMethods.MiImpersonationType impersonationType)
		{
			impersonationType = MiImpersonationType.Default;
			/*
			_MI_Result _MIResult;
			_MI_DestinationOptions_ImpersonationType _MIDestinationOptionsImpersonationType = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			impersonationType = DestinationOptionsMethods.MiImpersonationType.Default;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetImpersonationType((void*)dangerousHandleAccessor.DangerousGetHandle(), (_MI_DestinationOptions_ImpersonationType*)(&_MIDestinationOptionsImpersonationType));
				if (_MIResult == 0)
				{
					impersonationType = (DestinationOptionsMethods.MiImpersonationType)_MIDestinationOptionsImpersonationType;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetMaxEnvelopeSize(DestinationOptionsHandle destinationOptionsHandle, out uint sizeInKB)
		{
			sizeInKB = uint.MaxValue;
			/*
			_MI_Result _MIResult;
			uint num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			sizeInKB = 0;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetMaxEnvelopeSize((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					sizeInKB = num;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static unsafe MiResult GetPacketEncoding(DestinationOptionsHandle destinationOptionsHandle, out string encoding)
		{
			encoding = packetEncoding_UTF8;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			encoding = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetPacketEncoding((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
					encoding = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetPacketIntegrity(DestinationOptionsHandle destinationOptionsHandle, out bool integrity)
		{
			integrity = false;
			/*
			_MI_Result _MIResult;
			byte num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			integrity = false;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetPacketIntegrity((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					bool flag = num == 1;
					integrity = flag;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetPacketPrivacy(DestinationOptionsHandle destinationOptionsHandle, out bool privacy)
		{
			privacy = false;
			/*
			_MI_Result _MIResult;
			byte num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			privacy = false;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetPacketPrivacy((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					bool flag = num == 1;
					privacy = flag;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static unsafe MiResult GetProxyType(DestinationOptionsHandle destinationOptionsHandle, out string proxyType)
		{
			proxyType = proxyType_None;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			proxyType = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetProxyType((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
					proxyType = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult GetTimeout(DestinationOptionsHandle destinationOptionsHandle, out TimeSpan timeout)
		{
			timeout = TimeSpan.MaxValue;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				timeout = TimeSpan.Zero;
				_MI_Datetime _MIDatetime = null;
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetTimeout((void*)dangerousHandleAccessor.DangerousGetHandle(), &_MIDatetime + 4);
				if (_MIResult == 0)
				{
					timeout = (TimeSpan)InstanceMethods.ConvertMiDateTimeToManagedObject(ref _MIDatetime);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static unsafe MiResult GetTransport(DestinationOptionsHandle destinationOptionsHandle, out string transport)
		{
			transport = DestinationOptionsMethods.transport_Http;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			transport = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetTransport((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
					transport = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static unsafe MiResult GetUILocale(DestinationOptionsHandle destinationOptionsHandle, out string locale)
		{
			locale = CultureInfo.CurrentCulture.Name;
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			locale = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				UInt16 modopt(System.Runtime.CompilerServices.IsConst)* uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer = (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x5fa54d02.MI_DestinationOptions_GetUILocale((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16 modopt(System.Runtime.CompilerServices.IsConst)*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16 modopt(System.Runtime.CompilerServices.IsConst)Pointer;
					locale = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetCertCACheck(DestinationOptionsHandle destinationOptionsHandle, bool check)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetCertCACheck((void*)dangerousHandleAccessor.DangerousGetHandle(), check);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetCertCNCheck(DestinationOptionsHandle destinationOptionsHandle, bool check)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetCertCNCheck((void*)dangerousHandleAccessor.DangerousGetHandle(), check);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetCertRevocationCheck(DestinationOptionsHandle destinationOptionsHandle, bool check)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetCertRevocationCheck((void*)dangerousHandleAccessor.DangerousGetHandle(), check);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetCustomOption(DestinationOptionsHandle destinationOptionsHandle, string optionName, string optionValue)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(optionName);
				IntPtr intPtr = hGlobalUni;
				try
				{
					IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(optionValue);
					IntPtr intPtr1 = hGlobalUni1;
					try
					{
						miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetString((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni, (void*)hGlobalUni1);
					}
					finally
					{
						if (intPtr1 != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr1);
						}
					}
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetDataLocale(DestinationOptionsHandle destinationOptionsHandle, string locale)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(locale);
				IntPtr intPtr = hGlobalUni;
				try
				{
					miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetDataLocale((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetDestinationPort(DestinationOptionsHandle destinationOptionsHandle, uint port)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetDestinationPort((void*)dangerousHandleAccessor.DangerousGetHandle(), port);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetEncodePortInSPN(DestinationOptionsHandle destinationOptionsHandle, bool encodePort)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetEncodePortInSPN((void*)dangerousHandleAccessor.DangerousGetHandle(), encodePort);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetHttpUrlPrefix(DestinationOptionsHandle destinationOptionsHandle, string prefix)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(prefix);
				IntPtr intPtr = hGlobalUni;
				try
				{
					miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetHttpUrlPrefix((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetImpersonationType(DestinationOptionsHandle destinationOptionsHandle, DestinationOptionsMethods.MiImpersonationType impersonationType)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetImpersonationType((void*)dangerousHandleAccessor.DangerousGetHandle(), (_MI_DestinationOptions_ImpersonationType)impersonationType);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetMaxEnvelopeSize(DestinationOptionsHandle destinationOptionsHandle, uint sizeInKB)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetMaxEnvelopeSize((void*)dangerousHandleAccessor.DangerousGetHandle(), sizeInKB);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetPacketEncoding(DestinationOptionsHandle destinationOptionsHandle, string encoding)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(encoding);
				IntPtr intPtr = hGlobalUni;
				try
				{
					miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetPacketEncoding((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetPacketIntegrity(DestinationOptionsHandle destinationOptionsHandle, bool integrity)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetPacketIntegrity((void*)dangerousHandleAccessor.DangerousGetHandle(), integrity);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetPacketPrivacy(DestinationOptionsHandle destinationOptionsHandle, bool privacy)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetPacketPrivacy((void*)dangerousHandleAccessor.DangerousGetHandle(), privacy);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetProxyType(DestinationOptionsHandle destinationOptionsHandle, string proxyType)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(proxyType);
				IntPtr intPtr = hGlobalUni;
				try
				{
					miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetProxyType((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetTimeout(DestinationOptionsHandle destinationOptionsHandle, TimeSpan timeout)
		{
			/*
			MiResult miResult;
			_MI_Datetime _MIDatetime;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				InstanceMethods.ConvertManagedObjectToMiDateTime(timeout, ref _MIDatetime);
				miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetTimeout((void*)dangerousHandleAccessor.DangerousGetHandle(), &_MIDatetime + 4);
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetTransport(DestinationOptionsHandle destinationOptionsHandle, string transport)
		{

			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(transport);
				IntPtr intPtr = hGlobalUni;
				try
				{
					miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetTransport((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal static MiResult SetUILocale(DestinationOptionsHandle destinationOptionsHandle, string locale)
		{
			/*
			MiResult miResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst) = new DangerousHandleAccessor(destinationOptionsHandle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor modopt(System.Runtime.CompilerServices.IsConst);
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(locale);
				IntPtr intPtr = hGlobalUni;
				try
				{
					miResult = (MiResult)<Module>.?A0x5fa54d02.MI_DestinationOptions_SetUILocale((void*)dangerousHandleAccessor.DangerousGetHandle(), (void*)hGlobalUni);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return miResult;
			*/
			return MiResult.NOT_SUPPORTED;
		}

		internal enum MiImpersonationType
		{
			Default = 0,
			None = 1,
			Identify = 2,
			Impersonate = 3,
			Delegate = 4
		}
	}
}