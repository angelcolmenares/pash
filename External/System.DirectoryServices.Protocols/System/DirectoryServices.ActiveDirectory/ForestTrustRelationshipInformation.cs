using System;
using System.Collections;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ForestTrustRelationshipInformation : TrustRelationshipInformation
	{
		private TopLevelNameCollection topLevelNames;

		private StringCollection excludedNames;

		private ForestTrustDomainInfoCollection domainInfo;

		private ArrayList binaryData;

		private Hashtable excludedNameTime;

		private ArrayList binaryDataTime;

		internal bool retrieved;

		public StringCollection ExcludedTopLevelNames
		{
			get
			{
				if (!this.retrieved)
				{
					this.GetForestTrustInfoHelper();
				}
				return this.excludedNames;
			}
		}

		public TopLevelNameCollection TopLevelNames
		{
			get
			{
				if (!this.retrieved)
				{
					this.GetForestTrustInfoHelper();
				}
				return this.topLevelNames;
			}
		}

		public ForestTrustDomainInfoCollection TrustedDomainInformation
		{
			get
			{
				if (!this.retrieved)
				{
					this.GetForestTrustInfoHelper();
				}
				return this.domainInfo;
			}
		}

		internal ForestTrustRelationshipInformation(DirectoryContext context, string source, DS_DOMAIN_TRUSTS unmanagedTrust, TrustType type)
		{
			string str;
			this.topLevelNames = new TopLevelNameCollection();
			this.excludedNames = new StringCollection();
			this.domainInfo = new ForestTrustDomainInfoCollection();
			this.binaryData = new ArrayList();
			this.excludedNameTime = new Hashtable();
			this.binaryDataTime = new ArrayList();
			string stringUni = null;
			string stringUni1 = null;
			this.context = context;
			this.source = source;
			if (unmanagedTrust.DnsDomainName != (IntPtr)0)
			{
				stringUni = Marshal.PtrToStringUni(unmanagedTrust.DnsDomainName);
			}
			if (unmanagedTrust.NetbiosDomainName != (IntPtr)0)
			{
				stringUni1 = Marshal.PtrToStringUni(unmanagedTrust.NetbiosDomainName);
			}
			ForestTrustRelationshipInformation forestTrustRelationshipInformation = this;
			if (stringUni == null)
			{
				str = stringUni1;
			}
			else
			{
				str = stringUni;
			}
			forestTrustRelationshipInformation.target = str;
			if ((unmanagedTrust.Flags & 2) == 0 || (unmanagedTrust.Flags & 32) == 0)
			{
				if ((unmanagedTrust.Flags & 2) == 0)
				{
					if ((unmanagedTrust.Flags & 32) != 0)
					{
						this.direction = TrustDirection.Inbound;
					}
				}
				else
				{
					this.direction = TrustDirection.Outbound;
				}
			}
			else
			{
				this.direction = TrustDirection.Bidirectional;
			}
			this.type = type;
		}

		private void GetForestTrustInfoHelper()
		{
			IntPtr intPtr = (IntPtr)0;
			bool flag = false;
			IntPtr hGlobalUni = (IntPtr)0;
			TopLevelNameCollection topLevelNameCollection = new TopLevelNameCollection();
			StringCollection stringCollections = new StringCollection();
			ForestTrustDomainInfoCollection forestTrustDomainInfoCollection = new ForestTrustDomainInfoCollection();
			ArrayList arrayLists = new ArrayList();
			Hashtable hashtables = new Hashtable();
			ArrayList arrayLists1 = new ArrayList();
			try
			{
				try
				{
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni = Marshal.StringToHGlobalUni(base.TargetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni);
					string policyServerName = Utils.GetPolicyServerName(this.context, true, false, this.source);
					flag = Utils.Impersonate(this.context);
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					int num = UnsafeNativeMethods.LsaQueryForestTrustInformation(policySafeHandle, lSAUNICODESTRING, ref intPtr);
					if (num != 0)
					{
						int winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
						if (winError != 0)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
					}
					try
					{
						if (intPtr != (IntPtr)0)
						{
							LSA_FOREST_TRUST_INFORMATION lSAFORESTTRUSTINFORMATION = new LSA_FOREST_TRUST_INFORMATION();
							Marshal.PtrToStructure(intPtr, lSAFORESTTRUSTINFORMATION);
							int recordCount = lSAFORESTTRUSTINFORMATION.RecordCount;
							for (int i = 0; i < recordCount; i++)
							{
								IntPtr intPtr1 = Marshal.ReadIntPtr(lSAFORESTTRUSTINFORMATION.Entries, i * Marshal.SizeOf(typeof(IntPtr)));
								LSA_FOREST_TRUST_RECORD lSAFORESTTRUSTRECORD = new LSA_FOREST_TRUST_RECORD();
								Marshal.PtrToStructure(intPtr1, lSAFORESTTRUSTRECORD);
								if (lSAFORESTTRUSTRECORD.ForestTrustType != LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelName)
								{
									if (lSAFORESTTRUSTRECORD.ForestTrustType != LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelNameEx)
									{
										if (lSAFORESTTRUSTRECORD.ForestTrustType != LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustDomainInfo)
										{
											if (lSAFORESTTRUSTRECORD.ForestTrustType != LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustRecordTypeLast)
											{
												int length = lSAFORESTTRUSTRECORD.Data.Length;
												byte[] numArray = new byte[length];
												if (lSAFORESTTRUSTRECORD.Data.Buffer != (IntPtr)0 && length != 0)
												{
													Marshal.Copy(lSAFORESTTRUSTRECORD.Data.Buffer, numArray, 0, length);
												}
												arrayLists.Add(numArray);
												arrayLists1.Add(lSAFORESTTRUSTRECORD.Time);
											}
										}
										else
										{
											ForestTrustDomainInformation forestTrustDomainInformation = new ForestTrustDomainInformation(lSAFORESTTRUSTRECORD.Flags, lSAFORESTTRUSTRECORD.DomainInfo, lSAFORESTTRUSTRECORD.Time);
											forestTrustDomainInfoCollection.Add(forestTrustDomainInformation);
										}
									}
									else
									{
										IntPtr intPtr2 = (IntPtr)((long)intPtr1 + (long)16);
										Marshal.PtrToStructure(intPtr2, lSAFORESTTRUSTRECORD.TopLevelName);
										string stringUni = Marshal.PtrToStringUni(lSAFORESTTRUSTRECORD.TopLevelName.Buffer, lSAFORESTTRUSTRECORD.TopLevelName.Length / 2);
										stringCollections.Add(stringUni);
										hashtables.Add(stringUni, lSAFORESTTRUSTRECORD.Time);
									}
								}
								else
								{
									IntPtr intPtr3 = (IntPtr)((long)intPtr1 + (long)16);
									Marshal.PtrToStructure(intPtr3, lSAFORESTTRUSTRECORD.TopLevelName);
									TopLevelName topLevelName = new TopLevelName(lSAFORESTTRUSTRECORD.Flags, lSAFORESTTRUSTRECORD.TopLevelName, lSAFORESTTRUSTRECORD.Time);
									topLevelNameCollection.Add(topLevelName);
								}
							}
						}
					}
					finally
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr);
					}
					this.topLevelNames = topLevelNameCollection;
					this.excludedNames = stringCollections;
					this.domainInfo = forestTrustDomainInfoCollection;
					this.binaryData = arrayLists;
					this.excludedNameTime = hashtables;
					this.binaryDataTime = arrayLists1;
					this.retrieved = true;
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
				}
			}
			catch
			{
				throw;
			}
		}

		public void Save()
		{
			IntPtr intPtr;
			IntPtr hGlobalUni;
			object length;
			object obj;
			object length1;
			object obj1;
			int count = 0;
			int num = 0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			ArrayList arrayLists = new ArrayList();
			ArrayList arrayLists1 = new ArrayList();
			bool flag = false;
			IntPtr hGlobalUni1 = (IntPtr)0;
			IntPtr intPtr3 = (IntPtr)0;
			count = count + this.TopLevelNames.Count;
			count = count + this.ExcludedTopLevelNames.Count;
			count = count + this.TrustedDomainInformation.Count;
			if (this.binaryData.Count != 0)
			{
				count++;
				count = count + this.binaryData.Count;
			}
			IntPtr intPtr4 = Marshal.AllocHGlobal(count * Marshal.SizeOf(typeof(IntPtr)));
			try
			{
				try
				{
					intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
					UnsafeNativeMethods.GetSystemTimeAsFileTime(intPtr3);
					FileTime fileTime = new FileTime();
					Marshal.PtrToStructure(intPtr3, fileTime);
					for (int i = 0; i < this.topLevelNames.Count; i++)
					{
						LSA_FOREST_TRUST_RECORD lSAFORESTTRUSTRECORD = new LSA_FOREST_TRUST_RECORD();
						lSAFORESTTRUSTRECORD.Flags = (int)this.topLevelNames[i].Status;
						lSAFORESTTRUSTRECORD.ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelName;
						TopLevelName item = this.topLevelNames[i];
						lSAFORESTTRUSTRECORD.Time = item.time;
						lSAFORESTTRUSTRECORD.TopLevelName = new LSA_UNICODE_STRING();
						hGlobalUni = Marshal.StringToHGlobalUni(item.Name);
						arrayLists.Add(hGlobalUni);
						UnsafeNativeMethods.RtlInitUnicodeString(lSAFORESTTRUSTRECORD.TopLevelName, hGlobalUni);
						intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
						arrayLists.Add(intPtr);
						Marshal.StructureToPtr(lSAFORESTTRUSTRECORD, intPtr, false);
						Marshal.WriteIntPtr(intPtr4, Marshal.SizeOf(typeof(IntPtr)) * num, intPtr);
						num++;
					}
					for (int j = 0; j < this.excludedNames.Count; j++)
					{
						LSA_FOREST_TRUST_RECORD lARGEINTEGER = new LSA_FOREST_TRUST_RECORD();
						lARGEINTEGER.Flags = 0;
						lARGEINTEGER.ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelNameEx;
						if (!this.excludedNameTime.Contains(this.excludedNames[j]))
						{
							lARGEINTEGER.Time = new LARGE_INTEGER();
							lARGEINTEGER.Time.lowPart = fileTime.lower;
							lARGEINTEGER.Time.highPart = fileTime.higher;
						}
						else
						{
							lARGEINTEGER.Time = (LARGE_INTEGER)this.excludedNameTime[(object)j];
						}
						lARGEINTEGER.TopLevelName = new LSA_UNICODE_STRING();
						hGlobalUni = Marshal.StringToHGlobalUni(this.excludedNames[j]);
						arrayLists.Add(hGlobalUni);
						UnsafeNativeMethods.RtlInitUnicodeString(lARGEINTEGER.TopLevelName, hGlobalUni);
						intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
						arrayLists.Add(intPtr);
						Marshal.StructureToPtr(lARGEINTEGER, intPtr, false);
						Marshal.WriteIntPtr(intPtr4, Marshal.SizeOf(typeof(IntPtr)) * num, intPtr);
						num++;
					}
					int num1 = 0;
					while (num1 < this.domainInfo.Count)
					{
						LSA_FOREST_TRUST_RECORD status = new LSA_FOREST_TRUST_RECORD();
						status.Flags = (int)this.domainInfo[num1].Status;
						status.ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustDomainInfo;
						ForestTrustDomainInformation forestTrustDomainInformation = this.domainInfo[num1];
						status.Time = forestTrustDomainInformation.time;
						IntPtr intPtr5 = (IntPtr)0;
						IntPtr hGlobalUni2 = Marshal.StringToHGlobalUni(forestTrustDomainInformation.DomainSid);
						arrayLists.Add(hGlobalUni2);
						int sidW = UnsafeNativeMethods.ConvertStringSidToSidW(hGlobalUni2, ref intPtr5);
						if (sidW != 0)
						{
							status.DomainInfo = new LSA_FOREST_TRUST_DOMAIN_INFO();
							status.DomainInfo.sid = intPtr5;
							arrayLists1.Add(intPtr5);
							status.DomainInfo.DNSNameBuffer = Marshal.StringToHGlobalUni(forestTrustDomainInformation.DnsName);
							arrayLists.Add(status.DomainInfo.DNSNameBuffer);
							LSA_FOREST_TRUST_DOMAIN_INFO domainInfo = status.DomainInfo;
							if (forestTrustDomainInformation.DnsName == null)
							{
								length = null;
							}
							else
							{
								length = forestTrustDomainInformation.DnsName.Length * 2;
							}
							domainInfo.DNSNameLength = (short)length;
							LSA_FOREST_TRUST_DOMAIN_INFO lSAFORESTTRUSTDOMAININFO = status.DomainInfo;
							if (forestTrustDomainInformation.DnsName == null)
							{
								obj = null;
							}
							else
							{
								obj = forestTrustDomainInformation.DnsName.Length * 2;
							}
							lSAFORESTTRUSTDOMAININFO.DNSNameMaximumLength = (short)obj;
							status.DomainInfo.NetBIOSNameBuffer = Marshal.StringToHGlobalUni(forestTrustDomainInformation.NetBiosName);
							arrayLists.Add(status.DomainInfo.NetBIOSNameBuffer);
							LSA_FOREST_TRUST_DOMAIN_INFO domainInfo1 = status.DomainInfo;
							if (forestTrustDomainInformation.NetBiosName == null)
							{
								length1 = null;
							}
							else
							{
								length1 = forestTrustDomainInformation.NetBiosName.Length * 2;
							}
							domainInfo1.NetBIOSNameLength = (short)length1;
							LSA_FOREST_TRUST_DOMAIN_INFO lSAFORESTTRUSTDOMAININFO1 = status.DomainInfo;
							if (forestTrustDomainInformation.NetBiosName == null)
							{
								obj1 = null;
							}
							else
							{
								obj1 = forestTrustDomainInformation.NetBiosName.Length * 2;
							}
							lSAFORESTTRUSTDOMAININFO1.NetBIOSNameMaximumLength = (short)obj1;
							intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
							arrayLists.Add(intPtr);
							Marshal.StructureToPtr(status, intPtr, false);
							Marshal.WriteIntPtr(intPtr4, Marshal.SizeOf(typeof(IntPtr)) * num, intPtr);
							num++;
							num1++;
						}
						else
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
						}
					}
					if (this.binaryData.Count > 0)
					{
						LSA_FOREST_TRUST_RECORD lSAFORESTTRUSTRECORD1 = new LSA_FOREST_TRUST_RECORD();
						lSAFORESTTRUSTRECORD1.Flags = 0;
						lSAFORESTTRUSTRECORD1.ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustRecordTypeLast;
						intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
						arrayLists.Add(intPtr);
						Marshal.StructureToPtr(lSAFORESTTRUSTRECORD1, intPtr, false);
						Marshal.WriteIntPtr(intPtr4, Marshal.SizeOf(typeof(IntPtr)) * num, intPtr);
						num++;
						for (int k = 0; k < this.binaryData.Count; k++)
						{
							LSA_FOREST_TRUST_RECORD item1 = new LSA_FOREST_TRUST_RECORD();
							item1.Flags = 0;
							item1.Time = (LARGE_INTEGER)this.binaryDataTime[k];
							item1.Data.Length = (int)((byte[])this.binaryData[k]).Length;
							if (item1.Data.Length != 0)
							{
								item1.Data.Buffer = Marshal.AllocHGlobal(item1.Data.Length);
								arrayLists.Add(item1.Data.Buffer);
								Marshal.Copy((byte[])this.binaryData[k], 0, item1.Data.Buffer, item1.Data.Length);
							}
							else
							{
								item1.Data.Buffer = (IntPtr)0;
							}
							intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
							arrayLists.Add(intPtr);
							Marshal.StructureToPtr(item1, intPtr, false);
							Marshal.WriteIntPtr(intPtr4, Marshal.SizeOf(typeof(IntPtr)) * num, intPtr);
							num++;
						}
					}
					LSA_FOREST_TRUST_INFORMATION lSAFORESTTRUSTINFORMATION = new LSA_FOREST_TRUST_INFORMATION();
					lSAFORESTTRUSTINFORMATION.RecordCount = count;
					lSAFORESTTRUSTINFORMATION.Entries = intPtr4;
					intPtr1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_INFORMATION)));
					Marshal.StructureToPtr(lSAFORESTTRUSTINFORMATION, intPtr1, false);
					string policyServerName = Utils.GetPolicyServerName(this.context, true, true, base.SourceName);
					flag = Utils.Impersonate(this.context);
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni1 = Marshal.StringToHGlobalUni(base.TargetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni1);
					int num2 = UnsafeNativeMethods.LsaSetForestTrustInformation(policySafeHandle, lSAUNICODESTRING, intPtr1, 1, out intPtr2);
					if (num2 == 0)
					{
						if (intPtr2 == (IntPtr)0)
						{
							num2 = UnsafeNativeMethods.LsaSetForestTrustInformation(policySafeHandle, lSAUNICODESTRING, intPtr1, 0, out intPtr2);
							if (num2 == 0)
							{
								this.retrieved = false;
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num2, policyServerName);
							}
						}
						else
						{
							throw ExceptionHelper.CreateForestTrustCollisionException(intPtr2);
						}
					}
					else
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num2), policyServerName);
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					for (int l = 0; l < arrayLists.Count; l++)
					{
						Marshal.FreeHGlobal((IntPtr)arrayLists[l]);
					}
					for (int m = 0; m < arrayLists1.Count; m++)
					{
						UnsafeNativeMethods.LocalFree((IntPtr)arrayLists1[m]);
					}
					if (intPtr4 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr4);
					}
					if (intPtr1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr1);
					}
					if (intPtr2 != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr2);
					}
					if (hGlobalUni1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni1);
					}
					if (intPtr3 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr3);
					}
				}
			}
			catch
			{
				throw;
			}
		}
	}
}