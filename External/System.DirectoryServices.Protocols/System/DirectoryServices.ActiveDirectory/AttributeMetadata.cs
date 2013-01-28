using System;
using System.Collections;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class AttributeMetadata
	{
		private string pszAttributeName;

		private int dwVersion;

		private DateTime ftimeLastOriginatingChange;

		private Guid uuidLastOriginatingDsaInvocationID;

		private long usnOriginatingChange;

		private long usnLocalChange;

		private string pszLastOriginatingDsaDN;

		private string originatingServerName;

		private DirectoryServer server;

		private Hashtable nameTable;

		private bool advanced;

		public DateTime LastOriginatingChangeTime
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.ftimeLastOriginatingChange;
			}
		}

		public Guid LastOriginatingInvocationId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.uuidLastOriginatingDsaInvocationID;
			}
		}

		public long LocalChangeUsn
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.usnLocalChange;
			}
		}

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.pszAttributeName;
			}
		}

		public long OriginatingChangeUsn
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.usnOriginatingChange;
			}
		}

		public string OriginatingServer
		{
			get
			{
				if (this.originatingServerName == null)
				{
					if (!this.nameTable.Contains(this.LastOriginatingInvocationId))
					{
						if (!this.advanced || this.advanced && this.pszLastOriginatingDsaDN != null)
						{
							this.originatingServerName = Utils.GetServerNameFromInvocationID(this.pszLastOriginatingDsaDN, this.LastOriginatingInvocationId, this.server);
							this.nameTable.Add(this.LastOriginatingInvocationId, this.originatingServerName);
						}
					}
					else
					{
						this.originatingServerName = (string)this.nameTable[(object)this.LastOriginatingInvocationId];
					}
				}
				return this.originatingServerName;
			}
		}

		public int Version
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dwVersion;
			}
		}

		internal AttributeMetadata(IntPtr info, bool advanced, DirectoryServer server, Hashtable table)
		{
			if (!advanced)
			{
				DS_REPL_ATTR_META_DATA dSREPLATTRMETADATum = new DS_REPL_ATTR_META_DATA();
				Marshal.PtrToStructure(info, dSREPLATTRMETADATum);
				this.pszAttributeName = Marshal.PtrToStringUni(dSREPLATTRMETADATum.pszAttributeName);
				this.dwVersion = dSREPLATTRMETADATum.dwVersion;
				long num = (long)dSREPLATTRMETADATum.ftimeLastOriginatingChange1 + ((long)dSREPLATTRMETADATum.ftimeLastOriginatingChange2 << 32);
				this.ftimeLastOriginatingChange = DateTime.FromFileTime(num);
				this.uuidLastOriginatingDsaInvocationID = dSREPLATTRMETADATum.uuidLastOriginatingDsaInvocationID;
				this.usnOriginatingChange = dSREPLATTRMETADATum.usnOriginatingChange;
				this.usnLocalChange = dSREPLATTRMETADATum.usnLocalChange;
			}
			else
			{
				DS_REPL_ATTR_META_DATA_2 dSREPLATTRMETADATA2 = new DS_REPL_ATTR_META_DATA_2();
				Marshal.PtrToStructure(info, dSREPLATTRMETADATA2);
				this.pszAttributeName = Marshal.PtrToStringUni(dSREPLATTRMETADATA2.pszAttributeName);
				this.dwVersion = dSREPLATTRMETADATA2.dwVersion;
				long num1 = (long)dSREPLATTRMETADATA2.ftimeLastOriginatingChange1 + ((long)dSREPLATTRMETADATA2.ftimeLastOriginatingChange2 << 32);
				this.ftimeLastOriginatingChange = DateTime.FromFileTime(num1);
				this.uuidLastOriginatingDsaInvocationID = dSREPLATTRMETADATA2.uuidLastOriginatingDsaInvocationID;
				this.usnOriginatingChange = dSREPLATTRMETADATA2.usnOriginatingChange;
				this.usnLocalChange = dSREPLATTRMETADATA2.usnLocalChange;
				this.pszLastOriginatingDsaDN = Marshal.PtrToStringUni(dSREPLATTRMETADATA2.pszLastOriginatingDsaDN);
			}
			this.server = server;
			this.nameTable = table;
			this.advanced = advanced;
		}
	}
}