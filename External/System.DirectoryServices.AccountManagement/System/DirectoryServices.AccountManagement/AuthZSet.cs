using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace System.DirectoryServices.AccountManagement
{
	internal class AuthZSet : ResultSet
	{
		private AuthZSet.SafeMemoryPtr psUserSid;

		private AuthZSet.SafeMemoryPtr psMachineSid;

		private StoreCtx userStoreCtx;

		private NetCred credentials;

		private ContextOptions contextOptions;

		private object userCtxBase;

		private ContextType userType;

		private string flatUserAuthority;

		private int currentGroup;

		private SidList groupSidList;

		private AuthZSet.SafeMemoryPtr psBuffer;

		private bool disposed;

		private Hashtable contexts;

		private bool? localMachineIsDC;

		internal override object CurrentAsPrincipal
		{
			[SecurityCritical]
			get
			{
				GroupPrincipal groupPrincipal;
				IntPtr item = this.groupSidList[this.currentGroup].pSid;
				byte[] byteArray = Utils.ConvertNativeSidToByteArray(item);
				SidType sidType = Utils.ClassifySID(item);
				if (sidType != SidType.FakeObject)
				{
					string str = null;
					if (sidType != SidType.RealObjectFakeDomain)
					{
						bool flag = false;
						bool flag1 = UnsafeNativeMethods.EqualDomainSid(this.psUserSid.DangerousGetHandle(), item, ref flag);
						if (!flag1)
						{
							flag = false;
						}
						if (flag)
						{
							str = this.flatUserAuthority;
						}
					}
					else
					{
						str = this.flatUserAuthority;
					}
					if (str == null)
					{
						str = this.groupSidList[this.currentGroup].sidIssuerName;
					}
					bool value = false;
					if (this.userType != ContextType.Machine)
					{
						bool flag2 = false;
						if (UnsafeNativeMethods.EqualDomainSid(this.psMachineSid.DangerousGetHandle(), item, ref flag2) && flag2)
						{
							if (!this.localMachineIsDC.HasValue)
							{
								this.localMachineIsDC = new bool?(Utils.IsMachineDC(null));
							}
							value = !this.localMachineIsDC.Value;
						}
					}
					else
					{
						value = true;
					}
					if (!value)
					{
						PrincipalContext context = SDSCache.Domain.GetContext(str, this.credentials, this.contextOptions);
						IdentityReference identityReference = new IdentityReference();
						SecurityIdentifier securityIdentifier = new SecurityIdentifier(byteArray, 0);
						identityReference.UrnScheme = "ms-sid";
						identityReference.UrnValue = securityIdentifier.ToString();
						groupPrincipal = (GroupPrincipal)((ADStoreCtx)context.QueryCtx).FindPrincipalBySID(typeof(GroupPrincipal), identityReference, true);
					}
					else
					{
						PrincipalContext principalContext = SDSCache.LocalMachine.GetContext(str, this.credentials, DefaultContextOptions.MachineDefaultContextOption);
						SecurityIdentifier securityIdentifier1 = new SecurityIdentifier(byteArray, 0);
						groupPrincipal = GroupPrincipal.FindByIdentity(principalContext, IdentityType.Sid, securityIdentifier1.ToString());
					}
					if (groupPrincipal != null)
					{
						return groupPrincipal;
					}
					else
					{
						throw new NoMatchingPrincipalException(StringResources.AuthZCantFindGroup);
					}
				}
				else
				{
					return this.userStoreCtx.ConstructFakePrincipalFromSID(byteArray);
				}
			}
		}

		[SecurityCritical]
		internal AuthZSet(byte[] userSid, NetCred credentials, ContextOptions contextOptions, string flatUserAuthority, StoreCtx userStoreCtx, object userCtxBase)
		{
			this.currentGroup = -1;
			this.contexts = new Hashtable();
			this.localMachineIsDC = null;
			this.userType = userStoreCtx.OwningContext.ContextType;
			this.userCtxBase = userCtxBase;
			this.userStoreCtx = userStoreCtx;
			this.credentials = credentials;
			this.contextOptions = contextOptions;
			this.flatUserAuthority = flatUserAuthority;
			this.contexts[flatUserAuthority] = userStoreCtx.OwningContext;
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			try
			{
				try
				{
					UnsafeNativeMethods.LUID lUID = new UnsafeNativeMethods.LUID();
					lUID.low = 0;
					lUID.high = 0;
					this.psMachineSid = new AuthZSet.SafeMemoryPtr(Utils.GetMachineDomainSid());
					this.psUserSid = new AuthZSet.SafeMemoryPtr(Utils.ConvertByteArrayToIntPtr(userSid));
					int lastWin32Error = 0;
					bool flag = UnsafeNativeMethods.AuthzInitializeResourceManager(UnsafeNativeMethods.AUTHZ_RM_FLAG.AUTHZ_RM_FLAG_NO_AUDIT, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, null, out intPtr);
					if (!flag)
					{
						lastWin32Error = Marshal.GetLastWin32Error();
					}
					else
					{
						flag = UnsafeNativeMethods.AuthzInitializeContextFromSid(0, this.psUserSid.DangerousGetHandle(), intPtr, IntPtr.Zero, lUID, IntPtr.Zero, out zero);
						if (!flag)
						{
							lastWin32Error = Marshal.GetLastWin32Error();
						}
						else
						{
							int num = 0;
							flag = UnsafeNativeMethods.AuthzGetInformationFromContext(zero, 2, 0, out num, IntPtr.Zero);
							if (flag || num <= 0 || Marshal.GetLastWin32Error() != 122)
							{
								lastWin32Error = Marshal.GetLastWin32Error();
							}
							else
							{
								zero1 = Marshal.AllocHGlobal(num);
								flag = UnsafeNativeMethods.AuthzGetInformationFromContext(zero, 2, num, out num, zero1);
								if (!flag)
								{
									lastWin32Error = Marshal.GetLastWin32Error();
								}
								else
								{
									UnsafeNativeMethods.TOKEN_GROUPS structure = (UnsafeNativeMethods.TOKEN_GROUPS)Marshal.PtrToStructure(zero1, typeof(UnsafeNativeMethods.TOKEN_GROUPS));
									int num1 = structure.groupCount;
									UnsafeNativeMethods.SID_AND_ATTR[] sIDANDATTRArray = new UnsafeNativeMethods.SID_AND_ATTR[num1];
									IntPtr intPtr1 = new IntPtr(zero1.ToInt64() + (long)Marshal.SizeOf(typeof(UnsafeNativeMethods.TOKEN_GROUPS)) - (long)Marshal.SizeOf(typeof(IntPtr)));
									for (int i = 0; i < num1; i++)
									{
										sIDANDATTRArray[i] = (UnsafeNativeMethods.SID_AND_ATTR)Marshal.PtrToStructure(intPtr1, typeof(UnsafeNativeMethods.SID_AND_ATTR));
										intPtr1 = new IntPtr(intPtr1.ToInt64() + (long)Marshal.SizeOf(typeof(UnsafeNativeMethods.SID_AND_ATTR)));
									}
									this.groupSidList = new SidList(sIDANDATTRArray);
								}
							}
						}
					}
					if (flag)
					{
						this.psBuffer = new AuthZSet.SafeMemoryPtr(zero1);
						zero1 = IntPtr.Zero;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = lastWin32Error;
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.AuthZFailedToRetrieveGroupList, objArray));
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (this.psBuffer != null && !this.psBuffer.IsInvalid)
					{
						this.psBuffer.Close();
					}
					if (this.psUserSid != null && !this.psUserSid.IsInvalid)
					{
						this.psUserSid.Close();
					}
					if (this.psMachineSid != null && !this.psMachineSid.IsInvalid)
					{
						this.psMachineSid.Close();
					}
					if (exception as DllNotFoundException == null)
					{
						if (exception as EntryPointNotFoundException == null)
						{
							throw;
						}
						else
						{
							throw new NotSupportedException(StringResources.AuthZNotSupported, exception);
						}
					}
					else
					{
						throw new NotSupportedException(StringResources.AuthZNotSupported, exception);
					}
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.AuthzFreeContext(zero);
				}
				if (intPtr != IntPtr.Zero)
				{
					UnsafeNativeMethods.AuthzFreeResourceManager(intPtr);
				}
				if (zero1 != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero1);
				}
			}
		}

		[SecurityCritical]
		public override void Dispose()
		{
			try
			{
				if (!this.disposed)
				{
					this.psBuffer.Close();
					this.psUserSid.Close();
					this.psMachineSid.Close();
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		[SecurityCritical]
		internal override bool MoveNext()
		{
			bool flag;
			do
			{
				flag = false;
				AuthZSet authZSet = this;
				authZSet.currentGroup = authZSet.currentGroup + 1;
				if (this.currentGroup < this.groupSidList.Length)
				{
					if (this.userType != ContextType.Machine)
					{
						continue;
					}
					IntPtr item = this.groupSidList[this.currentGroup].pSid;
					bool flag1 = false;
					if (Utils.ClassifySID(item) != SidType.RealObject || !UnsafeNativeMethods.EqualDomainSid(this.psUserSid.DangerousGetHandle(), item, ref flag1) || !flag1)
					{
						continue;
					}
					int lastRidFromSid = Utils.GetLastRidFromSid(item);
					if (lastRidFromSid != 0x201)
					{
						continue;
					}
					flag = true;
				}
				else
				{
					return false;
				}
			}
			while (flag);
			return true;
		}

		internal override void Reset()
		{
			this.currentGroup = -1;
		}

		[SecurityCritical(SecurityCriticalScope.Everything)]
		private sealed class SafeMemoryPtr : SafeHandle
		{
			public override bool IsInvalid
			{
				get
				{
					return this.handle == IntPtr.Zero;
				}
			}

			private SafeMemoryPtr() : base(IntPtr.Zero, true)
			{
			}

			internal SafeMemoryPtr(IntPtr handle) : base(IntPtr.Zero, true)
			{
				base.SetHandle(handle);
			}

			protected override bool ReleaseHandle()
			{
				if (this.handle != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(this.handle);
				}
				return true;
			}
		}
	}
}