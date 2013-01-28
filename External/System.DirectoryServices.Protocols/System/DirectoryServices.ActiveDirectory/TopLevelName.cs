using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class TopLevelName
	{
		private string name;

		private TopLevelNameStatus status;

		internal LARGE_INTEGER time;

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		public TopLevelNameStatus Status
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.status;
			}
			set
			{
				if (value == TopLevelNameStatus.Enabled || value == TopLevelNameStatus.NewlyCreated || value == TopLevelNameStatus.AdminDisabled || value == TopLevelNameStatus.ConflictDisabled)
				{
					this.status = value;
					return;
				}
				else
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(TopLevelNameStatus));
				}
			}
		}

		internal TopLevelName(int flag, LSA_UNICODE_STRING val, LARGE_INTEGER time)
		{
			this.status = (TopLevelNameStatus)flag;
			this.name = Marshal.PtrToStringUni(val.Buffer, val.Length / 2);
			this.time = time;
		}
	}
}