using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenCloseBrace : Token
	{
		public override TokenType Type
		{
			get
			{
				return TokenType.CloseBrace;
			}
		}

		internal TokenCloseBrace(DocumentRange range) : base(range)
		{
		}

		public override string ToString()
		{
			DocumentRange location = base.Location;
			return string.Format("'}}'@{0}", location.Start);
		}
	}
}