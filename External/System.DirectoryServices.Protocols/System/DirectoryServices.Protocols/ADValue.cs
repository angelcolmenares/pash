using System;

namespace System.DirectoryServices.Protocols
{
	internal class ADValue
	{
		public bool IsBinary;

		public string StringVal;

		public byte[] BinaryVal;

		public ADValue()
		{
			this.IsBinary = false;
			this.BinaryVal = null;
		}
	}
}