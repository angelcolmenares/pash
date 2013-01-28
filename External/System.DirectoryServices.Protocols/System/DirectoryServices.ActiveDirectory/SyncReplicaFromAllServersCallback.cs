using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal delegate bool SyncReplicaFromAllServersCallback(IntPtr data, IntPtr update);
}