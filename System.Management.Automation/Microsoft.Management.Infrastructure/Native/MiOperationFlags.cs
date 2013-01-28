using System;

namespace Microsoft.Management.Infrastructure.Native
{
	[Flags]
	internal enum MiOperationFlags : uint
	{
		ManualAckResults = 1,
		BasicRtti = 2,
		FullRtti = 4,
		LocalizedQualifiers = 8,
		ExpensiveProperties = 64,
		PolymorphismShallow = 128,
		PolymorphismDeepBasePropsOnly = 384,
		ReportOperationStarted = 512,
		NoRtti = 1024,
		StandardRtti = 2048
	}
}