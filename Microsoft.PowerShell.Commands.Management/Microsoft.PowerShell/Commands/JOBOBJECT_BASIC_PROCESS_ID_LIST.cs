using System;

namespace Microsoft.PowerShell.Commands
{
	internal struct JOBOBJECT_BASIC_PROCESS_ID_LIST
	{
		public uint NumberOfAssignedProcess;

		public uint NumberOfProcessIdsInList;

		public IntPtr ProcessIdList;

	}
}