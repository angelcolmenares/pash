using System;

namespace System.Management
{
	internal class InternalObjectPutEventArgs : EventArgs
	{
		private ManagementPath path;

		internal ManagementPath Path
		{
			get
			{
				return this.path;
			}
		}

		internal InternalObjectPutEventArgs(ManagementPath path)
		{
			this.path = path.Clone();
		}
	}
}