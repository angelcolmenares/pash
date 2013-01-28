using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Workflow
{
	internal class CompareBasedOnCommand : IEqualityComparer<WorkflowJobDefinition>
	{
		private readonly static CompareBasedOnCommand Comparer;

		static CompareBasedOnCommand()
		{
			CompareBasedOnCommand.Comparer = new CompareBasedOnCommand();
		}

		public CompareBasedOnCommand()
		{
		}

		internal static bool Compare(WorkflowJobDefinition x, WorkflowJobDefinition y)
		{
			return CompareBasedOnCommand.Comparer.Equals(x, y);
		}

		public bool Equals(WorkflowJobDefinition x, WorkflowJobDefinition y)
		{
			bool flag = false;
			if (string.Equals(y.ModulePath, x.ModulePath, StringComparison.OrdinalIgnoreCase) && string.Equals(y.Command, x.Command, StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
			}
			return flag;
		}

		public int GetHashCode(WorkflowJobDefinition obj)
		{
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(obj.ModulePath))
			{
				empty = string.Concat(empty, obj.ModulePath);
			}
			if (!string.IsNullOrEmpty(obj.Command))
			{
				empty = string.Concat(empty, obj.Command);
			}
			int hashCode = empty.GetHashCode();
			return hashCode;
		}
	}
}