using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class SidList
	{
		private List<SidListEntry> entries;

		public SidListEntry this[int index]
		{
			get
			{
				return this.entries[index];
			}
		}

		public int Length
		{
			get
			{
				return this.entries.Count;
			}
		}

		internal SidList(List<byte[]> sidListByteFormat) : this(sidListByteFormat, null, null)
		{
		}

		internal SidList(List<byte[]> sidListByteFormat, string target, NetCred credentials)
		{
			this.entries = new List<SidListEntry>();
			IntPtr zero = IntPtr.Zero;
			int count = sidListByteFormat.Count;
			IntPtr[] intPtr = new IntPtr[count];
			for (int i = 0; i < count; i++)
			{
				intPtr[i] = Utils.ConvertByteArrayToIntPtr(sidListByteFormat[i]);
			}
			try
			{
				if (credentials != null)
				{
					Utils.BeginImpersonation(credentials, out zero);
				}
				this.TranslateSids(target, intPtr);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Utils.EndImpersonation(zero);
				}
			}
		}

		internal SidList(UnsafeNativeMethods.SID_AND_ATTR[] sidAndAttr)
		{
			this.entries = new List<SidListEntry>();
			int length = (int)sidAndAttr.Length;
			IntPtr[] intPtrArray = new IntPtr[length];
			for (int i = 0; i < length; i++)
			{
				intPtrArray[i] = sidAndAttr[i].pSid;
			}
			this.TranslateSids(null, intPtrArray);
		}

		public void Clear()
		{
			foreach (SidListEntry entry in this.entries)
			{
				entry.Dispose();
			}
			this.entries.Clear();
		}

		public void RemoveAt(int index)
		{
			this.entries[index].Dispose();
			this.entries.RemoveAt(index);
		}

		protected void TranslateSids(string target, IntPtr[] pSids)
		{
			if ((int)pSids.Length != 0)
			{
				int length = (int)pSids.Length;
				IntPtr zero = IntPtr.Zero;
				IntPtr intPtr = IntPtr.Zero;
				IntPtr zero1 = IntPtr.Zero;
				IntPtr intPtr1 = IntPtr.Zero;
				try
				{
					UnsafeNativeMethods.LSA_OBJECT_ATTRIBUTES lSAOBJECTATTRIBUTE = new UnsafeNativeMethods.LSA_OBJECT_ATTRIBUTES();
					zero = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafeNativeMethods.LSA_OBJECT_ATTRIBUTES)));
					Marshal.StructureToPtr(lSAOBJECTATTRIBUTE, zero, false);
					int num = 0;
					if (target != null)
					{
						UnsafeNativeMethods.LSA_UNICODE_STRING_Managed lSAUNICODESTRINGManaged = new UnsafeNativeMethods.LSA_UNICODE_STRING_Managed();
						lSAUNICODESTRINGManaged.buffer = target;
						lSAUNICODESTRINGManaged.length = (ushort)(target.Length * 2);
						lSAUNICODESTRINGManaged.maximumLength = lSAUNICODESTRINGManaged.length;
						IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafeNativeMethods.LSA_UNICODE_STRING)));
						try
						{
							Marshal.StructureToPtr(lSAUNICODESTRINGManaged, intPtr2, false);
							num = UnsafeNativeMethods.LsaOpenPolicy(intPtr2, zero, 0x800, ref intPtr);
						}
						finally
						{
							if (intPtr2 != IntPtr.Zero)
							{
								UnsafeNativeMethods.LSA_UNICODE_STRING structure = (UnsafeNativeMethods.LSA_UNICODE_STRING)Marshal.PtrToStructure(intPtr2, typeof(UnsafeNativeMethods.LSA_UNICODE_STRING));
								if (structure.buffer != IntPtr.Zero)
								{
									Marshal.FreeHGlobal(structure.buffer);
									structure.buffer = IntPtr.Zero;
								}
								Marshal.FreeHGlobal(intPtr2);
							}
						}
					}
					else
					{
						num = UnsafeNativeMethods.LsaOpenPolicy(IntPtr.Zero, zero, 0x800, ref intPtr);
					}
					if (num == 0)
					{
						num = UnsafeNativeMethods.LsaLookupSids(intPtr, length, pSids, out zero1, out intPtr1);
						if (num == 0)
						{
							UnsafeNativeMethods.LSA_TRANSLATED_NAME[] lSATRANSLATEDNAMEArray = new UnsafeNativeMethods.LSA_TRANSLATED_NAME[length];
							IntPtr intPtr3 = intPtr1;
							for (int i = 0; i < length; i++)
							{
								lSATRANSLATEDNAMEArray[i] = (UnsafeNativeMethods.LSA_TRANSLATED_NAME)Marshal.PtrToStructure(intPtr3, typeof(UnsafeNativeMethods.LSA_TRANSLATED_NAME));
								intPtr3 = new IntPtr(intPtr3.ToInt64() + (long)Marshal.SizeOf(typeof(UnsafeNativeMethods.LSA_TRANSLATED_NAME)));
							}
							UnsafeNativeMethods.LSA_REFERENCED_DOMAIN_LIST lSAREFERENCEDDOMAINLIST = (UnsafeNativeMethods.LSA_REFERENCED_DOMAIN_LIST)Marshal.PtrToStructure(zero1, typeof(UnsafeNativeMethods.LSA_REFERENCED_DOMAIN_LIST));
							int num1 = lSAREFERENCEDDOMAINLIST.entries;
							UnsafeNativeMethods.LSA_TRUST_INFORMATION[] lSATRUSTINFORMATIONArray = new UnsafeNativeMethods.LSA_TRUST_INFORMATION[num1];
							IntPtr intPtr4 = lSAREFERENCEDDOMAINLIST.domains;
							for (int j = 0; j < num1; j++)
							{
								lSATRUSTINFORMATIONArray[j] = (UnsafeNativeMethods.LSA_TRUST_INFORMATION)Marshal.PtrToStructure(intPtr4, typeof(UnsafeNativeMethods.LSA_TRUST_INFORMATION));
								intPtr4 = new IntPtr(intPtr4.ToInt64() + (long)Marshal.SizeOf(typeof(UnsafeNativeMethods.LSA_TRUST_INFORMATION)));
							}
							for (int k = 0; k < (int)lSATRANSLATEDNAMEArray.Length; k++)
							{
								UnsafeNativeMethods.LSA_TRANSLATED_NAME lSATRANSLATEDNAME = lSATRANSLATEDNAMEArray[k];
								UnsafeNativeMethods.LSA_TRUST_INFORMATION lSATRUSTINFORMATION = lSATRUSTINFORMATIONArray[lSATRANSLATEDNAME.domainIndex];
								SidListEntry sidListEntry = new SidListEntry();
								sidListEntry.name = Marshal.PtrToStringUni(lSATRANSLATEDNAME.name.buffer, lSATRANSLATEDNAME.name.length / 2);
								sidListEntry.sidIssuerName = Marshal.PtrToStringUni(lSATRUSTINFORMATION.name.buffer, lSATRUSTINFORMATION.name.length / 2);
								sidListEntry.pSid = pSids[k];
								this.entries.Add(sidListEntry);
							}
						}
						else
						{
							object[] winError = new object[1];
							winError[0] = SafeNativeMethods.LsaNtStatusToWinError(num);
							throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.AuthZErrorEnumeratingGroups, winError));
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = SafeNativeMethods.LsaNtStatusToWinError(num);
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.AuthZErrorEnumeratingGroups, objArray));
					}
				}
				finally
				{
					if (zero1 != IntPtr.Zero)
					{
						UnsafeNativeMethods.LsaFreeMemory(zero1);
					}
					if (intPtr1 != IntPtr.Zero)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr1);
					}
					if (intPtr != IntPtr.Zero)
					{
						UnsafeNativeMethods.LsaClose(intPtr);
					}
					if (zero != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(zero);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}