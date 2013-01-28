using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal abstract class AScanner<YYSTYPE, YYLTYPE>
	where YYSTYPE : struct
	where YYLTYPE : IMerge<YYLTYPE>
	{
		public YYSTYPE yylval;

		public YYLTYPE yylloc;

		protected AScanner()
		{
		}

		public virtual void yyerror(string[] expectedTokens, string actualToken)
		{
		}

		public virtual void yyerror(string format, object[] args)
		{
		}

		public abstract int yylex();
	}
}