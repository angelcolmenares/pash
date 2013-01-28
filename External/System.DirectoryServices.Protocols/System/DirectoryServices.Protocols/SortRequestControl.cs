using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	public class SortRequestControl : DirectoryControl
	{
		private SortKey[] keys;

		public SortKey[] SortKeys
		{
			get
			{
				if (this.keys != null)
				{
					SortKey[] sortKey = new SortKey[(int)this.keys.Length];
					for (int i = 0; i < (int)this.keys.Length; i++)
					{
						sortKey[i] = new SortKey(this.keys[i].AttributeName, this.keys[i].MatchingRule, this.keys[i].ReverseOrder);
					}
					return sortKey;
				}
				else
				{
					return new SortKey[0];
				}
			}
			set
			{
				if (value != null)
				{
					int num = 0;
					while (num < (int)value.Length)
					{
						if (value[num] != null)
						{
							num++;
						}
						else
						{
							throw new ArgumentException(Res.GetString("NullValueArray"), "value");
						}
					}
					this.keys = new SortKey[(int)value.Length];
					for (int i = 0; i < (int)value.Length; i++)
					{
						this.keys[i] = new SortKey(value[i].AttributeName, value[i].MatchingRule, value[i].ReverseOrder);
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public SortRequestControl(SortKey[] sortKeys) : base("1.2.840.113556.1.4.473", null, true, true)
		{
			this.keys = new SortKey[0];
			if (sortKeys != null)
			{
				int num = 0;
				while (num < (int)sortKeys.Length)
				{
					if (sortKeys[num] != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException(Res.GetString("NullValueArray"), "sortKeys");
					}
				}
				this.keys = new SortKey[(int)sortKeys.Length];
				for (int i = 0; i < (int)sortKeys.Length; i++)
				{
					this.keys[i] = new SortKey(sortKeys[i].AttributeName, sortKeys[i].MatchingRule, sortKeys[i].ReverseOrder);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("sortKeys");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SortRequestControl(string attributeName, bool reverseOrder) : this(attributeName, null, reverseOrder)
		{
		}

		public SortRequestControl(string attributeName, string matchingRule, bool reverseOrder) : base("1.2.840.113556.1.4.473", null, true, true)
		{
			this.keys = new SortKey[0];
			SortKey sortKey = new SortKey(attributeName, matchingRule, reverseOrder);
			this.keys = new SortKey[1];
			this.keys[0] = sortKey;
		}

		public override byte[] GetValue()
		{
			IntPtr intPtr;
			byte num;
			IntPtr intPtr1 = (IntPtr)0;
			int num1 = Marshal.SizeOf(typeof(SortKey));
			IntPtr intPtr2 = Utility.AllocHGlobalIntPtrArray((int)this.keys.Length + 1);
			try
			{
				int i = 0;
				for (i = 0; i < (int)this.keys.Length; i++)
				{
					IntPtr intPtr3 = Marshal.AllocHGlobal(num1);
					Marshal.StructureToPtr(this.keys[i], intPtr3, false);
					intPtr = (IntPtr)((long)intPtr2 + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
					Marshal.WriteIntPtr(intPtr, intPtr3);
				}
				intPtr = (IntPtr)((long)intPtr2 + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
				Marshal.WriteIntPtr(intPtr, (IntPtr)0);
				bool isCritical = base.IsCritical;
				ConnectionHandle handle = UtilityHandle.GetHandle();
				IntPtr intPtr4 = intPtr2;
				if (isCritical)
				{
					num = 1;
				}
				else
				{
					num = 0;
				}
				int num2 = Wldap32.ldap_create_sort_control(handle, intPtr4, num, ref intPtr1);
				if (num2 == 0)
				{
					LdapControl ldapControl = new LdapControl();
					Marshal.PtrToStructure(intPtr1, ldapControl);
					berval ldctlValue = ldapControl.ldctl_value;
					this.directoryControlValue = null;
					if (ldctlValue != null)
					{
						this.directoryControlValue = new byte[ldctlValue.bv_len];
						Marshal.Copy(ldctlValue.bv_val, this.directoryControlValue, 0, ldctlValue.bv_len);
					}
				}
				else
				{
					if (!Utility.IsLdapError((LdapError)num2))
					{
						throw new LdapException(num2);
					}
					else
					{
						string str = LdapErrorMappings.MapResultCode(num2);
						throw new LdapException(num2, str);
					}
				}
			}
			finally
			{
				if (intPtr1 != (IntPtr)0)
				{
					Wldap32.ldap_control_free(intPtr1);
				}
				if (intPtr2 != (IntPtr)0)
				{
					for (int j = 0; j < (int)this.keys.Length; j++)
					{
						IntPtr intPtr5 = Marshal.ReadIntPtr(intPtr2, Marshal.SizeOf(typeof(IntPtr)) * j);
						if (intPtr5 != (IntPtr)0)
						{
							IntPtr intPtr6 = Marshal.ReadIntPtr(intPtr5);
							if (intPtr6 != (IntPtr)0)
							{
								Marshal.FreeHGlobal(intPtr6);
							}
							intPtr6 = Marshal.ReadIntPtr(intPtr5, Marshal.SizeOf(typeof(IntPtr)));
							if (intPtr6 != (IntPtr)0)
							{
								Marshal.FreeHGlobal(intPtr6);
							}
							Marshal.FreeHGlobal(intPtr5);
						}
					}
					Marshal.FreeHGlobal(intPtr2);
				}
			}
			return base.GetValue();
		}
	}
}