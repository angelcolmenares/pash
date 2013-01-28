using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenPragma : Token
	{
		private readonly string m_value;

		public override TokenType Type
		{
			get
			{
				return TokenType.Pragma;
			}
		}

		internal TokenPragma(string value, DocumentRange range) : base(range)
		{
			this.m_value = value;
		}

		public override string ToString()
		{
			return string.Format("PRAGMA@{0}", base.Location);
		}
	}
}