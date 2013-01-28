using System;
using System.Diagnostics;
using System.Runtime;

namespace System.Runtime.Diagnostics
{
	internal class DiagnosticTraceSource : TraceSource
	{
		private const string PropagateActivityValue = "propagateActivity";

		internal bool PropagateActivity
		{
			get
			{
				bool flag = false;
				string item = base.Attributes["propagateActivity"];
				if (!string.IsNullOrEmpty(item) && !bool.TryParse(item, out flag))
				{
					flag = false;
				}
				return flag;
			}
			set
			{
				base.Attributes["propagateActivity"] = value.ToString();
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal DiagnosticTraceSource(string name) : base(name)
		{
		}

		protected override string[] GetSupportedAttributes()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "propagateActivity";
			return strArrays;
		}
	}
}