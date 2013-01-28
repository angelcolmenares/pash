namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal interface IMerge<YYLTYPE>
	{
		YYLTYPE Merge(YYLTYPE last);
	}
}