using System;
using System.DirectoryServices;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	internal class dSPropertyCollection
	{
		private PropertyCollection pc;

		private ResultPropertyCollection rp;

		public dSPropertyValueCollection this[string propertyName]
		{
			[SecurityCritical]
			get
			{
				if (propertyName != null)
				{
					if (this.pc == null)
					{
						return new dSPropertyValueCollection(this.rp[propertyName]);
					}
					else
					{
						return new dSPropertyValueCollection(this.pc[propertyName]);
					}
				}
				else
				{
					throw new ArgumentNullException("propertyName");
				}
			}
		}

		private dSPropertyCollection()
		{
		}

		internal dSPropertyCollection(PropertyCollection pc)
		{
			this.pc = pc;
		}

		internal dSPropertyCollection(ResultPropertyCollection rp)
		{
			this.rp = rp;
		}
	}
}