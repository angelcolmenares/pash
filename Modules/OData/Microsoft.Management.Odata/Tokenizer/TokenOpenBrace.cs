using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenOpenBrace : Token
	{
		public override TokenType Type
		{
			get
			{
				return TokenType.OpenBrace;
			}
		}

		internal TokenOpenBrace(DocumentRange range) : base(range)
		{
		}

		public override string ToString()
		{
			DocumentRange location = base.Location;
			return string.Format("'{{'@{0}", location.Start);
		}
	}
}