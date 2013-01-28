
namespace System.DirectoryServices {
	
	[Flags]
	public enum ActiveDirectoryRights
	{
		CreateChild = 1,
		DeleteChild = 2,
		ListChildren = 4,
		Self = 8,
		ReadProperty = 16,
		WriteProperty = 32,
		DeleteTree = 64,
		ListObject = 128,
		ExtendedRight = 256,
		Delete = 65536,
		ReadControl = 131072,
		GenericExecute = 131076,
		GenericWrite = 131112,
		GenericRead = 131220,
		WriteDacl = 262144,
		WriteOwner = 524288,
		GenericAll = 983551,
		Synchronize = 1048576,
		AccessSystemSecurity = 16777216
	}
}