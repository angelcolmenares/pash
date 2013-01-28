namespace System.DirectoryServices.AccountManagement
{
	internal static class DefaultContextOptions
	{
		internal static ContextOptions MachineDefaultContextOption;

		internal static ContextOptions ADDefaultContextOption;

		static DefaultContextOptions()
		{
			DefaultContextOptions.MachineDefaultContextOption = ContextOptions.Negotiate;
			DefaultContextOptions.ADDefaultContextOption = ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing;
		}
	}
}