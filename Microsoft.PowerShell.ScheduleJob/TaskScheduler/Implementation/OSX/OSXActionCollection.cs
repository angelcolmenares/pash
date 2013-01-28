using System;

namespace TaskScheduler.Implementation
{
	public class OSXActionCollection : IActionCollection
	{
		public OSXActionCollection ()
		{

		}

		#region IActionCollection implementation

		public void _VtblGap1_5 ()
		{

		}

		public IAction Create (_TASK_ACTION_TYPE type)
		{
			IAction action = null;
			switch (type) {
				case _TASK_ACTION_TYPE.TASK_ACTION_COM_HANDLER:
					throw new NotSupportedException();
				case _TASK_ACTION_TYPE.TASK_ACTION_EXEC:
					action = new OSXExecAction();
					break;
				case _TASK_ACTION_TYPE.TASK_ACTION_SEND_EMAIL:
					break;
				case _TASK_ACTION_TYPE.TASK_ACTION_SHOW_MESSAGE:
					break;
			}
			return action;
		}

		public void _VtblGap2_1 ()
		{

		}

		public void Clear ()
		{

		}

		#endregion

		#region IEnumerable implementation

		public System.Collections.IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

