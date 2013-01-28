namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Diagnostics
{
	internal static class DebugLog
	{
		private readonly static TraceLog _policyEngineTraceLog;

		public static TraceLog PolicyEngineTraceLog
		{
			get
			{
				return DebugLog._policyEngineTraceLog;
			}
		}

		static DebugLog()
		{
			DebugLog._policyEngineTraceLog = new TraceLog("ClaimsTransformationRulesParser");
		}
	}
}