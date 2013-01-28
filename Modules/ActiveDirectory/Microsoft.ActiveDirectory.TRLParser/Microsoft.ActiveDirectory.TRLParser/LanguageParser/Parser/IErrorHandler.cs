using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal interface IErrorHandler
	{
		void AddError(string msg, int lin, int col, int len, int severity);
	}
}