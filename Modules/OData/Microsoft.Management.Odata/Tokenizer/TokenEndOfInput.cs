using Microsoft.Management.Odata.MofParser;

namespace Tokenizer
{
	internal sealed class TokenEndOfInput : Token
	{
		public override TokenType Type
		{
			get
			{
				return TokenType.EndOfInput;
			}
		}

		internal TokenEndOfInput(DocumentRange range) : base(range)
		{
		}
	}
}