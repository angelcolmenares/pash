using System;
using System.Collections;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	internal class dSPropertyValueCollection
	{
		private PropertyValueCollection pc;

		private ResultPropertyValueCollection rc;

		public int Count
		{
			get
			{
				if (this.pc != null)
				{
					return this.pc.Count;
				}
				else
				{
					return this.rc.Count;
				}
			}
		}

		public object this[int index]
		{
			[SecurityCritical]
			get
			{
				if (this.pc == null)
				{
					return this.rc[index];
				}
				else
				{
					return this.pc[index];
				}
			}
		}

		private dSPropertyValueCollection()
		{
		}

		internal dSPropertyValueCollection(PropertyValueCollection pc)
		{
			this.pc = pc;
		}

		internal dSPropertyValueCollection(ResultPropertyValueCollection rc)
		{
			this.rc = rc;
		}

		public IEnumerator GetEnumerator()
		{
			if (this.pc != null)
			{
				return this.pc.GetEnumerator();
			}
			else
			{
				return this.rc.GetEnumerator();
			}
		}
	}
}