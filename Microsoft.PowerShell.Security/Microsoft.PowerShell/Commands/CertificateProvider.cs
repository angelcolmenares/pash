using Microsoft.PowerShell.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.PowerShell.Commands
{
	[CmdletProvider("Certificate", ProviderCapabilities.ShouldProcess)]
	[OutputType(new Type[] { typeof(X509StoreLocation), typeof(X509Certificate2) }, ProviderCmdlet="Get-Item")]
	[OutputType(new Type[] { typeof(string), typeof(PathInfo) }, ProviderCmdlet="Resolve-Path")]
	[OutputType(new Type[] { typeof(PathInfo) }, ProviderCmdlet="Push-Location")]
	[OutputType(new Type[] { typeof(X509Store), typeof(X509Certificate2) }, ProviderCmdlet="Get-ChildItem")]
	public sealed class CertificateProvider : NavigationCmdletProvider, ICmdletProviderSupportsHelp
	{
		private const string certPathPattern = "^\\\\((?<StoreLocation>CurrentUser|LocalMachine)(\\\\(?<StoreName>[a-zA-Z]+)(\\\\(?<Thumbprint>[0-9a-f]{40}))?)?)?$";

		[TraceSource("CertificateProvider", "The core command provider for certificates")]
		private readonly static PSTraceSource tracer;

		private bool _hasAttemptedToLoadPkiModule;

		private static object staticLock;

		private static List<X509StoreLocation> storeLocations;

		private static Hashtable pathCache;

		private static bool fIsWin8AndAbove;

		private readonly static char[] pathSeparators;

		private static X509NativeStore storeCache;

		private static Regex certPathRegex;

		private static Regex CertPathRegex
		{
			get
			{
				lock (CertificateProvider.staticLock)
				{
					if (CertificateProvider.certPathRegex == null)
					{
						CertificateProvider.certPathRegex = new Regex("^\\\\((?<StoreLocation>CurrentUser|LocalMachine)(\\\\(?<StoreName>[a-zA-Z]+)(\\\\(?<Thumbprint>[0-9a-f]{40}))?)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
					}
				}
				return CertificateProvider.certPathRegex;
			}
		}

		static CertificateProvider()
		{
			CertificateProvider.tracer = PSTraceSource.GetTracer("CertificateProvider", "The core command provider for certificates");
			CertificateProvider.staticLock = new object();
			CertificateProvider.storeLocations = null;
			CertificateProvider.pathCache = null;
			CertificateProvider.fIsWin8AndAbove = DownLevelHelper.IsWin8AndAbove();
			char[] chrArray = new char[2];
			chrArray[0] = '/';
			chrArray[1] = '\\';
			CertificateProvider.pathSeparators = chrArray;
			CertificateProvider.storeCache = null;
			CertificateProvider.certPathRegex = null;
		}

		public CertificateProvider()
		{
			lock (CertificateProvider.staticLock)
			{
				if (CertificateProvider.storeLocations == null)
				{
					CertificateProvider.pathCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
					CertificateProvider.storeLocations = new List<X509StoreLocation>();
					X509StoreLocation x509StoreLocation = new X509StoreLocation(StoreLocation.CurrentUser);
					CertificateProvider.storeLocations.Add(x509StoreLocation);
					CertificateProvider.AddItemToCache(StoreLocation.CurrentUser.ToString(), x509StoreLocation);
					X509StoreLocation x509StoreLocation1 = new X509StoreLocation(StoreLocation.LocalMachine);
					CertificateProvider.storeLocations.Add(x509StoreLocation1);
					CertificateProvider.AddItemToCache(StoreLocation.LocalMachine.ToString(), x509StoreLocation1);
					CertificateProvider.AddItemToCache("", CertificateProvider.storeLocations);
				}
			}
		}

		private static void AddItemToCache(string path, object item)
		{
			lock (CertificateProvider.staticLock)
			{
				if (item != null && !CertificateProvider.pathCache.ContainsKey(path))
				{
					CertificateProvider.pathCache.Add(path, item);
				}
			}
		}

		private void AttemptToImportPkiModule()
		{
			if (Runspace.DefaultRunspace != null)
			{
				CommandInfo cmdletInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand));
				Command command = new Command(cmdletInfo);
				object[] objArray = new object[1];
				objArray[0] = "pki";
				CertificateProvider.tracer.WriteLine("Attempting to load module: {0}", objArray);
				try
				{
                    System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand(command).AddParameter("Name", "pki").AddParameter("Scope", "GLOBAL").AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false);
					powerShell.Invoke();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
				}
				this._hasAttemptedToLoadPkiModule = true;
				return;
			}
			else
			{
				return;
			}
		}

		private void CommitUserDS(IntPtr storeHandle)
		{
			if (NativeMethods.CertControlStore(storeHandle, 0, NativeMethods.CertControlStoreType.CERT_STORE_CTRL_COMMIT, IntPtr.Zero))
			{
				return;
			}
			else
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		private static ErrorRecord CreateErrorRecord(string path, CertificateProviderItem itemType)
		{
			Exception certificateNotFoundException = null;
			string certificateNotFound = null;
			CertificateProviderItem certificateProviderItem = itemType;
			switch (certificateProviderItem)
			{
				case CertificateProviderItem.Certificate:
				{
					certificateNotFound = CertificateProviderStrings.CertificateNotFound;
					break;
				}
				case CertificateProviderItem.Store:
				{
					certificateNotFound = CertificateProviderStrings.CertificateStoreNotFound;
					break;
				}
				case CertificateProviderItem.StoreLocation:
				{
					certificateNotFound = CertificateProviderStrings.CertificateStoreLocationNotFound;
					break;
				}
				default:
				{
					certificateNotFound = CertificateProviderStrings.InvalidPath;
					break;
				}
			}
			object[] objArray = new object[1];
			objArray[0] = path;
			certificateNotFound = string.Format(CultureInfo.CurrentCulture, certificateNotFound, objArray);
			ErrorDetails errorDetail = new ErrorDetails(certificateNotFound);
			CertificateProviderItem certificateProviderItem1 = itemType;
			switch (certificateProviderItem1)
			{
				case CertificateProviderItem.Certificate:
				{
					certificateNotFoundException = new CertificateNotFoundException(certificateNotFound);
					break;
				}
				case CertificateProviderItem.Store:
				{
					certificateNotFoundException = new CertificateStoreNotFoundException(certificateNotFound);
					break;
				}
				case CertificateProviderItem.StoreLocation:
				{
					certificateNotFoundException = new CertificateStoreLocationNotFoundException(certificateNotFound);
					break;
				}
				default:
				{
					certificateNotFoundException = new ArgumentException(certificateNotFound);
					break;
				}
			}
			ErrorRecord errorRecord = new ErrorRecord(certificateNotFoundException, "CertProviderItemNotFound", ErrorCategory.ObjectNotFound, null);
			errorRecord.ErrorDetails = errorDetail;
			return errorRecord;
		}

		private unsafe void DoDeleteKey(IntPtr pProvInfo)
		{
			int num;
			IntPtr zero = IntPtr.Zero;
			NativeMethods.CRYPT_KEY_PROV_INFO structure = (NativeMethods.CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(pProvInfo, typeof(NativeMethods.CRYPT_KEY_PROV_INFO));
			IntPtr ownerWindow = DetectUIHelper.GetOwnerWindow(base.Host);
			if (structure.dwProvType == 0)
			{
				int num1 = 0;
				IntPtr intPtr = IntPtr.Zero;
				IntPtr zero1 = IntPtr.Zero;
				if ((structure.dwFlags & 32) != 0)
				{
					num1 = 32;
				}
				if (ownerWindow == IntPtr.Zero || (structure.dwFlags & 64) != 0)
				{
					num1 = num1 | 64;
				}
				try
				{
					int num2 = NativeMethods.NCryptOpenStorageProvider(ref intPtr, structure.pwszProvName, 0);
					if (num2 != 0)
					{
						this.ThrowErrorRemoting(num2);
					}
					num2 = NativeMethods.NCryptOpenKey(intPtr, ref zero1, structure.pwszContainerName, structure.dwKeySpec, num1);
					if (num2 != 0)
					{
						this.ThrowErrorRemoting(num2);
					}
					if ((num1 & 64) != 0)
					{
						void* pointer = ownerWindow.ToPointer();
						NativeMethods.NCryptSetProperty(intPtr, "HWND Handle", ref pointer, sizeof(void*) , 0);
					}
					num2 = NativeMethods.NCryptDeleteKey(zero1, 0);
					if (num2 != 0)
					{
						this.ThrowErrorRemoting(num2);
					}
					zero1 = IntPtr.Zero;
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						NativeMethods.NCryptFreeObject(intPtr);
					}
					if (zero1 != IntPtr.Zero)
					{
						NativeMethods.NCryptFreeObject(zero1);
					}
				}
			}
			else
			{
				if (ownerWindow != IntPtr.Zero && NativeMethods.CryptAcquireContext(ref zero, structure.pwszContainerName, structure.pwszProvName, (int)structure.dwProvType, -268435456))
				{
					void* voidPointer = ownerWindow.ToPointer();
					NativeMethods.CryptSetProvParam(zero, NativeMethods.ProviderParam.PP_CLIENT_HWND, ref voidPointer, 0);
					NativeMethods.CryptReleaseContext(zero, 0);
				}
				IntPtr intPtrPointer = zero;
				string str = structure.pwszContainerName;
				string str1 = structure.pwszProvName;
				NativeMethods.PROV pROV = structure.dwProvType;
				int num3 = structure.dwFlags | 16;
				if (ownerWindow == IntPtr.Zero)
				{
					num = 64;
				}
				else
				{
					num = 0;
				}
				if (!NativeMethods.CryptAcquireContext(ref intPtrPointer, str, str1, (int)pROV, (num3 | num)))
				{
					this.ThrowErrorRemoting(Marshal.GetLastWin32Error());
					return;
				}
			}
		}

		private void DoMove(string destination, X509Certificate2 cert, X509NativeStore store, string sourcePath)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = NativeMethods.CertDuplicateCertificateContext(cert.Handle);
			if (intPtr != IntPtr.Zero)
			{
				if (NativeMethods.CertAddCertificateContextToStore(store.StoreHandle, cert.Handle, 4, ref zero))
				{
					if (NativeMethods.CertDeleteCertificateFromStore(intPtr))
					{
						if (DownLevelHelper.IsWin8AndAbove())
						{
							bool location = store.Location.Location == StoreLocation.LocalMachine;
							if (cert.HasPrivateKey && string.Equals(store.StoreName, "MY", StringComparison.OrdinalIgnoreCase))
							{
								NativeMethods.LogCertCopy(location, cert.Handle);
							}
							NativeMethods.LogCertDelete(location, cert.Handle);
						}
						if (destination.Contains("UserDS"))
						{
							this.CommitUserDS(store.StoreHandle);
						}
						if (sourcePath.Contains("UserDS"))
						{
							NativeMethods.CERT_CONTEXT structure = (NativeMethods.CERT_CONTEXT)Marshal.PtrToStructure(cert.Handle, typeof(NativeMethods.CERT_CONTEXT));
							this.CommitUserDS(structure.hCertStore);
						}
						X509Certificate2 x509Certificate2 = new X509Certificate2(zero);
						string certName = CertificateProvider.GetCertName(x509Certificate2);
						string str = this.MakePath(destination, certName);
						base.WriteItemObject(x509Certificate2, str, false);
						return;
					}
					else
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				else
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			else
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		private void DoRemove(X509Certificate2 cert, bool fDeleteKey, bool fMachine, string sourcePath)
		{
			int num = 0;
			IntPtr zero = IntPtr.Zero;
			bool flag = false;
			try
			{
				if (fDeleteKey)
				{
					if (NativeMethods.CertGetCertificateContextProperty(cert.Handle, NativeMethods.CertPropertyId.CERT_KEY_PROV_INFO_PROP_ID, IntPtr.Zero, ref num))
					{
						zero = Marshal.AllocHGlobal(num);
						if (NativeMethods.CertGetCertificateContextProperty(cert.Handle, NativeMethods.CertPropertyId.CERT_KEY_PROV_INFO_PROP_ID, zero, ref num))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						string verboseNoPrivateKey = CertificateProviderStrings.VerboseNoPrivateKey;
						base.WriteVerbose(verboseNoPrivateKey);
					}
				}
				if (NativeMethods.CertDeleteCertificateFromStore(NativeMethods.CertDuplicateCertificateContext(cert.Handle)))
				{
					if (sourcePath.Contains("UserDS"))
					{
						NativeMethods.CERT_CONTEXT structure = (NativeMethods.CERT_CONTEXT)Marshal.PtrToStructure(cert.Handle, typeof(NativeMethods.CERT_CONTEXT));
						this.CommitUserDS(structure.hCertStore);
					}
					if (DownLevelHelper.IsWin8AndAbove())
					{
						NativeMethods.LogCertDelete(fMachine, cert.Handle);
					}
					if (fDeleteKey && flag)
					{
						this.DoDeleteKey(zero);
					}
				}
				else
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
			}
		}

		private static string EnsureDriveIsRooted(string path)
		{
			string str = path;
			int num = path.IndexOf(':');
			if (num == -1)
			{
				if (path.Length == 0 || path[0] != '\\')
				{
					str = string.Concat((char)92, path);
				}
			}
			else
			{
				if (num + 1 == path.Length)
				{
					str = string.Concat(path, (char)92);
				}
			}
			object[] objArray = new object[1];
			objArray[0] = str;
			CertificateProvider.tracer.WriteLine("result = {0}", objArray);
			return str;
		}

		private static object GetCachedItem(string path)
		{
			object item = null;
			lock (CertificateProvider.staticLock)
			{
				if (CertificateProvider.pathCache.ContainsKey(path))
				{
					item = CertificateProvider.pathCache[path];
				}
			}
			return item;
		}

		private void GetCertificatesOrNames(string path, string[] pathElements, bool returnNames, CertificateFilterInfo filter)
		{
			object obj;
			X509NativeStore store = this.GetStore(path, pathElements);
			store.Open(this.IncludeArchivedCerts());
			for (IntPtr i = store.GetFirstCert(filter); IntPtr.Zero != i; i = store.GetNextCert(i))
			{
				X509Certificate2 x509Certificate2 = new X509Certificate2(i);
				if (CertificateProvider.fIsWin8AndAbove || CertificateProvider.MatchesFilter(x509Certificate2, filter))
				{
					string certName = CertificateProvider.GetCertName(x509Certificate2);
					string str = this.MakePath(path, certName);
					if (!returnNames)
					{
						X509Certificate2 x509Certificate21 = new X509Certificate2(x509Certificate2);
						PSObject pSObject = new PSObject(x509Certificate21);
						obj = pSObject;
					}
					else
					{
						obj = certName;
					}
					base.WriteItemObject(obj, str, false);
				}
			}
		}

		private static string GetCertName(X509Certificate2 cert)
		{
			return cert.Thumbprint;
		}

		protected override void GetChildItems(string path, bool recurse)
		{
			path = CertificateProvider.NormalizePath(path);
			this.GetChildItemsOrNames(path, recurse, ReturnContainers.ReturnAllContainers, false, this.GetFilter());
		}

		protected override object GetChildItemsDynamicParameters(string path, bool recurse)
		{
			if (!CertificateProvider.fIsWin8AndAbove)
			{
				return new CertificateProviderCodeSigningDynamicParameters();
			}
			else
			{
				return new CertificateProviderDynamicParameters();
			}
		}

		private void GetChildItemsOrNames(string path, bool recurse, ReturnContainers returnContainers, bool returnNames, CertificateFilterInfo filter)
		{
			object locationName;
			Utils.CheckArgForNull(path, "path");
			if (path.Length != 0)
			{
				string[] pathElements = CertificateProvider.GetPathElements(path);
				if ((int)pathElements.Length != 1)
				{
					if ((int)pathElements.Length != 2)
					{
						this.ThrowItemNotFound(path, CertificateProviderItem.Certificate);
					}
					else
					{
						this.GetCertificatesOrNames(path, pathElements, returnNames, filter);
						return;
					}
				}
				else
				{
					this.GetStoresOrNames(pathElements[0], recurse, returnNames, filter);
					return;
				}
			}
			else
			{
				foreach (X509StoreLocation storeLocation in CertificateProvider.storeLocations)
				{
					if (returnNames)
					{
						locationName = storeLocation.LocationName;
					}
					else
					{
						locationName = storeLocation;
					}
					object obj = locationName;
					if (filter == null || returnNames)
					{
						base.WriteItemObject(obj, storeLocation.LocationName, true);
					}
					string str = storeLocation.LocationName;
					if (!recurse)
					{
						continue;
					}
					this.GetChildItemsOrNames(str, recurse, returnContainers, returnNames, filter);
				}
			}
		}

		protected override string GetChildName(string path)
		{
			if (path == null || path.Length != 0)
			{
				return this.MyGetChildName(path);
			}
			else
			{
				return path;
			}
		}

		protected override void GetChildNames(string path, ReturnContainers returnContainers)
		{
			path = CertificateProvider.NormalizePath(path);
			this.GetChildItemsOrNames(path, false, returnContainers, true, this.GetFilter());
		}

		private CertificateFilterInfo GetFilter()
		{
			CertificateFilterInfo certificateFilterInfo = null;
			if (base.DynamicParameters != null)
			{
				if (!CertificateProvider.fIsWin8AndAbove)
				{
					CertificateProviderCodeSigningDynamicParameters dynamicParameters = base.DynamicParameters as CertificateProviderCodeSigningDynamicParameters;
					if (dynamicParameters != null && dynamicParameters.CodeSigningCert)
					{
						certificateFilterInfo = new CertificateFilterInfo();
						certificateFilterInfo.Purpose = CertificatePurpose.CodeSigning;
					}
				}
				else
				{
					CertificateProviderDynamicParameters certificateProviderDynamicParameter = base.DynamicParameters as CertificateProviderDynamicParameters;
					if (certificateProviderDynamicParameter != null)
					{
						bool flag = false;
						certificateFilterInfo = new CertificateFilterInfo();
						if (certificateProviderDynamicParameter.CodeSigningCert)
						{
							certificateFilterInfo.Purpose = CertificatePurpose.CodeSigning;
							flag = true;
						}
						if (certificateProviderDynamicParameter.SSLServerAuthentication)
						{
							certificateFilterInfo.SSLServerAuthentication = true;
							flag = true;
						}
						DnsNameRepresentation dnsName = certificateProviderDynamicParameter.DnsName;
						if (dnsName.Punycode != null)
						{
							DnsNameRepresentation dnsNameRepresentation = certificateProviderDynamicParameter.DnsName;
							certificateFilterInfo.DnsName = dnsNameRepresentation.Punycode;
							flag = true;
						}
						if (certificateProviderDynamicParameter.Eku != null)
						{
							certificateFilterInfo.Eku = certificateProviderDynamicParameter.Eku;
							flag = true;
						}
						if (certificateProviderDynamicParameter.ExpiringInDays >= 0)
						{
							certificateFilterInfo.ExpiringInDays = certificateProviderDynamicParameter.ExpiringInDays;
							flag = true;
						}
						if (!flag)
						{
							certificateFilterInfo = null;
						}
					}
				}
			}
			return certificateFilterInfo;
		}

		protected override void GetItem(string path)
		{
			bool flag = false;
			path = CertificateProvider.NormalizePath(path);
			object itemAtPath = this.GetItemAtPath(path, out flag);
			CertificateFilterInfo filter = this.GetFilter();
			if (itemAtPath != null)
			{
				if (flag)
				{
					if (filter == null)
					{
						X509StoreLocation x509StoreLocation = itemAtPath as X509StoreLocation;
						if (x509StoreLocation == null)
						{
							X509NativeStore x509NativeStore = itemAtPath as X509NativeStore;
							if (x509NativeStore != null)
							{
								X509Store x509Store = new X509Store(x509NativeStore.StoreName, x509NativeStore.Location.Location);
								base.WriteItemObject(x509Store, path, flag);
							}
						}
						else
						{
							base.WriteItemObject(itemAtPath, path, flag);
							return;
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					if (filter != null)
					{
						X509Certificate2 x509Certificate2 = itemAtPath as X509Certificate2;
						if (CertificateProvider.fIsWin8AndAbove || CertificateProvider.MatchesFilter(x509Certificate2, filter))
						{
							base.WriteItemObject(itemAtPath, path, flag);
							return;
						}
					}
					else
					{
						base.WriteItemObject(itemAtPath, path, flag);
						return;
					}
				}
			}
		}

		private object GetItemAtPath(string path, out bool isContainer)
		{
			X509NativeStore store;
			bool length;
			Utils.CheckArgForNull(path, "path");
			string[] pathElements = CertificateProvider.GetPathElements(path);
			if ((int)pathElements.Length < 0)
			{
				length = false;
			}
			else
			{
				length = (int)pathElements.Length <= 2;
			}
            isContainer = length;
			if ((int)pathElements.Length > 3 || (int)pathElements.Length < 0)
			{
				this.ThrowItemNotFound(path, CertificateProviderItem.Certificate);
			}
			object cachedItem = CertificateProvider.GetCachedItem(path);
			if (cachedItem == null)
			{
				int num = (int)pathElements.Length;
				switch (num)
				{
					case 1:
					{
						this.ThrowItemNotFound(path, CertificateProviderItem.StoreLocation);
						break;
					}
					case 2:
					{
						store = this.GetStore(path, pathElements);
						cachedItem = store;
						break;
					}
					case 3:
					{
						string parentPath = this.GetParentPath(path, "");
						string[] strArrays = CertificateProvider.GetPathElements(parentPath);
						store = this.GetStore(parentPath, strArrays);
						store.Open(this.IncludeArchivedCerts());
						IntPtr certByName = store.GetCertByName(pathElements[2]);
						if (IntPtr.Zero == certByName)
						{
							this.ThrowItemNotFound(path, CertificateProviderItem.Certificate);
						}
						cachedItem = new X509Certificate2(certByName);
						store.FreeCert(certByName);
						break;
					}
				}
			}
			return cachedItem;
		}

		protected override object GetItemDynamicParameters(string path)
		{
			if (!CertificateProvider.fIsWin8AndAbove)
			{
				return new CertificateProviderCodeSigningDynamicParameters();
			}
			else
			{
				return new CertificateProviderDynamicParameters();
			}
		}

		protected override string GetParentPath(string path, string root)
		{
			string parentPath = base.GetParentPath(path, root);
			return parentPath;
		}

		private static string[] GetPathElements(string path)
		{
			string[] strArrays = path.Split(CertificateProvider.pathSeparators);
			Stack<string> strs = new Stack<string>();
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				if (!(str == ".") && !(str == string.Empty))
				{
					if (str != "..")
					{
						strs.Push(str);
					}
					else
					{
						if (strs.Count > 0)
						{
							strs.Pop();
						}
					}
				}
			}
			string[] array = strs.ToArray();
			Array.Reverse(array);
			return array;
		}

		private X509NativeStore GetStore(string path, string[] pathElements)
		{
			X509StoreLocation storeLocation = this.GetStoreLocation(pathElements[0]);
			X509NativeStore store = this.GetStore(path, pathElements[1], storeLocation);
			if (store == null)
			{
				this.ThrowItemNotFound(path, CertificateProviderItem.Store);
			}
			return store;
		}

		private X509NativeStore GetStore(string storePath, string storeName, X509StoreLocation storeLocation)
		{
			if (!storeLocation.StoreNames.ContainsKey(storeName))
			{
				this.ThrowItemNotFound(storePath, CertificateProviderItem.Store);
			}
			if (CertificateProvider.storeCache != null && (CertificateProvider.storeCache.Location != storeLocation || !string.Equals(CertificateProvider.storeCache.StoreName, storeName, StringComparison.OrdinalIgnoreCase)))
			{
				CertificateProvider.storeCache = null;
			}
			if (CertificateProvider.storeCache == null)
			{
				CertificateProvider.storeCache = new X509NativeStore(storeLocation, storeName);
			}
			return CertificateProvider.storeCache;
		}

		private X509StoreLocation GetStoreLocation(string path)
		{
			X509StoreLocation cachedItem = CertificateProvider.GetCachedItem(path) as X509StoreLocation;
			if (cachedItem == null)
			{
				this.ThrowItemNotFound(path, CertificateProviderItem.StoreLocation);
			}
			return cachedItem;
		}

		private void GetStoresOrNames(string path, bool recurse, bool returnNames, CertificateFilterInfo filter)
		{
			object obj;
			X509StoreLocation storeLocation = this.GetStoreLocation(path);
			foreach (string key in storeLocation.StoreNames.Keys)
			{
				string str = this.MakePath(path, key);
				if (!returnNames)
				{
					X509NativeStore store = this.GetStore(str, key, storeLocation);
					X509Store x509Store = new X509Store(store.StoreName, store.Location.Location);
					obj = x509Store;
				}
				else
				{
					obj = key;
				}
				if (filter == null || returnNames)
				{
					base.WriteItemObject(obj, key, true);
				}
				if (!recurse)
				{
					continue;
				}
				string[] pathElements = CertificateProvider.GetPathElements(str);
				this.GetCertificatesOrNames(str, pathElements, returnNames, filter);
			}
		}

		protected override bool HasChildItems(string path)
		{
			bool count = false;
			Utils.CheckArgForNull(path, "path");
			path = CertificateProvider.NormalizePath(path);
			if (path.Length != 0)
			{
				bool flag = false;
				object itemAtPath = this.GetItemAtPath(path, out flag);
				if (itemAtPath != null && flag)
				{
					X509StoreLocation x509StoreLocation = itemAtPath as X509StoreLocation;
					if (x509StoreLocation == null)
					{
						X509NativeStore x509NativeStore = itemAtPath as X509NativeStore;
						if (x509NativeStore != null)
						{
							x509NativeStore.Open(this.IncludeArchivedCerts());
							IntPtr firstCert = x509NativeStore.GetFirstCert(null);
							if (IntPtr.Zero != firstCert)
							{
								x509NativeStore.FreeCert(firstCert);
								count = true;
							}
						}
					}
					else
					{
						count = x509StoreLocation.StoreNames.Count > 0;
					}
				}
				return count;
			}
			else
			{
				return true;
			}
		}

		private bool IncludeArchivedCerts()
		{
			bool flag = false;
			if (base.Force)
			{
				flag = true;
			}
			return flag;
		}

		protected override Collection<PSDriveInfo> InitializeDefaultDrives()
		{
			string certProvidername = CertificateProviderStrings.CertProvidername;
			PSDriveInfo pSDriveInfo = new PSDriveInfo("Cert", base.ProviderInfo, "\\", certProvidername, null);
			Collection<PSDriveInfo> pSDriveInfos = new Collection<PSDriveInfo>();
			pSDriveInfos.Add(pSDriveInfo);
			return pSDriveInfos;
		}

		protected override void InvokeDefaultAction(string path)
		{
			path = CertificateProvider.NormalizePath(path);
			string actionInvoke = CertificateProviderStrings.Action_Invoke;
			string str = "certmgr.msc";
			string str1 = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32");
			if (base.ShouldProcess(path, actionInvoke))
			{
				Process.Start(Path.Combine(str1, str));
			}
		}

		protected override bool IsItemContainer(string path)
		{
			path = CertificateProvider.NormalizePath(path);
			Utils.CheckArgForNull(path, "path");
			bool flag = false;
			if (path.Length != 0)
			{
				this.GetItemAtPath(path, out flag);
			}
			else
			{
				flag = true;
			}
			object[] objArray = new object[1];
			objArray[0] = flag;
			CertificateProvider.tracer.WriteLine("result = {0}", objArray);
			return flag;
		}

		protected override bool IsValidPath(string path)
		{
			path = CertificateProvider.NormalizePath(path);
			path = CertificateProvider.EnsureDriveIsRooted(path);
			bool success = CertificateProvider.CertPathRegex.Match(path).Success;
			return success;
		}

		protected override bool ItemExists(string path)
		{
			bool flag;
			if (!this._hasAttemptedToLoadPkiModule)
			{
				this.AttemptToImportPkiModule();
			}
			Utils.CheckArgForNull(path, "path");
			bool flag1 = false;
			object itemAtPath = null;
			path = CertificateProvider.NormalizePath(path);
			if (path.Length != 0)
			{
				try
				{
					itemAtPath = this.GetItemAtPath(path, out flag1);
				}
				catch (ProviderInvocationException providerInvocationException1)
				{
					ProviderInvocationException providerInvocationException = providerInvocationException1;
					if (providerInvocationException.InnerException as CertificateProviderItemNotFoundException == null)
					{
						throw;
					}
				}
				flag = itemAtPath != null;
			}
			else
			{
				flag = true;
			}
			object[] objArray = new object[1];
			objArray[0] = flag;
			CertificateProvider.tracer.WriteLine("result = {0}", objArray);
			return flag;
		}

		private static bool MatchesFilter(X509Certificate2 cert, CertificateFilterInfo filter)
		{
			if (filter == null || filter.Purpose == CertificatePurpose.All)
			{
				return true;
			}
			else
			{
				CertificatePurpose purpose = filter.Purpose;
				if (purpose != CertificatePurpose.CodeSigning || !SecuritySupport.CertIsGoodForSigning(cert))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		protected override void MoveItem(string path, string destination)
		{
			path = CertificateProvider.NormalizePath(path);
			destination = CertificateProvider.NormalizePath(destination);
			string[] pathElements = CertificateProvider.GetPathElements(path);
			string[] strArrays = CertificateProvider.GetPathElements(destination);
			bool flag = false;
			object itemAtPath = this.GetItemAtPath(path, out flag);
			if (flag)
			{
				string cannotMoveContainer = CertificateProviderStrings.CannotMoveContainer;
				string str = "CannotMoveContainer";
				this.ThrowInvalidOperation(str, cannotMoveContainer);
			}
			if ((int)strArrays.Length != 2)
			{
				if ((int)strArrays.Length != 3 || !string.Equals(pathElements[2], strArrays[2], StringComparison.OrdinalIgnoreCase))
				{
					string invalidDestStore = CertificateProviderStrings.InvalidDestStore;
					string str1 = "InvalidDestStore";
					this.ThrowInvalidOperation(str1, invalidDestStore);
				}
				else
				{
					destination = Path.GetDirectoryName(destination);
				}
			}
			if (!string.Equals(pathElements[0], strArrays[0], StringComparison.OrdinalIgnoreCase))
			{
				string cannotMoveCrossContext = CertificateProviderStrings.CannotMoveCrossContext;
				string str2 = "CannotMoveCrossContext";
				this.ThrowInvalidOperation(str2, cannotMoveCrossContext);
			}
			if (string.Equals(pathElements[1], strArrays[1], StringComparison.OrdinalIgnoreCase))
			{
				string cannotMoveToSameStore = CertificateProviderStrings.CannotMoveToSameStore;
				string str3 = "CannotMoveToSameStore";
				this.ThrowInvalidOperation(str3, cannotMoveToSameStore);
			}
			if (DetectUIHelper.GetOwnerWindow(base.Host) == IntPtr.Zero && (string.Equals(pathElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase) && string.Equals(pathElements[1], "ROOT", StringComparison.OrdinalIgnoreCase) || string.Equals(strArrays[0], "CurrentUser", StringComparison.OrdinalIgnoreCase) && string.Equals(strArrays[1], "ROOT", StringComparison.OrdinalIgnoreCase)))
			{
				string uINotAllowed = CertificateProviderStrings.UINotAllowed;
				string str4 = "UINotAllowed";
				this.ThrowInvalidOperation(str4, uINotAllowed);
			}
			if (itemAtPath == null)
			{
				this.ThrowItemNotFound(path, CertificateProviderItem.Certificate);
			}
			else
			{
				bool flag1 = false;
				object obj = this.GetItemAtPath(destination, out flag1);
				X509Certificate2 x509Certificate2 = itemAtPath as X509Certificate2;
				X509NativeStore x509NativeStore = obj as X509NativeStore;
				if (x509NativeStore != null)
				{
					x509NativeStore.Open(true);
					string actionMove = CertificateProviderStrings.Action_Move;
					object[] objArray = new object[2];
					objArray[0] = path;
					objArray[1] = destination;
					string str5 = string.Format(CultureInfo.CurrentCulture, CertificateProviderStrings.MoveItemTemplate, objArray);
					if (base.ShouldProcess(str5, actionMove))
					{
						this.DoMove(destination, x509Certificate2, x509NativeStore, path);
						return;
					}
				}
			}
		}

		private string MyGetChildName(string path)
		{
			string str;
			if (!string.IsNullOrEmpty(path))
			{
				path = path.Replace('/', '\\');
				char[] chrArray = new char[1];
				chrArray[0] = '\\';
				path = path.TrimEnd(chrArray);
				int num = path.LastIndexOf('\\');
				if (num != -1)
				{
					str = path.Substring(num + 1);
				}
				else
				{
					str = path;
				}
				return str;
			}
			else
			{
				throw PSTraceSource.NewArgumentException("path");
			}
		}

		protected override void NewItem(string path, string type, object value)
		{
			if (!this._hasAttemptedToLoadPkiModule)
			{
				this.AttemptToImportPkiModule();
			}
			path = CertificateProvider.NormalizePath(path);
			string[] pathElements = CertificateProvider.GetPathElements(path);
			if ((int)pathElements.Length != 2)
			{
				string cannotCreateItem = CertificateProviderStrings.CannotCreateItem;
				string str = "CannotCreateItem";
				this.ThrowInvalidOperation(str, cannotCreateItem);
			}
			bool flag = string.Equals(pathElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase);
			if (flag)
			{
				string cannotCreateUserStore = CertificateProviderStrings.CannotCreateUserStore;
				string str1 = "CannotCreateUserStore";
				this.ThrowInvalidOperation(str1, cannotCreateUserStore);
			}
			NativeMethods.CertOpenStoreFlags certOpenStoreFlag = NativeMethods.CertOpenStoreFlags.CERT_STORE_CREATE_NEW_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_MAXIMUM_ALLOWED_FLAG | NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
			IntPtr intPtr = NativeMethods.CertOpenStore(NativeMethods.CertOpenStoreProvider.CERT_STORE_PROV_SYSTEM, NativeMethods.CertOpenStoreEncodingType.X509_ASN_ENCODING, IntPtr.Zero, certOpenStoreFlag, pathElements[1]);
			if (IntPtr.Zero != intPtr)
			{
				NativeMethods.CertCloseStore(intPtr, 0);
				X509Store x509Store = new X509Store(pathElements[1], StoreLocation.LocalMachine);
				base.WriteItemObject(x509Store, path, true);
				return;
			}
			else
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		private static string NormalizePath(string path)
		{
			if (path.Length > 0)
			{
				char chr = path[path.Length - 1];
				if (chr == '/' || chr == '\\')
				{
					path = path.Substring(0, path.Length - 1);
				}
				string[] pathElements = CertificateProvider.GetPathElements(path);
				path = string.Join("\\", pathElements);
			}
			return path;
		}

		private void RemoveCertItem(X509Certificate2 cert, bool fDeleteKey, bool fMachine, string sourcePath)
		{
			string actionRemove;
			if (cert != null)
			{
				if (!fDeleteKey)
				{
					actionRemove = CertificateProviderStrings.Action_Remove;
				}
				else
				{
					actionRemove = CertificateProviderStrings.Action_RemoveAndDeleteKey;
				}
				object[] objArray = new object[1];
				objArray[0] = sourcePath;
				string str = string.Format(CultureInfo.CurrentCulture, CertificateProviderStrings.RemoveItemTemplate, objArray);
				if (base.ShouldProcess(str, actionRemove))
				{
					this.DoRemove(cert, fDeleteKey, fMachine, sourcePath);
				}
			}
		}

		private void RemoveCertStore(string storeName, bool fDeleteKey, string sourcePath)
		{
			string str = NativeMethods.CryptFindLocalizedName(storeName);
			string[] pathElements = CertificateProvider.GetPathElements(sourcePath);
			if (str != null)
			{
				object[] objArray = new object[1];
				objArray[0] = storeName;
				string str1 = string.Format(CultureInfo.CurrentCulture, CertificateProviderStrings.RemoveStoreTemplate, objArray);
				string str2 = "CannotRemoveSystemStore";
				this.ThrowInvalidOperation(str2, str1);
				return;
			}
			else
			{
				X509NativeStore store = this.GetStore(sourcePath, pathElements);
				store.Open(this.IncludeArchivedCerts());
				for (IntPtr i = store.GetFirstCert(null); IntPtr.Zero != i; i = store.GetNextCert(i))
				{
					X509Certificate2 x509Certificate2 = new X509Certificate2(i);
					string str3 = string.Concat(sourcePath, x509Certificate2.Thumbprint);
					this.RemoveCertItem(x509Certificate2, fDeleteKey, true, str3);
				}
				NativeMethods.CertOpenStoreFlags certOpenStoreFlag = NativeMethods.CertOpenStoreFlags.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_DELETE_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_READONLY_FLAG | NativeMethods.CertOpenStoreFlags.CERT_STORE_OPEN_EXISTING_FLAG | NativeMethods.CertOpenStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
				NativeMethods.CertOpenStore(NativeMethods.CertOpenStoreProvider.CERT_STORE_PROV_SYSTEM, NativeMethods.CertOpenStoreEncodingType.X509_ASN_ENCODING, IntPtr.Zero, certOpenStoreFlag, storeName);
				return;
			}
		}

		protected override void RemoveItem(string path, bool recurse)
		{
			path = CertificateProvider.NormalizePath(path);
			bool flag = false;
			bool flag1 = false;
			object itemAtPath = this.GetItemAtPath(path, out flag);
			string[] pathElements = CertificateProvider.GetPathElements(path);
			bool flag2 = string.Equals(pathElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase);
			if (DetectUIHelper.GetOwnerWindow(base.Host) == IntPtr.Zero && flag2 && string.Equals(pathElements[1], "ROOT", StringComparison.OrdinalIgnoreCase))
			{
				string uINotAllowed = CertificateProviderStrings.UINotAllowed;
				string str = "UINotAllowed";
				this.ThrowInvalidOperation(str, uINotAllowed);
			}
			if (base.DynamicParameters != null)
			{
				ProviderRemoveItemDynamicParameters dynamicParameters = base.DynamicParameters as ProviderRemoveItemDynamicParameters;
				if (dynamicParameters != null && dynamicParameters.DeleteKey)
				{
					flag1 = true;
				}
			}
			if (!flag)
			{
				X509Certificate2 x509Certificate2 = itemAtPath as X509Certificate2;
				this.RemoveCertItem(x509Certificate2, flag1, !flag2, path);
				return;
			}
			else
			{
				if ((int)pathElements.Length != 2)
				{
					string cannotRemoveContainer = CertificateProviderStrings.CannotRemoveContainer;
					string str1 = "CannotRemoveContainer";
					this.ThrowInvalidOperation(str1, cannotRemoveContainer);
					return;
				}
				else
				{
					if (flag2)
					{
						string cannotDeleteUserStore = CertificateProviderStrings.CannotDeleteUserStore;
						string str2 = "CannotDeleteUserStore";
						this.ThrowInvalidOperation(str2, cannotDeleteUserStore);
					}
					this.RemoveCertStore(pathElements[1], flag1, path);
					return;
				}
			}
		}

		protected override object RemoveItemDynamicParameters(string path, bool recurse)
		{
			return new ProviderRemoveItemDynamicParameters();
		}

		string System.Management.Automation.Provider.ICmdletProviderSupportsHelp.GetHelpMaml(string helpItemName, string path)
		{
			string empty;
			string str = null;
			string str1 = null;
			try
			{
				if (string.IsNullOrEmpty(helpItemName))
				{
					empty = string.Empty;
				}
				else
				{
					CmdletInfo.SplitCmdletName(helpItemName, out str, out str1);
					if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str1))
					{
						empty = string.Empty;
					}
					else
					{
						XmlDocument xmlDocument = new XmlDocument();
						CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
						string str2 = Path.Combine(base.ProviderInfo.ApplicationBase, currentUICulture.ToString(), base.ProviderInfo.HelpFile);
						XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
						xmlReaderSetting.XmlResolver = null;
						XmlReader xmlReader = XmlReader.Create(str2, xmlReaderSetting);
						xmlDocument.Load(xmlReader);
						XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
						xmlNamespaceManagers.AddNamespace("command", HelpCommentsParser.commandURI);
						object[] objArray = new object[3];
						objArray[0] = string.Empty;
						objArray[1] = str;
						objArray[2] = str1;
						string str3 = string.Format(CultureInfo.InvariantCulture, HelpCommentsParser.ProviderHelpCommandXPath, objArray);
						XmlNode xmlNodes = xmlDocument.SelectSingleNode(str3, xmlNamespaceManagers);
						if (xmlNodes == null)
						{
							return string.Empty;
						}
						else
						{
							empty = xmlNodes.OuterXml;
						}
					}
				}
			}
			catch (XmlException xmlException)
			{
				empty = string.Empty;
			}
			catch (PathTooLongException pathTooLongException)
			{
				empty = string.Empty;
			}
			catch (IOException oException)
			{
				empty = string.Empty;
			}
			catch (UnauthorizedAccessException unauthorizedAccessException)
			{
				empty = string.Empty;
			}
			catch (NotSupportedException notSupportedException)
			{
				empty = string.Empty;
			}
			catch (SecurityException securityException)
			{
				empty = string.Empty;
			}
			catch (XPathException xPathException)
			{
				empty = string.Empty;
			}
			return empty;
		}

		private void ThrowErrorRemoting(int stat)
		{
			if (!base.Host.Name.Equals("ServerRemoteHost", StringComparison.OrdinalIgnoreCase))
			{
				throw new Win32Exception(stat);
			}
			else
			{
				Exception win32Exception = new Win32Exception(stat);
				string message = win32Exception.Message;
				string remoteErrorMessage = CertificateProviderStrings.RemoteErrorMessage;
				message = string.Concat(message, remoteErrorMessage);
				Exception exception = new Exception(message);
				base.ThrowTerminatingError(new ErrorRecord(exception, "RemotingFailure", ErrorCategory.NotSpecified, null));
				return;
			}
		}

		private void ThrowInvalidOperation(string errorId, string message)
		{
			ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(message), errorId, ErrorCategory.InvalidOperation, null);
			errorRecord.ErrorDetails = new ErrorDetails(message);
			base.ThrowTerminatingError(errorRecord);
		}

		private void ThrowItemNotFound(string path, CertificateProviderItem itemType)
		{
			ErrorRecord errorRecord = CertificateProvider.CreateErrorRecord(path, itemType);
			base.ThrowTerminatingError(errorRecord);
		}
	}
}