using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LSA_OBJECT_ATTRIBUTES
	{
		internal int Length;

		private IntPtr RootDirectory;

		private IntPtr ObjectName;

		internal int Attributes;

		private IntPtr SecurityDescriptor;

		private IntPtr SecurityQualityOfService;

		public LSA_OBJECT_ATTRIBUTES()
		{
			this.Length = 0;
			this.RootDirectory = (IntPtr)0;
			this.ObjectName = (IntPtr)0;
			this.Attributes = 0;
			this.SecurityDescriptor = (IntPtr)0;
			this.SecurityQualityOfService = (IntPtr)0;
		}
	}
}