namespace System.DirectoryServices.ActiveDirectory
{
	internal enum SearchFlags
	{
		None = 0,
		IsIndexed = 1,
		IsIndexedOverContainer = 2,
		IsInAnr = 4,
		IsOnTombstonedObject = 8,
		IsTupleIndexed = 32
	}
}