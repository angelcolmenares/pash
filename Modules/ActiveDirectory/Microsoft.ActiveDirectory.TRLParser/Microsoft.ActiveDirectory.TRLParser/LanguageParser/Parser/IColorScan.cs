using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal interface IColorScan
	{
		int GetNext(ref int state, out int startPosition, out int endPosition);

		void SetSource(string source, int offset);
	}
}