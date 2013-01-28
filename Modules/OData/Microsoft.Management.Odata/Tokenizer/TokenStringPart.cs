using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenStringPart : Token
	{
		private readonly string m_stringValue;

		public string StringValue
		{
			get
			{
				return this.m_stringValue;
			}
		}

		public override TokenType Type
		{
			get
			{
				return TokenType.StringPart;
			}
		}

		internal TokenStringPart(DocumentRange range, string value) : base(range)
		{
			this.m_stringValue = value;
		}

		public override string ToString()
		{
			return string.Format("\"{0}\"", this.m_stringValue);
		}
	}
}