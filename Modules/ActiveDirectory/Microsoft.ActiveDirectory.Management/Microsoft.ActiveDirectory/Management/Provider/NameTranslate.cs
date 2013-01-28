namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal static class NameTranslate
	{
		public enum HostType
		{
			Domain = 1,
			Server = 2,
			GC = 3
		}

		public enum PathFormat
		{
			DistinguishedName = 1,
			CanonicalName = 2
		}
	}
}