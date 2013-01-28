using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenAlias : Token
	{
		private readonly string m_identifier;

		public override TokenType Type
		{
			get
			{
				return TokenType.Alias;
			}
		}

		internal TokenAlias(string identifier, DocumentRange range) : base(range)
		{
			this.m_identifier = identifier;
		}

		public override string ToString()
		{
			return string.Concat("ALIAS: ", this.m_identifier);
		}
	}
}