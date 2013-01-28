namespace System.DirectoryServices.Protocols
{
	internal class UtilityHandle
	{
		private static ConnectionHandle handle;

		static UtilityHandle()
		{
			UtilityHandle.handle = new ConnectionHandle();
		}

		public UtilityHandle()
		{
		}

		public static ConnectionHandle GetHandle()
		{
			return UtilityHandle.handle;
		}
	}
}