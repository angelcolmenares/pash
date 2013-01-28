using System;

namespace TaskScheduler.Implementation
{
	public class CronTaskFolder : ITaskFolder
	{
		public CronTaskFolder ()
		{
		}

		#region ITaskFolder implementation

		public void _VtblGap1_4 ()
		{

		}

		public ITaskFolder CreateFolder (string subFolderName, object sddl)
		{
			return null;
		}

		public void _VtblGap2_1 ()
		{

		}

		public IRegisteredTask GetTask (string Path)
		{
			throw new NotImplementedException ();
		}

		public void _VtblGap3_1 ()
		{

		}

		public void DeleteTask (string Name, int flags)
		{

		}

		public void _VtblGap4_1 ()
		{

		}

		public IRegisteredTask RegisterTaskDefinition (string Path, ITaskDefinition pDefinition, int flags, object UserId, object password, _TASK_LOGON_TYPE LogonType, object sddl)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

