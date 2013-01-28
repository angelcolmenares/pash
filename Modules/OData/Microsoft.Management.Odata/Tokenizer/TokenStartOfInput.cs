using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenStartOfInput : Token
	{
		public override TokenType Type
		{
			get
			{
				return TokenType.StartOfInput;
			}
		}

		internal TokenStartOfInput(string documentPath) : base(new DocumentRange(documentPath, new DocumentCoordinate(1, 1), new DocumentCoordinate(1, 1)))
		{
		}
	}
}