using System;

namespace System.DirectoryServices.Protocols
{
	internal sealed class LdapVlvInfo
	{
		private int version;

		private int beforeCount;

		private int afterCount;

		private int offset;

		private int count;

		private IntPtr attrvalue;

		private IntPtr context;

		private IntPtr extraData;

		public LdapVlvInfo(int version, int before, int after, int offset, int count, IntPtr attribute, IntPtr context)
		{
			this.version = 1;
			this.attrvalue = (IntPtr)0;
			this.context = (IntPtr)0;
			this.extraData = (IntPtr)0;
			this.version = version;
			this.beforeCount = before;
			this.afterCount = after;
			this.offset = offset;
			this.count = count;
			this.attrvalue = attribute;
			this.context = context;
		}
	}
}