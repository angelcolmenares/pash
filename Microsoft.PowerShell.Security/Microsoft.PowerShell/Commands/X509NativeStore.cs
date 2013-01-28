using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	internal sealed class X509NativeStore
	{
		private static bool fIsWin8AndAbove;

		private bool archivedCerts;

		private X509StoreLocation storeLocation;

		private string storeName;

		private CertificateStoreHandle storeHandle;

		private CertificateFilterHandle filterHandle;

		private bool valid;

		private bool open;

		public X509StoreLocation Location
		{
			get
			{
				return this.storeLocation;
			}
		}

		public IntPtr StoreHandle
		{
			get
			{
				return this.storeHandle.Handle;
			}
		}

		public string StoreName
		{
			get
			{
				return this.storeName;
			}
		}

		public bool Valid
		{
			get
			{
				return this.valid;
			}
		}

		static X509NativeStore()
		{
			X509NativeStore.fIsWin8AndAbove = DownLevelHelper.IsWin8AndAbove();
		}

		public X509NativeStore(X509StoreLocation StoreLocation, string StoreName)
		{
			this.storeLocation = StoreLocation;
			this.storeName = StoreName;
		}

		public void FreeCert(IntPtr certContext)
		{
			NativeMethods.CertFreeCertificateContext(certContext);
		}

		public IntPtr GetCertByName(string Name)
		{
			X509Certificate2 x509Certificate2;
			IntPtr zero = IntPtr.Zero;
			if (this.open)
			{
				if (this.Valid)
				{
					if (!X509NativeStore.fIsWin8AndAbove)
					{
						do
						{
							zero = this.GetNextCert(zero);
							if (IntPtr.Zero == zero)
							{
								break;
							}
							x509Certificate2 = new X509Certificate2(zero);
						}
						while (!string.Equals(x509Certificate2.Thumbprint, Name, StringComparison.OrdinalIgnoreCase));
					}
					else
					{
						zero = NativeMethods.CertFindCertificateInStore(this.storeHandle.Handle, NativeMethods.CertOpenStoreEncodingType.X509_ASN_ENCODING, 0, NativeMethods.CertFindType.CERT_FIND_HASH_STR, Name, IntPtr.Zero);
					}
				}
				return zero;
			}
			else
			{
				throw Marshal.GetExceptionForHR(-2146885628);
			}
		}

		public IntPtr GetFirstCert(CertificateFilterInfo filter)
		{
			this.filterHandle = null;
			if (X509NativeStore.fIsWin8AndAbove && filter != null)
			{
				IntPtr zero = IntPtr.Zero;
				this.filterHandle = new CertificateFilterHandle();
				int num = NativeMethods.CCFindCertificateBuildFilter(filter.FilterString, ref zero);
				if (num == 0)
				{
					this.filterHandle.Handle = zero;
				}
				else
				{
					this.filterHandle = null;
					throw new Win32Exception(num);
				}
			}
			return this.GetNextCert(IntPtr.Zero);
		}

		public IntPtr GetNextCert(IntPtr certContext)
		{
			if (this.open)
			{
				if (!this.Valid)
				{
					certContext = IntPtr.Zero;
				}
				else
				{
					if (this.filterHandle == null)
					{
						certContext = NativeMethods.CertEnumCertificatesInStore(this.storeHandle.Handle, certContext);
					}
					else
					{
						certContext = NativeMethods.CCFindCertificateFromFilter(this.storeHandle.Handle, this.filterHandle.Handle, certContext);
					}
				}
				return certContext;
			}
			else
			{
				throw Marshal.GetExceptionForHR(-2146885628);
			}
		}

		public void Open(bool includeArchivedCerts)
		{
			if (this.storeHandle != null && this.archivedCerts != includeArchivedCerts)
			{
				this.storeHandle = null;
			}
			if (this.storeHandle == null)
			{
				this.valid = false;
				this.open = false;
				NativeMethods.CertOpenStoreFlags certOpenStoreFlag = NativeMethods.CertOpenStoreFlags.CERT_STORE_SHARE_STORE_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_SHARE_CONTEXT_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_OPEN_EXISTING_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_MAXIMUM_ALLOWED_FLAG;
				if (includeArchivedCerts)
				{
					certOpenStoreFlag = certOpenStoreFlag | NativeMethods.CertOpenStoreFlags.CERT_STORE_ENUM_ARCHIVED_FLAG;
				}
				StoreLocation location = this.storeLocation.Location;
				switch (location)
				{
					case StoreLocation.CurrentUser:
					{
						certOpenStoreFlag = certOpenStoreFlag | NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_CURRENT_USER;
						break;
					}
					case StoreLocation.LocalMachine:
					{
						certOpenStoreFlag = certOpenStoreFlag | NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
						break;
					}
				}
				IntPtr intPtr = NativeMethods.CertOpenStore(NativeMethods.CertOpenStoreProvider.CERT_STORE_PROV_SYSTEM, NativeMethods.CertOpenStoreEncodingType.X509_ASN_ENCODING, IntPtr.Zero, certOpenStoreFlag, this.storeName);
				if (IntPtr.Zero != intPtr)
				{
					this.storeHandle = new CertificateStoreHandle();
					this.storeHandle.Handle = intPtr;
					if (string.Equals(this.storeName, "UserDS", StringComparison.OrdinalIgnoreCase) || NativeMethods.CertControlStore(this.storeHandle.Handle, 0, NativeMethods.CertControlStoreType.CERT_STORE_CTRL_AUTO_RESYNC, IntPtr.Zero))
					{
						this.valid = true;
						this.open = true;
						this.archivedCerts = includeArchivedCerts;
					}
					else
					{
						this.storeHandle = null;
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				else
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
		}
	}
}