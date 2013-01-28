using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenKeyword : TokenIdentifier
	{
		private readonly KeywordType m_type;

		public override bool IsKeyword
		{
			get
			{
				return true;
			}
		}

		public KeywordType KeywordType
		{
			get
			{
				return this.m_type;
			}
		}

		internal TokenKeyword(KeywordType type, string identifier, DocumentRange range) : base(identifier, range)
		{
			this.m_type = type;
		}

		public override string ToString()
		{
			return string.Format("KEYWORD:{0}@{1}", this.KeywordType, base.Location);
		}
	}
}