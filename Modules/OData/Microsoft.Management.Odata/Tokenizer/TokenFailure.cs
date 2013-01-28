using Microsoft.Management.Odata.MofParser;

namespace Tokenizer
{
	internal sealed class TokenFailure : Token
	{
		public override TokenType Type
		{
			get
			{
				return TokenType.Failure;
			}
		}

		internal TokenFailure(DocumentRange range) : base(range)
		{
		}
	}
}