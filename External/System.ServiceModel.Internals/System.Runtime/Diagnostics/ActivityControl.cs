using System;

namespace System.Runtime.Diagnostics
{
	internal enum ActivityControl : uint
	{
		EVENT_ACTIVITY_CTRL_GET_ID = 1,
		EVENT_ACTIVITY_CTRL_SET_ID = 2,
		EVENT_ACTIVITY_CTRL_CREATE_ID = 3,
		EVENT_ACTIVITY_CTRL_GET_SET_ID = 4,
		EVENT_ACTIVITY_CTRL_CREATE_SET_ID = 5
	}
}