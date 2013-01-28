using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer
{
	internal sealed class TokenStringValue : Token
	{
		private readonly Token[] m_parts;

		private readonly string m_string;

		public IEnumerable<Token> Parts
		{
			get
			{
				return this.m_parts;
			}
		}

		public string StringValue
		{
			get
			{
				return this.m_string;
			}
		}

		public override TokenType Type
		{
			get
			{
				return TokenType.StringValue;
			}
		}

		internal TokenStringValue(Token[] parts, DocumentRange range) : base(range)
		{
			this.m_parts = parts;
			StringBuilder stringBuilder = new StringBuilder();
			Token[] tokenArray = parts;
			for (int i = 0; i < (int)tokenArray.Length; i++)
			{
				Token token = tokenArray[i];
				if (token.Type == TokenType.StringPart)
				{
					stringBuilder.Append(((TokenStringPart)token).StringValue);
				}
			}
			this.m_string = stringBuilder.ToString();
		}

		public override string ToString()
		{
			return string.Format("\"{0}\"", this.StringValue);
		}
	}
}