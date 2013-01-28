using System;

namespace System.Runtime.Diagnostics
{
	internal enum EventFacility : uint
	{
		Tracing = 65536,
		ServiceModel = 131072,
		TransactionBridge = 196608,
		SMSvcHost = 262144,
		InfoCards = 327680,
		SecurityAudit = 393216
	}
}