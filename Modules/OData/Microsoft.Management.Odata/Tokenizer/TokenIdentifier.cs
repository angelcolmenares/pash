using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal class TokenIdentifier : Token
	{
		private readonly string m_identifier;

		public virtual bool IsKeyword
		{
			get
			{
				return false;
			}
		}

		public override TokenType Type
		{
			get
			{
				return TokenType.Identifier;
			}
		}

		public string Value
		{
			get
			{
				return this.m_identifier;
			}
		}

		public string ValueUpperCase
		{
			get
			{
				return this.Value.ToUpperInvariant();
			}
		}

		internal TokenIdentifier(string identifier, DocumentRange range) : base(range)
		{
			this.m_identifier = identifier;
		}

		public bool MatchesCaseInsensitive(string value)
		{
			return string.Compare(value, this.m_identifier, true) == 0;
		}

		public override string ToString()
		{
			return string.Format("IDENTIFIER:{0}@{1}", this.m_identifier, base.Location);
		}
	}
}