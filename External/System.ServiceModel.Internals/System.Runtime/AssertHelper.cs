using System;
using System.Diagnostics;

namespace System.Runtime
{
	internal static class AssertHelper
	{
		internal static void FireAssert(string message)
		{
			try
			{
			}
			finally
			{
				Debug.Assert(false, message);
			}
		}
	}
}