using Microsoft.Management.Odata.MofParser;
using System;

namespace Tokenizer
{
	internal sealed class TokenInteger : Token
	{
		private readonly long m_value;

		public override TokenType Type
		{
			get
			{
				return TokenType.Integer;
			}
		}

		public long Value
		{
			get
			{
				return this.m_value;
			}
		}

		internal TokenInteger(long value, DocumentRange range) : base(range)
		{
			this.m_value = value;
		}

		public override string ToString()
		{
			return string.Format("INTEGER:{0}@{1}", this.Value, base.Location);
		}
	}
}