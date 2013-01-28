using Microsoft.Management.Odata.MofParser;

namespace Tokenizer
{
	internal class TokenWhitespace : Token
	{
		public override TokenType Type
		{
			get
			{
				return TokenType.Whitespace;
			}
		}

		public virtual WhitespaceType WhitespaceType
		{
			get
			{
				return WhitespaceType.Other;
			}
		}

		internal TokenWhitespace(DocumentRange range) : base(range)
		{
		}
	}
}