using System;

namespace System.Runtime.Diagnostics
{
	internal enum EventLogCategory : ushort
	{
		ServiceAuthorization = 1,
		MessageAuthentication = 2,
		ObjectAccess = 3,
		Tracing = 4,
		WebHost = 5,
		FailFast = 6,
		MessageLogging = 7,
		PerformanceCounter = 8,
		Wmi = 9,
		ComPlus = 10,
		StateMachine = 11,
		Wsat = 12,
		SharingService = 13,
		ListenerAdapter = 14
	}
}