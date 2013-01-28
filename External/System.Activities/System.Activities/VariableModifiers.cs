using System;

namespace System.Activities
{
	[Flags]
	public enum VariableModifiers
	{
		None = 0,
		ReadOnly = 1,
		Mapped = 2
	}
}
