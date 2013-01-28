using System;

namespace Microsoft.Management.Infrastructure.Options
{
	[Flags]
	public enum CimOperationFlags : long
	{
		None = 0,
		BasicTypeInformation = 2,
		FullTypeInformation = 4,
		LocalizedQualifiers = 8,
		ExpensiveProperties = 64,
		PolymorphismShallow = 128,
		PolymorphismDeepBasePropsOnly = 384,
		ReportOperationStarted = 512,
		NoTypeInformation = 1024,
		StandardTypeInformation = 2048
	}
}